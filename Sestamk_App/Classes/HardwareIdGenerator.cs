using System;
using System.Collections.Generic;
using System.Management;
using System.Net.NetworkInformation;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sestamk.Classes
{

    public static class HardwareIdGenerator
    {
        static readonly byte[] AES_KEY = Encoding.UTF8.GetBytes("A1B2C3D4E5F6G7H8A1B2C3D4E5F6G7H8");
        static readonly byte[] AES_IV = Encoding.UTF8.GetBytes("1A2B3C4D5E6F7G8H");
        static readonly byte[] HMAC_KEY = Encoding.UTF8.GetBytes("SUPER_SECRET_HMAC_KEY");

        static string LicensePath = @"C:\ProgramData\Sestamk\license.dat";
        public static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = AES_KEY;
            aes.IV = AES_IV;

            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }
        public static string Decrypt(byte[] cipher)
        {
            using var aes = Aes.Create();
            aes.Key = AES_KEY;
            aes.IV = AES_IV;

            using var decryptor = aes.CreateDecryptor();
            var bytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            return Encoding.UTF8.GetString(bytes);
        }
        public static string ComputeHmac(string data)
        {
            using var hmac = new HMACSHA256(HMAC_KEY);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash);
        }

        public static void SaveLicense(LicenseModel license)
        {
            license.Signature = ComputeHmac(
                license.CompanyId +
                license.SubscriptionEnd.ToString("O") +
                license.DeviceFingerprint
            );

            string json = JsonSerializer.Serialize(license);
            byte[] encrypted = Encrypt(json);

            File.WriteAllBytes(LicensePath, encrypted);
        }
        public static LicenseModel LoadLicense()
        {
            if (!File.Exists(LicensePath))
                return null;

            byte[] encrypted = File.ReadAllBytes(LicensePath);
            string json = Decrypt(encrypted);

            var license = JsonSerializer.Deserialize<LicenseModel>(json);

            string expectedSig = ComputeHmac(
                license.CompanyId +
                license.SubscriptionEnd.ToString("O") +
                license.DeviceFingerprint
            );

            if (license.Signature != expectedSig)
                throw new SecurityException("License Tampered");

            if (license.DeviceFingerprint != HardwareIdGenerator.GetHWID())
                throw new SecurityException("Invalid Device");

            if (DateTime.UtcNow > license.SubscriptionEnd)
                throw new SecurityException("License Expired");

            return license;
        }
        public static void ValidateOfflineUsage(LicenseModel license)
        {
            // انتهى الاشتراك
            if (DateTime.UtcNow > license.SubscriptionEnd)
                throw new SecurityException("انتهت مدة الاشتراك");

            // أقصى مدة أوفلاين
            var offlineDays = (DateTime.UtcNow - license.LastOnlineCheck).TotalDays;

            if (offlineDays > license.MaxOfflineDays)
                throw new SecurityException("يجب الاتصال بالإنترنت للتحقق من التفعيل");

            // حماية إضافية ضد التلاعب بالساعة
            if (license.LastOnlineCheck > DateTime.UtcNow.AddMinutes(5))
                throw new SecurityException("تم اكتشاف تلاعب في وقت النظام");
        }


        public static string GetHWID()
        {
            string cpu = GetWmi("Win32_Processor", "ProcessorId");
            string board = GetWmi("Win32_BaseBoard", "SerialNumber");
            string mac = GetMacAddress();

            string raw = cpu + board + mac;
            return ComputeSha256(raw);
        }

        private static string GetWmi(string className, string property)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {className}");
                foreach (ManagementObject obj in searcher.Get())
                    return obj[property]?.ToString();
            }
            catch { }
            return "UNKNOWN";
        }

        private static string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Select(n => n.GetPhysicalAddress().ToString())
                .FirstOrDefault() ?? "NOMAC";
        }

        private static string ComputeSha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
