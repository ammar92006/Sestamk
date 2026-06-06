using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Octokit;
using SestamkAdmin.Models;

namespace SestamkAdmin.Services;

// نماذج الـ Manifest (متوافقة مع SestamkPublisher و UpdateManager في برنامج العميل)
public class UpdateManifest
{
    public string Version { get; set; } = "";
    public string PreviousVersion { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public string Channel { get; set; } = "beta";
    public bool IsMandatory { get; set; }
    public string MinVersionToUpdate { get; set; } = "0.0.0";
    public List<string> WhatsNew { get; set; } = new();
    public List<FileChange> Changes { get; set; } = new();
    public long TotalSizeBytes { get; set; }
    public long FullVersionSizeBytes { get; set; }
    public long DownloadSizeBytes { get; set; }
    public string? FullVersionUrl { get; set; }
}

public class FileChange
{
    public string OriginalName { get; set; } = "";
    public string TargetPath { get; set; } = "";
    public string Hash { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Url { get; set; } = "";
    public string Action { get; set; } = "add";
}

public static class UpdateService
{
    public static async Task<List<UpdateRow>> GetHistoryAsync()
    {
        var arr = await SupabaseService.Instance.SelectAsync("updates",
            "select=*&order=created_at.desc&limit=50");
        return arr.Select(UpdateRow.From).ToList();
    }

    public static async Task<List<string>> GetChannelsAsync()
    {
        try
        {
            var arr = await SupabaseService.Instance.SelectAsync("update_channels",
                "select=channel_name&is_active=eq.true&order=channel_name.asc");
            var list = arr.Select(c => c["channel_name"]?.ToString() ?? "").Where(s => s.Length > 0).ToList();
            if (list.Count > 0) return list;
        }
        catch { }
        return new List<string> { "public", "beta", "vip", "stable" };
    }

