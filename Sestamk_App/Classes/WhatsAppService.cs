using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace Sestamk.Classes
{
    public enum WhatsAppStatus
    {
        Unknown,
        Connecting,
        QrReady,
        Connected,
        Disconnected,
        ServiceDown,
        AuthFailure
    }

    // ═══════════════════════════════════════════════════════════════════
    //  WhatsAppService (V2)
    //  • AES encryption with Environment Variable Master Key
    //  • Idempotency key headers to prevent duplicates
    //  • Network-safe media handling (VARBINARY arrays)
    // ═══════════════════════════════════════════════════════════════════
    public static class WhatsAppService
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15) // Increased timeout for file uploads
        };

        private static string?  _jwtToken;
        private static DateTime _tokenExpiry = DateTime.MinValue;
        private static readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

        private static int      _consecutiveFailures = 0;
        private static DateTime _circuitOpenUntil    = DateTime.MinValue;
        private const  int      CircuitOpenThreshold = 10;
        private static readonly TimeSpan CircuitOpenDuration = TimeSpan.FromMinutes(2);

        public static string BaseUrl    => SettingsService.GetString("WhatsApp_ServerUrl", "http://127.0.0.1:3000");
        public static bool   IsEnabled  => SettingsService.GetBool("WhatsApp_Enabled", false);

        // ── Security (AES) ────────────────────────────────────────────────
        private static readonly byte[] DefaultFallbackKey = Encoding.UTF8.GetBytes("SestamkSecureDefaultKey_32bytes!");

        private static byte[] GetMasterKey()
        {
            // Separation of concerns: Key from OS environment, encrypted data in DB
            string? envKey = Environment.GetEnvironmentVariable("SESTAMK_MASTER_KEY", EnvironmentVariableTarget.Machine) 
                          ?? Environment.GetEnvironmentVariable("SESTAMK_MASTER_KEY", EnvironmentVariableTarget.User);
            
            if (!string.IsNullOrEmpty(envKey))
            {
                byte[] key = Encoding.UTF8.GetBytes(envKey);
                if (key.Length >= 32) return key.Take(32).ToArray();
                var padded = new byte[32];
                Array.Copy(key, padded, key.Length);
                return padded;
            }
            return DefaultFallbackKey;
        }

        private static string GetSecret()
        {
            string encrypted = SettingsService.GetString("WhatsApp_ApiSecretEncrypted", "");
            if (string.IsNullOrEmpty(encrypted))
            {
                string legacy = SettingsService.GetString("WhatsApp_ApiSecret", "");
                WhatsAppLogger.Info("Service", $"Using legacy secret (encrypted empty). Legacy length: {legacy.Length}", Guid.Empty);
                return legacy; // Legacy fallback
            }
            WhatsAppLogger.Info("Service", $"Found encrypted secret, length: {encrypted.Length}", Guid.Empty);

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(encrypted);
                using var aes = Aes.Create();
                aes.Key = GetMasterKey();
                
                var iv = new byte[16];
                Array.Copy(cipherBytes, 0, iv, 0, 16);
                aes.IV = iv;

                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 16, cipherBytes.Length - 16);
                }
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Security", "Failed to decrypt API secret — clearing corrupted value", Guid.Empty, ex);
                // Clear the bad value so it gets re-saved correctly on next startup
                _ = SettingsService.SetAsync("WhatsApp_ApiSecretEncrypted", "");
                // Reset cached token so a fresh auth attempt will be made
                _jwtToken = null;
                return SettingsService.GetString("WhatsApp_ApiSecret", "");
            }
        }

        public static async Task SaveSecretAsync(string secret)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = GetMasterKey();
                aes.GenerateIV();

                byte[] plainBytes = Encoding.UTF8.GetBytes(secret);
                using var ms = new MemoryStream();
                ms.Write(aes.IV, 0, 16); 
                using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(plainBytes, 0, plainBytes.Length);
                }

                string b64 = Convert.ToBase64String(ms.ToArray());
                await SettingsService.SetAsync("WhatsApp_ApiSecretEncrypted", b64);
                await SettingsService.SetAsync("WhatsApp_ApiSecret", ""); 
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Security", "Failed to encrypt API secret", Guid.Empty, ex);
            }
        }

        // ── JWT Token ─────────────────────────────────────────────────────
        private static async Task<string?> GetValidTokenAsync(CancellationToken ct = default)
        {
            await _tokenLock.WaitAsync(ct);
            try
            {
                if (_jwtToken != null && DateTime.UtcNow < _tokenExpiry.AddSeconds(-30))
                    return _jwtToken;

                string secret = GetSecret();
                if (string.IsNullOrEmpty(secret))
                {
                    WhatsAppLogger.Warn("Service", "API secret is not configured", Guid.Empty);
                    return null;
                }
                WhatsAppLogger.Info("Service", $"GetValidTokenAsync: Secret retrieved (length={secret.Length}). Sending to /auth endpoint at {BaseUrl}/auth", Guid.Empty);

                var payload = new { secret };
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                var response = await _http.PostAsync($"{BaseUrl}/auth", content, ct);
                string responseBody = await response.Content.ReadAsStringAsync(ct);
                if (!response.IsSuccessStatusCode)
                {
                    WhatsAppLogger.Error("Service", $"JWT request failed: {response.StatusCode}. Response: {responseBody}", Guid.Empty);
                    return null;
                }
                WhatsAppLogger.Info("Service", "GetValidTokenAsync: JWT token obtained successfully", Guid.Empty);

                var json = JObject.Parse(responseBody);

                _jwtToken    = json["token"]?.ToString();
                int expiresIn = json["expiresIn"]?.Value<int>() ?? 900;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn);

                return _jwtToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        // ── Circuit Breaker ───────────────────────────────────────────────
        private static bool IsCircuitOpen()
        {
            if (_consecutiveFailures < CircuitOpenThreshold) return false;
            if (DateTime.UtcNow < _circuitOpenUntil) return true;
            _consecutiveFailures = 0;
            return false;
        }

        private static void RecordSuccess() => _consecutiveFailures = 0;

        private static void RecordFailure()
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= CircuitOpenThreshold)
                _circuitOpenUntil = DateTime.UtcNow.Add(CircuitOpenDuration);
        }

        // ── Auto-Start Bridge ──────────────────────────────────────────────
        private static Process? _bridgeProcess;
        private static readonly SemaphoreSlim _bridgeLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Gracefully shuts down the Node.js bridge process. Call this on application exit.
        /// Works even if _bridgeProcess is null (finds the process via .lock file).
        /// </summary>
        public static async Task ShutdownBridgeAsync()
        {
            _jwtToken = null;

            // 1. Kill the process we directly spawned (if any)
            if (_bridgeProcess != null && !_bridgeProcess.HasExited)
            {
                try
                {
                    WhatsAppLogger.Info("Bridge", $"ShutdownBridge: Killing tracked bridge PID {_bridgeProcess.Id}", Guid.Empty);
                    _bridgeProcess.Kill(entireProcessTree: true);
                    await Task.Run(() => _bridgeProcess.WaitForExit(5000));
                }
                catch (Exception ex)
                {
                    WhatsAppLogger.Warn("Bridge", $"ShutdownBridge: Error killing tracked process: {ex.Message}", Guid.Empty);
                }
                finally
                {
                    _bridgeProcess.Dispose();
                    _bridgeProcess = null;
                }
            }

            // 2. Also kill any Node process that holds our .lock file
            //    (covers the case where bridge was already running before this app session started)
            foreach (var p in _serviceSearchPaths)
            {
                try
                {
                    string dir = Path.GetFullPath(p);
                    string lockFile = Path.Combine(dir, ".lock");
                    if (!File.Exists(lockFile)) continue;

                    string raw = File.ReadAllText(lockFile).Trim();
                    if (!int.TryParse(raw, out int lockedPid)) continue;

                    try
                    {
                        using var proc = Process.GetProcessById(lockedPid);
                        if (!proc.HasExited)
                        {
                            proc.Kill(entireProcessTree: true);
                            WhatsAppLogger.Info("Bridge", $"ShutdownBridge: Killed lock-held node PID {lockedPid}", Guid.Empty);
                        }
                    }
                    catch { /* process already gone */ }

                    break; // found the service dir — stop searching
                }
                catch { }
            }

            // 3. Clean up lock/port files so next launch starts cleanly
            foreach (var p in _serviceSearchPaths)
            {
                try
                {
                    string dir = Path.GetFullPath(p);
                    string lockFile = Path.Combine(dir, ".lock");
                    string portFile = Path.Combine(dir, ".port");
                    if (File.Exists(lockFile)) File.Delete(lockFile);
                    if (File.Exists(portFile)) File.Delete(portFile);
                }
                catch { }
            }

            WhatsAppLogger.Info("Bridge", "ShutdownBridge: Done.", Guid.Empty);
        }

        // ── Known paths for the whatsapp-service directory ────────────────
        private static readonly string[] _serviceSearchPaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "whatsapp-service"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "whatsapp-service"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "whatsapp-service"),
            @"E:\Sestamk\whatsapp-service"
        };

        /// <summary>
        /// Reads the .port file written by Node.js and updates WhatsApp_ServerUrl if the port changed.
        /// Node.js picks a fallback port if 3000 is already in use.
        /// </summary>
        private static async Task RefreshServerUrlFromPortFileAsync()
        {
            foreach (var p in _serviceSearchPaths)
            {
                string portFile = Path.Combine(Path.GetFullPath(p), ".port");
                if (!File.Exists(portFile)) continue;
                try
                {
                    string portStr = File.ReadAllText(portFile).Trim();
                    if (int.TryParse(portStr, out int dynamicPort))
                    {
                        string newUrl = $"http://127.0.0.1:{dynamicPort}";
                        if (BaseUrl != newUrl)
                        {
                            await SettingsService.SetAsync("WhatsApp_ServerUrl", newUrl);
                            WhatsAppLogger.Info("Bridge", $"Server URL updated to {newUrl} (read from .port file)", Guid.Empty);
                        }
                    }
                }
                catch { /* ignore read errors */ }
                break;
            }
        }

        /// <summary>
        /// Kills any orphaned node.exe processes whose working directory matches the service path.
        /// This prevents multiple WhatsApp clients sharing the same session folder.
        /// </summary>
        private static void KillOrphanedNodeProcesses(string servicePath)
        {
            try
            {
                string normalizedService = Path.GetFullPath(servicePath).TrimEnd('\\', '/').ToLowerInvariant();

                foreach (var proc in Process.GetProcessesByName("node"))
                {
                    // Skip the process we ourselves started in this session
                    if (_bridgeProcess != null && proc.Id == _bridgeProcess.Id) continue;

                    try
                    {
                        // On Windows we can't easily read process working directory without P/Invoke,
                        // so we kill any node process that has the lock file registered to its PID.
                        string lockFile = Path.Combine(servicePath, ".lock");
                        if (File.Exists(lockFile))
                        {
                            string raw = File.ReadAllText(lockFile).Trim();
                            if (int.TryParse(raw, out int lockedPid) && lockedPid == proc.Id)
                            {
                                proc.Kill(entireProcessTree: true);
                                WhatsAppLogger.Info("Bridge", $"Killed orphaned node process PID {proc.Id} (held .lock file)", Guid.Empty);
                                File.Delete(lockFile);
                                break;
                            }
                        }
                    }
                    catch { /* ignore — process may have already exited */ }
                    finally { proc.Dispose(); }
                }
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Warn("Bridge", $"KillOrphanedNodeProcesses error (non-fatal): {ex.Message}", Guid.Empty);
            }
        }

        /// <summary>
        /// يتأكد إن سيرفر الواتساب (Node.js) شغال — لو مش شغال يشغّله تلقائياً
        /// </summary>
        public static async Task<bool> EnsureBridgeRunningAsync()
        {
            if (!await _bridgeLock.WaitAsync(0))
            {
                await _bridgeLock.WaitAsync();
                _bridgeLock.Release();
                await RefreshServerUrlFromPortFileAsync();
                var (running, _, _) = await HealthCheckAsync();
                return running;
            }

            try
            {
            // 0. Check .port file first — Node.js may have chosen a different port
            await RefreshServerUrlFromPortFileAsync();

            // 1. تحقق لو السيرفر شغال بالفعل ومش DISCONNECTED
            var (ok, currentStatus, _) = await HealthCheckAsync();
            if (ok && currentStatus != "DISCONNECTED") return true;
            // If DISCONNECTED, fall through to start a fresh bridge

            // 2. ابحث عن مسار whatsapp-service
            string[] possiblePaths = _serviceSearchPaths;

            string? servicePath = null;
            foreach (var p in possiblePaths)
            {
                string full = Path.GetFullPath(p);
                if (File.Exists(Path.Combine(full, "index.js")))
                {
                    servicePath = full;
                    break;
                }
            }

            if (servicePath == null)
            {
                WhatsAppLogger.Error("Bridge", "whatsapp-service/index.js not found in any expected path", Guid.Empty);
                return false;
            }

            // 3. Kill any orphaned node processes that might be using our session folder
            //    (prevents "Failed to link device" caused by multiple WhatsApp clients)
            KillOrphanedNodeProcesses(servicePath);

            // 4. شغّل السيرفر (بدون نافذة)
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "node",
                    Arguments = "index.js",
                    WorkingDirectory = servicePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };
                _bridgeProcess = Process.Start(psi);
                WhatsAppLogger.Info("Bridge", $"Node.js bridge started (PID: {_bridgeProcess?.Id}) from {servicePath}", Guid.Empty);
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Bridge", $"Failed to start Node.js: {ex.Message}", Guid.Empty, ex);
                return false;
            }

            // 5. انتظر حتى يستجيب (أقصى 20 ثانية)، وحدّث المنفذ من .port عند كل محاولة
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(500);
                await RefreshServerUrlFromPortFileAsync();
                var (alive, _, _) = await HealthCheckAsync();
                if (alive) return true;
            }

            WhatsAppLogger.Error("Bridge", "Node.js started but health check timed out after 20s", Guid.Empty);
            return false;
            }
            finally
            {
                _bridgeLock.Release();
            }
        }

        // ── API Methods ───────────────────────────────────────────────────
        public static async Task<(bool ok, string status, string? me)> HealthCheckAsync(CancellationToken ct = default)
        {
            try
            {
                var resp = await _http.GetAsync($"{BaseUrl}/health", ct);
                if (!resp.IsSuccessStatusCode) return (false, "ServiceError", null);

                string body = await resp.Content.ReadAsStringAsync(ct);
                var json = JObject.Parse(body);
                return (true, json["waStatus"]?.ToString() ?? "UNKNOWN", json["me"]?.ToString());
            }
            catch
            {
                return (false, "ServiceDown", null);
            }
        }

        public static async Task<string?> GetQrBase64Async(CancellationToken ct = default)
        {
            try
            {
                string? token = await GetValidTokenAsync(ct);
                if (token == null)
                {
                    WhatsAppLogger.Warn("Service", "GetQrBase64Async: Token is null - auth failed", Guid.Empty);
                    return null;
                }

                var req = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/qr");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var resp = await _http.SendAsync(req, ct);

                string body = await resp.Content.ReadAsStringAsync(ct);
                if (!resp.IsSuccessStatusCode)
                {
                    WhatsAppLogger.Error("Service", $"GetQrBase64Async: /qr returned {resp.StatusCode}: {body}", Guid.Empty);
                    return null;
                }

                var json = JObject.Parse(body);
                string? qrBase64 = json["qr"]?.ToString();
                if (string.IsNullOrEmpty(qrBase64))
                {
                    WhatsAppLogger.Warn("Service", "GetQrBase64Async: QR returned but value is empty", Guid.Empty);
                }
                else
                {
                    WhatsAppLogger.Info("Service", "GetQrBase64Async: QR code retrieved successfully", Guid.Empty);
                }
                return qrBase64;
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Service", $"GetQrBase64Async exception: {ex.Message}", Guid.Empty, ex);
                return null;
            }
        }

        public static async Task<(bool success, string? error)> SendTextAsync(WhatsAppMessage msg, CancellationToken ct = default)
        {
            if (IsCircuitOpen()) return (false, "Circuit breaker open — service unreachable");

            try
            {
                string? token = await GetValidTokenAsync(ct);
                if (token == null) return (false, "JWT token unavailable");

                var payload = new
                {
                    phone         = msg.Phone,
                    message       = msg.Text,
                    messageId     = msg.MessageId.ToString(),
                    correlationId = msg.CorrelationId.ToString()
                };

                var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/send")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Headers.Add("X-Idempotency-Key",    msg.MessageId.ToString());
                req.Headers.Add("X-Correlation-Id",     msg.CorrelationId.ToString());

                var resp = await _http.SendAsync(req, ct);
                string body = await resp.Content.ReadAsStringAsync(ct);

                if (resp.IsSuccessStatusCode)
                {
                    RecordSuccess();
                    return (true, null);
                }

                RecordFailure();
                return (false, JObject.Parse(body)["error"]?.ToString() ?? $"HTTP {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                RecordFailure();
                return (false, ex.Message);
            }
        }

        public static async Task<(bool success, string? error)> SendWithMediaAsync(WhatsAppMessage msg, CancellationToken ct = default)
        {
            if (IsCircuitOpen()) return (false, "Circuit breaker open — service unreachable");

            if (msg.MediaContent == null || msg.MediaContent.Length == 0)
            {
                WhatsAppLogger.Warn("Service", $"Media bytes not found for {msg.MessageId} — falling back to text", msg.CorrelationId);
                return await SendTextAsync(msg, ct);
            }

            try
            {
                string? token = await GetValidTokenAsync(ct);
                if (token == null) return (false, "JWT token unavailable");

                using var form = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(msg.MediaContent);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(msg.MediaMimeType ?? "application/octet-stream");

                string fileName = msg.MediaFileName ?? "document.pdf";
                form.Add(fileContent, "media", fileName);
                form.Add(new StringContent(msg.Phone),                   "phone");
                form.Add(new StringContent(msg.Text),                    "message");
                form.Add(new StringContent(msg.MessageId.ToString()),    "messageId");
                form.Add(new StringContent(msg.CorrelationId.ToString()),"correlationId");

                var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/send")
                {
                    Content = form
                };
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Headers.Add("X-Idempotency-Key", msg.MessageId.ToString());
                req.Headers.Add("X-Correlation-Id",  msg.CorrelationId.ToString());

                var resp = await _http.SendAsync(req, ct);
                string body = await resp.Content.ReadAsStringAsync(ct);

                if (resp.IsSuccessStatusCode)
                {
                    RecordSuccess();
                    return (true, null);
                }

                RecordFailure();
                return (false, JObject.Parse(body)["error"]?.ToString() ?? $"HTTP {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                RecordFailure();
                return (false, ex.Message);
            }
        }

        public static async Task<bool> ResetSessionAsync(CancellationToken ct = default)
        {
            try
            {
                string? token = await GetValidTokenAsync(ct);
                if (token == null) return false;

                var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/reset");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                req.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var resp = await _http.SendAsync(req, ct);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
