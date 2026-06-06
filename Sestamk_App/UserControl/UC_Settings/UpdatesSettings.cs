//using Guna.UI2.WinForms;
//using Sestamk.Classes;
//using Sestamk.Forms;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Text;
//using System.Windows.Forms;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Newtonsoft.Json.Linq;


//namespace Sestamk.UserControl.UC_Settings
//{
//    public partial class UpdatesSettings : System.Windows.Forms.UserControl, ICloseRequest
//    {
//        public event EventHandler CloseRequested;

//        public UpdatesSettings()
//        {
//            InitializeComponent();
//        }

//        private void btnClose_Click(object sender, EventArgs e)
//        {
//            CloseRequested?.Invoke(this, EventArgs.Empty);
//        }

//        private void toggleAutoUpdate_CheckedChanged(object sender, EventArgs e)
//        {
//            //SettingsService.AutoUpdateEnabled = toggleAutoUpdate.Checked;
//            SaveSettingsAsync();
//        }

//        private async System.Threading.Tasks.Task SaveSettingsAsync()
//        {
//            try
//            {

//                await SettingsService.SetAsync("AutoUpdate_Enabled", toggleAutoUpdate.Checked.ToString());

//                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات التحديث التلقائي بنجاح ✅");
//            }
//            catch (Exception ex)
//            {
//                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
//            }
//        }
//        private async void UpdatesSettings_Load(object sender, EventArgs e)
//        {
//            lblCurrentVersion.Text = LicenseManager.AppVersion;

//            var localLicense = LocalLicenseManager.ReadLocalLicense();
//            if (localLicense != null)
//            {
//                // جلب القناة الخاصة بالشركة من الملف المحلي
//                string userChannel = localLicense["channel"]?.ToString() ?? "public";
//                lblChannel.Text = userChannel.ToUpper();

//                // التحقق مما إذا كان هناك تحديث متوفر من بيانات الترخيص
//                if ((bool)localLicense["update_available"])
//                {
//                    string manifestUrl = localLicense["update"]["manifest_url"].ToString();
//                    var updateInfo = await UpdateManager.GetUpdateInfoAsync(manifestUrl);

//                    if (updateInfo != null)
//                    {
//                        lblLatestVersion.Text = updateInfo.Version;
//                        lblLatestUpdateDate.Text = updateInfo.ReleaseDate;
//                    }
//                }
//                else
//                {
//                    lblLatestVersion.Text = LicenseManager.AppVersion;
//                    lblLatestUpdateDate.Text = "محدث";
//                }

//                // جلب ورسم سجل التحديثات للقناة الحالية
//                await LoadUpdateHistoryCards(userChannel);
//            }

//            // ربط حالة التحديث التلقائي
//            toggleAutoUpdate.Checked = SettingsService.AutoUpdateEnabled;
//        }

//        private async Task LoadUpdateHistoryCards(string channel)
//        {
//            try
//            {
//                using (HttpClient client = new HttpClient())
//                {
//                    client.DefaultRequestHeaders.Add("apikey", SecureConfig.SupabaseKey);
//                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + SecureConfig.SupabaseKey);

//                    var requestBody = new { p_channel = channel };
//                    string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
//                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

//                    // استدعاء دالة جلب آخر 5 تحديثات من الداتابيز
//                    string rpcUrl = $"{SecureConfig.SupabaseUrl}/rest/v1/rpc/get_update_history";
//                    HttpResponseMessage response = await client.PostAsync(rpcUrl, content);

//                    if (response.IsSuccessStatusCode)
//                    {
//                        string responseString = await response.Content.ReadAsStringAsync();
//                        JArray updatesArray = JArray.Parse(responseString);

//                        flpUpdateHistory.Controls.Clear(); // تنظيف اللوحة

//                        foreach (var update in updatesArray)
//                        {
//                            DrawUpdateCard(update); // رسم كارت لكل تحديث
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show("فشل جلب سجل التحديثات: " + ex.Message);
//            }
//        }

//        // رسم الواجهة ديناميكياً بالكود
//        private void DrawUpdateCard(JToken update)
//        {
//            // 1. إنشاء الكارت الأساسي (Panel)
//            Panel card = new Panel();
//            card.Width = flpUpdateHistory.Width - 25; // عرض الكارت يملأ الحاوية مع مسافة للسكرول
//            card.BackColor = Color.FromArgb(45, 45, 48);
//            card.Margin = new Padding(5, 5, 5, 10);
//            card.Padding = new Padding(10);

//            // 2. عنوان التحديث (Version & Title)
//            Label lblTitle = new Label();
//            lblTitle.Text = $"الإصدار {update["version"]} - {update["title"]}";
//            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
//            lblTitle.ForeColor = Color.White;
//            lblTitle.AutoSize = true;
//            lblTitle.Location = new Point(10, 10);
//            card.Controls.Add(lblTitle);

