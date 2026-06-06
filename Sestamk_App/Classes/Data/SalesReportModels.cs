using System;
using System.Collections.Generic;

namespace Sestamk.Classes.Data
{
    /// <summary>
    /// فلاتر التقارير — يُمرر لكل دوال SalesReportService
    /// </summary>
    public class ReportFilters
    {
        public DateTime DateFrom { get; set; } = DateTime.Today;
        public DateTime DateTo { get; set; } = DateTime.Today;
        public int? OrderType { get; set; }       // null = الكل, 0=تيك اوي, 1=صالة, 2=دليفري
        public int? PaymentMethod { get; set; }    // null = الكل, 0=نقدي, 1=بطاقة, 2=آجل, 3=تحويل
        public int? UserID { get; set; }           // null = الكل
        public int? CustomerID { get; set; }       // null = الكل
        public bool ExcludeVoided { get; set; } = true;
    }

    /// <summary>
    /// بطاقات الملخص — 6 بطاقات في أعلى الفورم
    /// </summary>
    public class SalesSummary
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal TotalDiscounts { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetSales { get; set; }
        public decimal TotalCashPayments { get; set; }
        public decimal TotalCardPayments { get; set; }
        public decimal TotalCreditPayments { get; set; }
        public int VoidedOrders { get; set; }
    }
}
