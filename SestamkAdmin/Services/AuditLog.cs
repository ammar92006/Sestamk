namespace SestamkAdmin.Services;

/// <summary>كتابة سجل تدقيق لكل عملية إدارية مهمة في جدول audit_logs.</summary>
public static class AuditLog
{
    public static async Task WriteAsync(string entityType, string? entityId, string action, string notes)
    {
        try
        {
            await SupabaseService.Instance.InsertAsync("audit_logs", new
            {
                actor_type = "admin",
                actor_id = AppSession.AdminId,
                actor_name = AppSession.AdminName,
                entity_type = entityType,
                entity_id = entityId,
                action,
                notes
            });
        }
        catch
        {
            // لا نعطّل العملية الأساسية لو فشل تسجيل التدقيق
        }
    }
}
