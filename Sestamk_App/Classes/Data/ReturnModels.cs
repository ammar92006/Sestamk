using System;
using System.Collections.Generic;

namespace Sestamk.Classes.Data
{
    /// <summary>
    /// نموذج المرتجع الكامل — يُستخدم في الذاكرة أثناء تسجيل المرتجع ثم يُحفظ في DB
    /// </summary>
    public class ReturnModel
    {
        public int ReturnID { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;       // RET-20260512-001
        public int OriginalOrderID { get; set; }
        public string OriginalOrderNumber { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public int? CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int UserID { get; set; }
        public int ShiftID { get; set; }

        public string ReturnReason { get; set; } = string.Empty;
        public decimal TotalReturnAmount { get; set; }

        // 0=نقدي  1=بطاقة  2=رصيد آجل
        public int RefundMethod { get; set; }
        public string Notes { get; set; } = string.Empty;

        public List<ReturnItemModel> Items { get; set; } = new();

        public string RefundMethodText => RefundMethod switch
        {
            0 => "نقدي",
            1 => "بطاقة",
            2 => "رصيد آجل",
            _ => "غير محدد"
        };

        /// <summary>
        /// يعيد حساب إجمالي المرتجع من الأصناف المحددة
        /// </summary>
        public void Recalculate()
        {
            TotalReturnAmount = 0;
            foreach (var item in Items)
            {
                item.LineTotal = item.UnitPrice * item.ReturnQuantity;
                TotalReturnAmount += item.LineTotal;
            }
        }
    }

    /// <summary>
    /// صنف واحد في المرتجع — يرتبط بسطر OrderItem من الفاتورة الأصلية
    /// </summary>
    public class ReturnItemModel
    {
        public int ReturnItemID { get; set; }
        public int ReturnID { get; set; }
        public int OrderItemID { get; set; }
        public int ProductID { get; set; }
        public int? ProductSizeID { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string SizeName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }

        // الكمية الأصلية في الفاتورة (للتحقق — لا تُحفظ في DB)
        public int OriginalQuantity { get; set; }

        // الكمية التي تم إرجاعها في مرتجعات سابقة (للتحقق — لا تُحفظ في DB)
        public int AlreadyReturnedQuantity { get; set; }

        // الكمية المتاحة للإرجاع = الأصلية - المُرجعة مسبقاً
        public int AvailableToReturn => OriginalQuantity - AlreadyReturnedQuantity;

        // الكمية التي يريد المستخدم إرجاعها الآن
        public int ReturnQuantity { get; set; }

        public decimal LineTotal { get; set; }  // UnitPrice * ReturnQuantity

        // للعرض في الجريد
        public bool IsSelected { get; set; }
        public string DisplayName => string.IsNullOrEmpty(SizeName)
            ? ProductName
            : $"{ProductName} ({SizeName})";
    }

    /// <summary>
    /// ملخص المرتجعات لتقارير فترة زمنية
    /// </summary>
    public class ReturnsSummary
    {
        public int TotalReturns { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public int TotalReturnedItems { get; set; }
        public decimal CashRefunds { get; set; }
        public decimal CardRefunds { get; set; }
        public decimal CreditRefunds { get; set; }
    }
}
