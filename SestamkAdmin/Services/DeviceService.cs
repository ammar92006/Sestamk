using Newtonsoft.Json.Linq;
using SestamkAdmin.Models;

namespace SestamkAdmin.Services;

public static class DeviceService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<DeviceRow>> GetAllAsync()
    {
        var arr = await Db.SelectAsync("devices",
            "select=*,companies(company_name)&order=last_seen.desc");
        return arr.Select(DeviceRow.From).ToList();
    }

    public static async Task SetBlockedAsync(string id, string hwid, bool blocked, string? reason)
    {
        await Db.UpdateAsync("devices", $"id=eq.{id}", new
        {
            is_blocked = blocked,
            block_reason = blocked ? reason : null,
            blocked_at = blocked ? DateTime.UtcNow : (DateTime?)null,
            blocked_by = blocked ? AppSession.AdminId : null,
            updated_at = DateTime.UtcNow
        });
        await AuditLog.WriteAsync("device", id, blocked ? "block" : "unblock",
            blocked ? $"حظر جهاز {hwid}: {reason}" : $"فك حظر جهاز {hwid}");
    }

    public static async Task DeleteAsync(string id, string hwid)
    {
        await Db.DeleteAsync("devices", $"id=eq.{id}");
        await AuditLog.WriteAsync("device", id, "delete", $"حذف/فك ربط جهاز {hwid}");
    }
}
