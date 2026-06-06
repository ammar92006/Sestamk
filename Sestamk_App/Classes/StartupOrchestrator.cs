using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Sestamk.Forms;

namespace Sestamk.Classes
{
    /// <summary>
    /// Coordinates app startup steps and reports progress to the splash form.
    /// </summary>
    public sealed class StartupOrchestrator
    {
        private readonly Action<string> _setStatus;
        private readonly IWin32Window? _owner;

        public StartupOrchestrator(Action<string> setStatus, IWin32Window? owner = null)
        {
            _setStatus = setStatus;
            _owner = owner;
        }

        public async Task<StartupAction> RunAsync()
        {
            // ── Step 1: Database connection ──
            if (!await EnsureDatabaseAsync())
                return StartupAction.Exit;

            // ── Step 2: Load settings cache from DB ──
            _setStatus("جاري تحميل الإعدادات...");
            await SettingsService.LoadAllAsync();

            // ── Step 3: WhatsApp services (non-fatal) ──
            _setStatus("جاري تهيئة خدمات الواتساب...");
            await InitWhatsAppAsync();

            // ── Step 4: License + optional update notifier ──
            _setStatus("جاري التحقق من الترخيص...");
            var (action, licenseData) = await CheckLicenseAsync();
            if (action != StartupAction.LaunchLogin)
                return action;

            if (licenseData != null)
            {
                _setStatus("جاري التحقق من التحديثات...");
                if (!await HandleUpdateIfAnyAsync(licenseData))
                    return StartupAction.Exit; // user dodged mandatory update
            }

            _setStatus("جاهز...");
            return StartupAction.LaunchLogin;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  DB
        // ─────────────────────────────────────────────────────────────────────
        private async Task<bool> EnsureDatabaseAsync()
        {
            _setStatus("جاري الاتصال بقاعدة البيانات...");

            while (true)
            {
                var (ok, error) = await TryPingDatabaseAsync();
                if (ok) return true;

                bool fixedIt = frmDbConnect.ShowAndConnect(_owner, error);
                if (!fixedIt) return false;

                _setStatus("جاري إعادة المحاولة...");
            }
        }

        /// <summary>
        /// Tries to open a connection using current SecureConfig settings.
        /// Enforces a hard wall-clock timeout because SqlClient's built-in
        /// ConnectTimeout (and even OpenAsync's CancellationToken) do NOT reliably
        /// cover SQL Browser UDP resolution for non-existent named instances —
        /// that path can hang for tens of seconds before anything throws.
        /// We race the connect against a Task.Delay and abandon the connect task
        /// if it loses.
        /// </summary>
        public static async Task<(bool ok, string? error)> TryPingDatabaseAsync(int timeoutSeconds = 12)
        {
            var openTask = Task.Run(() =>
            {
                try
                {
                    using var conn = DB_Server.GetConnection();
                    conn.Open();
                    return (true, (string?)null);
                }
                catch (SqlException ex) { return (false, (string?)FormatSqlError(ex)); }
                catch (Exception ex)    { return (false, (string?)ex.Message); }
            });

            var finished = await Task.WhenAny(openTask, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));

            if (finished != openTask)
            {
                // Abandon the in-flight connect — it may keep running in the background
                // but our startup flow moves on. (Unavoidable until SqlClient fixes its
                // UDP-resolution cancellation.)
                return (false, $"انتهت مهلة الاتصال بعد {timeoutSeconds} ثانية.\n" +
                              $"تأكد من تشغيل SQL Server وأن اسم الخادم (\"{SecureConfig.DbDataSource}\") صحيح.");
            }

            return openTask.Result;
        }

