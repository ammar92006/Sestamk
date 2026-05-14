using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sestamk
{
    public class LocalLicenseManager
    {
        // مسار حفظ ملف الترخيص المحلي (مثلاً في فولدر ProgramData عشان ميتسمحش بسهولة)
        private static string LicenseFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sestamk", "license.dat");

        // 1. دالة حفظ الترخيص محلياً (مشفر)
        public static void SaveLicenseLocally(JObject serverResponse)
        {
            try
            {
                // إضافة تاريخ فحص اليوم للبيانات
                serverResponse["last_successful_sync"] = DateTime.Now.ToString("yyyy-MM-dd");

                string json = serverResponse.ToString();
                byte[] rawData = Encoding.UTF8.GetBytes(json);

                // التشفير باستخدام DPAPI (مربوط بالجهاز الحالي فقط)
                byte[] encryptedData = ProtectedData.Protect(rawData, null, DataProtectionScope.LocalMachine);

                // إنشاء المجلد إذا لم يكن موجود
                Directory.CreateDirectory(Path.GetDirectoryName(LicenseFilePath));

                // حفظ الملف
                File.WriteAllBytes(LicenseFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ في ملف لوج داخلي
            }
        }

        // 2. دالة قراءة الترخيص المحلي عند انقطاع الإنترنت
        public static JObject ReadLocalLicense()
        {
            try
            {
                if (!File.Exists(LicenseFilePath))
                    return null;

                byte[] encryptedData = File.ReadAllBytes(LicenseFilePath);

                // فك التشفير
                byte[] rawData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
                string json = Encoding.UTF8.GetString(rawData);

                return JObject.Parse(json);
            }
            catch
            {
                // إذا تم التلاعب بالملف أو نقله لجهاز آخر سيفشل فك التشفير
                return null;
            }
        }

        // 3. التحقق هل مسموح بالعمل أوفلاين أم لا؟
        public static bool IsOfflinePlayAllowed(JObject localLicense, out string message)
        {
            if (localLicense == null)
            {
                message = "هذه أول مرة لتشغيل البرنامج أو تم تغيير بيانات التفعيل. يرجى الاتصال بالإنترنت للتحقق من الترخيص.";
                return false;
            }

            // التحقق من تاريخ انتهاء الاشتراك الفعلي فقط
            DateTime expiresAt = DateTime.Parse(localLicense["expires_at"].ToString());

            if (DateTime.Now.Date <= expiresAt.Date)
            {
                message = $"أنت تعمل في وضع عدم الاتصال (Offline). الترخيص صالح حتى {expiresAt.ToShortDateString()}.";
                return true;
            }
            else
            {
                message = "لقد انتهى اشتراكك. يرجى الاتصال بالإنترنت لتجديد الترخيص.";
                return false;
            }
        }


        // 4. دالة حذف الترخيص المحلي (تُستخدم عند إيقاف الترخيص من السيرفر)
        public static void DeleteLocalLicense()
        {
            try
            {
                // LicenseFilePath هو المتغير اللي عرفناه فوق في الكلاس
                if (File.Exists(LicenseFilePath))
                {
                    File.Delete(LicenseFilePath);
                }
            }
            catch (Exception)
            {
                // تم استخدام try-catch لتجنب أي Crash في البرنامج 
                // إذا كان الملف محمي أو مفتوح بواسطة عملية أخرى بالخطأ
            }
        }
    }
}