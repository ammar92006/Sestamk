using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Models;

/// <summary>صف شركة مبسّط للعرض والاختيار.</summary>
public class CompanyRow
{
    public string Id { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string? Phone { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public string? UpdateChannel { get; set; }
    public string? ContractEnd { get; set; }

    public string Display => CompanyName;

    public static CompanyRow From(JToken j) => new()
    {
        Id = j["id"]?.ToString() ?? "",
        CompanyName = j["company_name"]?.ToString() ?? "—",
        Phone = j["phone"]?.ToString(),
        City = j["city"]?.ToString(),
        IsActive = j["is_active"]?.Value<bool>() ?? true,
        IsSuspended = j["is_suspended"]?.Value<bool>() ?? false,
        UpdateChannel = j["update_channel"]?.ToString(),
        ContractEnd = j["contract_end"]?.ToString(),
    };

    public string StatusText => IsSuspended ? "موقوفة" : IsActive ? "نشطة" : "غير مفعّلة";
}

/// <summary>صف جهاز.</summary>
public class DeviceRow
{
    public string Id { get; set; } = "";
    public string Hwid { get; set; } = "";
    public string CompanyName { get; set; } = "—";
    public string? DeviceName { get; set; }
    public string? OsInfo { get; set; }
    public string? Processor { get; set; }
    public int? RamGb { get; set; }
    public string? AppVersion { get; set; }
    public string? LastSeen { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }

    public string ShortHwid => Hwid.Length > 16 ? Hwid[..16] + "…" : Hwid;
    public string Specs => $"{Processor ?? "—"} · {(RamGb.HasValue ? RamGb + "GB" : "—")}";
    public string LastSeenDisplay =>
        DateTime.TryParse(LastSeen, out var d) ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "—";
    public string StatusText => IsBlocked ? "محظور" : "نشط";
    public string StatusColor => IsBlocked ? "#EF4444" : "#22C55E";

    public static DeviceRow From(JToken j)
    {
        var r = new DeviceRow
        {
            Id = j["id"]?.ToString() ?? "",
            Hwid = j["hwid"]?.ToString() ?? "",
            DeviceName = j["device_name"]?.ToString(),
            OsInfo = j["os_info"]?.ToString(),
            Processor = j["processor"]?.ToString(),
            RamGb = j["ram_gb"]?.Type == JTokenType.Null ? null : j["ram_gb"]?.Value<int>(),
            AppVersion = j["app_version"]?.ToString(),
            LastSeen = j["last_seen"]?.ToString(),
            IsBlocked = j["is_blocked"]?.Value<bool>() ?? false,
            BlockReason = j["block_reason"]?.ToString(),
        };
        var c = j["companies"];
        if (c != null && c.Type == JTokenType.Object) r.CompanyName = c["company_name"]?.ToString() ?? "—";
        return r;
    }
}

/// <summary>صف مستخدم شركة.</summary>
public class UserRow
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string FullName { get; set; } = "";
    public string CompanyName { get; set; } = "—";
    public string Role { get; set; } = "user";
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public string? LastLogin { get; set; }

    public string RoleDisplay => Role switch
    {
        "admin" => "مدير", "manager" => "مشرف", "user" => "مستخدم", "viewer" => "مشاهد", _ => Role
    };
    public string StatusText => IsActive ? "نشط" : "موقوف";
    public string LastLoginDisplay =>
        DateTime.TryParse(LastLogin, out var d) ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "—";

    public static UserRow From(JToken j)
    {
        var r = new UserRow
        {
            Id = j["id"]?.ToString() ?? "",
            Username = j["username"]?.ToString() ?? "",
            FullName = j["full_name"]?.ToString() ?? "",
            Role = j["role"]?.ToString() ?? "user",
            Phone = j["phone"]?.ToString(),
            IsActive = j["is_active"]?.Value<bool>() ?? true,
            LastLogin = j["last_login_at"]?.ToString(),
        };
        var c = j["companies"];
        if (c != null && c.Type == JTokenType.Object) r.CompanyName = c["company_name"]?.ToString() ?? "—";
        return r;
    }
}

/// <summary>صف طلب وارد من عميل.</summary>
public class RequestRow
{
    public string Id { get; set; } = "";
    public string CompanyName { get; set; } = "—";
    public string RequestType { get; set; } = "";
    public string? Hwid { get; set; }
    public string Details { get; set; } = "";
    public string Status { get; set; } = "pending";
    public string? CreatedAt { get; set; }

    public string TypeDisplay => RequestType switch
    {
        "trial_request" => "طلب تجربة",
        "add_user" => "إضافة مستخدم",
        "add_device" => "إضافة جهاز",
        "support" => "دعم",
        "upgrade" => "ترقية",
        "unblock_device" => "فك حظر جهاز",
        _ => "أخرى"
    };
    public string StatusText => Status switch
    {
        "pending" => "معلّق", "approved" => "مقبول", "rejected" => "مرفوض", "in_progress" => "قيد المعالجة", _ => Status
    };
    public string StatusColor => Status switch
    {
        "pending" => "#F59E0B", "approved" => "#22C55E", "rejected" => "#EF4444", "in_progress" => "#0EA5E9", _ => "#94A3B8"
    };
    public string CreatedDisplay =>
        DateTime.TryParse(CreatedAt, out var d) ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "—";

