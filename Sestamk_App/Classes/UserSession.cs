using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sestamk.Classes
{
    public static class UserSession
    {
        public static int UserId { get; set; }
        public static int Role_Id { get; set; }
        public static string RoleName { get; set; } = string.Empty;
        public static string UserName { get; set; }
        public static string Full_Name { get; set; }
        public static string Password { get; set; }
        public static string Email { get; set; }
        public static string UserImage { get; set; } = string.Empty;
        public static string raw { get; set; } =  Main_Methods.GetMachineGuid() + "|" +
                                                  Main_Methods.GetCpuId() + "|" +
                                                  Main_Methods.GetMotherboardSerial();
        public static string hwid { get; set; } = Main_Methods.GenerateHWID(raw); // SHA256


        public static DataTable Permissions { get; set; }
        
        /// <summary>
        /// Load permissions from database for the given role
        /// </summary>
        public static async Task LoadPermissionsAsync(int roleid)
        {
            try
            {
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

        /// <summary>
        /// Check if current user has a specific permission
        /// </summary>
        /// <param name="formname">The form/menu name to check</param>
        /// <param name="permissioncolumn">The permission column name in Permissions_TBL</param>
        public static bool HasPermission(string formname, string permissioncolumn)
        {
            try
            {
                if (Permissions == null || Permissions.Rows.Count == 0)
                    return false;

                // Check if the permission column exists and has a true value
                foreach (DataRow row in Permissions.Rows)
                {
                    if (Permissions.Columns.Contains(permissioncolumn))
                    {
                        object val = row[permissioncolumn];
                        if (val != DBNull.Value)
                        {
                            if (val is bool boolVal && boolVal)
                                return true;
                            if (val is int intVal && intVal == 1)
                                return true;
                            if (bool.TryParse(val.ToString(), out bool parsed) && parsed)
                                return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Load the role name from database based on Role_Id
        /// </summary>
        public static async Task LoadRoleNameAsync(int roleid)
        {
            try
            {
                using (SqlConnection conn = DB_Server.GetConnection())
                {
                    // Try common role table names
                    string sql = "SELECT RoleName FROM Roles WHERE RoleID = @RoleID";
                    
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@RoleID", roleid);
                        await conn.OpenAsync();
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null)
                        {
                            RoleName = result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Loading Role Name: " + ex.Message);
                RoleName = "مستخدم"; // Default
            }
        }

        public static void Logout()
        {
            UserId = 0;
            Role_Id = 0;
            RoleName = string.Empty;
            UserName = string.Empty;
            Full_Name = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            UserImage = string.Empty;
            Permissions = new DataTable();
        }

        /// <summary>
        /// تحديث بيانات المستخدم على أي فورم — يستبدل القيم الافتراضية من الـ Designer
        /// </summary>
        /// <param name="lblName">لابل اسم المستخدم (يمكن أن يكون null)</param>
        /// <param name="lblRole">لابل الصلاحية/الوظيفة (يمكن أن يكون null)</param>
        /// <param name="picUser">صورة المستخدم الدائرية (يمكن أن يكون null)</param>
        public static void UpdateUserDisplay(Control lblName = null, Control lblRole = null, PictureBox picUser = null)
        {
            string displayName = string.IsNullOrEmpty(Full_Name) ? UserName : Full_Name;
            string displayRole = string.IsNullOrEmpty(RoleName) ? "مدير النظام" : RoleName;

            if (lblName != null)
                lblName.Text = displayName ?? "مستخدم";
            if (lblRole != null)
                lblRole.Text = displayRole;

            // ── تحميل صورة المستخدم ──
            if (picUser != null)
            {
                LoadUserImage(picUser);
            }
        }

        /// <summary>
        /// تحميل صورة المستخدم من المسار المحفوظ في الجلسة
        /// </summary>
        public static void LoadUserImage(PictureBox picUser)
        {
            if (picUser == null) return;

            try
            {
                if (!string.IsNullOrEmpty(UserImage) && File.Exists(UserImage))
                {
                    // نستخدم MemoryStream لتجنب قفل الملف
                    using (var fs = new FileStream(UserImage, FileMode.Open, FileAccess.Read))
                    {
                        var ms = new MemoryStream();
                        fs.CopyTo(ms);
                        ms.Position = 0;
                        var oldImage = picUser.Image;
                        picUser.Image = Image.FromStream(ms);
                        // تحرير الصورة القديمة فقط إذا لم تكن ريسورس مدمجة
                        if (oldImage != null && oldImage != Properties.Resources.profile)
                            oldImage.Dispose();
                    }
                }
                // إذا لم يوجد صورة، نترك الصورة الافتراضية كما هي
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading user image: " + ex.Message);
                // لا نغير الصورة في حالة الخطأ — تبقى الافتراضية
            }
        }

    }
}