//            // 3. تاريخ التحديث
//            Label lblDate = new Label();
//            DateTime releaseDate = DateTime.Parse(update["release_date"].ToString());
//            lblDate.Text = releaseDate.ToString("yyyy-MM-dd");
//            lblDate.Font = new Font("Segoe UI", 9, FontStyle.Regular);
//            lblDate.ForeColor = Color.LightGray;
//            lblDate.AutoSize = true;
//            lblDate.Location = new Point(10, 35);
//            card.Controls.Add(lblDate);

//            // 4. مميزات التحديث (Whats New)
//            Label lblFeatures = new Label();
//            StringBuilder sb = new StringBuilder();
//            foreach (var feature in update["whats_new"])
//            {
//                sb.AppendLine($"• {feature}");
//            }
//            lblFeatures.Text = sb.ToString();
//            lblFeatures.Font = new Font("Segoe UI", 10, FontStyle.Regular);
//            lblFeatures.ForeColor = Color.WhiteSmoke;
//            lblFeatures.AutoSize = true;
//            lblFeatures.Location = new Point(10, 60);
//            card.Controls.Add(lblFeatures);

//            // ضبط طول الكارت ديناميكياً بناءً على حجم النص
//            card.Height = lblFeatures.Bottom + 15;

//            // 5. إضافة الكارت للوحة الرئيسية
//            flpUpdateHistory.Controls.Add(card);
//        }

//        private async void btnCheckForUpdates_Click(object sender, EventArgs e)
//        {
//            btnCheckForUpdates.Enabled = false;
//            btnCheckForUpdates.Text = "جاري الفحص...";

//            try
//            {
//                var localLicense = LocalLicenseManager.ReadLocalLicense();
//                if (localLicense != null && localLicense["update_available"] != null && (bool)localLicense["update_available"])
//                {
//                    string manifestUrl = localLicense["update"]["manifest_url"].ToString();
//                    var updateInfo = await UpdateManager.GetUpdateInfoAsync(manifestUrl);

//                    if (updateInfo != null)
//                    {
//                        // يوجد تحديث، نظهر شاشة التحديث
//                        using (frmUpdateNotifier updateForm = new frmUpdateNotifier(updateInfo))
//                        {
//                            updateForm.ShowDialog();
//                        }
//                    }
//                    else
//                    {
//                        ToastManager.ShowInfo("لا يوجد تحديث", "أنت تستخدم أحدث إصدار حالياً.");
//                    }
//                }
//                else
//                {
//                    ToastManager.ShowInfo("لا يوجد تحديث", "أنت تستخدم أحدث إصدار حالياً.");
//                }
//            }
//            catch (Exception ex)
//            {
//                ToastManager.ShowError("خطأ", "فشل الاتصال بالسيرفر أثناء فحص التحديثات.");
//            }
//            finally
//            {
//                btnCheckForUpdates.Enabled = true;
//                btnCheckForUpdates.Text = "البحث عن تحديثات";
//            }
//        }
//    }
//}



