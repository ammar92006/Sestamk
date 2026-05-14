//using Sestamk.Classes;
//using Sestamk.Forms;

//namespace Sestamk
//{
//    internal static class Program
//    {
//        /// <summary>
//        ///  The main entry point for the application.
//        /// </summary>
//        [STAThread]
//        static void Main()
//        {
//            ApplicationConfiguration.Initialize();

//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            // ── Initialise WhatsApp persistent queue ──
//            WhatsAppQueueManager.InitialiseAsync().GetAwaiter().GetResult();

//            // ── Start background worker if WhatsApp is enabled ──
//            if (SettingsService.WhatsAppEnabled)
//                WhatsAppWorkerService.Start();

//            // ── Graceful shutdown on app exit ──
//            Application.ApplicationExit += (s, e) =>
//            {
//                WhatsAppWorkerService.StopAsync().GetAwaiter().GetResult();
//            };



//            // فحص التحديثات بهدوء في الخلفية
//            var updateInfo = UpdateManager.GetUpdateInfoAsync().GetAwaiter().GetResult();

//            if (updateInfo != null)
//            {
//                // لو في تحديث، نظهر الفورم الاحترافية
//                frmUpdateNotifier updateForm = new frmUpdateNotifier(updateInfo);
//                updateForm.ShowDialog();
//                // استخدام ShowDialog بيوقف الكود لحد ما المستخدم ياخد قرار (يحدث أو يتخطى)
//            }

//            // بعد الفورم ما تقفل (أو لو مفيش تحديث أصلاً)، كمل تشغيل البرنامج...

//            bool isActivated = false;

//            // 1. فحص وجود ملف الترخيص في صمت
//            var localLicense = LocalLicenseManager.ReadLocalLicense();

//            if (localLicense != null)
//            {
//                // تم العثور على ترخيص محلي صالح
//                isActivated = true;
//            }
//            else
//            {
//                // 2. العميل جديد أو الترخيص ممسوح -> نظهر شاشة التفعيل
//                using (frmActivation activationForm = new frmActivation())
//                {
//                    // ShowDialog بتوقف الكود هنا لحد ما الشاشة تقفل
//                    DialogResult result = activationForm.ShowDialog();

//                    if (result == DialogResult.OK)
//                    {
//                        // العميل قام بالتفعيل بنجاح
//                        isActivated = true;
//                    }
//                    else
//                    {
//                        // العميل ضغط علامة X وقفل شاشة التفعيل بدون ما يفعل
//                        Application.Exit();
//                        return;
//                    }
//                }
//            }

//            // 3. إذا كان مفعلاً -> نفتح شاشة الدخول (أو الشاشة الرئيسية)
//            if (isActivated)
//            {
//                // استبدل 'frmLogin' باسم شاشة الدخول الخاصة بك
//                Application.Run(new Login());
//            }

//            //Application.Run(new Login());
//        }
//    }
//}




using Sestamk.Classes;
using Sestamk.Forms;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace Sestamk
{
    internal static class Program
    {

        [STAThread]

        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. تهيئة خدمات WhatsApp
            WhatsAppQueueManager.InitialiseAsync().GetAwaiter().GetResult();
            if (SettingsService.WhatsAppEnabled)
            {
                WhatsAppWorkerService.Start();
                // If the bridge was already running from a previous session, ensure the URL is synced
                _ = Task.Run(() => WhatsAppService.EnsureBridgeRunningAsync());
            }

            Application.ApplicationExit += (s, e) =>
            {
                // Stop the background message worker first
                WhatsAppWorkerService.StopAsync().GetAwaiter().GetResult();
                // Kill the Node.js bridge process so it doesn't linger after app closes
                WhatsAppService.ShutdownBridgeAsync().GetAwaiter().GetResult();
            };

            bool isActivated = false;
            JObject licenseData = null;

            // 2. فحص الترخيص (نفس اللوجيك السابق مع الاحتفاظ بالبيانات)
            var localLicense = LocalLicenseManager.ReadLocalLicense();

            if (localLicense != null)
            {
                string savedSerial = localLicense["saved_serial"]?.ToString();
                try
                {
                    //MessageBox.Show(savedSerial);

                    var result = LicenseManager.CheckLicenseAsync(savedSerial).GetAwaiter().GetResult();
                    if (result["status"]?.ToString() == "active")
                    {
                        result["saved_serial"] = savedSerial;
                        LocalLicenseManager.SaveLicenseLocally(result);
                        licenseData = result;
                        isActivated = true;
                    }
                    else
                    {
                        LocalLicenseManager.DeleteLocalLicense();
                        MessageBox.Show("تم إيقاف الترخيص: " + result["message"], "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit(); return;
                    }
                }
                catch (HttpRequestException)
                {
                    string msg;
                    if (LocalLicenseManager.IsOfflinePlayAllowed(localLicense, out msg))
                    {
                        licenseData = localLicense;
                        isActivated = true;
                    }
                    else
                    {
                        MessageBox.Show(msg, "مطلوب إنترنت", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Application.Exit(); return;
                    }
                }
            }
            else
            {
                using (frmActivation activationForm = new frmActivation())
                {
                    if (activationForm.ShowDialog() == DialogResult.OK)
                    {
                        isActivated = true;
                        licenseData = LocalLicenseManager.ReadLocalLicense();
                    }
                    else { Application.Exit(); return; }
                }
            }

            // 3. 🔴 ميزة التحديث الذكي (Mandatory vs Optional)
            if (isActivated && licenseData != null)
            {
                // التحقق من وجود تحديث من خلال بيانات السيرفر
                if (licenseData["update_available"] != null && (bool)licenseData["update_available"])
                {
                    try
                    {
                        string manifestUrl = licenseData["update"]["manifest_url"].ToString();
                        var updateInfo = UpdateManager.GetUpdateInfoAsync(manifestUrl).GetAwaiter().GetResult();

                        if (updateInfo != null)
                        {
                            if (updateInfo.IsMandatory)
                            {
                                // تحديث إجباري: لا يمكن إغلاق النافذة وتجاوزها
                                using (frmUpdateNotifier updateForm = new frmUpdateNotifier(updateInfo))
                                {
                                    updateForm.ShowDialog();
                                    // إذا رجعنا هنا بدون ما البرنامج يقفل (يعني المستخدم قفل الشاشة بدون تحديث)
                                    // نخرج من البرنامج لأن التحديث إجباري
                                    Application.Exit();
                                    return;
                                }
                            }
                            else
                            {
                                // تحديث اختياري: نظهر النافذة وإذا ضغط "لاحقاً" نكمل
                                using (frmUpdateNotifier updateForm = new frmUpdateNotifier(updateInfo))
                                {
                                    // نفترض أن زر "لاحقاً" يغلق الفورم بـ DialogResult.Cancel أو No
                                    var dr = updateForm.ShowDialog();
                                    // إذا ضغط تحديث، البرنامج هيقفل لوحده ويشغل Updater.exe من داخل الفورم
                                    // إذا ضغط لاحقاً، هنكمل الكود هنا ونفتح Login
                                }
                            }
                        }
                    }
                    catch { /* في حال فشل جلب الـ Manifest، نكمل للبرنامج لضمان عدم توقف العميل */ }
                }
            }

            // 4. تشغيل البرنامج
            if (isActivated)
            {
                Application.Run(new Login());
            }
        }
    }
}
