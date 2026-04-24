using System;

namespace Sestamk.Classes.Data
{
    /// <summary>
    /// نموذج الطيار (سائق التوصيل) — يعكس جدول delivery_staff
    /// </summary>
    public class DeliveryStaffModel
    {
        public int DeliveryId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;    // موتوسيكل / عجلة
        public string LicenseNumber { get; set; } = string.Empty;
        public decimal DeliveryFee { get; set; }                    // سعر التوصيلة
        public string SalaryType { get; set; } = string.Empty;     // per_order / daily / monthly
        public TimeSpan? ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? HireDate { get; set; }
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// نص نوع المركبة بالعربية
        /// </summary>
        public string VehicleTypeText => VehicleType?.ToLower() switch
        {
            "motorcycle" or "موتوسيكل" => "🛵 موتوسيكل",
            "bicycle" or "عجلة" => "🚲 عجلة",
            "car" or "سيارة" => "🚗 سيارة",
            _ => VehicleType ?? ""
        };

        /// <summary>
        /// نص نوع المرتب بالعربية
        /// </summary>
        public string SalaryTypeText => SalaryType?.ToLower() switch
        {
            "per_order" => "بالأوردر",
            "daily" => "يومي",
            "monthly" => "شهري",
            _ => SalaryType ?? ""
        };
    }
}
