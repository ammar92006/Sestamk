using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

public class NotificationRow
{
    public string  Id        { get; set; } = "";
    public string  Type      { get; set; } = "";
    public string  Title     { get; set; } = "";
    public string  Message   { get; set; } = "";
    public bool    IsRead    { get; set; }
    public string? CreatedAt { get; set; }

    public string Icon => Type switch
    {
        "license_expiring" => "Clock24",
        "license_expired"  => "Warning24",
        "device_blocked"   => "LockClosed24",
        "request_pending"  => "MailInbox24",
        "new_company"      => "BuildingMultiple24",
        _                  => "Alert24"
    };
    public string Color => Type switch
    {
        "license_expired" or "device_blocked" => "#EF4444",
        "license_expiring"                    => "#F59E0B",
        "new_company"                         => "#22C55E",
        "request_pending"                     => "#0EA5E9",
        _                                     => "#94A3B8"
    };
    public string When =>
        DateTime.TryParse(CreatedAt, out var d) ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "";

    public static NotificationRow From(JToken j) => new()
    {
        Id        = j["id"]?.ToString() ?? "",
        Type      = j["type"]?.ToString() ?? "",
        Title     = j["title"]?.ToString() ?? "",
        Message   = j["message"]?.ToString() ?? "",
        IsRead    = j["is_read"]?.Value<bool>() ?? false,
        CreatedAt = j["created_at"]?.ToString(),
    };
}

public static class NotificationService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<NotificationRow>> GetAllAsync(bool unreadOnly = false)
    {
        string q = "select=*&order=created_at.desc&limit=200";
        if (unreadOnly) q += "&is_read=eq.false";
        var arr = await Db.SelectAsync("notifications", q);
        return arr.Select(NotificationRow.From).ToList();
    }

    public static async Task<int> GetUnreadCountAsync()
    {
        var arr = await Db.SelectAsync("notifications", "select=id&is_read=eq.false");
        return arr.Count;
    }

    public static async Task MarkReadAsync(string id)
    {
        await Db.UpdateAsync("notifications", $"id=eq.{id}", new
        {
            is_read = true, read_by = AppSession.AdminId, read_at = DateTime.UtcNow
        });
    }

    public static async Task MarkAllReadAsync()
    {
        await Db.UpdateAsync("notifications", "is_read=eq.false", new
        {
            is_read = true, read_by = AppSession.AdminId, read_at = DateTime.UtcNow
        });
    }

    public static async Task DeleteAsync(string id)
        => await Db.DeleteAsync("notifications", $"id=eq.{id}");
}
