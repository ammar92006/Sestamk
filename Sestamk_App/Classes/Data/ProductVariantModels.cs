namespace Sestamk.Classes.Data
{
    public class ProductSizeModel
    {
        public int ProductSizeID { get; set; }
        public int ProductID { get; set; }
        public string ProductNameAr { get; set; }
        public int SizeID { get; set; }
        public string SizeNameAr { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal VAT { get; set; }
        public string Barcode { get; set; }
        public byte[] ProductImage { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductAddonModel
    {
        public int ProductAddonID { get; set; }
        public int ProductID { get; set; }
        public string ProductNameAr { get; set; }
        public int AddonID { get; set; }
        public string AddonNameAr { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal VAT { get; set; }
        public byte[] ProductImage { get; set; }
        public bool IsActive { get; set; }
    }
}
