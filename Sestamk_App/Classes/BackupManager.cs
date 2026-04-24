using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    public enum BackupType
    {
        Manual,
        Auto
    }

    public class BackupLogModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public double FileSizeMB { get; set; }
        public string BackupType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
    }

    public class BackupSettingsModel
    {
        public bool AutoBackupEnabled { get; set; }
        public int BackupIntervalHours { get; set; }
        public TimeSpan BackupTime { get; set; }
        public string BackupPath { get; set; }
    }

    public static class BackupManager
    {
        private const string DatabaseName = "DB_Sestamk";

        public static async Task InitializeDatabaseAsync()
        {
            string query = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BackupLogs')
                BEGIN
                    CREATE TABLE BackupLogs (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        FileName NVARCHAR(255),
                        FilePath NVARCHAR(500),
                        FileSizeMB FLOAT,
                        BackupType NVARCHAR(50),
                        CreatedBy NVARCHAR(100),
                        CreatedDate DATETIME,
                        Status NVARCHAR(50)
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BackupSettings')
                BEGIN
                    CREATE TABLE BackupSettings (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        AutoBackupEnabled BIT,
                        BackupIntervalHours INT,
                        BackupTime TIME,
                        BackupPath NVARCHAR(500)
                    );
                    INSERT INTO BackupSettings (AutoBackupEnabled, BackupIntervalHours, BackupTime, BackupPath)
                    VALUES (0, 24, '02:00:00', 'C:\SestamkBackups');
                END";
            await DB_Server.ExecuteAsync(query);
        }

        public static async Task<bool> CreateBackupAsync(string targetFolder, string customFileName = "", BackupType type = BackupType.Manual)
        {
            try
            {
                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                string fileName = string.IsNullOrEmpty(customFileName) 
                    ? $"restaurant_backup_{DateTime.Now:yyyy_MM_dd_HH_mm}.bak" 
                    : $"{customFileName}.bak";

                string fullPath = Path.Combine(targetFolder, fileName);

                // SQL Command for Backup
                string query = $@"BACKUP DATABASE [{DatabaseName}] 
                                TO DISK = @path 
                                WITH FORMAT, INIT, NAME = 'Full Backup of {DatabaseName}';";

                SqlParameter[] parameters = { new SqlParameter("@path", fullPath) };

                await DB_Server.ExecuteAsync(query, parameters);

                // Log the backup
                FileInfo fileInfo = new FileInfo(fullPath);
                double sizeMB = fileInfo.Exists ? (double)fileInfo.Length / (1024 * 1024) : 0;

                await LogBackupAsync(fileName, fullPath, sizeMB, type.ToString(), "Success");

                return true;
            }
            catch (Exception ex)
            {
                await LogBackupAsync("Failed Backup", "", 0, type.ToString(), "Failed: " + ex.Message);
                throw;
            }
        }

        public static async Task<bool> RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!File.Exists(backupFilePath))
                    throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود.");

                // To restore, we need to switch to master database and kill existing connections
                string query = $@"
                    USE master;
                    ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{DatabaseName}] FROM DISK = @path WITH REPLACE;
                    ALTER DATABASE [{DatabaseName}] SET MULTI_USER;";

                SqlParameter[] parameters = { new SqlParameter("@path", backupFilePath) };

                // Since we are switching to master, we might need a connection to master
                // However, our DB_Server uses DB_Sestamk in InitialCatalog.
                // Let's use a custom connection for restore
                
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(DB_Server.GetConnection().ConnectionString);
                builder.InitialCatalog = "master";

                using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 300; // Restore can take time
                        cmd.Parameters.AddRange(parameters);
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task LogBackupAsync(string fileName, string filePath, double sizeMB, string type, string status)
        {
            string query = @"INSERT INTO BackupLogs (FileName, FilePath, FileSizeMB, BackupType, CreatedBy, CreatedDate, Status)
                            VALUES (@name, @path, @size, @type, @user, @date, @status)";

            SqlParameter[] parameters = {
                new SqlParameter("@name", fileName),
                new SqlParameter("@path", filePath),
                new SqlParameter("@size", sizeMB),
                new SqlParameter("@type", type),
                new SqlParameter("@user", UserSession.UserName ?? "System"),
                new SqlParameter("@date", DateTime.Now),
                new SqlParameter("@status", status)
            };

            await DB_Server.ExecuteAsync(query, parameters);
        }

        public static async Task<DataTable> GetBackupHistoryAsync()
        {
            return await DB_Server.GetTableAsync("SELECT * FROM BackupLogs ORDER BY CreatedDate DESC");
        }

        public static async Task<BackupSettingsModel> GetSettingsAsync()
        {
            DataTable dt = await DB_Server.GetTableAsync("SELECT TOP 1 * FROM BackupSettings");
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new BackupSettingsModel
                {
                    AutoBackupEnabled = Convert.ToBoolean(row["AutoBackupEnabled"]),
                    BackupIntervalHours = Convert.ToInt32(row["BackupIntervalHours"]),
                    BackupTime = (TimeSpan)row["BackupTime"],
                    BackupPath = row["BackupPath"].ToString()
                };
            }
            return new BackupSettingsModel { AutoBackupEnabled = false, BackupIntervalHours = 24, BackupTime = new TimeSpan(2, 0, 0), BackupPath = "" };
        }

        public static async Task SaveSettingsAsync(BackupSettingsModel settings)
        {
            string query = @"
                IF EXISTS (SELECT 1 FROM BackupSettings)
                    UPDATE BackupSettings SET AutoBackupEnabled = @enabled, BackupIntervalHours = @interval, BackupTime = @time, BackupPath = @path;
                ELSE
                    INSERT INTO BackupSettings (AutoBackupEnabled, BackupIntervalHours, BackupTime, BackupPath) VALUES (@enabled, @interval, @time, @path);";

            SqlParameter[] parameters = {
                new SqlParameter("@enabled", settings.AutoBackupEnabled),
                new SqlParameter("@interval", settings.BackupIntervalHours),
                new SqlParameter("@time", settings.BackupTime),
                new SqlParameter("@path", settings.BackupPath)
            };

            await DB_Server.ExecuteAsync(query, parameters);
        }

        public static async Task DeleteBackupAsync(int id, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            await DB_Server.ExecuteAsync("DELETE FROM BackupLogs WHERE Id = @id", new[] { new SqlParameter("@id", id) });
        }

        public static async Task CleanupOldBackupsAsync(int keepLast)
        {
            DataTable dt = await DB_Server.GetTableAsync($"SELECT * FROM BackupLogs ORDER BY CreatedDate DESC OFFSET {keepLast} ROWS");
            foreach (DataRow row in dt.Rows)
            {
                await DeleteBackupAsync(Convert.ToInt32(row["Id"]), row["FilePath"].ToString());
            }
        }

        public static void CompressBackup(string filePath)
        {
            string zipPath = Path.ChangeExtension(filePath, ".zip");
            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
            }
            File.Delete(filePath);
        }
    }
}
