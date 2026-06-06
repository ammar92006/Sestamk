using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

/// <summary>
/// مصادقة المالك/المشرف. تستدعي Edge Function اسمها admin-login التي:
/// 1) تتحقق من اسم المستخدم وكلمة المرور (bcrypt) مقابل جدول admin_users.
/// 2) تتأكد أن الحساب is_active = true.
/// 3) تُرجع توكن جلسة + بيانات المشرف، وتحدّث last_login_at.
/// </summary>
public static class AuthService
{
    public static async Task<(bool ok, string? error)> LoginAsync(string username, string password)
    {
        try
        {
            var resp = await SupabaseService.Instance.FunctionAsync("admin-login", new
            {
                username,
                password
            });

            var obj = resp as JObject;
            if (obj == null)
                return (false, "استجابة غير صالحة من الخادم.");

            bool success = obj["success"]?.Value<bool>() ?? false;
            if (!success)
                return (false, obj["error"]?.ToString() ?? "بيانات الدخول غير صحيحة.");

            var admin = obj["admin"] as JObject;
            AppSession.AccessToken = obj["token"]?.ToString();
            AppSession.AdminId = admin?["id"]?.ToString();
            AppSession.AdminName = admin?["full_name"]?.ToString() ?? admin?["username"]?.ToString();
            AppSession.AdminUsername = admin?["username"]?.ToString();
            AppSession.AdminRole = admin?["role"]?.ToString();

            return (true, null);
        }
        catch (SupabaseException ex)
        {
            // محاولة استخراج رسالة الخطأ من جسم الرد
            try
            {
                var detail = JObject.Parse(ex.Details);
                return (false, detail["error"]?.ToString() ?? ex.Message);
            }
            catch
            {
                return (false, "تعذّر الاتصال بالخادم. تأكد من الإنترنت ومن إعداد Edge Function.");
            }
        }
        catch (Exception ex)
        {
            return (false, $"خطأ: {ex.Message}");
        }
    }
}
