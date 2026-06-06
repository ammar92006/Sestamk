using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using SestamkAdmin.Models;

namespace SestamkAdmin.Services;

public static class LicenseService
{
    private static SupabaseService Db => SupabaseService.Instance;

    public static async Task<List<LicenseRow>> GetLicensesAsync()
    {
        var arr = await Db.SelectAsync("licenses",
            "select=*,companies(company_name)&order=created_at.desc");
        return arr.Select(LicenseRow.From).ToList();
    }

    public static async Task<List<CompanyRow>> GetCompaniesAsync()
    {
        var arr = await Db.SelectAsync("companies",
            "select=id,company_name,phone,city,is_active,is_suspended,update_channel,contract_end&order=company_name.asc");
        return arr.Select(CompanyRow.From).ToList();
    }

    /// <summary>توليد سيريال فريد بصيغة SEST-XXXX-XXXX-XXXX.</summary>
    public static string GenerateSerial()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // بدون أحرف ملتبسة
        var sb = new StringBuilder("SEST-");
        var bytes = RandomNumberGenerator.GetBytes(12);
        for (int i = 0; i < 12; i++)
        {
            sb.Append(chars[bytes[i] % chars.Length]);
            if (i is 3 or 7) sb.Append('-');
        }
        return sb.ToString();
    }

    public static async Task<JObject> CreateLicenseAsync(
        string companyId, string serial, string planType,
        int maxUsers, int maxDevices, int maxBranches,
        DateTime startsAt, DateTime expiresAt,
        bool isTrial, decimal? price, string currency)
    {
        var row = new
        {
            company_id = companyId,
            serial_key = serial,
            plan_type = planType,
            max_users = maxUsers,
            max_devices = maxDevices,
            max_branches = maxBranches,
            starts_at = startsAt.ToString("yyyy-MM-dd"),
            expires_at = expiresAt.ToString("yyyy-MM-dd"),
            is_trial = isTrial,
            is_active = true,
            is_suspended = false,
            price,
            currency,
            created_by = AppSession.AdminId
        };
        var created = await Db.InsertAsync("licenses", row);
        await AuditLog.WriteAsync("license", created["id"]?.ToString(), "create",
            $"إنشاء ترخيص {serial} ({planType})");
        return created;
    }

    public static async Task RenewAsync(string licenseId, string serial, DateTime newExpiry)
    {
        await Db.UpdateAsync("licenses", $"id=eq.{licenseId}", new
        {
            expires_at = newExpiry.ToString("yyyy-MM-dd"),
            is_active = true,
            is_suspended = false,
            updated_at = DateTime.UtcNow
        });
        await AuditLog.WriteAsync("license", licenseId, "renew",
            $"تجديد ترخيص {serial} حتى {newExpiry:yyyy-MM-dd}");
    }

    public static async Task SetSuspendedAsync(string licenseId, string serial, bool suspended, string? reason)
    {
        await Db.UpdateAsync("licenses", $"id=eq.{licenseId}", new
        {
            is_suspended = suspended,
            suspension_reason = suspended ? reason : null,
            updated_at = DateTime.UtcNow
        });
        await AuditLog.WriteAsync("license", licenseId, suspended ? "suspend" : "resume",
            suspended ? $"إيقاف ترخيص {serial}: {reason}" : $"إعادة تفعيل ترخيص {serial}");
    }

    public static async Task DeleteAsync(string licenseId, string serial)
    {
        await Db.DeleteAsync("licenses", $"id=eq.{licenseId}");
        await AuditLog.WriteAsync("license", licenseId, "delete", $"حذف ترخيص {serial}");
    }
}
