using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SestamkAdmin.Services;

/// <summary>
/// طبقة الاتصال بـ Supabase. كل عمليات القراءة/الكتابة الإدارية تمر عبر
/// Edge Function اسمها admin-db (بصلاحية service_role على السيرفر) بعد التحقق
/// من توكن جلسة الأدمن — لوحة التحكم لا تحمل أي مفتاح حسّاس.
/// تسجيل الدخول فقط (admin-login) يُستدعى مباشرة عبر FunctionAsync.
/// </summary>
public class SupabaseService
{
    private static readonly Lazy<SupabaseService> _instance = new(() => new SupabaseService());
    public static SupabaseService Instance => _instance.Value;

    private readonly HttpClient _http;

    private SupabaseService()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(40) };
    }

    private string BaseUrl => AdminConfig.SupabaseUrl.TrimEnd('/');
    private string AnonKey => AdminConfig.SupabaseAnonKey;

    // ---------------- الوسيط admin-db ----------------

    private async Task<(int status, string body)> ProxyAsync(object payload)
    {
        string json = JsonConvert.SerializeObject(payload);
        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/functions/v1/admin-db");
        req.Headers.TryAddWithoutValidation("apikey", AnonKey);
        req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {AnonKey}");
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req);
        string content = await res.Content.ReadAsStringAsync();
        return ((int)res.StatusCode, content);
    }

    private async Task<string> ProxyOrThrow(object payload, string what)
    {
        var (status, body) = await ProxyAsync(payload);
        if (status is < 200 or >= 300)
        {
            string msg = $"{what} فشل ({status})";
            try { msg = JObject.Parse(body)["error"]?.ToString() ?? msg; } catch { }
            throw new SupabaseException(msg, body);
        }
        return body;
    }

    // ---------------- العمليات العامة ----------------

    /// <summary>SELECT — يرجّع مصفوفة صفوف.</summary>
    public async Task<JArray> SelectAsync(string table, string query = "select=*")
    {
        string body = await ProxyOrThrow(new { token = AppSession.AccessToken, op = "select", table, query }, $"قراءة {table}");
        return JArray.Parse(string.IsNullOrWhiteSpace(body) ? "[]" : body);
    }

    /// <summary>INSERT — يرجّع الصف المُنشأ.</summary>
    public async Task<JObject> InsertAsync(string table, object row)
    {
        string body = await ProxyOrThrow(new
        {
            token = AppSession.AccessToken,
            op = "insert",
            table,
            payload = row,
            prefer = "return=representation"
        }, $"إضافة في {table}");
        var arr = JArray.Parse(body);
        return (JObject)arr.First!;
    }

    /// <summary>UPDATE — على صفوف مطابقة للفلتر (مثال: "id=eq.{guid}").</summary>
    public async Task<JArray> UpdateAsync(string table, string filter, object changes)
    {
        string body = await ProxyOrThrow(new
        {
            token = AppSession.AccessToken,
            op = "update",
            table,
            filter,
            payload = changes,
            prefer = "return=representation"
        }, $"تعديل {table}");
        return JArray.Parse(string.IsNullOrWhiteSpace(body) ? "[]" : body);
    }

    /// <summary>DELETE — على صفوف مطابقة للفلتر.</summary>
    public async Task DeleteAsync(string table, string filter)
    {
        await ProxyOrThrow(new { token = AppSession.AccessToken, op = "delete", table, filter }, $"حذف من {table}");
    }

    /// <summary>استدعاء دالة RPC مسموح بها عبر الوسيط.</summary>
    public async Task<JToken> RpcAsync(string fn, object args)
    {
        string body = await ProxyOrThrow(new { token = AppSession.AccessToken, op = "rpc", fn, args }, $"تنفيذ {fn}");
        return JToken.Parse(string.IsNullOrWhiteSpace(body) ? "null" : body);
    }

    // ---------------- استدعاء Edge Function مباشر (لتسجيل الدخول) ----------------

    public async Task<JToken> FunctionAsync(string fn, object payload)
    {
        string body = JsonConvert.SerializeObject(payload);
        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/functions/v1/{fn}");
        req.Headers.TryAddWithoutValidation("apikey", AnonKey);
        req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {AnonKey}");
        req.Content = new StringContent(body, Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req);
        string content = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            throw new SupabaseException($"Function {fn} فشل: {res.StatusCode}", content);
        return JToken.Parse(string.IsNullOrWhiteSpace(content) ? "null" : content);
    }
}

public class SupabaseException : Exception
{
    public string Details { get; }
    public SupabaseException(string message, string details) : base(message)
    {
        Details = details;
    }
}
