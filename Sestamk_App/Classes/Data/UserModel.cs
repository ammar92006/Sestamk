using System;

namespace Sestamk.Classes.Data
{
    /// <summary>
    /// موديل بيانات المستخدم - يطابق جدول Users في قاعدة البيانات
    /// </summary>
    public class UserModel
    {
        public int ID { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int Role_Id { get; set; }
        public string RoleName { get; set; } = "";
        public string User_Code { get; set; } = "";
        public bool User_Stats { get; set; }
        public string Email { get; set; } = "";
        public string full_name { get; set; } = "";
        public string User_Image { get; set; } = "";
        public DateTime? Created_At { get; set; }
        public DateTime? Last_Login { get; set; }
        public string User_Phone { get; set; } = "";
        public string User_Address { get; set; } = "";
        public string Usre_Job_Title { get; set; } = "";
        public string User_department { get; set; } = "";
        public bool Is_active { get; set; } = true;
        public bool Is_blocked { get; set; }
        public string block_reason { get; set; } = "";
        public bool IsDeleted { get; set; }
        public int bank_id { get; set; }

        /// <summary>
        /// نص الحالة للعرض في الجدول
        /// </summary>
        public string StatusDisplay =>
            Is_blocked ? "محظور 🚫" :
            Is_active ? "نشط ✅" : "غير نشط ⛔";
    }
}