    public static RequestRow From(JToken j)
    {
        var r = new RequestRow
        {
            Id = j["id"]?.ToString() ?? "",
            RequestType = j["request_type"]?.ToString() ?? "",
            Hwid = j["hwid"]?.ToString(),
            Status = j["status"]?.ToString() ?? "pending",
            CreatedAt = j["created_at"]?.ToString(),
            Details = j["details"]?.ToString(Newtonsoft.Json.Formatting.None) ?? "",
        };
        var c = j["companies"];
        if (c != null && c.Type == JTokenType.Object) r.CompanyName = c["company_name"]?.ToString() ?? "—";
        return r;
    }
}

/// <summary>صف سجل تدقيق.</summary>
public class AuditRow
{
    public string ActorName { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string Action { get; set; } = "";
    public string Notes { get; set; } = "";
    public string? CreatedAt { get; set; }

    public string When =>
        DateTime.TryParse(CreatedAt, out var d) ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "—";

    public static AuditRow From(JToken j) => new()
    {
        ActorName = j["actor_name"]?.ToString() ?? "نظام",
        EntityType = j["entity_type"]?.ToString() ?? "",
        Action = j["action"]?.ToString() ?? "",
        Notes = j["notes"]?.ToString() ?? "",
        CreatedAt = j["created_at"]?.ToString(),
    };
}

/// <summary>صف إصدار/تحديث للعرض.</summary>
public class UpdateRow
{
    public string Id { get; set; } = "";
    public string Version { get; set; } = "";
    public string Channel { get; set; } = "";
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; }
    public string? ReleaseDate { get; set; }
    public long? DeltaSize { get; set; }
    public long? FullSize { get; set; }

    public string ChannelDisplay => Channel switch
    {
        "public" => "عام",
        "beta" => "تجريبي",
        "vip" => "VIP",
        "stable" => "مستقر",
        _ => Channel
    };

    public string SizeDisplay =>
        DeltaSize.HasValue ? $"{DeltaSize.Value / 1024.0 / 1024:F1} MB" : "—";

    public string StatusText => IsActive ? "نشط" : "موقوف";
    public string StatusColor => IsActive ? "#22C55E" : "#94A3B8";
    public string MandatoryText => IsMandatory ? "إجباري" : "اختياري";

    public static UpdateRow From(JToken j) => new()
    {
        Id = j["id"]?.ToString() ?? "",
        Version = j["version"]?.ToString() ?? "",
        Channel = j["channel"]?.ToString() ?? "",
        IsMandatory = j["is_mandatory"]?.Value<bool>() ?? false,
        IsActive = j["is_active"]?.Value<bool>() ?? true,
        ReleaseDate = j["release_date"]?.ToString(),
        DeltaSize = j["delta_size_bytes"]?.Type == JTokenType.Null ? null : j["delta_size_bytes"]?.Value<long>(),
        FullSize = j["full_size_bytes"]?.Type == JTokenType.Null ? null : j["full_size_bytes"]?.Value<long>(),
    };
}

/// <summary>صف ترخيص للعرض في الجدول.</summary>
public class LicenseRow
{
    public string Id { get; set; } = "";
    public string SerialKey { get; set; } = "";
    public string CompanyId { get; set; } = "";
    public string CompanyName { get; set; } = "—";
    public string PlanType { get; set; } = "basic";
    public int MaxUsers { get; set; }
    public int MaxDevices { get; set; }
    public int MaxBranches { get; set; }
    public string? StartsAt { get; set; }
    public string? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }

    public string PlanDisplay => PlanType switch
    {
        "trial" => "تجريبي",
        "basic" => "أساسي",
        "pro" => "احترافي",
        "vip" => "VIP",
        "custom" => "مخصّص",
        _ => PlanType
    };

    public int DaysLeft
    {
        get
        {
            if (DateTime.TryParse(ExpiresAt, out var dt))
                return (int)Math.Ceiling((dt.Date - DateTime.Now.Date).TotalDays);
            return 0;
        }
    }

    public string StatusText
    {
        get
        {
            if (IsSuspended) return "موقوف";
            if (!IsActive) return "غير مفعّل";
            if (DaysLeft < 0) return "منتهٍ";
            if (DaysLeft <= 7) return $"ينتهي خلال {DaysLeft} يوم";
            return "نشط";
        }
    }

    public string StatusColor
    {
        get
        {
            if (IsSuspended || !IsActive || DaysLeft < 0) return "#EF4444";
            if (DaysLeft <= 7) return "#F59E0B";
            return "#22C55E";
        }
    }

    public string Limits => $"{MaxUsers} مستخدم · {MaxDevices} جهاز · {MaxBranches} فرع";
    public string PriceDisplay => Price.HasValue ? $"{Price:0.##} {Currency ?? "EGP"}" : "—";

    public static LicenseRow From(JToken j)
    {
        var row = new LicenseRow
        {
            Id = j["id"]?.ToString() ?? "",
            SerialKey = j["serial_key"]?.ToString() ?? "",
            CompanyId = j["company_id"]?.ToString() ?? "",
            PlanType = j["plan_type"]?.ToString() ?? "basic",
            MaxUsers = j["max_users"]?.Value<int>() ?? 0,
            MaxDevices = j["max_devices"]?.Value<int>() ?? 0,
            MaxBranches = j["max_branches"]?.Value<int>() ?? 0,
            StartsAt = j["starts_at"]?.ToString(),
            ExpiresAt = j["expires_at"]?.ToString(),
            IsActive = j["is_active"]?.Value<bool>() ?? true,
            IsSuspended = j["is_suspended"]?.Value<bool>() ?? false,
            Price = j["price"]?.Type == JTokenType.Null ? null : j["price"]?.Value<decimal>(),
            Currency = j["currency"]?.ToString(),
        };
        // اسم الشركة من الـ join المضمّن إن وُجد
        var comp = j["companies"];
        if (comp != null && comp.Type == JTokenType.Object)
            row.CompanyName = comp["company_name"]?.ToString() ?? "—";
        return row;
    }
}