    /// <summary>
    /// ينشر إصدارًا جديدًا: يحسب الهاش، يقارن بالإصدار السابق، ينشئ GitHub Release،
    /// يرفع الملفات المتغيّرة + manifest.json، ثم يسجّل صفًا في جدول updates.
    /// </summary>
    public static async Task PublishAsync(
        string version, string previousVersion, string channel,
        bool isMandatory, List<string> whatsNew, Action<string> log)
    {
        string buildFolder = AdminConfig.BuildFolderPath;
        string owner = AdminConfig.GitHubOwner;
        string repo = AdminConfig.GitHubRepo;
        string token = AdminConfig.GitHubToken;

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("توكن GitHub غير مُعد. أضفه من الإعدادات.");
        if (!Directory.Exists(buildFolder))
            throw new DirectoryNotFoundException($"مجلد الإصدار غير موجود:\n{buildFolder}");

        log($"📂 فحص الملفات في: {buildFolder}");
        var newFiles = new List<FileChange>();
        string[] allFiles = Directory.GetFiles(buildFolder, "*.*", SearchOption.AllDirectories);
        foreach (string file in allFiles)
        {
            string hash = Sha256(file);
            var fi = new FileInfo(file);
            string rel = file.Substring(buildFolder.Length + 1).Replace("\\", "/");
            newFiles.Add(new FileChange
            {
                OriginalName = Path.GetFileName(file),
                TargetPath = rel,
                Hash = hash,
                SizeBytes = fi.Length,
                Url = $"https://github.com/{owner}/{repo}/releases/download/v{version}/{hash}{fi.Extension}",
                Action = "add"
            });
        }
        long installedSize = newFiles.Sum(f => f.SizeBytes);
        log($"✔ عدد الملفات: {newFiles.Count} — الحجم: {installedSize / 1024.0 / 1024:F2} MB");

        log("🔄 جلب manifest الإصدار السابق...");
        var oldManifest = await GetOldManifestAsync(owner, repo);

        var filesToUpload = new List<FileChange>();
        if (oldManifest == null)
        {
            log("لا يوجد إصدار سابق — سيتم رفع كل الملفات.");
            filesToUpload = newFiles;
        }
        else
        {
            foreach (var nf in newFiles)
            {
                var old = oldManifest.Changes.FirstOrDefault(f => f.TargetPath == nf.TargetPath);
                if (old == null) { nf.Action = "add"; filesToUpload.Add(nf); }
                else if (old.Hash != nf.Hash) { nf.Action = "replace"; filesToUpload.Add(nf); }
                else { nf.Action = "replace"; nf.Url = old.Url; } // لم يتغيّر
            }
        }
        long deltaSize = filesToUpload.Sum(f => f.SizeBytes);
        log($"📦 ملفات للرفع (Delta): {filesToUpload.Count} — {deltaSize / 1024.0 / 1024:F2} MB");

        log("🌐 الاتصال بـ GitHub...");
        var github = new GitHubClient(new ProductHeaderValue("SestamkAdmin"))
        {
            Credentials = new Credentials(token)
        };

        var newRelease = new NewRelease("v" + version)
        {
            Name = $"تحديث Sestamk الإصدار {version}",
            Body = whatsNew.Count > 0 ? "- " + string.Join("\n- ", whatsNew) : "تحديث جديد",
            Draft = false,
            Prerelease = channel != "public"
        };
        var release = await github.Repository.Release.Create(owner, repo, newRelease);
        log($"✔ تم إنشاء Release: {release.HtmlUrl}");

        foreach (var f in filesToUpload)
        {
            string localPath = Path.Combine(buildFolder, f.TargetPath.Replace("/", "\\"));
            string safeName = f.Hash + Path.GetExtension(f.OriginalName);
            log($"⬆ رفع {f.OriginalName} ...");
            using var stream = File.OpenRead(localPath);
            await github.Repository.Release.UploadAsset(release,
                new ReleaseAssetUpload(safeName, "application/octet-stream", stream, null));
        }

        log("📝 إنشاء ورفع manifest.json...");
        var manifest = new UpdateManifest
        {
            Version = version,
            PreviousVersion = previousVersion,
            Channel = channel,
            ReleaseDate = DateTime.Now.ToString("yyyy-MM-dd"),
            MinVersionToUpdate = "0.0.0",
            IsMandatory = isMandatory,
            TotalSizeBytes = deltaSize,
            FullVersionSizeBytes = installedSize,
            DownloadSizeBytes = installedSize,
            Changes = newFiles,
            WhatsNew = whatsNew
        };
        string manifestJson = JsonConvert.SerializeObject(manifest, Formatting.Indented);
        string tmp = Path.Combine(Path.GetTempPath(), "manifest.json");
        await File.WriteAllTextAsync(tmp, manifestJson);
        using (var ms = File.OpenRead(tmp))
            await github.Repository.Release.UploadAsset(release,
                new ReleaseAssetUpload("manifest.json", "application/json", ms, null));

        string manifestUrl = $"https://github.com/{owner}/{repo}/releases/download/v{version}/manifest.json";

        log("🗄 تسجيل الإصدار في قاعدة البيانات...");
        await SupabaseService.Instance.InsertAsync("updates", new
        {
            version,
            previous_version = previousVersion,
            channel,
            title = $"إصدار {version}",
            title_ar = $"إصدار {version}",
            whats_new = whatsNew.ToArray(),
            is_mandatory = isMandatory,
            min_version_to_update = "0.0.0",
            manifest_url = manifestUrl,
            delta_size_bytes = deltaSize,
            full_size_bytes = installedSize,
            is_active = true,
            release_date = DateTime.Now.ToString("yyyy-MM-dd"),
            created_by = AppSession.AdminId
        });

        await AuditLog.WriteAsync("update", null, "publish", $"نشر تحديث {version} على قناة {channel}");
        log($"🎉 تم نشر التحديث {version} بنجاح على قناة {channel}!");
    }

    public static async Task SetActiveAsync(string id, string version, bool active)
    {
        await SupabaseService.Instance.UpdateAsync("updates", $"id=eq.{id}", new { is_active = active });
        await AuditLog.WriteAsync("update", id, active ? "enable" : "disable",
            $"{(active ? "تفعيل" : "إيقاف")} إصدار {version}");
    }

    private static async Task<UpdateManifest?> GetOldManifestAsync(string owner, string repo)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SestamkAdmin");
            string url = $"https://github.com/{owner}/{repo}/releases/latest/download/manifest.json";
            string json = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<UpdateManifest>(json);
        }
        catch { return null; }
    }

    private static string Sha256(string filePath)
    {
        using var sha = SHA256.Create();
        using var fs = File.OpenRead(filePath);
        return BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
    }
}
