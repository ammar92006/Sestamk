using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sestamk
{
    // نفس كلاسات الـ Manifest اللي في أداة الرفع عشان نقدر نقرأ الملف
    public class UpdateManifest
    {
        public string Version { get; set; }
        public string ReleaseDate { get; set; }
        public string Channel { get; set; } 
        public List<string> WhatsNew { get; set; } = new List<string>();
        public bool IsMandatory { get; set; }
        public List<FileChange> Changes { get; set; } = new List<FileChange>();
    }

    public class FileChange
    {
        public string OriginalName { get; set; }
        public string TargetPath { get; set; }
        public string Hash { get; set; }
        public long SizeBytes { get; set; }
        public string Url { get; set; }
        public string Action { get; set; }
    }

    public class UpdateManager
    {
        // 🔴 ضع رابط تحميل الـ manifest من إصداراتك على GitHub (تأكد من تعديل اسم حسابك والمستودع)
        private const string ManifestUrl = "https://github.com/ammar92006/Sestamk/releases/latest/download/manifest.json";

        public static async Task<bool> CheckAndDownloadUpdatesAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // GitHub يتطلب User-Agent
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Sestamk-Client");

                    // 1. تحميل الـ Manifest من السيرفر
                    string json = await client.GetStringAsync(ManifestUrl);
                    UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(json);

                    if (manifest.Version == LicenseManager.AppVersion)
                    {
                        return false; // لا يوجد تحديث، النسخة متطابقة
                    }

                    return await DownloadUpdateFilesAsync(manifest);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("فشل في تحميل التحديثات: " + ex.Message);
            }
        }

        //public static async Task<bool> DownloadUpdateFilesAsync(UpdateManifest manifest)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.UserAgent.ParseAdd("Sestamk-Client");

        //            // 2. مقارنة الملفات وتحديد ما يجب تحميله
        //            List<FileChange> filesToDownload = new List<FileChange>();
        //            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        //            foreach (var file in manifest.Changes)
        //            {
        //                string localFilePath = Path.Combine(baseDir, file.TargetPath);

        //                // إذا كان الملف غير موجود أو الهاش الخاص به مختلف، نضيفه لقائمة التحميل
        //                if (!File.Exists(localFilePath) || CalculateSHA256(localFilePath) != file.Hash)
        //                {
        //                    filesToDownload.Add(file);
        //                }
        //            }

        //            if (filesToDownload.Count == 0)
        //            {
        //                return true; // الملفات متطابقة تماماً أو تم تحميلها مسبقاً
        //            }

        //            // 3. إنشاء مجلد TempUpdate للتحميل فيه
        //            string tempDir = Path.Combine(baseDir, "TempUpdate");
        //            if (!Directory.Exists(tempDir))
        //            {
        //                Directory.CreateDirectory(tempDir);
        //            }

        //            // 4. تحميل الملفات المتغيرة فقط
        //            foreach (var file in filesToDownload)
        //            {
        //                string tempFilePath = Path.Combine(tempDir, file.TargetPath);

        //                // إنشاء المجلدات الفرعية إن وجدت (مثلاً مجلد Data أو Images)
        //                Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

        //                byte[] fileData = await client.GetByteArrayAsync(file.Url);
        //                File.WriteAllBytes(tempFilePath, fileData);
        //            }

        //            return true; // تم تحميل التحديثات بنجاح في مجلد TempUpdate
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}

        // دالة حساب الهاش للمقارنة المحلية
        private static string CalculateSHA256(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        // الدالة الأولى: بتجيب البيانات بس (عشان نعرضها للمستخدم)
        //public static async Task<UpdateManifest> GetUpdateInfoAsync()
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.UserAgent.ParseAdd("Sestamk-Client");
        //            string json = await client.GetStringAsync(ManifestUrl);
        //            UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(json);

        //            //MessageBox.Show(manifest.Version);
        //            //MessageBox.Show(LicenseManager.AppVersion);
        //            // لو الإصدار مختلف عن الحالي، نرجع البيانات
        //            if (manifest.Version != LicenseManager.AppVersion)
        //            {
        //                return manifest;
        //            }
        //            return null; // لا يوجد تحديث
        //        }
        //    }
        //    catch
        //    {
        //        return null; // خطأ في الاتصال
        //    }
        //}

        // 🔴 الدالة الآن تستقبل الرابط الديناميكي
        public static async Task<UpdateManifest> GetUpdateInfoAsync(string dynamicManifestUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(dynamicManifestUrl)) return null;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Sestamk-Client");
                    string json = await client.GetStringAsync(dynamicManifestUrl);
                    UpdateManifest manifest = JsonConvert.DeserializeObject<UpdateManifest>(json);

                    if (manifest.Version != LicenseManager.AppVersion)
                    {
                        return manifest;
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }


        // الدالة الثانية: بتقوم بالتحميل الفعلي لما المستخدم يضغط "تحديث الآن"
        //public static async Task<bool> DownloadUpdateFilesAsync(UpdateManifest manifest)
        //{
        //    try
        //    {
        //        using (HttpClient client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.UserAgent.ParseAdd("Sestamk-Client");

        //            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        //            string tempDir = Path.Combine(baseDir, "TempUpdate");

        //            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

        //            foreach (var file in manifest.Changes)
        //            {
        //                string localFilePath = Path.Combine(baseDir, file.TargetPath);

        //                // لو الملف اتغير أو مش موجود، نحمله
        //                if (!File.Exists(localFilePath) || CalculateSHA256(localFilePath) != file.Hash)
        //                {
        //                    string tempFilePath = Path.Combine(tempDir, file.TargetPath);
        //                    Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

        //                    byte[] fileData = await client.GetByteArrayAsync(file.Url);
        //                    File.WriteAllBytes(tempFilePath, fileData);
        //                }
        //            }
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        // تمت إضافة IProgress<int> progress = null
        public static async Task<bool> DownloadUpdateFilesAsync(UpdateManifest manifest, IProgress<int> progress = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Sestamk-Client");

                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string tempDir = Path.Combine(baseDir, "TempUpdate");

                    if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

                    int totalFiles = manifest.Changes.Count;
                    int processedFiles = 0;

                    foreach (var file in manifest.Changes)
                    {
                        string localFilePath = Path.Combine(baseDir, file.TargetPath);

                        // لو الملف اتغير أو مش موجود، نحمله
                        if (!File.Exists(localFilePath) || CalculateSHA256(localFilePath) != file.Hash)
                        {
                            string tempFilePath = Path.Combine(tempDir, file.TargetPath);
                            Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath));

                            byte[] fileData = await client.GetByteArrayAsync(file.Url);
                            File.WriteAllBytes(tempFilePath, fileData);
                        }

                        // 🔴 حساب النسبة المئوية وإرسالها للفورم
                        processedFiles++;
                        if (progress != null && totalFiles > 0)
                        {
                            int percentage = (int)Math.Round((double)(100 * processedFiles) / totalFiles);
                            progress.Report(percentage);
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}