using Microsoft.Data.SqlClient;
using System.Data;

namespace Sestamk.Classes
{
    public static class DB_Server
    {
        private static string _dataSource = SecureConfig.DbDataSource;
        private static string _userID = SecureConfig.DbUserId;
        private static string _password = SecureConfig.DbPassword;

        private static string ConnectionString => new SqlConnectionStringBuilder
        {
            DataSource = _dataSource,
            InitialCatalog = "DB_Sestamk",
            IntegratedSecurity = true,
            MultipleActiveResultSets = true,
            Encrypt = false,
            TrustServerCertificate = true,
            ConnectTimeout = 15
        }.ConnectionString;

        public static void Initialize(string dataSource, string userId, string password)
        {
            if (!string.IsNullOrWhiteSpace(dataSource)) _dataSource = dataSource;
            if (!string.IsNullOrWhiteSpace(userId)) _userID = userId;
            if (!string.IsNullOrWhiteSpace(password)) _password = password;
        }

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

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
                string errorMsg = ex is SqlException ? "خطأ في الاتصال بالسيرفر: " : "خطأ في جلب البيانات: ";
                ToastManager.ShowError("خطأ", errorMsg + ex.Message);
            }
            return dt;
        }

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

        public static async Task<int> ExecuteWithIdentityAsync(string query, SqlParameter[] parameters = null)
        {
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
