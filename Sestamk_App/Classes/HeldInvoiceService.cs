using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Sestamk.Classes
{
    /// <summary>
    /// Replaces the static List&lt;HeldInvoice&gt; in frmSales with DB persistence.
    /// Held invoices survive app restarts and are visible across machines.
    ///
    /// Migration path for frmSales.cs:
    ///   BEFORE: _heldInvoices.Add(heldInvoice);
    ///   AFTER:  await HeldInvoiceService.HoldAsync(heldInvoice);
    ///
    ///   BEFORE: _heldInvoices (static list)
    ///   AFTER:  await HeldInvoiceService.GetActiveAsync()
    /// </summary>
    public static class HeldInvoiceService
    {
        /// <summary>
        /// Save a held invoice to the database.
        /// </summary>
        public static async Task<int> HoldAsync(HeldInvoiceDto invoice)
        {
            int heldId = 0;

            await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                string insertHeader = @"
                    INSERT INTO HeldInvoices
                    (InvoiceNumber, HoldTime, InvoiceType, CustomerID, CustomerName,
                     TableID, TableName, DriverID, DriverName, TotalAmount,
                     UserID, MachineName, Notes)
                    VALUES
                    (@Number, @HoldTime, @Type, @CustID, @CustName,
                     @TableID, @TableName, @DriverID, @DriverName, @Total,
                     @UserID, @Machine, @Notes);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(insertHeader, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@Number", invoice.InvoiceNumber);
                    cmd.Parameters.AddWithValue("@HoldTime", invoice.HoldTime);
                    cmd.Parameters.AddWithValue("@Type", invoice.InvoiceType);
                    cmd.Parameters.AddWithValue("@CustID", invoice.CustomerId > 0 ? invoice.CustomerId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustName", invoice.CustomerName ?? "عميل نقدي");
                    cmd.Parameters.AddWithValue("@TableID", invoice.TableId > 0 ? invoice.TableId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TableName", invoice.TableName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DriverID", invoice.DriverId > 0 ? invoice.DriverId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DriverName", invoice.DriverName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Total", invoice.TotalAmount);
                    cmd.Parameters.AddWithValue("@UserID", invoice.UserID);
                    cmd.Parameters.AddWithValue("@Machine", Environment.MachineName);
                    cmd.Parameters.AddWithValue("@Notes", invoice.Notes ?? (object)DBNull.Value);

                    object result = await cmd.ExecuteScalarAsync();
                    heldId = Convert.ToInt32(result);
                }

                foreach (var item in invoice.Items)
                {
                    string insertItem = @"
                        INSERT INTO HeldInvoiceItems
                        (HeldInvoiceID, ProductID, ProductSizeID, ItemName, SizeName,
                         UnitPrice, Quantity, Notes, AddonsJson)
                        VALUES
                        (@HeldID, @ProductID, @SizeID, @Name, @SizeName,
                         @Price, @Qty, @Notes, @Addons)";

                    using (var cmd = new SqlCommand(insertItem, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@HeldID", heldId);
                        cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                        cmd.Parameters.AddWithValue("@SizeID", item.ProductSizeID.HasValue ? item.ProductSizeID.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Name", item.ItemName);
                        cmd.Parameters.AddWithValue("@SizeName", item.SizeName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Price", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@Qty", item.Quantity);
                        cmd.Parameters.AddWithValue("@Notes", item.Notes ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Addons", item.AddonsJson ?? (object)DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            });

            return heldId;
        }

        /// <summary>
        /// Get all active (non-restored) held invoices with their items.
        /// </summary>
        public static async Task<List<HeldInvoiceDto>> GetActiveAsync()
        {
            var invoices = new List<HeldInvoiceDto>();

            string query = @"
                SELECT h.*, hi.*
                FROM HeldInvoices h
                LEFT JOIN HeldInvoiceItems hi ON h.HeldInvoiceID = hi.HeldInvoiceID
                WHERE h.IsRestored = 0
                ORDER BY h.HoldTime DESC, hi.HeldInvoiceItemID ASC";

            var dt = await DB_Server.GetTableAsync(query);
            if (dt.Rows.Count == 0) return invoices;

            HeldInvoiceDto? current = null;
            int lastId = -1;

            foreach (System.Data.DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["HeldInvoiceID"]);
                if (id != lastId)
                {
                    current = new HeldInvoiceDto
                    {
                        HeldInvoiceID = id,
                        InvoiceNumber = row["InvoiceNumber"]?.ToString() ?? "",
                        HoldTime = Convert.ToDateTime(row["HoldTime"]),
                        InvoiceType = Convert.ToInt32(row["InvoiceType"]),
                        CustomerId = row["CustomerID"] != DBNull.Value ? Convert.ToInt32(row["CustomerID"]) : 0,
                        CustomerName = row["CustomerName"]?.ToString() ?? "",
                        TableId = row["TableID"] != DBNull.Value ? Convert.ToInt64(row["TableID"]) : 0,
                        TableName = row["TableName"]?.ToString() ?? "",
                        DriverId = row["DriverID"] != DBNull.Value ? Convert.ToInt32(row["DriverID"]) : 0,
                        DriverName = row["DriverName"]?.ToString() ?? "",
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                        MachineName = row["MachineName"]?.ToString() ?? ""
                    };
                    invoices.Add(current);
                    lastId = id;
                }

                if (current != null && row["HeldInvoiceItemID"] != DBNull.Value)
                {
                    current.Items.Add(new HeldInvoiceItemDto
                    {
                        ProductID = Convert.ToInt32(row["ProductID"]),
                        ProductSizeID = row["ProductSizeID"] != DBNull.Value ? Convert.ToInt32(row["ProductSizeID"]) : null,
                        ItemName = row["ItemName"]?.ToString() ?? "",
                        SizeName = row["SizeName"]?.ToString(),
                        UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                        Quantity = Convert.ToInt32(row["Quantity"]),
                        AddonsJson = row["AddonsJson"]?.ToString()
                    });
                }
            }

            return invoices;
        }

        /// <summary>
        /// Mark a held invoice as restored (soft-delete — keeps audit trail).
        /// </summary>
        public static async Task RestoreAsync(int heldInvoiceId, int userId)
        {
            string query = @"
                UPDATE HeldInvoices
                SET IsRestored = 1, RestoredAt = GETDATE(), RestoredByUserID = @UserID
                WHERE HeldInvoiceID = @ID";

            await DB_Server.ExecuteAsync(query, new SqlParameter[]
            {
                new("@ID", heldInvoiceId),
                new("@UserID", userId)
            });
        }

        /// <summary>
        /// Permanently delete a held invoice (user explicitly discards it).
        /// </summary>
        public static async Task DeleteAsync(int heldInvoiceId)
        {
            // CASCADE delete handles items
            string query = "DELETE FROM HeldInvoices WHERE HeldInvoiceID = @ID";
            await DB_Server.ExecuteAsync(query, new SqlParameter[]
            {
                new("@ID", heldInvoiceId)
            });
        }

        /// <summary>
        /// Count active held invoices (for badge display).
        /// </summary>
        public static async Task<int> CountActiveAsync()
        {
            object? result = await DB_Server.ScalarAsync(
                "SELECT COUNT(*) FROM HeldInvoices WHERE IsRestored = 0");
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }

    // ═══ DTOs ═══

    public class HeldInvoiceDto
    {
        public int HeldInvoiceID { get; set; }
        public string InvoiceNumber { get; set; } = "";
        public DateTime HoldTime { get; set; } = DateTime.Now;
        public int InvoiceType { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "عميل نقدي";
        public long TableId { get; set; }
        public string TableName { get; set; } = "";
        public int DriverId { get; set; }
        public string DriverName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public int UserID { get; set; }
        public string MachineName { get; set; } = "";
        public string? Notes { get; set; }
        public List<HeldInvoiceItemDto> Items { get; set; } = new();

        public string InvoiceTypeText => InvoiceType switch
        {
            0 => "سفري",
            1 => "صالة",
            2 => "توصيل",
            _ => "غير محدد"
        };

        public string DisplayTitle
        {
            get
            {
                if (InvoiceType == 1 && !string.IsNullOrEmpty(TableName))
                    return $"طاولة {TableName}";
                if (InvoiceType == 2 && !string.IsNullOrEmpty(DriverName))
                    return $"توصيل - {DriverName}";
                if (InvoiceType == 2)
                    return "توصيل - طلب خارجي";
                if (InvoiceType == 0 && !string.IsNullOrEmpty(CustomerName) && CustomerName != "عميل نقدي")
                    return $"سفري - {CustomerName}";
                return "سفري";
            }
        }
    }

    public class HeldInvoiceItemDto
    {
        public int ProductID { get; set; }
        public int? ProductSizeID { get; set; }
        public string ItemName { get; set; } = "";
        public string? SizeName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }
        public string? AddonsJson { get; set; }
    }
}
