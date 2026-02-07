using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MyWinFormsApp.Classes
{
    public class UpdateManager
    {
        private readonly string githubOwner;
        private readonly string githubRepo;
        private readonly string currentVersion;

        public UpdateManager(string owner, string repo, string version)
        {
            githubOwner = owner;
            githubRepo = repo;
            currentVersion = version;
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                string apiUrl = $"https://api.github.com/repos/{githubOwner}/{githubRepo}/releases/latest";

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "MyApp-Updater");
                    client.Timeout = TimeSpan.FromSeconds(30);

                    string json = await client.GetStringAsync(apiUrl);
                    GitHubRelease release = JsonConvert.DeserializeObject<GitHubRelease>(json);

                    GitHubAsset zipAsset = release.assets.Find(a => a.name.EndsWith(".zip"));

                    if (zipAsset == null)
                        throw new Exception("لم يتم العثور على ملف ZIP");

                    bool updateAvailable = CompareVersions(currentVersion, release.tag_name);

                    return new UpdateInfo
                    {
                        UpdateAvailable = updateAvailable,
                        CurrentVersion = currentVersion,
                        LatestVersion = release.tag_name,
                        DownloadUrl = zipAsset.browser_download_url,
                        ChangeLog = release.body ?? "لا توجد ملاحظات",
                        FileSize = zipAsset.size,
                        FileName = zipAsset.name,
                        IsPrerelease = release.prerelease
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ: {ex.Message}");
            }
        }

        private bool CompareVersions(string current, string latest)
        {
            try
            {
                string cleanCurrent = current.TrimStart('v');
                string cleanLatest = latest.TrimStart('v');

                Version currentVer = new Version(cleanCurrent);
                Version latestVer = new Version(cleanLatest);

                return latestVer > currentVer;
            }
            catch
            {
                return string.Compare(latest, current, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }

        public async Task<string> DownloadUpdateAsync(string downloadUrl, IProgress<int> progress)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "AppUpdate.zip");

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(30);

                    using (HttpResponseMessage response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        long? totalBytes = response.Content.Headers.ContentLength;

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        using (FileStream fileStream = new FileStream(tempPath, FileMode.Create))
                        {
                            byte[] buffer = new byte[8192];
                            long totalRead = 0;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;

                                if (totalBytes.HasValue)
                                {
                                    int percentage = (int)((totalRead * 100) / totalBytes.Value);
                                    progress?.Report(percentage);
                                }
                            }
                        }
                    }
                }

                return tempPath;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);

                throw new Exception($"فشل التحميل: {ex.Message}");
            }
        }

        public void ApplyUpdate(string updateFilePath)
        {
            try
            {
                string updaterPath = Path.Combine(Application.StartupPath, "Updater.exe");

                if (!File.Exists(updaterPath))
                    throw new FileNotFoundException("Updater.exe غير موجود");

                var updateConfig = new
                {
                    UpdateFile = updateFilePath,
                    TargetPath = Application.StartupPath,
                    MainExecutable = Application.ExecutablePath,
                    ProcessId = Process.GetCurrentProcess().Id
                };

                string configPath = Path.Combine(Path.GetTempPath(), "update_config.json");
                File.WriteAllText(configPath, JsonConvert.SerializeObject(updateConfig));

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    Arguments = $"\"{configPath}\"",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Application.Exit();
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل التحديث: {ex.Message}");
            }
        }

        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}