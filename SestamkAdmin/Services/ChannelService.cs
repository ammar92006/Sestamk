using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

public class ChannelRow
{
    public int    Id          { get; set; }
    public string ChannelName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool   IsActive    { get; set; }
    public string CreatedAt   { get; set; } = "";

    public string StatusText  => IsActive ? "نشطة" : "موقوفة";
    public string StatusColor => IsActive ? "#22C55E" : "#94A3B8";

    public static ChannelRow From(JToken j) => new()
    {
        Id          = j["id"]?.Value<int>() ?? 0,
        ChannelName = j["channel_name"]?.ToString() ?? "",
        Description = j["description"]?.ToString() ?? "",
        IsActive    = j["is_active"]?.Value<bool>() ?? true,
        CreatedAt   = j["created_at"]?.ToString() ?? "",
    };
}

public static class ChannelService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<ChannelRow>> GetAllAsync()
    {
        var arr = await Db.SelectAsync("update_channels", "select=*&order=id.asc");
        return arr.Select(ChannelRow.From).ToList();
    }

    public static async Task<JObject> CreateAsync(string name, string description)
    {
        var created = await Db.InsertAsync("update_channels", new
        {
            channel_name = name.Trim().ToLower(),
            description  = description.Trim(),
            is_active    = true
        });
        await AuditLog.WriteAsync("channel", created["id"]?.ToString(), "create", $"إضافة قناة {name}");
        return created;
    }

    public static async Task UpdateAsync(int id, string name, string description)
    {
        await Db.UpdateAsync("update_channels", $"id=eq.{id}", new
        {
            channel_name = name.Trim().ToLower(),
            description  = description.Trim()
        });
        await AuditLog.WriteAsync("channel", id.ToString(), "update", $"تعديل قناة {name}");
    }

    public static async Task SetActiveAsync(int id, string name, bool active)
    {
        await Db.UpdateAsync("update_channels", $"id=eq.{id}", new { is_active = active });
        await AuditLog.WriteAsync("channel", id.ToString(), active ? "enable" : "disable",
            $"{(active ? "تفعيل" : "إيقاف")} قناة {name}");
    }

    public static async Task DeleteAsync(int id, string name)
    {
        await Db.DeleteAsync("update_channels", $"id=eq.{id}");
        await AuditLog.WriteAsync("channel", id.ToString(), "delete", $"حذف قناة {name}");
    }
}
