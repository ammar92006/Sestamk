using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    // ═══════════════════════════════════════════════════════════════════
    //  WhatsAppWorkerService (V2)
    //  • Leader-elected background processing loop
    //  • Anti-Ban strategies: Random Jitter (1.5-2.5s) & Batch Pauses
    //  • Rescues stuck messages automatically
    //  • Graceful stop via CancellationToken
    // ═══════════════════════════════════════════════════════════════════
    public static class WhatsAppWorkerService
    {
        private static CancellationTokenSource? _cts;
        private static Task?                    _workerTask;
        private static bool                     _isRunning;
        private static readonly Random         _random = new Random();

        // ── Worker Metrics (read by UI) ───────────────────────────────────
        public static int     MessagesProcessed   { get; private set; }
        public static int     MessagesFailed      { get; private set; }
        public static int     QueueDepth          { get; private set; }
        public static DateTime LastProcessedAt    { get; private set; } = DateTime.MinValue;
        public static WhatsAppStatus CurrentStatus { get; private set; } = WhatsAppStatus.Unknown;

        // Event for UI updates
        public static event Action<WhatsAppStatus>? StatusChanged;
        public static event Action<int>?            QueueDepthChanged;

        // ── Config (from SettingsService) ─────────────────────────────────
        private static int MaxParallelism => SettingsService.GetInt("WhatsApp_MaxParallel", 1); // Prefer 1 for safer Anti-Ban pacing
        private static int MaxRetries     => SettingsService.GetInt("WhatsApp_MaxRetries", 5);
        private static int PollIntervalMs => 500;
        private static int HealthIntervalMs => 5_000; 

        // ── Start ─────────────────────────────────────────────────────────
        public static void Start()
        {
            if (_isRunning) return;

            _cts        = new CancellationTokenSource();
            _workerTask = Task.Run(() => WorkerLoopAsync(_cts.Token));
            _isRunning  = true;

            WhatsAppLogger.Info("Worker", "Background worker started", Guid.Empty);
        }

        // ── Stop (graceful) ───────────────────────────────────────────────
        public static async Task StopAsync()
        {
            if (!_isRunning || _cts == null) return;

            _cts.Cancel();
            if (_workerTask != null)
            {
                try { await Task.WhenAny(_workerTask, Task.Delay(5_000)); }
                catch { /* ignore */ }
            }
            _isRunning = false;
            WhatsAppLogger.Info("Worker", "Background worker stopped", Guid.Empty);
        }

        // ── Main Worker Loop ──────────────────────────────────────────────
        private static async Task WorkerLoopAsync(CancellationToken ct)
        {
            var semaphore       = new SemaphoreSlim(MaxParallelism, MaxParallelism);
            var lastHealthCheck = DateTime.MinValue;
            var logPurgeDay     = DateTime.Today;

            int messagesSentThisBatch = 0;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // ── 1. Leader Election Check ──────────────────────────────
                    // Try to acquire or renew lease for 30 seconds
                    bool isLeader = await WhatsAppQueueManager.AcquireOrRenewLeaseAsync(30);
                    if (!isLeader)
                    {
                        // Not the leader, sleep longer before checking again
                        await Task.Delay(5000, ct);
                        continue;
                    }

                    // ── 2. Rescue Stuck Messages ──────────────────────────────
                    await WhatsAppQueueManager.RescueStuckMessagesAsync();

                    // ── 3. Health check (every 5 s) ───────────────────────────
                    if ((DateTime.UtcNow - lastHealthCheck).TotalMilliseconds >= HealthIntervalMs)
                    {
                        lastHealthCheck = DateTime.UtcNow;
                        await UpdateStatusAsync(ct);
                    }

                    // ── 4. Queue depth update ─────────────────────────────────
                    var (pending, processing, _, _) = await WhatsAppQueueManager.GetQueueStatsAsync();
                    QueueDepth = pending + processing;
                    QueueDepthChanged?.Invoke(QueueDepth);

                    // ── 5. Only process if WhatsApp is connected ──────────────
                    if (CurrentStatus != WhatsAppStatus.Connected)
                    {
                        await Task.Delay(PollIntervalMs, ct);
                        continue;
                    }

                    // ── 6. Anti-Ban Batch Pause Check ─────────────────────────
                    if (messagesSentThisBatch >= 20)
                    {
                        WhatsAppLogger.Info("Worker", "Anti-Ban: Pausing for 30 seconds after sending 20 messages.", Guid.Empty);
                        await Task.Delay(TimeSpan.FromSeconds(30), ct);
                        messagesSentThisBatch = 0;
                        continue;
                    }

                    // ── 7. Dequeue a batch (Atomic) ───────────────────────────
                    int toFetch = Math.Min(MaxParallelism, 20 - messagesSentThisBatch);
                    if (toFetch <= 0) toFetch = 1;

                    var batch = await WhatsAppQueueManager.DequeueBatchAsync(toFetch);

                    if (batch.Count == 0)
                    {
                        await Task.Delay(PollIntervalMs, ct);
                        continue;
                    }

                    foreach (var msg in batch)
                    {
                        if (ct.IsCancellationRequested) break;

                        // Split-brain protection: Check lease right before processing
                        if (!await WhatsAppQueueManager.IsStillLeaderAsync())
                            break;

                        await semaphore.WaitAsync(ct);

                        // Fire-and-forget each message dispatch
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await ProcessMessageAsync(msg, ct);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, ct);

                        messagesSentThisBatch++;
                    }

                    // ── 8. Daily log purge ────────────────────────────────────
                    if (DateTime.Today > logPurgeDay)
                    {
                        logPurgeDay = DateTime.Today;
                        WhatsAppLogger.PurgeOldLogs(30);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    WhatsAppLogger.Error("Worker", $"Unhandled loop error: {ex.Message}", Guid.Empty, ex);
                }

                await Task.Delay(PollIntervalMs, ct);
            }
        }

        // ── Process single message ────────────────────────────────────────
        private static async Task ProcessMessageAsync(WhatsAppMessage msg, CancellationToken ct)
        {
            WhatsAppLogger.Info("Worker", $"Processing msg {msg.MessageId} → {msg.Phone}", msg.CorrelationId);

            // Anti-ban: Random Jitter (1.5s to 2.5s delay before sending)
            int jitterMs = _random.Next(1500, 2500);
            try { await Task.Delay(jitterMs, ct); } catch { return; }

            bool   sendMedia = SettingsService.GetBool("WhatsApp_SendPDF", false)
                            && msg.MediaContent != null;

            (bool success, string? error) result = sendMedia
                ? await WhatsAppService.SendWithMediaAsync(msg, ct)
                : await WhatsAppService.SendTextAsync(msg, ct);

            if (result.success)
            {
                await WhatsAppQueueManager.MarkSentAsync(msg.Id);
                MessagesProcessed++;
                LastProcessedAt = DateTime.UtcNow;
                WhatsAppLogger.Info("Worker", $"✅ Sent to {msg.Phone}", msg.CorrelationId);
            }
            else
            {
                MessagesFailed++;
                WhatsAppLogger.Warn("Worker", $"❌ Failed for {msg.Phone}: {result.error}", msg.CorrelationId);
                await WhatsAppQueueManager.HandleFailureAsync(msg, result.error ?? "Unknown", MaxRetries);
            }
        }

        // ── Health/Status Update ──────────────────────────────────────────
        private static DateTime _lastBridgeRestartAttempt = DateTime.MinValue;
        private static readonly TimeSpan BridgeRestartCooldown = TimeSpan.FromMinutes(2);

        private static async Task UpdateStatusAsync(CancellationToken ct)
        {
            var (ok, rawStatus, _) = await WhatsAppService.HealthCheckAsync(ct);

            WhatsAppStatus newStatus;
            if (!ok)
            {
                newStatus = WhatsAppStatus.ServiceDown;

                // Auto-restart the bridge if it's been down long enough (cooldown = 2 min)
                if (WhatsAppService.IsEnabled &&
                    DateTime.UtcNow - _lastBridgeRestartAttempt > BridgeRestartCooldown)
                {
                    _lastBridgeRestartAttempt = DateTime.UtcNow;
                    WhatsAppLogger.Info("Worker", "Bridge appears down — attempting auto-restart", Guid.Empty);
                    _ = Task.Run(() => WhatsAppService.EnsureBridgeRunningAsync(), ct);
                }
            }
            else
            {
                newStatus = rawStatus switch
                {
                    "CONNECTED"    => WhatsAppStatus.Connected,
                    "QR_READY"     => WhatsAppStatus.QrReady,
                    "INITIALIZING" => WhatsAppStatus.Connecting,
                    "DISCONNECTED" => WhatsAppStatus.Disconnected,
                    "AUTH_FAILURE" => WhatsAppStatus.AuthFailure,
                    _              => WhatsAppStatus.Unknown
                };
            }

            if (newStatus != CurrentStatus)
            {
                CurrentStatus = newStatus;
                StatusChanged?.Invoke(newStatus);
                WhatsAppLogger.Info("Worker", $"Status changed → {newStatus}", Guid.Empty);
            }
        }
    }
}
