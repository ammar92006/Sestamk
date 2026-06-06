using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    /// <summary>
    /// خدمة تقارير المبيعات — تتبع نفس نمط OrderService و ShiftService
    /// كل الاستعلامات async عبر DB_Server.GetTableAsync
    /// </summary>
    public static class SalesReportService
    {
        // ═══════════════════════════════════════════════════════
        //  مساعد بناء شروط WHERE من الفلاتر
        // ═══════════════════════════════════════════════════════

        private static (string whereClause, List<SqlParameter> parameters) BuildWhereClause(
            ReportFilters filters, string orderAlias = "o")
        {
            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            // نطاق التاريخ
            conditions.Add($"CAST({orderAlias}.OrderDate AS DATE) >= @DateFrom");
            parameters.Add(new SqlParameter("@DateFrom", filters.DateFrom.Date));

            conditions.Add($"CAST({orderAlias}.OrderDate AS DATE) <= @DateTo");
            parameters.Add(new SqlParameter("@DateTo", filters.DateTo.Date));

            // استثناء الملغي
            if (filters.ExcludeVoided)
            {
                conditions.Add($"{orderAlias}.IsVoided = 0");
            }

            // نوع الطلب
            if (filters.OrderType.HasValue)
            {
                conditions.Add($"{orderAlias}.OrderType = @OrderType");
                parameters.Add(new SqlParameter("@OrderType", filters.OrderType.Value));
            }

            // المستخدم/الكاشير
            if (filters.UserID.HasValue)
            {
                conditions.Add($"{orderAlias}.UserID = @UserID");
                parameters.Add(new SqlParameter("@UserID", filters.UserID.Value));
            }

            // العميل
            if (filters.CustomerID.HasValue)
            {
                conditions.Add($"{orderAlias}.CustomerID = @CustomerID");
                parameters.Add(new SqlParameter("@CustomerID", filters.CustomerID.Value));
            }

            string whereClause = conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : "";

            return (whereClause, parameters);
        }

        // ═══════════════════════════════════════════════════════
        //  1. بطاقات الملخص (Summary Cards)
        // ═══════════════════════════════════════════════════════

        public static async Task<SalesSummary> GetSalesSummaryAsync(ReportFilters filters)
        {
            var summary = new SalesSummary();

            try
            {
                var (where, pars) = BuildWhereClause(filters);

                string query = $@"
                    SELECT 
                        ISNULL(SUM(o.TotalAmount), 0) AS TotalSales,
                        COUNT(*) AS TotalOrders,
                        ISNULL(AVG(o.TotalAmount), 0) AS AverageOrderValue,
                        ISNULL(SUM(o.DiscountAmount), 0) AS TotalDiscounts,
                        ISNULL(SUM(o.TaxAmount), 0) AS TotalTax,
                        ISNULL(SUM(o.TotalAmount - o.TaxAmount - o.DiscountAmount), 0) AS NetSales,
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o2 ON p.OrderID = o2.OrderID 
                                {where.Replace("o.", "o2.")}
                                AND p.PaymentMethod = 0), 0) AS TotalCashPayments,
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o2 ON p.OrderID = o2.OrderID 
                                {where.Replace("o.", "o2.")}
                                AND p.PaymentMethod = 1), 0) AS TotalCardPayments,
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o2 ON p.OrderID = o2.OrderID 
                                {where.Replace("o.", "o2.")}
                                AND p.PaymentMethod = 2), 0) AS TotalCreditPayments,
                        (SELECT COUNT(*) FROM Orders ov 
                         WHERE ov.IsVoided = 1 
                         AND CAST(ov.OrderDate AS DATE) >= @DateFrom_v 
                         AND CAST(ov.OrderDate AS DATE) <= @DateTo_v) AS VoidedOrders
                    FROM Orders o
                    {where}";

                // إضافة بارامترات الملغي
                var allPars = new List<SqlParameter>(pars);
                allPars.Add(new SqlParameter("@DateFrom_v", filters.DateFrom.Date));
                allPars.Add(new SqlParameter("@DateTo_v", filters.DateTo.Date));

                var dt = await DB_Server.GetTableAsync(query, allPars.ToArray());

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    summary.TotalSales = row["TotalSales"] != DBNull.Value ? Convert.ToDecimal(row["TotalSales"]) : 0;
                    summary.TotalOrders = row["TotalOrders"] != DBNull.Value ? Convert.ToInt32(row["TotalOrders"]) : 0;
                    summary.AverageOrderValue = row["AverageOrderValue"] != DBNull.Value ? Convert.ToDecimal(row["AverageOrderValue"]) : 0;
                    summary.TotalDiscounts = row["TotalDiscounts"] != DBNull.Value ? Convert.ToDecimal(row["TotalDiscounts"]) : 0;
                    summary.TotalTax = row["TotalTax"] != DBNull.Value ? Convert.ToDecimal(row["TotalTax"]) : 0;
                    summary.NetSales = row["NetSales"] != DBNull.Value ? Convert.ToDecimal(row["NetSales"]) : 0;
                    summary.TotalCashPayments = row["TotalCashPayments"] != DBNull.Value ? Convert.ToDecimal(row["TotalCashPayments"]) : 0;
                    summary.TotalCardPayments = row["TotalCardPayments"] != DBNull.Value ? Convert.ToDecimal(row["TotalCardPayments"]) : 0;
                    summary.TotalCreditPayments = row["TotalCreditPayments"] != DBNull.Value ? Convert.ToDecimal(row["TotalCreditPayments"]) : 0;
                    summary.VoidedOrders = row["VoidedOrders"] != DBNull.Value ? Convert.ToInt32(row["VoidedOrders"]) : 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting sales summary: {ex.Message}");
            }

            return summary;
        }

        // ═══════════════════════════════════════════════════════
        //  2. تفاصيل الفواتير
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetOrdersDetailAsync(ReportFilters filters)
        {
            try
            {
                var (where, pars) = BuildWhereClause(filters);

                // إضافة فلتر طريقة الدفع عبر EXISTS
                string paymentFilter = "";
                if (filters.PaymentMethod.HasValue)
                {
                    paymentFilter = " AND EXISTS (SELECT 1 FROM Payments p WHERE p.OrderID = o.OrderID AND p.PaymentMethod = @PaymentMethod)";
                    pars.Add(new SqlParameter("@PaymentMethod", filters.PaymentMethod.Value));
                }

                string query = $@"
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY o.OrderDate DESC) AS [#],
                        o.OrderNumber AS [رقم الفاتورة],
                        FORMAT(o.OrderDate, 'yyyy/MM/dd hh:mm tt') AS [التاريخ],
                        ISNULL(c.CustomerName, N'عميل نقدي') AS [العميل],
                        CASE o.OrderType 
                            WHEN 0 THEN N'تيك اوي'
                            WHEN 1 THEN N'صالة'
                            WHEN 2 THEN N'دليفري'
                            ELSE N'غير محدد'
                        END AS [نوع الطلب],
                        ISNULL(u.Full_Name, u.Username) AS [الكاشير],
                        o.SubTotal AS [المجموع الفرعي],
                        o.DiscountAmount AS [الخصم],
                        o.TaxAmount AS [الضريبة],
                        o.TotalAmount AS [الإجمالي],
                        ISNULL(
                            (SELECT TOP 1 
                                CASE p.PaymentMethod 
                                    WHEN 0 THEN N'نقدي'
                                    WHEN 1 THEN N'بطاقة'
                                    WHEN 2 THEN N'آجل'
                                    WHEN 3 THEN N'تحويل'
                                    ELSE N'غير محدد'
                                END 
                             FROM Payments p WHERE p.OrderID = o.OrderID ORDER BY p.Amount DESC), 
                            N'غير محدد'
                        ) AS [طريقة الدفع],
                        CASE o.Status
                            WHEN 0 THEN N'جديد'
                            WHEN 1 THEN N'قيد التحضير'
                            WHEN 2 THEN N'جاهز'
                            WHEN 3 THEN N'مُسلَّم'
                            WHEN 4 THEN N'ملغي'
                            WHEN 5 THEN N'مرتجع'
                            ELSE N'غير محدد'
                        END AS [الحالة],
                        o.IsVoided AS [ملغي]
                    FROM Orders o
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    LEFT JOIN Users u ON o.UserID = u.ID
                    {where} {paymentFilter}
                    ORDER BY o.OrderDate DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting orders detail: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  3. مبيعات المنتجات
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetProductSalesAsync(ReportFilters filters)
        {
            try
            {
                var (where, pars) = BuildWhereClause(filters);

                string query = $@"
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY SUM(oi.LineTotal) DESC) AS [#],
                        oi.ProductName AS [المنتج],
                        ISNULL(pc.CategoryNameAr, N'غير محدد') AS [القسم],
                        SUM(oi.Quantity) AS [الكمية المباعة],
                        SUM(oi.LineTotal) AS [إجمالي المبيعات],
                        COUNT(DISTINCT o.OrderID) AS [عدد الطلبات],
                        AVG(oi.UnitPrice) AS [متوسط السعر]
                    FROM OrderItems oi
                    INNER JOIN Orders o ON oi.OrderID = o.OrderID
                    LEFT JOIN Products pr ON oi.ProductID = pr.ProductID
                    LEFT JOIN ProductCategories pc ON pr.CategoryID = pc.CategoryID
                    {where}
                    AND oi.IsVoided = 0
                    GROUP BY oi.ProductName, pc.CategoryNameAr
                    ORDER BY SUM(oi.LineTotal) DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product sales: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  4. تقرير الورديات
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetShiftReportAsync(ReportFilters filters)
        {
            try
            {
                var pars = new List<SqlParameter>
                {
                    new SqlParameter("@DateFrom", filters.DateFrom.Date),
                    new SqlParameter("@DateTo", filters.DateTo.Date.AddDays(1))
                };

                string userFilter = "";
                if (filters.UserID.HasValue)
                {
                    userFilter = " AND s.UserID = @UserID";
                    pars.Add(new SqlParameter("@UserID", filters.UserID.Value));
                }

                string query = $@"
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY s.OpenDateTime DESC) AS [#],
                        s.ShiftID AS [رقم الوردية],
                        ISNULL(u.Full_Name, u.Username) AS [الكاشير],
                        FORMAT(s.OpenDateTime, 'yyyy/MM/dd hh:mm tt') AS [بداية الوردية],
                        CASE 
                            WHEN s.CloseDateTime IS NOT NULL 
                            THEN FORMAT(s.CloseDateTime, 'yyyy/MM/dd hh:mm tt')
                            ELSE N'مفتوحة'
                        END AS [نهاية الوردية],
                        CASE 
                            WHEN s.CloseDateTime IS NOT NULL 
                            THEN CONCAT(
                                DATEDIFF(HOUR, s.OpenDateTime, s.CloseDateTime), N' ساعة ',
                                DATEDIFF(MINUTE, s.OpenDateTime, s.CloseDateTime) % 60, N' دقيقة'
                            )
                            ELSE CONCAT(
                                DATEDIFF(HOUR, s.OpenDateTime, GETDATE()), N' ساعة ',
                                DATEDIFF(MINUTE, s.OpenDateTime, GETDATE()) % 60, N' دقيقة'
                            )
                        END AS [المدة],
                        ISNULL(s.OpeningCash, 0) AS [كاش الفتح],
                        ISNULL(s.ClosingCash, 0) AS [كاش الإغلاق],
                        ISNULL(s.TotalSales, 0) AS [المبيعات],
                        ISNULL(s.TotalOrders, 0) AS [عدد الطلبات],
                        ISNULL(s.CashDifference, 0) AS [الفرق],
                        CASE s.Status WHEN 0 THEN N'مفتوحة' WHEN 1 THEN N'مغلقة' ELSE N'غير محدد' END AS [الحالة]
                    FROM Shifts s
                    LEFT JOIN Users u ON s.UserID = u.ID
                    WHERE s.OpenDateTime >= @DateFrom AND s.OpenDateTime < @DateTo
                    {userFilter}
                    ORDER BY s.OpenDateTime DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting shift report: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  5. تقرير طرق الدفع
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetPaymentMethodsAsync(ReportFilters filters)
        {
            try
            {
                var (where, pars) = BuildWhereClause(filters);

                string query = $@"
                    SELECT 
                        CASE p.PaymentMethod 
                            WHEN 0 THEN N'نقدي 💵'
                            WHEN 1 THEN N'بطاقة 💳'
                            WHEN 2 THEN N'آجل 📋'
                            WHEN 3 THEN N'تحويل 🏦'
                            ELSE N'غير محدد'
                        END AS [طريقة الدفع],
                        COUNT(*) AS [عدد المعاملات],
                        SUM(p.Amount) AS [إجمالي المبلغ],
                        CAST(
                            ROUND(SUM(p.Amount) * 100.0 / NULLIF((
                                SELECT SUM(p2.Amount) FROM Payments p2 
                                INNER JOIN Orders o2 ON p2.OrderID = o2.OrderID 
                                {where.Replace("o.", "o2.")}
                            ), 0), 1) 
                        AS DECIMAL(5,1)) AS [النسبة المئوية %]
                    FROM Payments p
                    INNER JOIN Orders o ON p.OrderID = o.OrderID
                    {where}
                    GROUP BY p.PaymentMethod
                    ORDER BY SUM(p.Amount) DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting payment methods: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  6. تقرير العملاء
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetCustomerReportAsync(ReportFilters filters)
        {
            try
            {
                var (where, pars) = BuildWhereClause(filters);

                string query = $@"
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY SUM(o.TotalAmount) DESC) AS [#],
                        ISNULL(c.CustomerName, N'عميل نقدي') AS [العميل],
                        ISNULL(c.Phone1, '-') AS [الهاتف],
                        COUNT(*) AS [عدد الطلبات],
                        SUM(o.TotalAmount) AS [إجمالي المشتريات],
                        AVG(o.TotalAmount) AS [متوسط الطلب],
                        FORMAT(MAX(o.OrderDate), 'yyyy/MM/dd') AS [آخر طلب],
                        ISNULL(c.CurrentBalance, 0) AS [الرصيد الحالي]
                    FROM Orders o
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    {where}
                    GROUP BY c.CustomerName, c.Phone1, c.CurrentBalance
                    ORDER BY SUM(o.TotalAmount) DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting customer report: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  7. أكثر المنتجات مبيعاً (Top Products)
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetTopProductsAsync(ReportFilters filters, int top = 10)
        {
            try
            {
                var (where, pars) = BuildWhereClause(filters);
                pars.Add(new SqlParameter("@Top", top));

                string query = $@"
                    SELECT TOP (@Top)
                        ROW_NUMBER() OVER (ORDER BY SUM(oi.Quantity) DESC) AS [#],
                        oi.ProductName AS [المنتج],
                        SUM(oi.Quantity) AS [الكمية المباعة],
                        SUM(oi.LineTotal) AS [إجمالي المبيعات]
                    FROM OrderItems oi
                    INNER JOIN Orders o ON oi.OrderID = o.OrderID
                    {where}
                    AND oi.IsVoided = 0
                    GROUP BY oi.ProductName
                    ORDER BY SUM(oi.Quantity) DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting top products: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  8. مبيعات حسب الساعة (Hourly Sales)
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetHourlySalesAsync(ReportFilters filters)
        {
            try
            {
                var (where, pars) = BuildWhereClause(filters);

                string query = $@"
                    SELECT 
                        DATEPART(HOUR, o.OrderDate) AS [الساعة],
                        COUNT(*) AS [عدد الطلبات],
                        SUM(o.TotalAmount) AS [إجمالي المبيعات]
                    FROM Orders o
                    {where}
                    GROUP BY DATEPART(HOUR, o.OrderDate)
                    ORDER BY DATEPART(HOUR, o.OrderDate)";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting hourly sales: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  9. تقرير المرتجعات
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetReturnsDetailAsync(ReportFilters filters)
        {
            try
            {
                var conditions = new List<string>
                {
                    "CAST(r.ReturnDate AS DATE) >= @DateFrom",
                    "CAST(r.ReturnDate AS DATE) <= @DateTo"
                };
                var pars = new List<SqlParameter>
                {
                    new("@DateFrom", filters.DateFrom.Date),
                    new("@DateTo",   filters.DateTo.Date)
                };

                if (filters.UserID.HasValue)
                {
                    conditions.Add("r.UserID = @UserID");
                    pars.Add(new("@UserID", filters.UserID.Value));
                }
                if (filters.CustomerID.HasValue)
                {
                    conditions.Add("r.CustomerID = @CustomerID");
                    pars.Add(new("@CustomerID", filters.CustomerID.Value));
                }

                string where = "WHERE " + string.Join(" AND ", conditions);

                string query = $@"
                    SELECT
                        r.ReturnNumber                                          AS [رقم المرتجع],
                        CONVERT(NVARCHAR(16), r.ReturnDate, 120)                AS [تاريخ المرتجع],
                        r.OriginalOrderNumber                                   AS [رقم الفاتورة],
                        ISNULL(c.CustomerName, N'عميل نقدي')                   AS [العميل],
                        r.TotalReturnAmount                                     AS [إجمالي المرتجع],
                        CASE r.RefundMethod
                            WHEN 0 THEN N'نقدي'
                            WHEN 1 THEN N'بطاقة'
                            WHEN 2 THEN N'رصيد آجل'
                            ELSE N'غير محدد'
                        END                                                      AS [طريقة الاسترداد],
                        r.ReturnReason                                          AS [سبب الإرجاع],
                        ISNULL(u.Full_Name, u.Username)                         AS [الكاشير],
                        (SELECT COUNT(*) FROM ReturnItems ri WHERE ri.ReturnID = r.ReturnID)
                                                                                AS [عدد الأصناف]
                    FROM Returns r
                    LEFT JOIN Customers c  ON r.CustomerID = c.CustomerID
                    LEFT JOIN Users u      ON r.UserID     = u.ID
                    {where}
                    ORDER BY r.ReturnDate DESC";

                return await DB_Server.GetTableAsync(query, pars.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting returns report: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  مساعد: جلب قائمة الكاشيرين (لتعبئة ComboBox)
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetCashiersListAsync()
        {
            try
            {
                string query = @"
                    SELECT DISTINCT u.ID, ISNULL(u.Full_Name, u.Username) AS DisplayName
                    FROM Users u
                    INNER JOIN Orders o ON u.ID = o.UserID
                    WHERE u.IsDeleted = 0
                    ORDER BY DisplayName";

                return await DB_Server.GetTableAsync(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cashiers list: {ex.Message}");
                return new DataTable();
            }
        }
    }
}
