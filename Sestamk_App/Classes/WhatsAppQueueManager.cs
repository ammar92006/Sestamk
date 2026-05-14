using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Sestamk.Classes
{
    // ═══════════════════════════════════════════════════════════════════
    //  نموذج رسالة الواتساب في قاعدة بيانات القابض
    // ═══════════════════════════════════════════════════════════════════
    public class WhatsAppMessage
    {
        public long     Id            { get; set; }
        public Guid     MessageId     { get; set; } = Guid.NewGuid();
        public Guid     CorrelationId { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
        public string   Phone         { get; set; } = string.Empty;
        public string   CountryCode   { get; set; } = "+20";
        public string   Text          { get; set; } = string.Empty;
        
        public string?  MediaPath     { get; set; }
        public byte[]?  MediaContent  { get; set; }
        public string?  MediaMimeType { get; set; }
        public string?  MediaFileName { get; set; }
        
        public string   Status        { get; set; } = QueueStatus.Pending;
        public DateTime? ProcessingAt { get; set; }
        public int      RetryCount    { get; set; } = 0;
        public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
        public string?  LastError     { get; set; }
        public int      CreatedByUserId { get; set; }
        public int      OrderId       { get; set; }
    }

    public static class QueueStatus
    {
        public const string Pending    = "Pending";
        public const string Processing = "Processing";
        public const string Sent       = "Sent";
        public const string Failed     = "Failed";
    }

    // ═══════════════════════════════════════════════════════════════════
    //  WhatsAppQueueManager
    //  • Persistent SQL Server queue (zero message loss)
    //  • Leader Election via DB leases
    //  • Atomic dequeuing with UPDLOCK, READPAST
    // ═══════════════════════════════════════════════════════════════════
    public static class WhatsAppQueueManager
    {
        private static readonly System.Threading.SemaphoreSlim _semaphore =
            new System.Threading.SemaphoreSlim(1, 1);
        private static readonly Random _random = new Random();

        public static string CurrentNodeId { get; } = Environment.MachineName + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);

        // ── Initialise (called once at startup) ──────────────────────────
        public static async Task InitialiseAsync()
        {
            try
            {
                // Node Registry (Leader Election)
                string createRegistry = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhatsAppNodeRegistry]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[WhatsAppNodeRegistry] (
                            [NodeId] NVARCHAR(100) PRIMARY KEY,
                            [LastHeartbeat] DATETIME2 NOT NULL,
                            [IsLeader] BIT NOT NULL DEFAULT 0,
                            [LeaseExpiry] DATETIME2 NOT NULL
                        );
                    END";
                await DB_Server.ExecuteAsync(createRegistry);

                // Outbox table
                string createOutbox = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhatsAppOutbox]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[WhatsAppOutbox] (
                            [Id]              BIGINT IDENTITY(1,1) PRIMARY KEY,
                            [MessageId]       UNIQUEIDENTIFIER NOT NULL UNIQUE,
                            [CorrelationId]   UNIQUEIDENTIFIER NOT NULL,
                            [CreatedAt]       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [Phone]           NVARCHAR(50) NOT NULL,
                            [CountryCode]     NVARCHAR(10) NOT NULL DEFAULT '+20',
                            [Text]            NVARCHAR(MAX) NOT NULL,
                            [MediaPath]       NVARCHAR(MAX),
                            [MediaContent]    VARBINARY(MAX) NULL,
                            [MediaMimeType]   NVARCHAR(50) NULL,
                            [MediaFileName]   NVARCHAR(255) NULL,
                            [Status]          NVARCHAR(20) NOT NULL DEFAULT 'Pending',
                            [ProcessingAt]    DATETIME2 NULL,
                            [RetryCount]      INT NOT NULL DEFAULT 0,
                            [NextAttemptAt]   DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [LastError]       NVARCHAR(MAX),
                            [CreatedByUserId] INT NOT NULL DEFAULT 0,
                            [OrderId]         INT NOT NULL DEFAULT 0
                        );
                        CREATE INDEX [idx_outbox_status_next] ON [dbo].[WhatsAppOutbox]([Status], [NextAttemptAt]);
                    END
                    ELSE
                    BEGIN
                        -- Add new columns if missing (V2 upgrade)
                        IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'ProcessingAt' AND Object_ID = Object_ID(N'dbo.WhatsAppOutbox'))
                            ALTER TABLE [dbo].[WhatsAppOutbox] ADD [ProcessingAt] DATETIME2 NULL;
                        IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'CountryCode' AND Object_ID = Object_ID(N'dbo.WhatsAppOutbox'))
                            ALTER TABLE [dbo].[WhatsAppOutbox] ADD [CountryCode] NVARCHAR(10) NOT NULL DEFAULT '+20';
                        IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'MediaContent' AND Object_ID = Object_ID(N'dbo.WhatsAppOutbox'))
                        BEGIN
                            ALTER TABLE [dbo].[WhatsAppOutbox] ADD [MediaContent] VARBINARY(MAX) NULL;
                            ALTER TABLE [dbo].[WhatsAppOutbox] ADD [MediaMimeType] NVARCHAR(50) NULL;
                            ALTER TABLE [dbo].[WhatsAppOutbox] ADD [MediaFileName] NVARCHAR(255) NULL;
                        END
                    END";
                await DB_Server.ExecuteAsync(createOutbox);

                // Dead-letter queue (DLQ)
                string createDLQ = @"
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WhatsAppDLQ]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[WhatsAppDLQ] (
                            [Id]              BIGINT IDENTITY(1,1) PRIMARY KEY,
                            [MessageId]       UNIQUEIDENTIFIER NOT NULL,
                            [CorrelationId]   UNIQUEIDENTIFIER NOT NULL,
                            [CreatedAt]       DATETIME2 NOT NULL,
                            [FailedAt]        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                            [Phone]           NVARCHAR(50) NOT NULL,
                            [Text]            NVARCHAR(MAX) NOT NULL,
                            [MediaPath]       NVARCHAR(MAX),
                            [RetryCount]      INT NOT NULL,
                            [LastError]       NVARCHAR(MAX),
                            [CreatedByUserId] INT NOT NULL DEFAULT 0,
                            [OrderId]         INT NOT NULL DEFAULT 0
                        );
                    END";
                await DB_Server.ExecuteAsync(createDLQ);

                WhatsAppLogger.Info("Queue", "WhatsApp SQL Server queue initialised (V2)", Guid.Empty);
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Queue", $"Failed to initialise SQL Server queue: {ex.Message}", Guid.Empty, ex);
                throw;
            }
        }

        // ── Leader Election ────────────────────────────────────────────────
        public static async Task<bool> AcquireOrRenewLeaseAsync(int leaseDurationSeconds = 30)
        {
            try
            {
                string query = @"
                    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
                    BEGIN TRAN;
                    DECLARE @Now DATETIME2 = GETUTCDATE();
                    DECLARE @Expiry DATETIME2 = DATEADD(second, @Duration, @Now);

                    IF EXISTS (SELECT 1 FROM WhatsAppNodeRegistry WHERE NodeId = @NodeId)
                        UPDATE WhatsAppNodeRegistry SET LastHeartbeat = @Now WHERE NodeId = @NodeId;
                    ELSE
                        INSERT INTO WhatsAppNodeRegistry (NodeId, LastHeartbeat, IsLeader, LeaseExpiry) 
                        VALUES (@NodeId, @Now, 0, @Now);

                    IF EXISTS (SELECT 1 FROM WhatsAppNodeRegistry WHERE IsLeader = 1 AND NodeId <> @NodeId AND LeaseExpiry > @Now)
                    BEGIN
                        COMMIT;
                        SELECT 0;
                    END
                    ELSE
                    BEGIN
                        UPDATE WhatsAppNodeRegistry SET IsLeader = 0 WHERE IsLeader = 1 AND NodeId <> @NodeId;
                        UPDATE WhatsAppNodeRegistry SET IsLeader = 1, LeaseExpiry = @Expiry WHERE NodeId = @NodeId;
                        COMMIT;
                        SELECT 1;
                    END
                ";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@NodeId", CurrentNodeId),
                    new SqlParameter("@Duration", leaseDurationSeconds)
                };

                var dt = await DB_Server.GetTableAsync(query, parameters);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0]) == 1;
                }
                return false;
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Queue", $"Acquire lease failed: {ex.Message}", Guid.Empty, ex);
                return false;
            }
        }

        public static async Task<bool> IsStillLeaderAsync()
        {
            try
            {
                string query = "SELECT IsLeader FROM WhatsAppNodeRegistry WHERE NodeId = @NodeId AND LeaseExpiry > GETUTCDATE()";
                var parameters = new SqlParameter[] { new SqlParameter("@NodeId", CurrentNodeId) };
                var dt = await DB_Server.GetTableAsync(query, parameters);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return Convert.ToBoolean(dt.Rows[0]["IsLeader"]);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // ── Rescue Stuck Messages ──────────────────────────────────────────
        public static async Task<int> RescueStuckMessagesAsync()
        {
            try
            {
                string query = @"
                    UPDATE WhatsAppOutbox
                    SET Status = 'Pending', ProcessingAt = NULL
                    WHERE Status = 'Processing' 
                      AND ProcessingAt IS NOT NULL 
                      AND ProcessingAt < DATEADD(minute, -2, GETUTCDATE())";

                return await DB_Server.ExecuteAsync(query);
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Queue", $"Rescue stuck messages failed: {ex.Message}", Guid.Empty, ex);
                return 0;
            }
        }

        // ── Enqueue ──────────────────────────────────────────────────────
        public static async Task<Guid> EnqueueAsync(WhatsAppMessage msg)
        {
            if (string.IsNullOrEmpty(msg.CountryCode)) msg.CountryCode = SettingsService.GetString("WhatsApp_DefaultCountryCode", "+20");
            msg.Phone = WhatsAppHelper.FormatPhoneNumber(msg.Phone, msg.CountryCode);

            if (string.IsNullOrEmpty(msg.Phone))
            {
                WhatsAppLogger.Warn("Queue", "Rejected message: Phone number is empty after formatting", msg.CorrelationId);
                return Guid.Empty;
            }

            // Load media into VARBINARY to share across network
            if (!string.IsNullOrEmpty(msg.MediaPath) && System.IO.File.Exists(msg.MediaPath) && msg.MediaContent == null)
            {
                try
                {
                    msg.MediaContent = System.IO.File.ReadAllBytes(msg.MediaPath);
                    msg.MediaFileName = System.IO.Path.GetFileName(msg.MediaPath);
                    msg.MediaMimeType = msg.MediaPath.ToLower().EndsWith(".pdf") ? "application/pdf" : "image/png";
                }
                catch (Exception ex)
                {
                    WhatsAppLogger.Error("Queue", $"Failed to read media file: {ex.Message}", msg.CorrelationId, ex);
                }
            }

            await _semaphore.WaitAsync();
            try
            {
                string query = @"
                    INSERT INTO WhatsAppOutbox
                        (MessageId, CorrelationId, CreatedAt, Phone, CountryCode, Text, MediaPath, MediaContent, MediaMimeType, MediaFileName,
                         Status, RetryCount, NextAttemptAt, CreatedByUserId, OrderId)
                    VALUES
                        (@mid, @cid, GETUTCDATE(), @ph, @cc, @txt, @mp, @mcontent, @mmime, @mname,
                         'Pending', 0, GETUTCDATE(), @uid, @oid)";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@mid",  msg.MessageId),
                    new SqlParameter("@cid",  msg.CorrelationId),
                    new SqlParameter("@ph",   msg.Phone),
                    new SqlParameter("@cc",   msg.CountryCode),
                    new SqlParameter("@txt",  msg.Text),
                    new SqlParameter("@mp",   (object)msg.MediaPath ?? DBNull.Value),
                    new SqlParameter("@mcontent", (object)msg.MediaContent ?? DBNull.Value) { SqlDbType = SqlDbType.VarBinary },
                    new SqlParameter("@mmime", (object)msg.MediaMimeType ?? DBNull.Value),
                    new SqlParameter("@mname", (object)msg.MediaFileName ?? DBNull.Value),
                    new SqlParameter("@uid",  msg.CreatedByUserId),
                    new SqlParameter("@oid",  msg.OrderId)
                };

                await DB_Server.ExecuteAsync(query, parameters);

                WhatsAppLogger.Info("Queue", $"Enqueued message for {msg.Phone} (OrderId={msg.OrderId})", msg.CorrelationId);
                return msg.MessageId;
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("Queue", $"Enqueue failed: {ex.Message}", msg.CorrelationId, ex);
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ── Dequeue batch (Atomic) ───────────────────────────────────────
        public static async Task<List<WhatsAppMessage>> DequeueBatchAsync(int batchSize = 10)
        {
            string query = @"
                UPDATE TOP (@batch) WhatsAppOutbox WITH (UPDLOCK, READPAST)
                SET Status = 'Processing', ProcessingAt = GETUTCDATE()
                OUTPUT 
                    INSERTED.Id, INSERTED.MessageId, INSERTED.CorrelationId, INSERTED.CreatedAt, 
                    INSERTED.Phone, INSERTED.CountryCode, INSERTED.Text, INSERTED.MediaPath, 
                    INSERTED.MediaContent, INSERTED.MediaMimeType, INSERTED.MediaFileName,
                    INSERTED.Status, INSERTED.ProcessingAt, INSERTED.RetryCount, INSERTED.NextAttemptAt, 
                    INSERTED.LastError, INSERTED.CreatedByUserId, INSERTED.OrderId
                WHERE Status = 'Pending' AND NextAttemptAt <= GETUTCDATE()";

            var parameters = new SqlParameter[] { new SqlParameter("@batch", batchSize) };
            DataTable dt = await DB_Server.GetTableAsync(query, parameters);

            var list = new List<WhatsAppMessage>();
            if (dt == null) return list;

            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    list.Add(new WhatsAppMessage
                    {
                        Id              = (long)row["Id"],
                        MessageId       = (Guid)row["MessageId"],
                        CorrelationId   = (Guid)row["CorrelationId"],
                        CreatedAt       = (DateTime)row["CreatedAt"],
                        Phone           = row["Phone"].ToString() ?? "",
                        CountryCode     = row["CountryCode"] != DBNull.Value ? row["CountryCode"].ToString() ?? "+20" : "+20",
                        Text            = row["Text"].ToString() ?? "",
                        MediaPath       = row["MediaPath"] == DBNull.Value ? null : row["MediaPath"].ToString(),
                        MediaContent    = row["MediaContent"] == DBNull.Value ? null : (byte[])row["MediaContent"],
                        MediaMimeType   = row["MediaMimeType"] == DBNull.Value ? null : row["MediaMimeType"].ToString(),
                        MediaFileName   = row["MediaFileName"] == DBNull.Value ? null : row["MediaFileName"].ToString(),
                        Status          = row["Status"].ToString() ?? "",
                        ProcessingAt    = row["ProcessingAt"] == DBNull.Value ? (DateTime?)null : (DateTime)row["ProcessingAt"],
                        RetryCount      = (int)row["RetryCount"],
                        NextAttemptAt   = (DateTime)row["NextAttemptAt"],
                        LastError       = row["LastError"] == DBNull.Value ? null : row["LastError"].ToString(),
                        CreatedByUserId = (int)row["CreatedByUserId"],
                        OrderId         = (int)row["OrderId"]
                    });
                }
                catch (Exception ex)
                {
                    WhatsAppLogger.Error("Queue", $"Row parsing failed: {ex.Message}", Guid.Empty, ex);
                }
            }
            return list;
        }

        // ── Mark Sent ────────────────────────────────────────────────────
        public static async Task MarkSentAsync(long id)
        {
            // Nullify MediaContent to save space
            string query = "UPDATE WhatsAppOutbox SET Status = 'Sent', ProcessingAt = NULL, MediaContent = NULL WHERE Id = @id";
            var parameters = new SqlParameter[] { new SqlParameter("@id", id) };
            await DB_Server.ExecuteAsync(query, parameters);
        }

        // ── Handle Failure (Exponential Backoff) ──────────────────────────
        public static async Task HandleFailureAsync(WhatsAppMessage msg, string error, int maxRetries = 5)
        {
            int nextRetry = msg.RetryCount + 1;

            if (nextRetry >= maxRetries)
            {
                await MoveToDLQAsync(msg, error);
                return;
            }

            double jitter  = 1.0 + (_random.NextDouble() * 0.4 - 0.2);
            double delaySec = Math.Pow(2, nextRetry) * 2.0 * jitter;   
            var nextAt     = DateTime.UtcNow.AddSeconds(delaySec);

            string query = @"
                UPDATE WhatsAppOutbox
                SET Status        = 'Pending',
                    ProcessingAt  = NULL,
                    RetryCount    = @rc,
                    NextAttemptAt = @next,
                    LastError     = @err
                WHERE Id = @id";
            
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@rc",   nextRetry),
                new SqlParameter("@next", nextAt),
                new SqlParameter("@err",  (object)error ?? "Unknown error"),
                new SqlParameter("@id",   msg.Id)
            };
            await DB_Server.ExecuteAsync(query, parameters);

            WhatsAppLogger.Warn("Queue", $"Retry {nextRetry}/{maxRetries} scheduled in {delaySec:F0}s", msg.CorrelationId);
        }

        // ── Move to DLQ ──────────────────────────────────────────────────
        private static async Task MoveToDLQAsync(WhatsAppMessage msg, string error)
        {
            await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                // Insert into DLQ (WITHOUT MediaContent to save DB bloat)
                string insertQuery = @"
                    INSERT INTO WhatsAppDLQ
                        (MessageId, CorrelationId, CreatedAt, FailedAt, Phone, Text,
                         MediaPath, RetryCount, LastError, CreatedByUserId, OrderId)
                    VALUES
                        (@mid, @cid, @cat, GETUTCDATE(), @ph, @txt,
                         @mp, @rc, @err, @uid, @oid)";

                using (var ins = new SqlCommand(insertQuery, conn, trans))
                {
                    ins.Parameters.AddWithValue("@mid",  msg.MessageId);
                    ins.Parameters.AddWithValue("@cid",  msg.CorrelationId);
                    ins.Parameters.AddWithValue("@cat",  msg.CreatedAt);
                    ins.Parameters.AddWithValue("@ph",   msg.Phone);
                    ins.Parameters.AddWithValue("@txt",  msg.Text);
                    ins.Parameters.AddWithValue("@mp",   (object)msg.MediaPath ?? DBNull.Value);
                    ins.Parameters.AddWithValue("@rc",   msg.RetryCount + 1);
                    ins.Parameters.AddWithValue("@err",  (object)error ?? "Max retries exceeded");
                    ins.Parameters.AddWithValue("@uid",  msg.CreatedByUserId);
                    ins.Parameters.AddWithValue("@oid",  msg.OrderId);
                    await ins.ExecuteNonQueryAsync();
                }

                // Delete from Outbox
                string deleteQuery = "DELETE FROM WhatsAppOutbox WHERE Id = @id";
                using (var del = new SqlCommand(deleteQuery, conn, trans))
                {
                    del.Parameters.AddWithValue("@id", msg.Id);
                    await del.ExecuteNonQueryAsync();
                }

                WhatsAppLogger.Error("Queue", $"Moved to DLQ after {msg.RetryCount + 1} attempts. Error: {error}", msg.CorrelationId);
            });
        }

        // ── Queue Stats ───────────────────────────────────────────────────
        public static async Task<(int pending, int processing, int failed, int dlq)> GetQueueStatsAsync()
        {
            string query = @"
                SELECT
                    (SELECT COUNT(*) FROM WhatsAppOutbox WHERE Status = 'Pending') as PendingCount,
                    (SELECT COUNT(*) FROM WhatsAppOutbox WHERE Status = 'Processing') as ProcessingCount,
                    (SELECT COUNT(*) FROM WhatsAppOutbox WHERE Status = 'Failed') as FailedCount,
                    (SELECT COUNT(*) FROM WhatsAppDLQ) as DLQCount";

            DataTable dt = await DB_Server.GetTableAsync(query);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow r = dt.Rows[0];
                return ((int)r["PendingCount"], (int)r["ProcessingCount"], (int)r["FailedCount"], (int)r["DLQCount"]);
            }

            return (0, 0, 0, 0);
        }

        // ── Re-queue DLQ messages ─────────────────────────────────────────
        public static async Task<int> RequeueDLQAsync()
        {
            int requeued = 0;
            await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                string insertQuery = @"
                    INSERT INTO WhatsAppOutbox
                        (MessageId, CorrelationId, CreatedAt, Phone, CountryCode, Text, MediaPath, MediaContent, MediaMimeType, MediaFileName,
                         Status, RetryCount, NextAttemptAt, LastError, CreatedByUserId, OrderId)
                    SELECT
                        MessageId, CorrelationId, CreatedAt, Phone, '+20', Text, MediaPath, NULL, NULL, NULL,
                        'Pending', 0, GETUTCDATE(), NULL, CreatedByUserId, OrderId
                    FROM WhatsAppDLQ";

                using (var ins = new SqlCommand(insertQuery, conn, trans))
                {
                    requeued = await ins.ExecuteNonQueryAsync();
                }

                string deleteQuery = "DELETE FROM WhatsAppDLQ";
                using (var del = new SqlCommand(deleteQuery, conn, trans))
                {
                    await del.ExecuteNonQueryAsync();
                }

                WhatsAppLogger.Info("Queue", $"Re-queued {requeued} DLQ messages", Guid.Empty);
            });
            return requeued;
        }

        public static async Task<DataTable> GetDLQMessagesAsync()
        {
            string query = @"
                SELECT TOP 100 Id, Phone, Text, RetryCount, LastError, FailedAt, OrderId
                FROM WhatsAppDLQ
                ORDER BY FailedAt DESC";
            return await DB_Server.GetTableAsync(query);
        }
    }
}
