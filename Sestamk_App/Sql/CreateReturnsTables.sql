-- ══════════════════════════════════════════════════════════
--  سكربت إنشاء جداول المرتجعات — DB_Sestamk
--  نفذ هذا السكربت مرة واحدة على قاعدة DB_Sestamk
--  آمن للتشغيل أكثر من مرة (IF NOT EXISTS)
-- ══════════════════════════════════════════════════════════

USE DB_Sestamk;
GO

-- ══════════════════════════════════════════════════════════
--  1. جدول Returns (رأس المرتجع)
-- ══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Returns')
BEGIN
    CREATE TABLE Returns (
        ReturnID            INT           IDENTITY(1,1) PRIMARY KEY,
        ReturnNumber        NVARCHAR(30)  NOT NULL,           -- RET-20260512-001
        OriginalOrderID     INT           NOT NULL,
        OriginalOrderNumber NVARCHAR(30)  NOT NULL DEFAULT '',
        ReturnDate          DATETIME      NOT NULL DEFAULT GETDATE(),
        CustomerID          INT           NULL,
        UserID              INT           NOT NULL,
        ShiftID             INT           NULL,
        ReturnReason        NVARCHAR(500) NOT NULL DEFAULT '',
        TotalReturnAmount   DECIMAL(18,3) NOT NULL DEFAULT 0,
        -- 0=نقدي  1=بطاقة  2=رصيد آجل (قيد في حساب العميل)
        RefundMethod        INT           NOT NULL DEFAULT 0,
        Notes               NVARCHAR(500) NOT NULL DEFAULT '',
        CreatedDate         DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_Returns_Orders
            FOREIGN KEY (OriginalOrderID) REFERENCES Orders(OrderID),
        CONSTRAINT FK_Returns_Customers
            FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
    );

    PRINT '✅ تم إنشاء جدول Returns';
END
ELSE
    PRINT '⚠️ جدول Returns موجود مسبقاً — تم التخطي';
GO

-- ══════════════════════════════════════════════════════════
--  2. جدول ReturnItems (تفاصيل الأصناف المرتجعة)
-- ══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReturnItems')
BEGIN
    CREATE TABLE ReturnItems (
        ReturnItemID    INT           IDENTITY(1,1) PRIMARY KEY,
        ReturnID        INT           NOT NULL,
        OrderItemID     INT           NOT NULL,
        ProductID       INT           NOT NULL,
        ProductSizeID   INT           NULL,
        ProductName     NVARCHAR(200) NOT NULL DEFAULT '',
        SizeName        NVARCHAR(100) NOT NULL DEFAULT '',
        UnitPrice       DECIMAL(18,3) NOT NULL DEFAULT 0,
        ReturnQuantity  INT           NOT NULL DEFAULT 1,
        LineTotal       DECIMAL(18,3) NOT NULL DEFAULT 0,   -- UnitPrice * ReturnQuantity

        CONSTRAINT FK_ReturnItems_Returns
            FOREIGN KEY (ReturnID) REFERENCES Returns(ReturnID),
        CONSTRAINT FK_ReturnItems_OrderItems
            FOREIGN KEY (OrderItemID) REFERENCES OrderItems(OrderItemID)
    );

    -- فهرس لتسريع البحث عن المرتجعات لفاتورة معينة
    CREATE NONCLUSTERED INDEX IX_ReturnItems_ReturnID
        ON ReturnItems (ReturnID);

    CREATE NONCLUSTERED INDEX IX_ReturnItems_OrderItemID
        ON ReturnItems (OrderItemID);

    PRINT '✅ تم إنشاء جدول ReturnItems';
END
ELSE
    PRINT '⚠️ جدول ReturnItems موجود مسبقاً — تم التخطي';
GO

-- ══════════════════════════════════════════════════════════
--  3. فهرس على Returns لتسريع البحث بالتاريخ والفاتورة الأصلية
-- ══════════════════════════════════════════════════════════
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Returns_ReturnDate' AND object_id = OBJECT_ID('Returns'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Returns_ReturnDate
        ON Returns (ReturnDate DESC);
    PRINT '✅ تم إنشاء فهرس IX_Returns_ReturnDate';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Returns_OriginalOrderID' AND object_id = OBJECT_ID('Returns'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Returns_OriginalOrderID
        ON Returns (OriginalOrderID);
    PRINT '✅ تم إنشاء فهرس IX_Returns_OriginalOrderID';
END
GO

-- ══════════════════════════════════════════════════════════
--  4. إضافة عمود HasReturn لجدول Orders (مؤشر سريع)
--     يُحدَّث تلقائياً عند حفظ أي مرتجع
-- ══════════════════════════════════════════════════════════
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Orders') AND name = 'HasReturn')
BEGIN
    ALTER TABLE Orders ADD HasReturn BIT NOT NULL DEFAULT 0;
    PRINT '✅ تم إضافة عمود HasReturn لجدول Orders';
END
ELSE
    PRINT '⚠️ عمود HasReturn موجود مسبقاً';
GO

-- ══════════════════════════════════════════════════════════
--  5. التحقق النهائي
-- ══════════════════════════════════════════════════════════
PRINT '';
PRINT '══ التحقق النهائي ══';
SELECT 'Returns'     AS [الجدول], COUNT(*) AS [السجلات] FROM Returns
UNION ALL
SELECT 'ReturnItems' AS [الجدول], COUNT(*) AS [السجلات] FROM ReturnItems;
GO
