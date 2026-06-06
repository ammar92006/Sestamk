using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

public class DashboardStats
{
    public int CompaniesTotal { get; set; }
    public int CompaniesActive { get; set; }
    public int LicensesTotal { get; set; }
    public int LicensesActive { get; set; }
    public int LicensesExpiring7 { get; set; }
    public int LicensesExpiring30 { get; set; }
    public int LicensesExpired { get; set; }
    public int DevicesTotal { get; set; }
    public int DevicesBlocked { get; set; }
    public int RequestsPending { get; set; }
    public int NotificationsUnread { get; set; }
    public decimal RevenueTotal { get; set; }

    public static DashboardStats From(JToken j) => new()
    {
        CompaniesTotal     = j["companies"]?["total"]?.Value<int>() ?? 0,
        CompaniesActive    = j["companies"]?["active"]?.Value<int>() ?? 0,
        LicensesTotal      = j["licenses"]?["total"]?.Value<int>() ?? 0,
        LicensesActive     = j["licenses"]?["active"]?.Value<int>() ?? 0,
        LicensesExpiring7  = j["licenses"]?["expiring_7d"]?.Value<int>() ?? 0,
        LicensesExpiring30 = j["licenses"]?["expiring_30d"]?.Value<int>() ?? 0,
        LicensesExpired    = j["licenses"]?["expired"]?.Value<int>() ?? 0,
        DevicesTotal       = j["devices"]?["total"]?.Value<int>() ?? 0,
        DevicesBlocked     = j["devices"]?["blocked"]?.Value<int>() ?? 0,
        RequestsPending    = j["requests"]?["pending"]?.Value<int>() ?? 0,
        NotificationsUnread= j["notifications"]?["unread"]?.Value<int>() ?? 0,
        RevenueTotal       = j["revenue_total"]?.Type == JTokenType.Null ? 0 : (j["revenue_total"]?.Value<decimal>() ?? 0),
    };
}

public static class DashboardService
{
    public static async Task<DashboardStats> GetStatsAsync()
    {
        var result = await SupabaseService.Instance.RpcAsync("get_dashboard_stats", new { });
        return DashboardStats.From(result);
    }

    public static async Task<JObject> SearchAsync(string query)
    {
        var result = await SupabaseService.Instance.RpcAsync("search_global", new { p_query = query });
        return (JObject)result;
    }
}
