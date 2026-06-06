using Microsoft.Data.SqlClient;

namespace Sestamk.Classes
{
    /// <summary>
    /// Inventory movement service — records stock changes in the Inventory table.
    /// The Inventory table already exists in the DB (see script.sql) but was never
    /// wired into OrderService. This class bridges that gap.
    ///
    /// TransactionType values:
    ///   0 = Purchase (stock in)
    ///   1 = Sale (stock out — auto-deducted on order save)
    ///   2 = Adjustment (manual correction)
    ///   3 = Return (stock back in from refund)
    ///   4 = Wastage/Spoilage
    /// </summary>
    public static class InventoryService
    {
        /// <summary>
        /// Deduct stock for all items in an order.
        /// Called INSIDE the OrderService transaction to ensure atomicity.
        /// If inventory deduction fails, the entire order rolls back.
        /// </summary>
        public static async Task DeductStockForOrderAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int orderId,
            List<Data.OrderItemModel> items,
            int userId)
        {
            foreach (var item in items)
            {
                if (item.IsVoided) continue;

                await InsertMovementAsync(
                    conn, trans,
                    productId: item.ProductID,
                    productSizeId: item.ProductSizeID,
                    transactionType: 1, // Sale
                    quantity: -item.Quantity, // negative = stock out
                    unitCost: null,
                    referenceId: orderId,
                    referenceType: "Order",
                    userId: userId);
            }
        }

        /// <summary>
        /// Return stock for a voided/refunded order.
        /// </summary>
        public static async Task ReturnStockForOrderAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int orderId,
            int userId)
        {
            string query = @"
                SELECT ProductID, ProductSizeID, Quantity
                FROM OrderItems
                WHERE OrderID = @OrderID AND ISNULL(IsVoided, 0) = 0";

            using var cmd = new SqlCommand(query, conn, trans);
            cmd.Parameters.AddWithValue("@OrderID", orderId);

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<(int ProductID, int? ProductSizeID, int Quantity)>();
            while (await reader.ReadAsync())
            {
                items.Add((
                    reader.GetInt32(0),
                    reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    reader.GetInt32(2)));
            }
            reader.Close();

            foreach (var (productId, productSizeId, quantity) in items)
            {
                await InsertMovementAsync(
                    conn, trans,
                    productId: productId,
                    productSizeId: productSizeId,
                    transactionType: 3, // Return
                    quantity: quantity, // positive = stock in
                    unitCost: null,
                    referenceId: orderId,
                    referenceType: "Return",
                    userId: userId);
            }
        }

        /// <summary>
        /// Return stock for a single item in a partial return (frmReturns).
        /// Called inside ReturnService transaction to maintain atomicity.
        /// </summary>
        public static async Task ReturnStockForItemAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int productId,
            int? productSizeId,
            int quantity,
            int returnId,
            int userId)
        {
            await InsertMovementAsync(
                conn, trans,
                productId: productId,
                productSizeId: productSizeId,
                transactionType: 3,   // Return — stock in
                quantity: quantity,   // موجب = إضافة للمخزون
                unitCost: null,
                referenceId: returnId,
                referenceType: "Return",
                userId: userId);
        }

        /// <summary>
        /// Get current stock balance for a product (or product+size).
        /// Returns the BalanceAfter from the most recent movement.
        /// </summary>
        public static async Task<decimal> GetCurrentStockAsync(int productId, int? productSizeId = null)
        {
            string query = productSizeId.HasValue
                ? @"SELECT TOP 1 BalanceAfter FROM Inventory
                    WHERE ProductID = @ProductID AND ProductSizeID = @SizeID
                    ORDER BY CreatedDate DESC, InventoryID DESC"
                : @"SELECT TOP 1 BalanceAfter FROM Inventory
                    WHERE ProductID = @ProductID AND ProductSizeID IS NULL
                    ORDER BY CreatedDate DESC, InventoryID DESC";

            var parameters = new List<SqlParameter>
            {
                new("@ProductID", productId)
            };
            if (productSizeId.HasValue)
                parameters.Add(new("@SizeID", productSizeId.Value));

            object? result = await DB_Server.ScalarAsync(query, parameters.ToArray());
            return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }

        /// <summary>
        /// Record a manual stock adjustment (from inventory management screen).
        /// </summary>
        public static async Task<bool> AdjustStockAsync(
            int productId,
            int? productSizeId,
            decimal quantity,
            decimal? unitCost,
            string notes,
            int userId)
        {
            return await DB_Server.ExecuteTransactionAsync(async (conn, trans) =>
            {
                await InsertMovementAsync(
                    conn, trans,
                    productId, productSizeId,
                    transactionType: 2, // Adjustment
                    quantity: quantity,
                    unitCost: unitCost,
                    referenceId: null,
                    referenceType: "Adjustment",
                    userId: userId);
            });
        }

        /// <summary>
        /// Core: insert a single inventory movement and compute running balance.
        /// </summary>
        private static async Task InsertMovementAsync(
            SqlConnection conn,
            SqlTransaction trans,
            int productId,
            int? productSizeId,
            int transactionType,
            decimal quantity,
            decimal? unitCost,
            int? referenceId,
            string referenceType,
            int userId)
        {
            // Get current balance WITHIN the transaction (row-level lock via UPDLOCK)
            string balanceQuery = productSizeId.HasValue
                ? @"SELECT TOP 1 BalanceAfter FROM Inventory WITH (UPDLOCK)
                    WHERE ProductID = @ProductID AND ProductSizeID = @SizeID
                    ORDER BY CreatedDate DESC, InventoryID DESC"
                : @"SELECT TOP 1 BalanceAfter FROM Inventory WITH (UPDLOCK)
                    WHERE ProductID = @ProductID AND ProductSizeID IS NULL
                    ORDER BY CreatedDate DESC, InventoryID DESC";

            decimal currentBalance = 0;
            using (var balCmd = new SqlCommand(balanceQuery, conn, trans))
            {
                balCmd.Parameters.AddWithValue("@ProductID", productId);
                if (productSizeId.HasValue)
                    balCmd.Parameters.AddWithValue("@SizeID", productSizeId.Value);

                object? result = await balCmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    currentBalance = Convert.ToDecimal(result);
            }

            decimal newBalance = currentBalance + quantity;

            string insertQuery = @"
                INSERT INTO Inventory
                (ProductID, ProductSizeID, TransactionType, Quantity, UnitCost,
                 ReferenceID, ReferenceType, BalanceAfter, CreatedByUserID, CreatedDate)
                VALUES
                (@ProductID, @SizeID, @TransType, @Qty, @UnitCost,
                 @RefID, @RefType, @Balance, @UserID, GETDATE())";

            using var cmd = new SqlCommand(insertQuery, conn, trans);
            cmd.Parameters.AddWithValue("@ProductID", productId);
            cmd.Parameters.AddWithValue("@SizeID", productSizeId.HasValue ? productSizeId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@TransType", transactionType);
            cmd.Parameters.AddWithValue("@Qty", quantity);
            cmd.Parameters.AddWithValue("@UnitCost", unitCost.HasValue ? unitCost.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@RefID", referenceId.HasValue ? referenceId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@RefType", referenceType ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Balance", newBalance);
            cmd.Parameters.AddWithValue("@UserID", userId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
