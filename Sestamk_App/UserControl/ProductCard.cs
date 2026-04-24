using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    public partial class ProductCard : System.Windows.Forms.UserControl
    {
        // تعريف الحدث الذي تطلبه الواجهة الرئيسية
        // استخدام EventHandler بشكل صريح ليتوافق مع Card_OnAddClicked
        public event EventHandler OnAddClicked;

        public ProductCard()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            lblName.Click += btnAdd_Click;
            lblDescription.Click += btnAdd_Click;
            picProduct.Click += btnAdd_Click;
            btnAdd.Click += btnAdd_Click;


        }

        // المشيد الجديد الذي يأخذ كائن المنتج كـ Argument
        public ProductCard(dynamic product) : this()
        {
            if (product != null)
            {
                this.ProductID = product.ProductID;
                this.ProductCode = product.ProductCode;
                this.ProductNameAr = product.ProductNameAr;
                this.ProductNameEn = product.ProductNameEn;
                this.CategoryID = product.CategoryID;
                this.ProductDescription = product.Description;
                this.DiscountPercent = (decimal)product.DiscountPercent;
                this.IsActive = product.IsActive;
                this.PreparationTime = product.PreparationTime?.ToString();
                this.PreNotes = product.Notes;
                this.ProductImageBase64 = product.Image;
            }
        }

        // دالة يتم استدعاؤها عند الضغط على زر الإضافة في الـ UserControl
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // استدعاء الحدث مع تمرير الكائن الحالي والبيانات
            OnAddClicked?.Invoke(this, e);
        }

        #region Private Fields
        private int _productid;
        private string _productCode;
        private string _productNameAr;
        private string _productNameEn;
        private int _categoryID;
        private decimal _productdiscountPercent;
        private bool _productisActive;
        private string _productPreparationTime;
        private string _productpreNotes;
        //private decimal _price;
        private string _productDescription;
        private string _productImageBase64;
        #endregion

        #region Public Properties

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ProductID { get => _productid; set => _productid = value; }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ProductCode { get => _productCode; set => _productCode = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ProductNameAr
        {
            get => _productNameAr;
            set { _productNameAr = value; if (lblName != null) lblName.Text = value; }
        }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ProductNameEn { get => _productNameEn; set => _productNameEn = value; }


        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CategoryID { get => _categoryID; set => _categoryID = value; }

        //[Category("Product Data")]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        //public decimal Price 
        //{ 
        //    get => _price;
        //    set 
        //    { 
        //        _price = value; 
        //        if (lblDescription != null) lblDescription.Text = value > 0 ? $"{value:N2} ج.م" : ""; 
        //    }
        //}

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public decimal DiscountPercent { get => _productdiscountPercent; set => _productdiscountPercent = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsActive { get => _productisActive; set => _productisActive = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PreparationTime { get => _productPreparationTime; set => _productPreparationTime = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string PreNotes { get => _productpreNotes; set => _productpreNotes = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ProductDescription { 
            get => _productDescription;
            set { _productDescription = value; if (lblDescription != null) lblDescription.Text = value; } 
        }
        [Category("Product UI")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image ProductImage
        {
            get => picProduct?.Image;
            set { if (picProduct != null) picProduct.Image = value; }
        }
        private static readonly HttpClient client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        [Category("Product Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProductImageBase64
        {
            get => _productImageBase64;
            set
            {
                _productImageBase64 = value;
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        if (!value.StartsWith("http"))
                        {
                            byte[] imgBytes = Convert.FromBase64String(value);
                            using (MemoryStream ms = new MemoryStream(imgBytes))
                            {
                                ProductImage = new Bitmap(ms);
                            }
                        }
                        else
                        {
                        }
                    }
                    catch { ProductImage = null; }
                }
            }
        }
        #endregion
    }
}