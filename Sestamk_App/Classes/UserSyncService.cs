using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sestamk.Classes;

namespace Sestamk
{
    /// <summary>
    /// يزامن مستخدمي الشركة المحليين (SQL Server) إلى جدول users على السيرفر
    /// ليظهروا في لوحة تحكم المالك. يُستدعى في الخلفية بعد التحقق من الترخيص.
    /// </summary>
    public static class UserSyncService
    {
        public static async Task SyncAsync()
        {
            try
            {
                string? companyId = LicenseManager.CompanyId;
                if (string.IsNullOrEmpty(companyId)) return;

                string supabaseUrl = SecureConfig.SupabaseUrl;
                string supabaseKey = SecureConfig.SupabaseKey;
                if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey)) return;

                // قراءة المستخدمين المحليين
                string query = @"SELECT u.Username, u.full_name, u.Email, u.User_Phone,
                                        ISNULL(r.RoleName, N'') AS RoleName
                                 FROM Users u
                                 LEFT JOIN Roles r ON u.Role_Id = r.RoleID
                                 WHERE ISNULL(u.IsDeleted, 0) = 0";

                DataTable dt = await DB_Server.GetTableAsync(query);
                if (dt == null || dt.Rows.Count == 0) return;

                var users = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    string username = row["Username"]?.ToString() ?? "";
                    if (string.IsNullOrWhiteSpace(username)) continue;

                    users.Add(new
                    {
                        username,
                        full_name = row["full_name"]?.ToString() ?? username,
                        email = row["Email"]?.ToString() ?? "",
                        phone = row["User_Phone"]?.ToString() ?? "",
                        role = MapRole(row["RoleName"]?.ToString()),
                        is_active = true
                    });
                }

                if (users.Count == 0) return;

                var body = new { p_company_id = companyId, p_users = users };
                string json = JsonConvert.SerializeObject(body);

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + supabaseKey);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync($"{supabaseUrl}/rest/v1/rpc/sync_company_users", content);
            }
            catch
            {
                // المزامنة غير حرجة — لا تعطّل تشغيل البرنامج أبداً
            }
        }

        /// <summary>تحويل اسم الدور المحلي إلى دور متوافق مع السيرفر.</summary>
        private static string MapRole(string? roleName)
        {
            string r = (roleName ?? "").ToLowerInvariant();
            if (r.Contains("مدير") || r.Contains("admin") || r.Contains("owner"))
                return "admin";
            if (r.Contains("مشرف") || r.Contains("manager") || r.Contains("supervisor"))
                return "manager";
            if (r.Contains("مشاهد") || r.Contains("viewer") || r.Contains("read"))
                return "viewer";
            return "user";
        }
    }
}
