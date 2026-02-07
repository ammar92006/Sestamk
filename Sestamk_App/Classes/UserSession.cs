using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Sestamk.Classes
{
    public static class UserSession
    {
        public static int UserId { get; set; }
        public static int Role_Id { get; set; }
        public static string UserName { get; set; }
        public static string Full_Name { get; set; }
        public static string Password { get; set; }
        public static string Email { get; set; }
        public static string raw { get; set; } =  Main_Methods.GetMachineGuid() + "|" +
                                                  Main_Methods.GetCpuId() + "|" +
                                                  Main_Methods.GetMotherboardSerial();
        public static string hwid { get; set; } = Main_Methods.GenerateHWID(raw); // SHA256


        public static DataTable Permissions { get; set; }
        // غيرنا النوع لـ Task عشان نقدر نستخدم await
        public static async Task LoadPermissionsAsync(int roleid)
        {
            try
            {
                // استخدام using هنا بيضمن قفل الاتصال فوراً حتى لو حصل خطأ
                using (SqlConnection conn = DB_Server.GetConnection())
                {
                    string sql = "SELECT * FROM Permissions_TBL WHERE RoleID = @RoleID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleID", roleid);
                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            Permissions = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Loading Permissions: " + ex.Message);
                Permissions = new DataTable();
            }
        }
        public static bool HasPermission(string formname, string permissioncolumn)
        {

            try
            {
                if (Permissions?.Rows.Count == 0)
                 return false;
                //DataRow rows = Permissions.Select("FormName", "sf";
            }
            catch (Exception)
            {

                throw;
            }
            
            return false;

        }

        public static void Logout()
        {
            UserId = 0;
            UserName = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            Permissions = new DataTable();
        }


    }
}
