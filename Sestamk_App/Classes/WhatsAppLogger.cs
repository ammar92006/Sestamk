using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Sestamk.Classes
{
    // ═══════════════════════════════════════════════════════════════════
    //  WhatsAppLogger — Structured JSON logger for the WhatsApp system
    //  Writes to: Data\Logs\whatsapp_YYYY-MM-DD.log (rolling daily)
    //  Each line = one JSON object for easy parsing
    // ═══════════════════════════════════════════════════════════════════
    public static class WhatsAppLogger
    {
        private static readonly string _logDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Data", "Logs");

        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        static WhatsAppLogger()
        {
            Directory.CreateDirectory(_logDir);
        }

        private static string LogFilePath =>
            Path.Combine(_logDir, $"whatsapp_{DateTime.Now:yyyy-MM-dd}.log");

        // ── Public API ───────────────────────────────────────────────────
        public static void Info(string component, string message, Guid correlationId)
            => Write("INFO", component, message, correlationId, null);

        public static void Warn(string component, string message, Guid correlationId)
            => Write("WARN", component, message, correlationId, null);

        public static void Error(string component, string message, Guid correlationId, Exception? ex = null)
            => Write("ERROR", component, message, correlationId, ex);

        // ── Writer ───────────────────────────────────────────────────────
        private static void Write(string level, string component, string message,
                                   Guid correlationId, Exception? ex)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("{");
                sb.Append($"\"ts\":\"{DateTime.UtcNow:o}\",");
                sb.Append($"\"level\":\"{level}\",");
                sb.Append($"\"component\":\"{Escape(component)}\",");
                sb.Append($"\"correlationId\":\"{correlationId}\",");
                sb.Append($"\"msg\":\"{Escape(message)}\"");
                if (ex != null)
                {
                    sb.Append($",\"error\":\"{Escape(ex.Message)}\"");
                    sb.Append($",\"stack\":\"{Escape(ex.StackTrace ?? "")}\"");
                }
                sb.Append("}");

                string line = sb.ToString();

                // Thread-safe file write (fire-and-forget async via lock)
                _fileLock.Wait();
                try
                {
                    File.AppendAllText(LogFilePath, line + Environment.NewLine, Encoding.UTF8);
                }
                finally
                {
                    _fileLock.Release();
                }

                // Also echo to debug console in development
                System.Diagnostics.Debug.WriteLine($"[WA-{level}] [{component}] {message}");
            }
            catch
            {
                // Logger must never throw
            }
        }

        private static string Escape(string s) =>
            s?.Replace("\\", "\\\\").Replace("\"", "\\\"")
               .Replace("\r", "\\r").Replace("\n", "\\n") ?? "";

        // ── Log retention cleanup (call weekly from worker) ──────────────
        public static void PurgeOldLogs(int retentionDays = 30)
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-retentionDays);
                foreach (var file in Directory.GetFiles(_logDir, "whatsapp_*.log"))
                {
                    if (File.GetLastWriteTime(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch { /* ignore cleanup errors */ }
        }
    }
}
