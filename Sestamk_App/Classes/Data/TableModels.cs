using System;

namespace Sestamk.Classes.Data
{
    /// <summary>
    /// نموذج القسم (منطقة الطاولات) — يعكس جدول Sections
    /// </summary>
    public class SectionModel
    {
        public long Id { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// نموذج الطاولة — يعكس جدول Tables
    /// </summary>
    public class TableModel
    {
        public long Id { get; set; }
        public long SectionId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = "available";   // available, occupied, reserved, cleaning, maintenance
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; } = string.Empty;

        // ═══ ميزات متقدمة ═══
        public string QRCode { get; set; }
        public int? WaiterId { get; set; }
        public DateTime? OpenedAt { get; set; }

        // ═══ بيانات العرض (من JOIN) ═══
        public string SectionName { get; set; } = string.Empty;

        /// <summary>
        /// نص الحالة بالعربية
        /// </summary>
        public string StatusText => Status?.ToLower() switch
        {
            "available" => "متاحة",
            "occupied" => "مشغولة",
            "reserved" => "محجوزة",
            "cleaning" => "تنظيف",
            "maintenance" => "صيانة",
            _ => Status ?? ""
        };

        /// <summary>
        /// اسم العرض: "رقم - اسم"
        /// </summary>
        public string DisplayName => $"{TableNumber} - {TableName}";
    }
}
