using System;
using System.Collections.Generic;

namespace Sestamk.Classes.Data
{
    /// <summary>
    /// نموذج الطلب الكامل — يُستخدم في الذاكرة أثناء البيع ثم يُحفظ في DB
    /// </summary>
    public class OrderModel
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int ShiftID { get; set; }
        public int? CustomerID { get; set; }
        public int UserID { get; set; }
        public int OrderType { get; set; }  // 0=تيك اوي, 1=صالة, 2=دليفري
        public int? TableID { get; set; }
        public int? DriverID { get; set; }         // delivery_staff.delivery_id — الطيار
        public string DriverName { get; set; } = string.Empty;  // اسم الطيار للعرض

        // ═══ المبالغ ═══
        public decimal SubTotal { get; set; }           // مجموع الأصناف قبل الخصم والضريبة
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal ServiceAmount { get; set; }       // خدمة الصالة/التوصيل
        public decimal TaxPercent { get; set; } = 14m;
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }         // الإجمالي النهائي
        public decimal PaidAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal RemainingAmount { get; set; }     // للآجل

        // ═══ الحالة ═══
        public int Status { get; set; }   // 0=جديد, 1=قيد التحضير, 2=جاهز, 3=مُسلَّم, 4=ملغي, 5=مرتجع
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
        public bool IsVoided { get; set; }
        public string VoidReason { get; set; } = string.Empty;

        // ═══ العناصر والمدفوعات ═══
        public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();
        public List<PaymentModel> Payments { get; set; } = new List<PaymentModel>();

        // ═══ بيانات العرض ═══
        public string CustomerName { get; set; } = "عميل نقدي";
        public string CashierName { get; set; } = string.Empty;

        /// <summary>
        /// نص نوع الطلب بالعربية
        /// </summary>
        public string OrderTypeText => OrderType switch
        {
            0 => "تيك اوي",
            1 => "صالة",
            2 => "دليفري",
            _ => "غير محدد"
        };

        /// <summary>
        /// توليد رقم فاتورة فريد
        /// </summary>
        public static string GenerateOrderNumber()
        {
            return $"INV-{DateTime.Now:yyyyMMdd-HHmmss}-{new Random().Next(100, 999)}";
        }

        /// <summary>
        /// إعادة حساب كل المبالغ من الأصناف
        /// </summary>
        public void Recalculate()
        {
            SubTotal = 0;
            foreach (var item in Items)
            {
                if (!item.IsVoided)
                {
                    item.LineTotal = item.UnitPrice * item.Quantity - item.DiscountAmount;
                    SubTotal += item.LineTotal;

                    // حساب ضريبة الصنف
                    foreach (var addon in item.Addons)
                    {
                        addon.LineTotal = addon.UnitPrice * addon.Quantity;
                        SubTotal += addon.LineTotal;
                    }
                }
            }

            // حساب الإجمالي
            decimal afterDiscount = SubTotal - DiscountAmount;
            if (afterDiscount < 0) afterDiscount = 0;

            TaxAmount = Math.Round(afterDiscount * TaxPercent / 100m, 2);
            TotalAmount = afterDiscount + ServiceAmount + TaxAmount;

            // حساب الباقي
            ChangeAmount = PaidAmount > TotalAmount ? PaidAmount - TotalAmount : 0;
            RemainingAmount = PaidAmount < TotalAmount ? TotalAmount - PaidAmount : 0;
        }
    }

    /// <summary>
    /// صنف واحد في الطلب (منتج + حجم)
    /// </summary>
    public class OrderItemModel
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public int? ProductSizeID { get; set; }

        // ═══ بيانات ثابتة وقت البيع (لا تتأثر بتغيير المنتج لاحقاً) ═══
        public string ProductName { get; set; } = string.Empty;
        public string SizeName { get; set; } = string.Empty;
        public string DisplayName => string.IsNullOrEmpty(SizeName)
            ? ProductName
            : $"{ProductName} ({SizeName})";

        // ═══ الأسعار ═══
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; } // UnitPrice * Quantity - Discount

        // ═══ الملاحظات والحالة ═══
        public string Notes { get; set; } = string.Empty;
        public bool IsVoided { get; set; }

        // ═══ الإضافات ═══
        public List<OrderItemAddonModel> Addons { get; set; } = new List<OrderItemAddonModel>();

        /// <summary>
        /// إجمالي الصنف شامل الإضافات
        /// </summary>
        public decimal TotalWithAddons
        {
            get
            {
                decimal total = UnitPrice * Quantity - DiscountAmount;
                foreach (var addon in Addons)
                    total += addon.LineTotal;
                return total;
            }
        }
    }

    /// <summary>
    /// إضافة واحدة على صنف في الطلب
    /// </summary>
    public class OrderItemAddonModel
    {
        public int OrderItemAddonID { get; set; }
        public int OrderItemID { get; set; }
        public int AddonID { get; set; }
        public string AddonName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// عملية دفع واحدة (يمكن تعدد طرق الدفع في فاتورة واحدة)
    /// </summary>
    public class PaymentModel
    {
        public int PaymentID { get; set; }
        public int OrderID { get; set; }
        public int PaymentMethod { get; set; }  // 0=نقدي, 1=بطاقة, 2=آجل, 3=تحويل
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// نص طريقة الدفع بالعربية
        /// </summary>
        public string PaymentMethodText => PaymentMethod switch
        {
            0 => "نقدي",
            1 => "بطاقة",
            2 => "آجل",
            3 => "تحويل",
            _ => "غير محدد"
        };
    }

    /// <summary>
    /// نموذج الوردية
    /// </summary>
    public class ShiftModel
    {
        public int ShiftID { get; set; }
        public int UserID { get; set; }
        public DateTime OpenDateTime { get; set; } = DateTime.Now;
        public DateTime? CloseDateTime { get; set; }
        public decimal OpeningCash { get; set; }
        public decimal ClosingCash { get; set; }
        public decimal ExpectedCash { get; set; }    // محسوب تلقائياً
        public decimal CashDifference { get; set; }  // الفرق
        public decimal TotalSales { get; set; }
        public decimal TotalRefunds { get; set; }
        public int TotalOrders { get; set; }
        public int Status { get; set; }   // 0=مفتوحة, 1=مغلقة
        public string Notes { get; set; } = string.Empty;
        public int? ClosedByUserID { get; set; }

        // ═══ بيانات العرض ═══
        public string UserName { get; set; } = string.Empty;
        public string StatusText => Status == 0 ? "مفتوحة" : "مغلقة";
        public TimeSpan Duration => (CloseDateTime ?? DateTime.Now) - OpenDateTime;
    }
}
