using Microsoft.Data.SqlClient;
using Sestamk.Classes.Data;

namespace Sestamk.Classes
{
    /// <summary>
    /// Order service — saves complete invoices to DB in a single transaction.
    /// CHANGED: Now deducts inventory automatically when saving an order (Step 7 added).
    /// </summary>
    public static class OrderService
    {
        public static async Task<int> SaveOrderAsync(OrderModel order)
        {
            int orderId = 0;

            await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                // ═══ 0. Generate the actual OrderNumber atomically inside the
                //     transaction. Any value passed in via order.OrderNumber is
                //     treated as a preview only and is overwritten here.
                order.OrderNumber = await GenerateOrderNumberInternalAsync(conn, trans);

                // ═══ 1. Insert order header ═══
                string insertOrder = @"
                    INSERT INTO Orders
                    (OrderNumber, ShiftID, CustomerID, UserID, OrderType,
                     SubTotal, DiscountAmount, DiscountPercent, ServiceAmount,
                     TaxPercent, TaxAmount, TotalAmount, PaidAmount, ChangeAmount,
                     RemainingAmount, Status, OrderDate, Notes, IsVoided, TableID, DriverID)
                    VALUES
                    (@OrderNumber, @ShiftID, @CustomerID, @UserID, @OrderType,
                     @SubTotal, @DiscountAmount, @DiscountPercent, @ServiceAmount,
                     @TaxPercent, @TaxAmount, @TotalAmount, @PaidAmount, @ChangeAmount,
                     @RemainingAmount, @Status, @OrderDate, @Notes, 0, @TableID, @DriverID);
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
                    cmd.Parameters.AddWithValue("@TableID", order.TableID.HasValue ? (object)order.TableID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DriverID", order.DriverID.HasValue ? (object)order.DriverID.Value : DBNull.Value);

                    object result = await cmd.ExecuteScalarAsync();
                    orderId = Convert.ToInt32(result);
                    order.OrderID = orderId;
                }

                // ═══ 2. Insert order items ═══
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

                    // ═══ 3. Insert item addons ═══
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

                // ═══ 4. Insert payments ═══
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

                // ═══ 5. Update customer balance (credit sales) ═══
                if (order.CustomerID.HasValue && order.RemainingAmount > 0)
                {
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

                // ═══ 6. Update shift statistics ═══
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

                // ═══ 7. Deduct inventory ═══
                try
                {
                    await InventoryService.DeductStockForOrderAsync(
                        conn, trans, orderId, order.Items, order.UserID);
                }
                catch (SqlException ex) when (ex.Number == 547) // FK constraint violation
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Inventory FK error: {ex.Message}");
                    // Skip inventory deduction — don't block the order
                }

                // ═══ 8. Mark table as occupied for dine-in orders ═══
                if (order.OrderType == 1 && order.TableID.HasValue)
                {
                    string updateTable = @"
                        UPDATE Tables
                        SET Status = 'occupied'
                        WHERE Id = @TableID";

                    using (var cmd = new SqlCommand(updateTable, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@TableID", order.TableID.Value);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            });

            // WhatsApp notification (fire-and-forget, outside transaction)
            TriggerWhatsAppNotification(order);

            return orderId;
        }

        private static void TriggerWhatsAppNotification(OrderModel order)
        {
            if (!SettingsService.WhatsAppEnabled || !SettingsService.WhatsAppAutoSendInvoice)
                return;

            if (string.IsNullOrWhiteSpace(order.CustomerPhone))
                return;

            string currency = SettingsService.CurrencySymbol;
            string msgText = WhatsAppHelper.FormatInvoiceMessage(
                SettingsService.WhatsAppInvoiceTemplate,
                order.OrderNumber,
                order.TotalAmount,
                currency,
                SettingsService.StoreName,
                order.CustomerName ?? "",
                order.OrderDate);

            var msg = new WhatsAppMessage
            {
                Phone = order.CustomerPhone,
                Text = msgText,
                OrderId = order.OrderID,
                CreatedByUserId = order.UserID
            };

            _ = Task.Run(async () =>
            {
                try { await WhatsAppQueueManager.EnqueueAsync(msg); }
                catch (Exception ex)
                {
                    WhatsAppLogger.Error("OrderService",
                        $"Failed to enqueue WhatsApp for order {order.OrderNumber}: {ex.Message}",
                        msg.CorrelationId, ex);
                }
            });
        }

        /// <summary>
        /// Preview the next order number for UI display. NOT race-safe — the
        /// real number is generated atomically inside SaveOrderAsync.
        /// </summary>
        public static async Task<string> PreviewNextOrderNumberAsync()
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) + 1
                    FROM Orders
                    WHERE CONVERT(DATE, OrderDate) = CONVERT(DATE, GETDATE())";

                object result = await DB_Server.ScalarAsync(query);
                int sequence = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;

                return $"INV-{DateTime.Now:yyyyMMdd}-{sequence:D3}";
            }
            catch
            {
                return OrderModel.GenerateOrderNumber();
            }
        }

        /// <summary>
        /// Atomic generation inside an existing transaction. Uses UPDLOCK+HOLDLOCK
        /// so two concurrent cashiers cannot get the same number.
        /// </summary>
        private static async Task<string> GenerateOrderNumberInternalAsync(SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                SELECT COUNT(*) + 1
                FROM Orders WITH (UPDLOCK, HOLDLOCK)
                WHERE CONVERT(DATE, OrderDate) = CONVERT(DATE, GETDATE())";

            using (var cmd = new SqlCommand(query, conn, trans))
            {
                object result = await cmd.ExecuteScalarAsync();
                int sequence = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;
                return $"INV-{DateTime.Now:yyyyMMdd}-{sequence:D3}";
            }
        }

        /// <summary>
        /// Void an order — now also returns stock to inventory.
        /// </summary>
        public static async Task<bool> VoidOrderAsync(int orderId, string reason, int userId)
        {
            try
            {
                return await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
                {
                    string query = @"
                        UPDATE Orders
                        SET IsVoided = 1, VoidReason = @Reason, Status = 4
                        WHERE OrderID = @OrderID AND ISNULL(IsVoided, 0) = 0";

                    using (var cmd = new SqlCommand(query, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Reason", reason);
                        cmd.Parameters.AddWithValue("@OrderID", orderId);

                        int affected = await cmd.ExecuteNonQueryAsync();
                        if (affected == 0) return; // already voided
                    }

                    // Return stock
                    await InventoryService.ReturnStockForOrderAsync(conn, trans, orderId, userId);
                });
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في إلغاء الطلب: " + ex.Message);
                return false;
            }
        }
    }
}
