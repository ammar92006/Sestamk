using Microsoft.Data.SqlClient;
using System.Data;

namespace Sestamk.Classes
{
    public static class DB_Server
    {
        // 1. خلي سلسلة الاتصال ثابتة وجاهزة
        private static string ConnectionString => new SqlConnectionStringBuilder
        {
            DataSource = @".\SQLEXPRESS",
            InitialCatalog = "DB_Sestamk",
            UserID = "Sestamk_App",
            Password = "StrongPassword123!",
            MultipleActiveResultSets = true,
            Encrypt = false,
            TrustServerCertificate = true
        }.ConnectionString;

        // 2. بدل ما نثبت "Conn"، بنعمل ميثود ترجع اتصال جديد (ADO.NET بيعمل Pooling تلقائي فده أسرع بكتير)
        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        // 3. ميثود سحرية لسحب البيانات "Async" عشان البرنامج ميهنجش نهائي
        public static async Task<DataTable> GetTableAsync(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = GetConnection()) // استخدام Using بيقفل الاتصال فوراً بعد ما يخلص
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);

                        await conn.OpenAsync(); // فتح الاتصال بدون تجميد الشاشة
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في جلب البيانات: " + ex.Message);
            }
            return dt;
        }
    }
}