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
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 30;
                        if (parameters != null) cmd.Parameters.AddRange(parameters);

                        await conn.OpenAsync();
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في جلب البيانات: " + ex.Message);
            }
            return dt;
        }

        // 4. تنفيذ INSERT / UPDATE / DELETE بدون إرجاع بيانات
        public static async Task<int> ExecuteAsync(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // 5. إرجاع قيمة واحدة (مثل COUNT, MAX, أو إعداد معين)
        public static async Task<object> ScalarAsync(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    await conn.OpenAsync();
                    return await cmd.ExecuteScalarAsync();
                }
            }
        }

        // 6. تنفيذ INSERT وإرجاع الـ ID الجديد (SCOPE_IDENTITY)
        public static async Task<int> ExecuteWithIdentityAsync(string query, SqlParameter[] parameters = null)
        {
            // يجب أن ينتهي الـ query بـ SELECT SCOPE_IDENTITY()
            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 30;
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    await conn.OpenAsync();
                    object result = await cmd.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        // 7. تنفيذ عدة عمليات في Transaction واحد (للحفاظ على تكامل البيانات)
        public static async Task<bool> ExecuteTransactionAsync(Func<SqlConnection, SqlTransaction, Task> action)
        {
            using (SqlConnection conn = GetConnection())
            {
                await conn.OpenAsync();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        await action(conn, transaction);
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}