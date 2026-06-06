using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    /// <summary>
    /// استعلامات لوحة التحكم الرئيسية — تتبع نفس نمط SalesReportService
    /// </summary>
    public static class DashboardService
    {
        // ═══════════════════════════════════════════════════════
        //  1. ملخص اليوم — مبيعات + طلبات + مقارنة بالأمس
        // ═══════════════════════════════════════════════════════

        public static async Task<(decimal TodaySales, int TodayOrders, decimal YesterdaySales, int YesterdayOrders)>
            GetTodaySummaryAsync()
        {
            string query = @"
                SELECT
                    ISNULL(SUM(CASE WHEN CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                                    THEN TotalAmount ELSE 0 END), 0)     AS TodaySales,
                    ISNULL(SUM(CASE WHEN CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                                    THEN 1 ELSE 0 END), 0)               AS TodayOrders,
                    ISNULL(SUM(CASE WHEN CAST(OrderDate AS DATE) = CAST(DATEADD(DAY,-1,GETDATE()) AS DATE)
                                    THEN TotalAmount ELSE 0 END), 0)     AS YesterdaySales,
                    ISNULL(SUM(CASE WHEN CAST(OrderDate AS DATE) = CAST(DATEADD(DAY,-1,GETDATE()) AS DATE)
                                    THEN 1 ELSE 0 END), 0)               AS YesterdayOrders
                FROM Orders
                WHERE IsVoided = 0
                  AND CAST(OrderDate AS DATE) >= CAST(DATEADD(DAY,-1,GETDATE()) AS DATE)";

            try
            {
                var dt = await DB_Server.GetTableAsync(query);
                if (dt.Rows.Count == 0) return (0, 0, 0, 0);
                var r = dt.Rows[0];
                return (
                    r["TodaySales"]     != DBNull.Value ? Convert.ToDecimal(r["TodaySales"])     : 0m,
                    r["TodayOrders"]    != DBNull.Value ? Convert.ToInt32(r["TodayOrders"])      : 0,
                    r["YesterdaySales"] != DBNull.Value ? Convert.ToDecimal(r["YesterdaySales"]) : 0m,
                    r["YesterdayOrders"]!= DBNull.Value ? Convert.ToInt32(r["YesterdayOrders"])  : 0
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DashboardService.GetTodaySummaryAsync: {ex.Message}");
                return (0, 0, 0, 0);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  2. عدد الطاولات حسب الحالة
        // ═══════════════════════════════════════════════════════

        public static async Task<(int Available, int Occupied, int Total)> GetTablesCountAsync()
        {
            string query = @"
                SELECT
                    ISNULL(SUM(CASE WHEN Status = 'available' THEN 1 ELSE 0 END), 0) AS Available,
                    ISNULL(SUM(CASE WHEN Status <> 'available' THEN 1 ELSE 0 END), 0) AS Occupied,
                    COUNT(*) AS Total
                FROM Tables
                WHERE IsActive = 1";

            try
            {
                var dt = await DB_Server.GetTableAsync(query);
                if (dt.Rows.Count == 0) return (0, 0, 0);
                var r = dt.Rows[0];
                return (
                    r["Available"] != DBNull.Value ? Convert.ToInt32(r["Available"]) : 0,
                    r["Occupied"]  != DBNull.Value ? Convert.ToInt32(r["Occupied"])  : 0,
                    r["Total"]     != DBNull.Value ? Convert.ToInt32(r["Total"])     : 0
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DashboardService.GetTablesCountAsync: {ex.Message}");
                return (0, 0, 0);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  3. مبيعات آخر 7 أيام (للرسم البياني)
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetWeeklySalesAsync()
        {
            string query = @"
                SELECT
                    CAST(OrderDate AS DATE)          AS OrderDay,
                    ISNULL(SUM(TotalAmount), 0)      AS TotalSales,
                    COUNT(*)                         AS TotalOrders
                FROM Orders
                WHERE IsVoided = 0
                  AND CAST(OrderDate AS DATE) >= CAST(DATEADD(DAY,-6,GETDATE()) AS DATE)
                  AND CAST(OrderDate AS DATE) <= CAST(GETDATE() AS DATE)
                GROUP BY CAST(OrderDate AS DATE)
                ORDER BY CAST(OrderDate AS DATE)";

            try
            {
                return await DB_Server.GetTableAsync(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DashboardService.GetWeeklySalesAsync: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  4. آخر طلبات اليوم (للجدول)
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetRecentOrdersAsync(int count = 8)
        {
            string query = @"
                SELECT TOP (@Count)
                    o.OrderNumber                                              AS [رقم الفاتورة],
                    FORMAT(o.OrderDate, 'hh:mm tt')                           AS [الوقت],
                    ISNULL(c.CustomerName, N'نقدي')                           AS [العميل],
                    CASE o.OrderType
                        WHEN 0 THEN N'تيك اوي'
                        WHEN 1 THEN N'صالة'
                        WHEN 2 THEN N'دليفري'
                        ELSE N'غير محدد'
                    END                                                        AS [النوع],
                    o.TotalAmount                                              AS [الإجمالي],
                    CASE o.Status
                        WHEN 0 THEN N'جديد'
                        WHEN 1 THEN N'قيد التحضير'
                        WHEN 2 THEN N'جاهز'
                        WHEN 3 THEN N'مُسلَّم'
                        WHEN 4 THEN N'ملغي'
                        WHEN 5 THEN N'مرتجع'
                        ELSE N'غير محدد'
                    END                                                        AS [الحالة],
                    o.Status                                                   AS StatusCode
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                WHERE CAST(o.OrderDate AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY o.OrderDate DESC";

            try
            {
                return await DB_Server.GetTableAsync(query,
                    new[] { new SqlParameter("@Count", count) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DashboardService.GetRecentOrdersAsync: {ex.Message}");
                return new DataTable();
            }
        }

        // ═══════════════════════════════════════════════════════
        //  5. تفاصيل حالة الطاولات (للجدول)
        // ═══════════════════════════════════════════════════════

        public static async Task<DataTable> GetTablesStatusAsync()
        {
            string query = @"
                SELECT
                    t.TableNumber                                              AS [رقم الطاولة],
                    t.TableName                                                AS [الاسم],
                    ISNULL(s.SectionName, N'-')                               AS [القسم],
                    t.Capacity                                                 AS [السعة],
                    CASE t.Status
                        WHEN 'available'   THEN N'متاحة'
                        WHEN 'occupied'    THEN N'مشغولة'
                        WHEN 'reserved'    THEN N'محجوزة'
                        WHEN 'cleaning'    THEN N'تنظيف'
                        WHEN 'maintenance' THEN N'صيانة'
                        ELSE t.Status
                    END                                                        AS [الحالة],
                    t.Status                                                   AS StatusRaw
                FROM Tables t
                LEFT JOIN Sections s ON t.SectionId = s.Id
                WHERE t.IsActive = 1
                ORDER BY t.TableNumber";

            try
            {
                return await DB_Server.GetTableAsync(query);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DashboardService.GetTablesStatusAsync: {ex.Message}");
                return new DataTable();
            }
        }
    }
}
