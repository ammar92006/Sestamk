using Newtonsoft.Json.Linq;
using SestamkAdmin.Models;

namespace SestamkAdmin.Services;

public class FeatureFlag
{
    public string Key { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsEnabled { get; set; }
    public int? RowId { get; set; } // معرّف صف company_features إن وُجد
}

public static class FeatureService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<FeatureFlag>> GetForCompanyAsync(string companyId)
    {
        var defs = await Db.SelectAsync("feature_definitions", "select=*&order=category.asc,display_name.asc");
        var current = await Db.SelectAsync("company_features", $"select=id,feature_key,is_enabled&company_id=eq.{companyId}");

        var map = new Dictionary<string, (int id, bool enabled)>();
        foreach (var c in current)
        {
            string key = c["feature_key"]?.ToString() ?? "";
            map[key] = (c["id"]?.Value<int>() ?? 0, c["is_enabled"]?.Value<bool>() ?? false);
        }

        var list = new List<FeatureFlag>();
        foreach (var d in defs)
        {
            string key = d["feature_key"]?.ToString() ?? "";
            bool hasRow = map.TryGetValue(key, out var v);
            list.Add(new FeatureFlag
            {
                Key = key,
                DisplayName = d["display_name"]?.ToString() ?? key,
                Description = d["description"]?.ToString(),
                Category = d["category"]?.ToString(),
                IsEnabled = hasRow ? v.enabled : (d["default_value"]?.Value<bool>() ?? false),
                RowId = hasRow ? v.id : null
            });
        }
        return list;
    }

    public static async Task SetAsync(string companyId, FeatureFlag flag, bool enabled)
    {
        if (flag.RowId.HasValue)
        {
            await Db.UpdateAsync("company_features", $"id=eq.{flag.RowId.Value}", new
            {
                is_enabled = enabled,
                enabled_at = enabled ? DateTime.UtcNow : (DateTime?)null,
                enabled_by = AppSession.AdminId,
                updated_at = DateTime.UtcNow
            });
        }
        else
        {
            var created = await Db.InsertAsync("company_features", new
            {
                company_id = companyId,
                feature_key = flag.Key,
                is_enabled = enabled,
                enabled_at = enabled ? DateTime.UtcNow : (DateTime?)null,
                enabled_by = AppSession.AdminId
            });
            flag.RowId = created["id"]?.Value<int>();
        }
        await AuditLog.WriteAsync("feature", companyId, enabled ? "enable" : "disable",
            $"{(enabled ? "تشغيل" : "إطفاء")} ميزة {flag.DisplayName}");
    }
}
