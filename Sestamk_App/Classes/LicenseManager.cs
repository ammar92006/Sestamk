using Newtonsoft.Json.Linq;
using Sestamk.Classes;
using System;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sestamk
{
    public class LicenseManager
    {
        public static string Version =>
            string.Join(".", Assembly.GetExecutingAssembly()
            .GetName().Version.ToString().Split('.').Take(3));

        public static string AppVersion = Version;

        public static string GetHWID()
        {
            try
            {
                string cpuInfo = string.Empty;
                ManagementClass mc = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if (cpuInfo == "")
                    {
                        cpuInfo = mo.Properties["processorID"].Value.ToString();
                        break;
                    }
                }

                string driveInfo = string.Empty;
                ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
                dsk.Get();
                driveInfo = dsk["VolumeSerialNumber"].ToString();

                string rawHwid = cpuInfo + driveInfo;

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawHwid));
                    string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpper();
                    return hashString.Substring(0, 16);
                }
            }
            catch
            {
                return "FALLBACK-" + Environment.MachineName + "-" + Environment.UserName;
            }
        }

        public static async Task<JObject> CheckLicenseAsync(string serialKey)
        {
            string supabaseUrl = SecureConfig.SupabaseUrl;
            string supabaseKey = SecureConfig.SupabaseKey;

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                throw new InvalidOperationException("لم يتم تكوين بيانات Supabase. قم بإعدادها من الإعدادات.");

            string raw = Main_Methods.GetMachineGuid() + "|" +
                         Main_Methods.GetCpuId() + "|" +
                         Main_Methods.GetMotherboardSerial();

            string hwid = UserSession.hwid ?? Main_Methods.GenerateHWID(raw);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + supabaseKey);

                var requestBody = new
                {
                    p_serial = serialKey,
                    p_hwid = hwid,
                    p_version = AppVersion
                };

                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string rpcUrl = $"{supabaseUrl}/rest/v1/rpc/check_license";

                HttpResponseMessage response = await client.PostAsync(rpcUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(responseString);
                }
                else
                {
                    throw new Exception($"فشل الاتصال بالسيرفر: {response.StatusCode}");
                }
            }
        }
    }
}
