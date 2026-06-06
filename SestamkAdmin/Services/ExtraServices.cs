using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

// ============ Resellers ============
public class ResellerRow
{
    public string  Id             { get; set; } = "";
    public string  Name           { get; set; } = "";
    public string? Phone          { get; set; }
    public string? Email          { get; set; }
    public string? City           { get; set; }
    public decimal CommissionRate { get; set; }
    public bool    IsActive       { get; set; }
    public string? Notes          { get; set; }

    public string StatusText => IsActive ? "نشط" : "موقوف";
    public string CommissionText => $"{CommissionRate:0.##}%";

    public static ResellerRow From(JToken j) => new()
    {
        Id             = j["id"]?.ToString() ?? "",
        Name           = j["name"]?.ToString() ?? "",
        Phone          = j["phone"]?.ToString(),
        Email          = j["email"]?.ToString(),
        City           = j["city"]?.ToString(),
        CommissionRate = j["commission_rate"]?.Type == JTokenType.Null ? 0 : (j["commission_rate"]?.Value<decimal>() ?? 0),
        IsActive       = j["is_active"]?.Value<bool>() ?? true,
        Notes          = j["notes"]?.ToString(),
    };
}

public static class ResellerService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<ResellerRow>> GetAllAsync()
    {
        var arr = await Db.SelectAsync("resellers", "select=*&order=created_at.desc");
        return arr.Select(ResellerRow.From).ToList();
    }

    public static async Task CreateAsync(object r)
    {
        var c = await Db.InsertAsync("resellers", r);
        await AuditLog.WriteAsync("reseller", c["id"]?.ToString(), "create", $"إضافة موزّع {c["name"]}");
    }

    public static async Task UpdateAsync(string id, object changes, string name)
    {
        await Db.UpdateAsync("resellers", $"id=eq.{id}", changes);
        await AuditLog.WriteAsync("reseller", id, "update", $"تعديل موزّع {name}");
    }

    public static async Task DeleteAsync(string id, string name)
    {
        await Db.DeleteAsync("resellers", $"id=eq.{id}");
        await AuditLog.WriteAsync("reseller", id, "delete", $"حذف موزّع {name}");
    }
}

// ============ Announcements ============
public class AnnouncementRow
{
    public string  Id            { get; set; } = "";
    public string  Title         { get; set; } = "";
    public string  Body          { get; set; } = "";
    public string? TargetChannel { get; set; }
    public bool    IsActive      { get; set; }
    public string? ShowUntil     { get; set; }
    public string? CreatedAt     { get; set; }

    public string StatusText  => IsActive ? "نشط" : "موقوف";
    public string StatusColor => IsActive ? "#22C55E" : "#94A3B8";
    public string TargetText  => string.IsNullOrEmpty(TargetChannel) ? "كل القنوات" : TargetChannel;
    public string UntilText   => DateTime.TryParse(ShowUntil, out var d) ? d.ToString("yyyy/MM/dd") : "بلا نهاية";

    public static AnnouncementRow From(JToken j) => new()
    {
        Id            = j["id"]?.ToString() ?? "",
        Title         = j["title"]?.ToString() ?? "",
        Body          = j["body"]?.ToString() ?? "",
        TargetChannel = j["target_channel"]?.ToString(),
        IsActive      = j["is_active"]?.Value<bool>() ?? true,
        ShowUntil     = j["show_until"]?.ToString(),
        CreatedAt     = j["created_at"]?.ToString(),
    };
}

public static class AnnouncementService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<AnnouncementRow>> GetAllAsync()
    {
        var arr = await Db.SelectAsync("announcements", "select=*&order=created_at.desc");
        return arr.Select(AnnouncementRow.From).ToList();
    }

    public static async Task CreateAsync(object a)
    {
        var c = await Db.InsertAsync("announcements", a);
        await AuditLog.WriteAsync("announcement", c["id"]?.ToString(), "create", $"إنشاء إعلان {c["title"]}");
    }

    public static async Task SetActiveAsync(string id, string title, bool active)
    {
        await Db.UpdateAsync("announcements", $"id=eq.{id}", new { is_active = active });
        await AuditLog.WriteAsync("announcement", id, active ? "enable" : "disable",
            $"{(active ? "تفعيل" : "إيقاف")} إعلان {title}");
    }

    public static async Task DeleteAsync(string id, string title)
    {
        await Db.DeleteAsync("announcements", $"id=eq.{id}");
        await AuditLog.WriteAsync("announcement", id, "delete", $"حذف إعلان {title}");
    }
}

// ============ License History ============
public class HistoryRow
{
    public string  Action       { get; set; } = "";
    public string? OldExpires   { get; set; }
    public string? NewExpires   { get; set; }
    public string? OldPlan      { get; set; }
    public string? NewPlan      { get; set; }
    public string? ChangedAt    { get; set; }

    public string ActionText => Action switch
    {
        "created"      => "إنشاء",
        "renewed"      => "تجديد",
        "suspended"    => "إيقاف",
        "resumed"      => "إعادة تفعيل",
        "plan_changed" => "تغيير باقة",
        "extended"     => "تمديد",
        "deleted"      => "حذف",
        _              => Action
    };
    public string ActionColor => Action switch
    {
        "suspended" or "deleted"     => "#EF4444",
        "renewed" or "resumed" or "created" => "#22C55E",
        "plan_changed" or "extended" => "#F59E0B",
        _                            => "#94A3B8"
    };
    public string Detail
    {
        get
        {
            if (Action == "plan_changed" && OldPlan != null) return $"{OldPlan} ← {NewPlan}";
            if (NewExpires != null) return $"حتى {NewExpires}";
            return "";
        }
    }
    public string When => DateTime.TryParse(ChangedAt, out var d)
        ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "";

    public static HistoryRow From(JToken j) => new()
    {
        Action     = j["action"]?.ToString() ?? "",
        OldExpires = j["old_expires_at"]?.ToString(),
        NewExpires = j["new_expires_at"]?.ToString(),
        OldPlan    = j["old_plan"]?.ToString(),
        NewPlan    = j["new_plan"]?.ToString(),
        ChangedAt  = j["changed_at"]?.ToString(),
    };
}

public static class HistoryService
{
    public static async Task<List<HistoryRow>> GetForLicenseAsync(string licenseId)
    {
        var arr = await SupabaseService.Instance.SelectAsync("license_history",
            $"select=*&license_id=eq.{licenseId}&order=changed_at.desc");
        return arr.Select(HistoryRow.From).ToList();
    }
}
