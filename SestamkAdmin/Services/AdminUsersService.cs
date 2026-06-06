using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

public class AdminUserRow
{
    public string  Id          { get; set; } = "";
    public string  Username    { get; set; } = "";
    public string  Email       { get; set; } = "";
    public string  FullName    { get; set; } = "";
    public string  Role        { get; set; } = "admin";
    public bool    IsActive    { get; set; }
    public string? LastLogin   { get; set; }
    public string  CreatedAt   { get; set; } = "";

    public string RoleDisplay => Role switch
    {
        "super_admin" => "مالك", "admin" => "مشرف", "support" => "دعم فني", _ => Role
    };
    public string RoleColor => Role switch
    {
        "super_admin" => "#10B981", "admin" => "#0EA5E9", _ => "#94A3B8"
    };
    public string AvatarLetter => string.IsNullOrEmpty(FullName)
        ? (string.IsNullOrEmpty(Username) ? "?" : Username[0].ToString().ToUpper())
        : FullName.Trim()[0].ToString();
    public string StatusText  => IsActive ? "نشط"    : "موقوف";
    public string StatusColor => IsActive ? "#22C55E" : "#94A3B8";
    public string LastLoginDisplay =>
        DateTime.TryParse(LastLogin, out var d)
            ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "لم يدخل بعد";

    public static AdminUserRow From(JToken j) => new()
    {
        Id        = j["id"]?.ToString() ?? "",
        Username  = j["username"]?.ToString() ?? "",
        Email     = j["email"]?.ToString() ?? "",
        FullName  = j["full_name"]?.ToString() ?? "",
        Role      = j["role"]?.ToString() ?? "admin",
        IsActive  = j["is_active"]?.Value<bool>() ?? true,
        LastLogin = j["last_login_at"]?.ToString(),
        CreatedAt = j["created_at"]?.ToString() ?? "",
    };
}

public static class AdminUsersService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<AdminUserRow>> GetAllAsync()
    {
        var arr = await Db.SelectAsync("admin_users",
            "select=id,username,email,full_name,role,is_active,last_login_at,created_at&order=created_at.asc");
        return arr.Select(AdminUserRow.From).ToList();
    }

    public static async Task<(bool ok, string? error)> CreateAsync(
        string username, string password, string email, string fullName, string role)
    {
        try
        {
            var result = await Db.RpcAsync("admin_create_user", new
            {
                p_username  = username,
                p_password  = password,
                p_email     = email,
                p_full_name = fullName,
                p_role      = role
            });
            var obj = result as JObject;
            bool ok  = obj?["success"]?.Value<bool>() ?? false;
            string? err = obj?["error"]?.ToString();
            if (ok)
                await AuditLog.WriteAsync("admin_user", null, "create", $"إنشاء مشرف {username} ({role})");
            return (ok, err);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static async Task SetActiveAsync(string id, string username, bool active)
    {
        await Db.UpdateAsync("admin_users", $"id=eq.{id}", new { is_active = active });
        await AuditLog.WriteAsync("admin_user", id, active ? "enable" : "disable",
            $"{(active ? "تفعيل" : "إيقاف")} مشرف {username}");
    }

    public static async Task SetRoleAsync(string id, string username, string role)
    {
        await Db.UpdateAsync("admin_users", $"id=eq.{id}", new { role });
        await AuditLog.WriteAsync("admin_user", id, "role_change", $"تغيير دور {username} إلى {role}");
    }

    public static async Task<(bool ok, string? error)> ChangePasswordAsync(
        string adminId, string oldPass, string newPass)
    {
        try
        {
            var result = await Db.RpcAsync("admin_change_password", new
            {
                p_admin_id = adminId,
                p_old      = oldPass,
                p_new      = newPass
            });
            var obj = result as JObject;
            bool ok = obj?["success"]?.Value<bool>() ?? false;
            return (ok, ok ? null : obj?["error"]?.ToString());
        }
        catch (Exception ex) { return (false, ex.Message); }
    }
}
