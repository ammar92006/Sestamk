using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;
using System;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    /// <summary>
    /// خدمة إدارة الورديات — فتح/إغلاق الوردية وتسجيل حركات النقد
    /// </summary>
    public static class ShiftService
    {
        /// <summary>
        /// الوردية الحالية المفتوحة (null إذا لا توجد وردية)
        /// </summary>
        public static ShiftModel CurrentShift { get; private set; }

        /// <summary>
        /// هل هناك وردية مفتوحة حالياً؟
        /// </summary>
        public static bool IsShiftOpen => CurrentShift != null && CurrentShift.Status == 0;

        /// <summary>
        /// فتح وردية جديدة
        /// </summary>
        public static async Task<ShiftModel> OpenShiftAsync(int userId, decimal openingCash, string notes = "")
        {
            try
            {
                // التحقق من عدم وجود وردية مفتوحة
                string checkQuery = "SELECT COUNT(*) FROM Shifts WHERE UserID = @UserID AND Status = 0";
                object existing = await DB_Server.ScalarAsync(checkQuery, new SqlParameter[]
                {
                    new SqlParameter("@UserID", userId)
                });

                if (existing != null && Convert.ToInt32(existing) > 0)
                {
                    // حمّل الوردية المفتوحة بدل فتح جديدة
                    await LoadCurrentShiftAsync(userId);
                    return CurrentShift;
                }

                string insertQuery = @"
                    INSERT INTO Shifts 
                    (UserID, OpenDateTime, OpeningCash, Status, Notes, TotalSales, TotalRefunds, TotalOrders)
                    VALUES 
                    (@UserID, GETDATE(), @OpeningCash, 0, @Notes, 0, 0, 0);
                    SELECT SCOPE_IDENTITY();";

                int shiftId = await DB_Server.ExecuteWithIdentityAsync(insertQuery, new SqlParameter[]
                {
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@OpeningCash", openingCash),
                    new SqlParameter("@Notes", notes ?? "")
                });

                CurrentShift = new ShiftModel
                {
                    ShiftID = shiftId,
                    UserID = userId,
                    OpenDateTime = DateTime.Now,
                    OpeningCash = openingCash,
                    Status = 0,
                    Notes = notes,
                    UserName = UserSession.Full_Name ?? UserSession.UserName
                };

                return CurrentShift;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في فتح الوردية: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// إغلاق الوردية الحالية
        /// </summary>
        public static async Task<bool> CloseShiftAsync(decimal closingCash, string notes = "")
        {
            if (CurrentShift == null || CurrentShift.Status != 0)
            {
                ToastManager.ShowWarning("تنبيه", "لا توجد وردية مفتوحة لإغلاقها");
                return false;
            }

            try
            {
                // حساب النقد المتوقع
                string calcQuery = @"
                    SELECT 
                        ISNULL(s.OpeningCash, 0) + 
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o ON p.OrderID = o.OrderID 
                                WHERE o.ShiftID = s.ShiftID AND p.PaymentMethod = 0 AND o.IsVoided = 0), 0) -
                        ISNULL(s.TotalRefunds, 0) as ExpectedCash,
                        ISNULL(s.TotalSales, 0) as TotalSales,
                        ISNULL(s.TotalOrders, 0) as TotalOrders,
                        ISNULL(s.TotalRefunds, 0) as TotalRefunds
                    FROM Shifts s WHERE s.ShiftID = @ShiftID";

                var dt = await DB_Server.GetTableAsync(calcQuery, new SqlParameter[]
                {
                    new SqlParameter("@ShiftID", CurrentShift.ShiftID)
                });

                decimal expectedCash = 0;
                decimal totalSales = 0;
                int totalOrders = 0;
                decimal totalRefunds = 0;

                if (dt.Rows.Count > 0)
                {
                    expectedCash = dt.Rows[0]["ExpectedCash"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["ExpectedCash"]) : 0;
                    totalSales = dt.Rows[0]["TotalSales"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["TotalSales"]) : 0;
                    totalOrders = dt.Rows[0]["TotalOrders"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["TotalOrders"]) : 0;
                    totalRefunds = dt.Rows[0]["TotalRefunds"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["TotalRefunds"]) : 0;
                }

                decimal cashDifference = closingCash - expectedCash;

                // تحديث الوردية
                string updateQuery = @"
                    UPDATE Shifts SET 
                        CloseDateTime = GETDATE(),
                        ClosingCash = @ClosingCash,
                        ExpectedCash = @ExpectedCash,
                        CashDifference = @CashDifference,
                        TotalSales = @TotalSales,
                        TotalRefunds = @TotalRefunds,
                        TotalOrders = @TotalOrders,
                        Status = 1,
                        ClosedByUserID = @ClosedByUserID,
                        Notes = ISNULL(Notes, '') + CHAR(13) + @Notes
                    WHERE ShiftID = @ShiftID";

                await DB_Server.ExecuteAsync(updateQuery, new SqlParameter[]
                {
                    new SqlParameter("@ClosingCash", closingCash),
                    new SqlParameter("@ExpectedCash", expectedCash),
                    new SqlParameter("@CashDifference", cashDifference),
                    new SqlParameter("@TotalSales", totalSales),
                    new SqlParameter("@TotalRefunds", totalRefunds),
                    new SqlParameter("@TotalOrders", totalOrders),
                    new SqlParameter("@ClosedByUserID", UserSession.UserId),
                    new SqlParameter("@Notes", notes ?? ""),
                    new SqlParameter("@ShiftID", CurrentShift.ShiftID)
                });

                // تحديث الكائن المحلي
                CurrentShift.CloseDateTime = DateTime.Now;
                CurrentShift.ClosingCash = closingCash;
                CurrentShift.ExpectedCash = expectedCash;
                CurrentShift.CashDifference = cashDifference;
                CurrentShift.TotalSales = totalSales;
                CurrentShift.TotalOrders = totalOrders;
                CurrentShift.Status = 1;

                // إفراغ الوردية الحالية
                var closedShift = CurrentShift;
                CurrentShift = null;

                return true;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في إغلاق الوردية: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// تحميل الوردية المفتوحة (إن وجدت) عند بدء التشغيل
        /// </summary>
        public static async Task LoadCurrentShiftAsync(int userId)
        {
            try
            {
                string query = @"
                    SELECT TOP 1 s.*, u.Full_Name as UserName
                    FROM Shifts s 
                    LEFT JOIN Users u ON s.UserID = u.ID
                    WHERE s.UserID = @UserID AND s.Status = 0
                    ORDER BY s.OpenDateTime DESC";

                var dt = await DB_Server.GetTableAsync(query, new SqlParameter[]
                {
                    new SqlParameter("@UserID", userId)
                });

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    CurrentShift = new ShiftModel
                    {
                        ShiftID = Convert.ToInt32(row["ShiftID"]),
                        UserID = Convert.ToInt32(row["UserID"]),
                        OpenDateTime = Convert.ToDateTime(row["OpenDateTime"]),
                        OpeningCash = row["OpeningCash"] != DBNull.Value ? Convert.ToDecimal(row["OpeningCash"]) : 0,
                        TotalSales = row["TotalSales"] != DBNull.Value ? Convert.ToDecimal(row["TotalSales"]) : 0,
                        TotalOrders = row["TotalOrders"] != DBNull.Value ? Convert.ToInt32(row["TotalOrders"]) : 0,
                        Status = 0,
                        UserName = row["UserName"]?.ToString() ?? ""
                    };
                }
                else
                {
                    CurrentShift = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading shift: {ex.Message}");
                CurrentShift = null;
            }
        }

        /// <summary>
        /// جلب ملخص الوردية الحالية
        /// </summary>
        public static async Task<ShiftModel> GetShiftSummaryAsync(int shiftId)
        {
            try
            {
                string query = @"
                    SELECT 
                        s.*,
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o ON p.OrderID = o.OrderID 
                                WHERE o.ShiftID = s.ShiftID AND p.PaymentMethod = 0 AND o.IsVoided = 0), 0) as CashPayments,
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o ON p.OrderID = o.OrderID 
                                WHERE o.ShiftID = s.ShiftID AND p.PaymentMethod = 1 AND o.IsVoided = 0), 0) as CardPayments,
                        ISNULL((SELECT SUM(p.Amount) FROM Payments p 
                                INNER JOIN Orders o ON p.OrderID = o.OrderID 
                                WHERE o.ShiftID = s.ShiftID AND p.PaymentMethod = 2 AND o.IsVoided = 0), 0) as CreditPayments
                    FROM Shifts s 
                    WHERE s.ShiftID = @ShiftID";

                var dt = await DB_Server.GetTableAsync(query, new SqlParameter[]
                {
                    new SqlParameter("@ShiftID", shiftId)
                });

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return new ShiftModel
                    {
                        ShiftID = Convert.ToInt32(row["ShiftID"]),
                        UserID = Convert.ToInt32(row["UserID"]),
                        OpenDateTime = Convert.ToDateTime(row["OpenDateTime"]),
                        CloseDateTime = row["CloseDateTime"] != DBNull.Value ? Convert.ToDateTime(row["CloseDateTime"]) : null,
                        OpeningCash = row["OpeningCash"] != DBNull.Value ? Convert.ToDecimal(row["OpeningCash"]) : 0,
                        ClosingCash = row["ClosingCash"] != DBNull.Value ? Convert.ToDecimal(row["ClosingCash"]) : 0,
                        ExpectedCash = row["ExpectedCash"] != DBNull.Value ? Convert.ToDecimal(row["ExpectedCash"]) : 0,
                        CashDifference = row["CashDifference"] != DBNull.Value ? Convert.ToDecimal(row["CashDifference"]) : 0,
                        TotalSales = row["TotalSales"] != DBNull.Value ? Convert.ToDecimal(row["TotalSales"]) : 0,
                        TotalRefunds = row["TotalRefunds"] != DBNull.Value ? Convert.ToDecimal(row["TotalRefunds"]) : 0,
                        TotalOrders = row["TotalOrders"] != DBNull.Value ? Convert.ToInt32(row["TotalOrders"]) : 0,
                        Status = Convert.ToInt32(row["Status"]),
                        Notes = row["Notes"]?.ToString() ?? ""
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting shift summary: {ex.Message}");
                return null;
            }
        }
    }
}
