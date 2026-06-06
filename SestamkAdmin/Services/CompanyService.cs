using Newtonsoft.Json.Linq;
using SestamkAdmin.Models;

namespace SestamkAdmin.Services;

public static class CompanyService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<CompanyRow>> GetAllAsync()
    {
        var arr = await Db.SelectAsync("companies",
            "select=id,company_name,phone,city,is_active,is_suspended,update_channel,contract_end&order=created_at.desc");
        return arr.Select(CompanyRow.From).ToList();
    }

    public static async Task<JObject> GetByIdAsync(string id)
    {
        var arr = await Db.SelectAsync("companies", $"select=*&id=eq.{id}&limit=1");
        return (JObject)arr.First!;
    }

    public static async Task<JObject> CreateAsync(object company)
    {
        var created = await Db.InsertAsync("companies", company);
        await AuditLog.WriteAsync("company", created["id"]?.ToString(), "create",
            $"إنشاء شركة {created["company_name"]}");
        return created;
    }

    public static async Task UpdateAsync(string id, object changes, string name)
    {
        await Db.UpdateAsync("companies", $"id=eq.{id}", changes);
        await AuditLog.WriteAsync("company", id, "update", $"تعديل بيانات شركة {name}");
    }

    public static async Task SetSuspendedAsync(string id, string name, bool suspended, string? reason)
    {
        await Db.UpdateAsync("companies", $"id=eq.{id}", new
        {
            is_suspended = suspended,
            suspended_reason = suspended ? reason : null,
            updated_at = DateTime.UtcNow
        });
        await AuditLog.WriteAsync("company", id, suspended ? "suspend" : "resume",
            suspended ? $"إيقاف شركة {name}: {reason}" : $"إعادة تفعيل شركة {name}");
    }

    public static async Task DeleteAsync(string id, string name)
    {
        await Db.DeleteAsync("companies", $"id=eq.{id}");
        await AuditLog.WriteAsync("company", id, "delete", $"حذف شركة {name}");
    }
}
