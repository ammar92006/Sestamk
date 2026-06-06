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

        /// <summary>معرّف الشركة على السيرفر — يُلتقط من نتيجة check_license.</summary>
        public static string? CompanyId { get; private set; }

        /// <summary>
        /// بصمة الجهاز الموحّدة — نفس البصمة التي تُسجَّل على السيرفر عبر check_license.
        /// (كانت سابقاً تحسب قيمة مختلفة 16-حرف مما سبّب عدم تطابق وتسجيل المواصفات.)
        /// المصدر الموحّد: UserSession.hwid = SHA256(MachineGuid|CpuId|MotherboardSerial)
        /// </summary>
        public static string GetHWID()
        {
            try
            {
                return UserSession.hwid ?? Main_Methods.GenerateHWID(
                    Main_Methods.GetMachineGuid() + "|" +
                    Main_Methods.GetCpuId() + "|" +
                    Main_Methods.GetMotherboardSerial());
            }
            catch
            {
                return "FALLBACK-" + Environment.MachineName + "-" + Environment.UserName;
            }
        }

        // استدعاء آمن لمعلومات الهاردوير — لا يكسر التحقق لو فشل WMI
        private static string? SafeHw(Func<string> getter)
        {
            try { var v = getter(); return string.IsNullOrWhiteSpace(v) ? null : v; }
            catch { return null; }
        }
        private static int? SafeRam()
        {
            try { int v = HardwareInfo.GetRamGB(); return v > 0 ? v : (int?)null; }
            catch { return null; }
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
                    p_version = AppVersion,
                    // إرسال اسم الجهاز ومواصفاته ليتم تخزينها في لوحة التحكم
                    p_device_name = SafeHw(HardwareInfo.GetDeviceName),
                    p_os_info = SafeHw(HardwareInfo.GetOSInfo),
                    p_processor = SafeHw(HardwareInfo.GetProcessorName),
                    p_ram_gb = SafeRam()
                };

                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string rpcUrl = $"{supabaseUrl}/rest/v1/rpc/check_license";

                HttpResponseMessage response = await client.PostAsync(rpcUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseString = await response.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(responseString);
                    // التقاط معرّف الشركة لاستخدامه في مزامنة المستخدمين
                    var cid = obj["company_id"]?.ToString();
                    if (!string.IsNullOrEmpty(cid)) CompanyId = cid;
                    return obj;
                }
                else
                {
                    throw new Exception($"فشل الاتصال بالسيرفر: {response.StatusCode}");
                }
            }
        }
    }
}
