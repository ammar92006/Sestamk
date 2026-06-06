using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SestamkAdmin.Services;

/// <summary>
/// إعدادات لوحة التحكم — تُخزَّن مشفّرة على جهاز المالك فقط باستخدام DPAPI
/// (نطاق CurrentUser: لا يفك تشفيرها إلا نفس مستخدم الويندوز على نفس الجهاز).
/// المسار: %LOCALAPPDATA%\SestamkAdmin\config.dat
///
/// ملاحظة أمنية: لوحة التحكم لا تحمل service_role key إطلاقاً. هي تتصل فقط
/// بـ Edge Functions باستخدام anon key + توكن جلسة الأدمن بعد تسجيل الدخول.
/// </summary>
public static class AdminConfig
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SestamkAdmin");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.dat");

    private static ConfigData? _cached;

    public static string SupabaseUrl => Load().SupabaseUrl;
    public static string SupabaseAnonKey => Load().SupabaseAnonKey;
    public static string GitHubToken { get => Load().GitHubToken; }
    public static string GitHubOwner => Load().GitHubOwner;
    public static string GitHubRepo => Load().GitHubRepo;
    public static string BuildFolderPath => Load().BuildFolderPath;

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

    public static void Save(ConfigData config)
    {
        Directory.CreateDirectory(ConfigDir);
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = false });
        byte[] plain = Encoding.UTF8.GetBytes(json);
        byte[] encrypted = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(ConfigPath, encrypted);
        _cached = config;
    }

    public static void ClearCache() => _cached = null;

    private static ConfigData CreateDefault() => new()
    {
        // نفس بروجيكت Supabase الخاص بسيستمك
        SupabaseUrl = "https://axigicbiydhfbkfqogma.supabase.co",
        SupabaseAnonKey = "sb_publishable__LRAn0WS56TL5LLa8Y3TLw_7yxI-UK7",
        GitHubOwner = "ammar92006",
        GitHubRepo = "Sestamk",
        GitHubToken = "",
        BuildFolderPath = @"E:\Sestamk\Sestamk_App\bin\Debug\net10.0-windows"
    };
}

public class ConfigData
{
    public string SupabaseUrl { get; set; } = "";
    public string SupabaseAnonKey { get; set; } = "";
    public string GitHubToken { get; set; } = "";
    public string GitHubOwner { get; set; } = "";
    public string GitHubRepo { get; set; } = "";
    public string BuildFolderPath { get; set; } = "";
}
