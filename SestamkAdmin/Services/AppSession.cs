namespace SestamkAdmin.Services;

/// <summary>
/// جلسة الأدمن الحالية بعد تسجيل الدخول.
/// </summary>
public static class AppSession
{
    public static string? AccessToken { get; set; }
    public static string? AdminId { get; set; }
    public static string? AdminName { get; set; }
    public static string? AdminUsername { get; set; }
    public static string? AdminRole { get; set; }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(AdminId);

    public static void Clear()
    {
        AccessToken = null;
        AdminId = null;
        AdminName = null;
        AdminUsername = null;
        AdminRole = null;
    }
}