        private static string FormatSqlError(SqlException ex)
        {
            // Common error codes for network/instance issues → friendly message
            if (ex.Number == 26 || ex.Number == 40 || ex.Number == -1 || ex.Number == 53)
            {
                return $"تعذّر الوصول إلى خادم قاعدة البيانات (\"{SecureConfig.DbDataSource}\").\n" +
                       "تأكد من تشغيل SQL Server وأن اسم الخادم صحيح.";
            }
            if (ex.Number == 4060 || ex.Number == 18456)
            {
                return $"تم الاتصال بالخادم لكن فشل التحقق من الصلاحيات.\n{ex.Message}";
            }
            return ex.Message;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  WhatsApp (non-fatal)
        // ─────────────────────────────────────────────────────────────────────
        private async Task InitWhatsAppAsync()
        {
            try
            {
                await WhatsAppQueueManager.InitialiseAsync();
                if (SettingsService.WhatsAppEnabled)
                {
                    WhatsAppWorkerService.Start();
                    _ = Task.Run(() => WhatsAppService.EnsureBridgeRunningAsync());
                }

                Application.ApplicationExit += (s, e) =>
                {
                    try { WhatsAppWorkerService.StopAsync().GetAwaiter().GetResult(); } catch { }
                    try { WhatsAppService.ShutdownBridgeAsync().GetAwaiter().GetResult(); } catch { }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WhatsApp init failed: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  License
        // ─────────────────────────────────────────────────────────────────────
        private async Task<(StartupAction action, JObject? licenseData)> CheckLicenseAsync()
        {
            var localLicense = LocalLicenseManager.ReadLocalLicense();

            if (localLicense == null)
                return (StartupAction.LaunchActivation, null);

            string? savedSerial = localLicense["saved_serial"]?.ToString();
            try
            {
                var result = await LicenseManager.CheckLicenseAsync(savedSerial);

                string status = result["status"]?.ToString();
                if (status == "active" || status == "grace_period")
                {
                    result["saved_serial"] = savedSerial;
                    LocalLicenseManager.SaveLicenseLocally(result);

                    // مزامنة مستخدمي الشركة إلى لوحة التحكم (خلفية، غير حرجة)
                    _ = Task.Run(UserSyncService.SyncAsync);

                    // عرض تحذير فترة السماح إن وُجد
                    if (status == "grace_period")
                    {
                        string? warn = result["warning"]?.ToString();
                        if (!string.IsNullOrEmpty(warn))
                            MessageBox.Show(_owner, warn, "تنبيه الاشتراك",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    return (StartupAction.LaunchLogin, result);
                }

                LocalLicenseManager.DeleteLocalLicense();
                MessageBox.Show(_owner,
                    "تم إيقاف الترخيص: " + result["message"],
                    "خطأ في التفعيل",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (StartupAction.Exit, null);
            }
            catch (HttpRequestException)
            {
                if (LocalLicenseManager.IsOfflinePlayAllowed(localLicense, out string msg))
                    return (StartupAction.LaunchLogin, localLicense);

                MessageBox.Show(_owner, msg, "مطلوب اتصال بالإنترنت",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (StartupAction.Exit, null);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Update notifier
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Returns false only if the user dodged a mandatory update (caller must Exit).
        /// All other paths return true.
        /// </summary>
        private async Task<bool> HandleUpdateIfAnyAsync(JObject licenseData)
        {
            try
            {
                if (licenseData["update_available"] == null || !(bool)licenseData["update_available"]!)
                    return true;

                string? manifestUrl = licenseData["update"]?["manifest_url"]?.ToString();
                if (string.IsNullOrEmpty(manifestUrl)) return true;

                var updateInfo = await UpdateManager.GetUpdateInfoAsync(manifestUrl);
                if (updateInfo == null) return true;

                using var updateForm = new frmUpdateNotifier(updateInfo);
                updateForm.ShowDialog(_owner);

                // If mandatory and user closed without updating, the form returns control here.
                // (Successful update calls Environment.Exit(0) inside the notifier.)
                if (updateInfo.IsMandatory) return false;
                return true;
            }
            catch
            {
                // Never let update check failure block the cashier
                return true;
            }
        }
    }
}
