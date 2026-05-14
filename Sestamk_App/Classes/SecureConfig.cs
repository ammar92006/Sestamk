using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Sestamk.Classes
{
    /// <summary>
    /// Manages sensitive configuration (DB credentials, API keys) using
    /// Windows DPAPI encryption (CurrentUser scope — only the installing user can decrypt).
    ///
    /// Replaces hardcoded credentials in DB_Server.cs and SettingsService.cs.
    ///
    /// File location: %LOCALAPPDATA%\Sestamk\config.dat (encrypted)
    /// On first run: if config.dat doesn't exist, creates it from defaults
    /// and prompts user to update via Settings → Database.
    /// </summary>
    public static class SecureConfig
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sestamk");

        private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.dat");

        private static ConfigData? _cached;

        public static string DbDataSource => Load().DbDataSource;
        public static string DbUserId => Load().DbUserId;
        public static string DbPassword => Load().DbPassword;
        public static string SupabaseUrl => Load().SupabaseUrl;
        public static string SupabaseKey => Load().SupabaseKey;
        public static string GeminiApiKey => Load().GeminiApiKey;

        /// <summary>
        /// Load config from encrypted file. Creates default if missing.
        /// </summary>
        public static ConfigData Load()
        {
            if (_cached != null) return _cached;

            if (File.Exists(ConfigPath))
            {
                try
                {
                    byte[] encrypted = File.ReadAllBytes(ConfigPath);
                    byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    string json = Encoding.UTF8.GetString(decrypted);
                    _cached = JsonSerializer.Deserialize<ConfigData>(json) ?? CreateDefault();
                    return _cached;
                }
                catch
                {
                    _cached = CreateDefault();
                    return _cached;
                }
            }

            _cached = CreateDefault();
            Save(_cached);
            return _cached;
        }

        /// <summary>
        /// Save config to encrypted file.
        /// Call this from the Database Settings panel after user updates credentials.
        /// </summary>
        public static void Save(ConfigData config)
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = false });
                byte[] plain = Encoding.UTF8.GetBytes(json);
                byte[] encrypted = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(ConfigPath, encrypted);
                _cached = config;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SecureConfig.Save failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Update just the DB credentials and re-initialize DB_Server.
        /// </summary>
        public static void UpdateDbCredentials(string dataSource, string userId, string password)
        {
            var config = Load();
            config.DbDataSource = dataSource;
            config.DbUserId = userId;
            config.DbPassword = password;
            Save(config);
            DB_Server.Initialize(dataSource, userId, password);
        }

        /// <summary>
        /// Clear cache to force re-read from disk.
        /// </summary>
        public static void ClearCache() => _cached = null;

        /// <summary>
        /// Check if this is the first run (no config file exists).
        /// </summary>
        public static bool IsFirstRun => !File.Exists(ConfigPath);

        private static ConfigData CreateDefault()
        {
            return new ConfigData
            {
                DbDataSource = @"DESKTOP-N2BHNP0\SQLEXPRESS",
                DbUserId = "Sestamk_App",
                DbPassword = "StrongPassword123!",
                SupabaseUrl = "https://axigicbiydhfbkfqogma.supabase.co",
                SupabaseKey = "sb_publishable__LRAn0WS56TL5LLa8Y3TLw_7yxI-UK7",
                GeminiApiKey = "AIzaSyCxnP6qJu72QTQsDZJO-8ZsidyPXu3B3Y8"
            };
        }
    }

    public class ConfigData
    {
        public string DbDataSource { get; set; } = @".\SQLEXPRESS";
        public string DbUserId { get; set; } = "Sestamk_App";
        public string DbPassword { get; set; } = "";
        public string SupabaseUrl { get; set; } = "";
        public string SupabaseKey { get; set; } = "";
        public string GeminiApiKey { get; set; } = "";
    }
}
