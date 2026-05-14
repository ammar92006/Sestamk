using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    /// <summary>
    /// خدمة المرتجعات — البحث عن الفواتير، حساب الكميات المتاحة، وحفظ المرتجع.
    ///
    /// منطق الحسابات:
    ///   كمية المتاحة للإرجاع = الكمية الأصلية - مجموع ما تم إرجاعه مسبقاً
    ///   إجمالي المرتجع = مجموع (UnitPrice × ReturnQuantity) لكل صنف
    ///   لا يتم احتساب ضريبة على المرتجع — يُرجع نفس سعر البيع بالضبط
    /// </summary>
    public static class ReturnService
    {
        // ═══════════════════════════════════════════════════════
        //  1. البحث عن فاتورة بالرقم
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// يبحث عن فاتورة برقمها أو بجزء منه ويُعيد معلوماتها الأساسية.
        /// يستبعد الفواتير الملغاة تلقائياً.
        /// </summary>
        public static async Task<DataRow?> SearchOrderAsync(string orderNumber)
        {
            string query = @"
                SELECT
                    o.OrderID,
                    o.OrderNumber,
                    o.OrderDate,
                    o.TotalAmount,
                    o.PaidAmount,
                    o.RemainingAmount,
                    o.Status,
                    o.IsVoided,
                    o.CustomerID,
                    ISNULL(c.CustomerName, 'عميل نقدي') AS CustomerName,
                    u.FullName AS CashierName,
                    ISNULL(o.HasReturn, 0) AS HasReturn
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Users u ON o.UserID = u.UserID
                WHERE o.OrderNumber = @OrderNumber
                  AND ISNULL(o.IsVoided, 0) = 0";

            var dt = await DB_Server.GetTableAsync(query, new[]
            {
                new SqlParameter("@OrderNumber", orderNumber.Trim())
            });

            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        // ═══════════════════════════════════════════════════════
        //  2. تحميل أصناف الفاتورة مع الكميات المتاحة للإرجاع
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// يُعيد أصناف الفاتورة مع حساب الكمية المتاحة للإرجاع لكل صنف.
        /// الكمية المتاحة = الكمية الأصلية - مجموع ما أُرجع من هذا الصنف مسبقاً.
        /// </summary>
        public static async Task<List<ReturnItemModel>> GetOrderItemsForReturnAsync(int orderId)
        {
            string query = @"
                SELECT
                    oi.OrderItemID,
                    oi.ProductID,
                    oi.ProductSizeID,
                    oi.ProductName,
                    oi.SizeName,
                    oi.UnitPrice,
                    oi.Quantity                                     AS OriginalQuantity,
                    ISNULL(
                        (SELECT SUM(ri.ReturnQuantity)
                         FROM ReturnItems ri
                         WHERE ri.OrderItemID = oi.OrderItemID), 0) AS AlreadyReturned
                FROM OrderItems oi
                WHERE oi.OrderID = @OrderID
                  AND ISNULL(oi.IsVoided, 0) = 0
                ORDER BY oi.OrderItemID";

            var dt = await DB_Server.GetTableAsync(query, new[]
            {
                new SqlParameter("@OrderID", orderId)
            });

            var items = new List<ReturnItemModel>();
            foreach (DataRow row in dt.Rows)
            {
                int originalQty = Convert.ToInt32(row["OriginalQuantity"]);
                int alreadyReturned = Convert.ToInt32(row["AlreadyReturned"]);

                // تجاهل الأصناف التي تم إرجاع كامل كميتها
                if (originalQty - alreadyReturned <= 0) continue;

                items.Add(new ReturnItemModel
                {
                    OrderItemID = Convert.ToInt32(row["OrderItemID"]),
                    ProductID = Convert.ToInt32(row["ProductID"]),
                    ProductSizeID = row["ProductSizeID"] != DBNull.Value
                        ? Convert.ToInt32(row["ProductSizeID"])
                        : null,
                    ProductName = row["ProductName"].ToString()!,
                    SizeName = row["SizeName"]?.ToString() ?? "",
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    OriginalQuantity = originalQty,
                    AlreadyReturnedQuantity = alreadyReturned,
                    ReturnQuantity = originalQty - alreadyReturned,  // افتراضي: إرجاع الكمية المتاحة
                    IsSelected = false
                });
            }

            return items;
        }

        // ═══════════════════════════════════════════════════════
        //  3. حفظ المرتجع — العملية الكاملة في تحويل واحد
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// يحفظ المرتجع بشكل كامل في تحويل واحد (ACID):
        ///   خطوة 0: توليد رقم المرتجع أتوماتيكياً
        ///   خطوة 1: INSERT رأس المرتجع في Returns
        ///   خطوة 2: INSERT تفاصيل الأصناف في ReturnItems
        ///   خطوة 3: إعادة المخزون للأصناف المرتجعة
        ///   خطوة 4: تحديث رصيد العميل (إذا كانت طريقة الاسترداد رصيد آجل)
        ///   خطوة 5: تحديث إحصائيات الوردية
        ///   خطوة 6: تعليم الفاتورة الأصلية بوجود مرتجع (HasReturn = 1)
        /// </summary>
        public static async Task<int> SaveReturnAsync(ReturnModel ret)
        {
            int returnId = 0;

            await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                // ═══ 0. توليد رقم المرتجع ═══
                ret.ReturnNumber = await GenerateReturnNumberAsync(conn, trans);

                // ═══ 1. INSERT رأس المرتجع ═══
                string insertReturn = @"
                    INSERT INTO Returns
                    (ReturnNumber, OriginalOrderID, OriginalOrderNumber, ReturnDate,
                     CustomerID, UserID, ShiftID, ReturnReason,
                     TotalReturnAmount, RefundMethod, Notes, CreatedDate)
                    VALUES
                    (@ReturnNumber, @OriginalOrderID, @OriginalOrderNumber, @ReturnDate,
                     @CustomerID, @UserID, @ShiftID, @ReturnReason,
                     @TotalReturnAmount, @RefundMethod, @Notes, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(insertReturn, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@ReturnNumber", ret.ReturnNumber);
                    cmd.Parameters.AddWithValue("@OriginalOrderID", ret.OriginalOrderID);
                    cmd.Parameters.AddWithValue("@OriginalOrderNumber", ret.OriginalOrderNumber);
                    cmd.Parameters.AddWithValue("@ReturnDate", ret.ReturnDate);
                    cmd.Parameters.AddWithValue("@CustomerID",
                        ret.CustomerID.HasValue ? (object)ret.CustomerID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserID", ret.UserID);
                    cmd.Parameters.AddWithValue("@ShiftID",
                        ret.ShiftID > 0 ? (object)ret.ShiftID : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReturnReason", ret.ReturnReason ?? "");
                    cmd.Parameters.AddWithValue("@TotalReturnAmount", ret.TotalReturnAmount);
                    cmd.Parameters.AddWithValue("@RefundMethod", ret.RefundMethod);
                    cmd.Parameters.AddWithValue("@Notes", ret.Notes ?? "");

                    object result = await cmd.ExecuteScalarAsync();
                    returnId = Convert.ToInt32(result);
                    ret.ReturnID = returnId;
                }

                // ═══ 2. INSERT تفاصيل الأصناف ═══
                foreach (var item in ret.Items)
                {
                    if (item.ReturnQuantity <= 0) continue;

                    string insertItem = @"
                        INSERT INTO ReturnItems
                        (ReturnID, OrderItemID, ProductID, ProductSizeID,
                         ProductName, SizeName, UnitPrice, ReturnQuantity, LineTotal)
                        VALUES
                        (@ReturnID, @OrderItemID, @ProductID, @ProductSizeID,
                         @ProductName, @SizeName, @UnitPrice, @ReturnQuantity, @LineTotal)";

                    using (var cmd = new SqlCommand(insertItem, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@ReturnID", returnId);
                        cmd.Parameters.AddWithValue("@OrderItemID", item.OrderItemID);
                        cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                        cmd.Parameters.AddWithValue("@ProductSizeID",
                            item.ProductSizeID.HasValue ? (object)item.ProductSizeID.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmd.Parameters.AddWithValue("@SizeName", item.SizeName ?? "");
                        cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@ReturnQuantity", item.ReturnQuantity);
                        cmd.Parameters.AddWithValue("@LineTotal", item.LineTotal);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // ═══ 3. إعادة المخزون ═══
                    await InventoryService.ReturnStockForItemAsync(
                        conn, trans,
                        item.ProductID, item.ProductSizeID,
                        item.ReturnQuantity,
                        returnId, ret.UserID);
                }

                // ═══ 4. تحديث رصيد العميل (رصيد آجل فقط) ═══
                if (ret.RefundMethod == 2 && ret.CustomerID.HasValue && ret.TotalReturnAmount > 0)
                {
                    // طريقة الاسترداد = رصيد آجل → نخفض ما يدين به العميل
                    string updateBalance = @"
                        UPDATE Customers
                        SET CurrentBalance = CurrentBalance - @Amount,
                            LastTransactionDate = GETDATE()
                        WHERE CustomerID = @CustomerID";

                    using (var cmd = new SqlCommand(updateBalance, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Amount", ret.TotalReturnAmount);
                        cmd.Parameters.AddWithValue("@CustomerID", ret.CustomerID.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // تسجيل حركة في سجل العميل
                    string insertTrans = @"
                        INSERT INTO CustomerTransactions
                        (CustomerID, OrderID, TransactionType, Amount, BalanceAfter, CreatedByUserID, Notes)
                        SELECT @CustomerID, NULL, 3, @Amount,
                               CurrentBalance, @UserID, @Notes
                        FROM Customers WHERE CustomerID = @CustomerID";

                    using (var cmd = new SqlCommand(insertTrans, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", ret.CustomerID.Value);
                        cmd.Parameters.AddWithValue("@Amount", ret.TotalReturnAmount);
                        cmd.Parameters.AddWithValue("@UserID", ret.UserID);
                        cmd.Parameters.AddWithValue("@Notes",
                            $"مرتجع فاتورة {ret.OriginalOrderNumber} — رقم {ret.ReturnNumber}");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // ═══ 5. تحديث إحصائيات الوردية ═══
                if (ret.ShiftID > 0)
                {
                    string updateShift = @"
                        UPDATE Shifts
                        SET TotalRefunds = ISNULL(TotalRefunds, 0) + @Amount
                        WHERE ShiftID = @ShiftID AND Status = 0";

                    using (var cmd = new SqlCommand(updateShift, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Amount", ret.TotalReturnAmount);
                        cmd.Parameters.AddWithValue("@ShiftID", ret.ShiftID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // ═══ 6. تعليم الفاتورة الأصلية بوجود مرتجع ═══
                string markOrder = @"
                    UPDATE Orders SET HasReturn = 1
                    WHERE OrderID = @OrderID";

                using (var cmd = new SqlCommand(markOrder, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@OrderID", ret.OriginalOrderID);
                    await cmd.ExecuteNonQueryAsync();
                }
            });

            return returnId;
        }

        // ═══════════════════════════════════════════════════════
        //  4. استعلامات التقارير
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// يُعيد جدول المرتجعات في نطاق زمني محدد — للتقارير
        /// </summary>
        public static async Task<DataTable> GetReturnsReportAsync(
            DateTime dateFrom, DateTime dateTo,
            int? userId = null, int? customerId = null)
        {
            var conditions = new List<string>
            {
                "CAST(r.ReturnDate AS DATE) >= @DateFrom",
                "CAST(r.ReturnDate AS DATE) <= @DateTo"
            };
            var parameters = new List<SqlParameter>
            {
                new("@DateFrom", dateFrom.Date),
                new("@DateTo", dateTo.Date)
            };

            if (userId.HasValue)
            {
                conditions.Add("r.UserID = @UserID");
                parameters.Add(new("@UserID", userId.Value));
            }
            if (customerId.HasValue)
            {
                conditions.Add("r.CustomerID = @CustomerID");
                parameters.Add(new("@CustomerID", customerId.Value));
            }

            string where = "WHERE " + string.Join(" AND ", conditions);

            string query = $@"
                SELECT
                    r.ReturnID,
                    r.ReturnNumber                                      AS [رقم المرتجع],
                    CONVERT(NVARCHAR(16), r.ReturnDate, 120)            AS [تاريخ المرتجع],
                    r.OriginalOrderNumber                               AS [رقم الفاتورة],
                    ISNULL(c.CustomerName, 'عميل نقدي')                AS [العميل],
                    r.TotalReturnAmount                                 AS [إجمالي المرتجع],
                    CASE r.RefundMethod
                        WHEN 0 THEN 'نقدي'
                        WHEN 1 THEN 'بطاقة'
                        WHEN 2 THEN 'رصيد آجل'
                        ELSE 'غير محدد'
                    END                                                  AS [طريقة الاسترداد],
                    r.ReturnReason                                      AS [سبب الإرجاع],
                    u.FullName                                           AS [الكاشير]
                FROM Returns r
                LEFT JOIN Customers c ON r.CustomerID = c.CustomerID
                LEFT JOIN Users u ON r.UserID = u.UserID
                {where}
                ORDER BY r.ReturnDate DESC";

            return await DB_Server.GetTableAsync(query, parameters.ToArray());
        }

        /// <summary>
        /// ملخص المرتجعات لبطاقة التقرير
        /// </summary>
        public static async Task<ReturnsSummary> GetReturnsSummaryAsync(DateTime dateFrom, DateTime dateTo)
        {
            string query = @"
                SELECT
                    COUNT(*)                                              AS TotalReturns,
                    ISNULL(SUM(r.TotalReturnAmount), 0)                  AS TotalRefundAmount,
                    ISNULL((SELECT SUM(ri.ReturnQuantity) FROM ReturnItems ri
                            INNER JOIN Returns r2 ON ri.ReturnID = r2.ReturnID
                            WHERE CAST(r2.ReturnDate AS DATE) BETWEEN @DateFrom AND @DateTo), 0)
                                                                          AS TotalReturnedItems,
                    ISNULL(SUM(CASE WHEN r.RefundMethod = 0 THEN r.TotalReturnAmount ELSE 0 END), 0) AS CashRefunds,
                    ISNULL(SUM(CASE WHEN r.RefundMethod = 1 THEN r.TotalReturnAmount ELSE 0 END), 0) AS CardRefunds,
                    ISNULL(SUM(CASE WHEN r.RefundMethod = 2 THEN r.TotalReturnAmount ELSE 0 END), 0) AS CreditRefunds
                FROM Returns r
                WHERE CAST(r.ReturnDate AS DATE) BETWEEN @DateFrom AND @DateTo";

            var dt = await DB_Server.GetTableAsync(query, new[]
            {
                new SqlParameter("@DateFrom", dateFrom.Date),
                new SqlParameter("@DateTo", dateTo.Date)
            });

            if (dt.Rows.Count == 0) return new ReturnsSummary();

            var row = dt.Rows[0];
            return new ReturnsSummary
            {
                TotalReturns = Convert.ToInt32(row["TotalReturns"]),
                TotalRefundAmount = Convert.ToDecimal(row["TotalRefundAmount"]),
                TotalReturnedItems = Convert.ToInt32(row["TotalReturnedItems"]),
                CashRefunds = Convert.ToDecimal(row["CashRefunds"]),
                CardRefunds = Convert.ToDecimal(row["CardRefunds"]),
                CreditRefunds = Convert.ToDecimal(row["CreditRefunds"])
            };
        }

        // ═══════════════════════════════════════════════════════
        //  مساعد: توليد رقم المرتجع أتوماتيكياً داخل التحويل
        // ═══════════════════════════════════════════════════════

        private static async Task<string> GenerateReturnNumberAsync(
            SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                SELECT COUNT(*) + 1
                FROM Returns WITH (UPDLOCK, HOLDLOCK)
                WHERE CONVERT(DATE, ReturnDate) = CONVERT(DATE, GETDATE())";

            using var cmd = new SqlCommand(query, conn, trans);
            object result = await cmd.ExecuteScalarAsync();
            int seq = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;
            return $"RET-{DateTime.Now:yyyyMMdd}-{seq:D3}";
        }
    }
}
