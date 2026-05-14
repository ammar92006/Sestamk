-- ══════════════════════════════════════════════════════════
--  سكربت التحقق من جداول تقارير المبيعات
--  نفذ هذا السكربت على قاعدة DB_Sestamk للتأكد من وجود كل الجداول
--  ملاحظة: هذا السكربت للقراءة فقط — لا يُعدل أي بيانات
-- ══════════════════════════════════════════════════════════

-- 1. التحقق من جدول Orders
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
    PRINT '✅ جدول Orders موجود'
ELSE
    PRINT '❌ جدول Orders غير موجود — مطلوب لتقارير الفواتير'

-- 2. التحقق من جدول OrderItems
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
    PRINT '✅ جدول OrderItems موجود'
ELSE
    PRINT '❌ جدول OrderItems غير موجود — مطلوب لتقارير المنتجات'

-- 3. التحقق من جدول Payments
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Payments')
    PRINT '✅ جدول Payments موجود'
ELSE
    PRINT '❌ جدول Payments غير موجود — مطلوب لتقارير طرق الدفع'

-- 4. التحقق من جدول Shifts
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Shifts')
    PRINT '✅ جدول Shifts موجود'
ELSE
    PRINT '❌ جدول Shifts غير موجود — مطلوب لتقارير الورديات'

-- 5. التحقق من جدول Customers
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Customers')
    PRINT '✅ جدول Customers موجود'
ELSE
    PRINT '❌ جدول Customers غير موجود — مطلوب لتقارير العملاء'

-- 6. التحقق من جدول Users
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users')
    PRINT '✅ جدول Users موجود'
ELSE
    PRINT '❌ جدول Users غير موجود — مطلوب لتقارير الكاشيرين'

-- 7. التحقق من جدول Products
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Products')
    PRINT '✅ جدول Products موجود'
ELSE
    PRINT '❌ جدول Products غير موجود — مطلوب لتقارير المنتجات'

-- 8. التحقق من جدول ProductCategories
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductCategories')
    PRINT '✅ جدول ProductCategories موجود'
ELSE
    PRINT '❌ جدول ProductCategories غير موجود — مطلوب لتقارير الأقسام'

PRINT ''
PRINT '══════════════════════════════════════════'
PRINT '  اختبار سريع — عدد السجلات في كل جدول'
PRINT '══════════════════════════════════════════'

IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders')
    SELECT 'Orders' AS [الجدول], COUNT(*) AS [عدد السجلات] FROM Orders
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems')
    SELECT 'OrderItems' AS [الجدول], COUNT(*) AS [عدد السجلات] FROM OrderItems
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Payments')
    SELECT 'Payments' AS [الجدول], COUNT(*) AS [عدد السجلات] FROM Payments
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Shifts')
    SELECT 'Shifts' AS [الجدول], COUNT(*) AS [عدد السجلات] FROM Shifts
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Customers')
    SELECT 'Customers' AS [الجدول], COUNT(*) AS [عدد السجلات] FROM Customers
