using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sestamk.Classes
{
    /// <summary>
    /// خدمة الطلبات — مسؤولة عن حفظ الفاتورة كاملة في قاعدة البيانات
    /// تستخدم Transaction واحد لضمان حفظ كل البيانات أو لا شيء
    /// </summary>
    public static class OrderService
    {
        /// <summary>
        /// حفظ طلب كامل (رأس الفاتورة + الأصناف + الإضافات + المدفوعات)
        /// في Transaction واحد لضمان تكامل البيانات
        /// </summary>
        public static async Task<int> SaveOrderAsync(OrderModel order)
        {
            int orderId = 0;

            await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                // ═══ 1. إدراج رأس الفاتورة ═══
                string insertOrder = @"
                    INSERT INTO Orders 
                    (OrderNumber, ShiftID, CustomerID, UserID, OrderType,
                     SubTotal, DiscountAmount, DiscountPercent, ServiceAmount,
                     TaxPercent, TaxAmount, TotalAmount, PaidAmount, ChangeAmount, 
                     RemainingAmount, Status, OrderDate, Notes, IsVoided)
                    VALUES 
                    (@OrderNumber, @ShiftID, @CustomerID, @UserID, @OrderType,
                     @SubTotal, @DiscountAmount, @DiscountPercent, @ServiceAmount,
                     @TaxPercent, @TaxAmount, @TotalAmount, @PaidAmount, @ChangeAmount,
                     @RemainingAmount, @Status, @OrderDate, @Notes, 0);
                    SELECT SCOPE_IDENTITY();";

                using (var cmd = new SqlCommand(insertOrder, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@OrderNumber", order.OrderNumber);
                    cmd.Parameters.AddWithValue("@ShiftID", order.ShiftID > 0 ? order.ShiftID : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID.HasValue ? (object)order.CustomerID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@UserID", order.UserID);
                    cmd.Parameters.AddWithValue("@OrderType", order.OrderType);
                    cmd.Parameters.AddWithValue("@SubTotal", order.SubTotal);
                    cmd.Parameters.AddWithValue("@DiscountAmount", order.DiscountAmount);
                    cmd.Parameters.AddWithValue("@DiscountPercent", order.DiscountPercent);
                    cmd.Parameters.AddWithValue("@ServiceAmount", order.ServiceAmount);
                    cmd.Parameters.AddWithValue("@TaxPercent", order.TaxPercent);
                    cmd.Parameters.AddWithValue("@TaxAmount", order.TaxAmount);
                    cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                    cmd.Parameters.AddWithValue("@PaidAmount", order.PaidAmount);
                    cmd.Parameters.AddWithValue("@ChangeAmount", order.ChangeAmount);
                    cmd.Parameters.AddWithValue("@RemainingAmount", order.RemainingAmount);
                    cmd.Parameters.AddWithValue("@Status", order.Status);
                    cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    cmd.Parameters.AddWithValue("@Notes", order.Notes ?? "");

                    object result = await cmd.ExecuteScalarAsync();
                    orderId = Convert.ToInt32(result);
                    order.OrderID = orderId;
                }

                // ═══ 2. إدراج أصناف الطلب ═══
                foreach (var item in order.Items)
                {
                    if (item.IsVoided) continue;

                    string insertItem = @"
                        INSERT INTO OrderItems
                        (OrderID, ProductID, ProductSizeID, ProductName, SizeName,
                         UnitPrice, Quantity, DiscountAmount, TaxAmount, LineTotal, Notes, IsVoided)
                        VALUES
                        (@OrderID, @ProductID, @ProductSizeID, @ProductName, @SizeName,
                         @UnitPrice, @Quantity, @DiscountAmount, @TaxAmount, @LineTotal, @Notes, 0);
                        SELECT SCOPE_IDENTITY();";

                    int orderItemId;
                    using (var cmd = new SqlCommand(insertItem, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                        cmd.Parameters.AddWithValue("@ProductSizeID", item.ProductSizeID.HasValue ? (object)item.ProductSizeID.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmd.Parameters.AddWithValue("@SizeName", item.SizeName ?? "");
                        cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@DiscountAmount", item.DiscountAmount);
                        cmd.Parameters.AddWithValue("@TaxAmount", item.TaxAmount);
                        cmd.Parameters.AddWithValue("@LineTotal", item.LineTotal);
                        cmd.Parameters.AddWithValue("@Notes", item.Notes ?? "");

                        object result = await cmd.ExecuteScalarAsync();
                        orderItemId = Convert.ToInt32(result);
                        item.OrderItemID = orderItemId;
                    }

                    // ═══ 3. إدراج إضافات الصنف ═══
                    foreach (var addon in item.Addons)
                    {
                        string insertAddon = @"
                            INSERT INTO OrderItemAddons
                            (OrderItemID, AddonID, AddonName, UnitPrice, Quantity, LineTotal)
                            VALUES
                            (@OrderItemID, @AddonID, @AddonName, @UnitPrice, @Quantity, @LineTotal)";

                        using (var cmd = new SqlCommand(insertAddon, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@OrderItemID", orderItemId);
                            cmd.Parameters.AddWithValue("@AddonID", addon.AddonID);
                            cmd.Parameters.AddWithValue("@AddonName", addon.AddonName);
                            cmd.Parameters.AddWithValue("@UnitPrice", addon.UnitPrice);
                            cmd.Parameters.AddWithValue("@Quantity", addon.Quantity);
                            cmd.Parameters.AddWithValue("@LineTotal", addon.LineTotal);

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // ═══ 4. إدراج المدفوعات ═══
                foreach (var payment in order.Payments)
                {
                    string insertPayment = @"
                        INSERT INTO Payments
                        (OrderID, PaymentMethod, Amount, PaymentDate, ReferenceNumber, Notes)
                        VALUES
                        (@OrderID, @PaymentMethod, @Amount, @PaymentDate, @ReferenceNumber, @Notes)";

                    using (var cmd = new SqlCommand(insertPayment, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);
                        cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                        cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                        cmd.Parameters.AddWithValue("@ReferenceNumber", payment.ReferenceNumber ?? "");
                        cmd.Parameters.AddWithValue("@Notes", payment.Notes ?? "");

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // ═══ 5. تحديث رصيد العميل (للدفع الآجل) ═══
                if (order.CustomerID.HasValue && order.RemainingAmount > 0)
                {
                    // تحديث الرصيد
                    string updateBalance = @"
                        UPDATE Customers 
                        SET CurrentBalance = CurrentBalance + @Amount,
                            LastTransactionDate = GETDATE()
                        WHERE CustomerID = @CustomerID";

                    using (var cmd = new SqlCommand(updateBalance, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Amount", order.RemainingAmount);
                        cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // تسجيل الحركة المالية
                    string insertTransaction = @"
                        INSERT INTO CustomerTransactions
                        (CustomerID, OrderID, TransactionType, Amount, BalanceAfter, CreatedByUserID, Notes)
                        SELECT @CustomerID, @OrderID, 0, @Amount, 
                               CurrentBalance, @UserID, @Notes
                        FROM Customers WHERE CustomerID = @CustomerID";

                    using (var cmd = new SqlCommand(insertTransaction, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID.Value);
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@Amount", order.RemainingAmount);
                        cmd.Parameters.AddWithValue("@UserID", order.UserID);
                        cmd.Parameters.AddWithValue("@Notes", $"فاتورة رقم {order.OrderNumber}");
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // ═══ 6. تحديث إحصائيات الوردية ═══
                if (order.ShiftID > 0)
                {
                    string updateShift = @"
                        UPDATE Shifts 
                        SET TotalSales = ISNULL(TotalSales, 0) + @TotalAmount,
                            TotalOrders = ISNULL(TotalOrders, 0) + 1
                        WHERE ShiftID = @ShiftID AND Status = 0";

                    using (var cmd = new SqlCommand(updateShift, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                        cmd.Parameters.AddWithValue("@ShiftID", order.ShiftID);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            });

            return orderId;
        }

        /// <summary>
        /// جلب آخر رقم طلب لتوليد الرقم التالي
        /// </summary>
        public static async Task<string> GetNextOrderNumberAsync()
        {
            try
            {
                string today = DateTime.Now.ToString("yyyyMMdd");
                string query = @"
                    SELECT COUNT(*) + 1 
                    FROM Orders 
                    WHERE CONVERT(DATE, OrderDate) = CONVERT(DATE, GETDATE())";

                object result = await DB_Server.ScalarAsync(query);
                int sequence = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;

                return $"INV-{today}-{sequence:D3}";
            }
            catch
            {
                return OrderModel.GenerateOrderNumber();
            }
        }

        /// <summary>
        /// إلغاء طلب (Void)
        /// </summary>
        public static async Task<bool> VoidOrderAsync(int orderId, string reason, int userId)
        {
            try
            {
                string query = @"
                    UPDATE Orders 
                    SET IsVoided = 1, VoidReason = @Reason, Status = 4
                    WHERE OrderID = @OrderID";

                int affected = await DB_Server.ExecuteAsync(query, new SqlParameter[]
                {
                    new SqlParameter("@Reason", reason),
                    new SqlParameter("@OrderID", orderId)
                });

                return affected > 0;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في إلغاء الطلب: " + ex.Message);
                return false;
            }
        }
    }
}
