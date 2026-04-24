using System.Security.Cryptography;
using System.Text;

namespace Sestamk.Classes
{
    /// <summary>
    /// مساعد تشفير كلمات المرور باستخدام SHA256
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// تشفير كلمة المرور بـ SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder(64);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>
        /// مقارنة كلمة المرور المدخلة بالهاش المخزن
        /// </summary>
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string hash = HashPassword(enteredPassword);
            return hash.Equals(storedHash, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