using Guna.UI2.WinForms;
using Sestamk.Classes;
using Sestamk.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Sestamk.UserControl.UC_Settings
{
    public partial class UpdatesSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        public UpdatesSettings()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void toggleAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            SaveSettingsAsync();
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {
                await SettingsService.SetAsync("AutoUpdate_Enabled", toggleAutoUpdate.Checked.ToString());
                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات التحديث التلقائي بنجاح");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }

        private async void UpdatesSettings_Load(object sender, EventArgs e)
        {
            lblCurrentVersion.Text = LicenseManager.AppVersion;

            var localLicense = LocalLicenseManager.ReadLocalLicense();
            if (localLicense != null)
            {
                string userChannel = localLicense["channel"]?.ToString() ?? "public";
                lblChannel.Text = userChannel.ToUpper();
                await LoadUpdateHistoryCards(userChannel);
            }

            toggleAutoUpdate.Checked = SettingsService.AutoUpdateEnabled;
        }

        private async Task LoadUpdateHistoryCards(string channel)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apikey", SecureConfig.SupabaseKey);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + SecureConfig.SupabaseKey);

                    // ترتيب بـ release_date تنازلياً — يتجنب مشاكل مقارنة النصوص في أرقام الإصدار
                    string url = $"{SecureConfig.SupabaseUrl}/rest/v1/updates?channel=eq.{channel}&is_active=eq.true&select=*&order=release_date.desc";
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        lblLatestVersion.Text = "-";
                        lblLatestUpdateDate.Text = "-";
                        return;
                    }

                    string responseString = await response.Content.ReadAsStringAsync();
                    JArray updatesArray = JArray.Parse(responseString);

                    flpUpdateHistory.Controls.Clear();

                    if (updatesArray.Count > 0)
                    {
                        var latest = updatesArray[0];
                        string latestVersionStr = latest["version"]?.ToString() ?? "-";
                        lblLatestVersion.Text = latestVersionStr;

                        // تاريخ الإصدار من release_date أولاً، ثم created_at كـ fallback
                        string dateStr = latest["release_date"]?.ToString() ?? latest["created_at"]?.ToString();
                        if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime releaseDate))
                            lblLatestUpdateDate.Text = releaseDate.ToString("yyyy-MM-dd");
                        else
                            lblLatestUpdateDate.Text = "-";

                        // مقارنة صحيحة للإصدارات بـ Version class تمنع مشاكل مقارنة النص
                        bool isUpToDate = Version.TryParse(LicenseManager.AppVersion, out var current)
                                       && Version.TryParse(latestVersionStr, out var latestV)
                                       && current >= latestV;

                        if (isUpToDate)
                        {
                            btnCheckForUpdates.Text = "النظام محدّث";
                            btnCheckForUpdates.FillColor = Color.SeaGreen;
                            btnCheckForUpdates.Enabled = false;
                        }
                        else
                        {
                            btnCheckForUpdates.Text = "تحديث الآن";
                            btnCheckForUpdates.FillColor = Color.FromArgb(0, 122, 204);
                            btnCheckForUpdates.Enabled = true;
                        }

                        foreach (var update in updatesArray)
                            DrawUpdateCard(update);
                    }
                    else
                    {
                        lblLatestVersion.Text = LicenseManager.AppVersion;
                        lblLatestUpdateDate.Text = "لا توجد سجلات";
                    }
                }
            }
            catch (Exception ex)
            {
                lblLatestVersion.Text = "-";
                lblLatestUpdateDate.Text = "-";
                ToastManager.ShowError("خطأ", "فشل جلب سجل التحديثات: " + ex.Message);
            }
        }

        private void DrawUpdateCard(JToken update)
        {
            Panel card = new Panel();
            card.Width = flpUpdateHistory.Width - 25;
            card.BackColor = Color.FromArgb(45, 45, 48);
            card.Margin = new Padding(5, 5, 5, 10);
            card.Padding = new Padding(10);

            Label lblTitle = new Label();
            lblTitle.Text = $"الإصدار {update["version"]} - {update["title"]}";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(10, 10);
            card.Controls.Add(lblTitle);

            Label lblDate = new Label();
            string dateStr = update["release_date"]?.ToString() ?? update["created_at"]?.ToString();
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out DateTime parsedDate))
                lblDate.Text = parsedDate.ToString("yyyy-MM-dd");
            lblDate.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblDate.ForeColor = Color.LightGray;
            lblDate.AutoSize = true;
            lblDate.Location = new Point(10, 35);
            card.Controls.Add(lblDate);

            Label lblFeatures = new Label();
            StringBuilder sb = new StringBuilder();
            if (update["whats_new"] != null && update["whats_new"].Type == JTokenType.Array)
            {
                foreach (var feature in update["whats_new"])
                    sb.AppendLine($"• {feature}");
            }
            else
            {
                sb.AppendLine("• تحسينات عامة");
            }
            lblFeatures.Text = sb.ToString();
            lblFeatures.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblFeatures.ForeColor = Color.WhiteSmoke;
            lblFeatures.AutoSize = true;
            lblFeatures.Location = new Point(10, 60);
            card.Controls.Add(lblFeatures);

            card.Height = lblFeatures.Bottom + 15;
            flpUpdateHistory.Controls.Add(card);
        }

        private async void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            btnCheckForUpdates.Enabled = false;
            btnCheckForUpdates.Text = "جاري الفحص...";

            try
            {
                var localLicense = LocalLicenseManager.ReadLocalLicense();
                if (localLicense != null && localLicense["update_available"] != null && (bool)localLicense["update_available"])
                {
                    string manifestUrl = localLicense["update"]["manifest_url"].ToString();
                    var updateInfo = await UpdateManager.GetUpdateInfoAsync(manifestUrl);

                    if (updateInfo != null)
                    {
                        using (frmUpdateNotifier updateForm = new frmUpdateNotifier(updateInfo))
                        {
                            updateForm.ShowDialog();
                        }
                    }
                    else
                    {
                        ToastManager.ShowInfo("لا يوجد تحديث", "أنت تستخدم أحدث إصدار حالياً.");
                    }
                }
                else
                {
                    ToastManager.ShowInfo("لا يوجد تحديث", "أنت تستخدم أحدث إصدار حالياً.");
                }
            }
            catch (Exception)
            {
                ToastManager.ShowError("خطأ", "فشل الاتصال بالسيرفر أثناء فحص التحديثات.");
            }
            finally
            {
                // أعد تفعيل الزر فقط لو النظام مش محدّث
                if (!Version.TryParse(LicenseManager.AppVersion, out var cur)
                    || !Version.TryParse(lblLatestVersion.Text, out var lat)
                    || cur < lat)
                {
                    btnCheckForUpdates.Enabled = true;
                    btnCheckForUpdates.Text = "تحديث الآن";
                }
            }
        }
    }
}
