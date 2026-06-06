using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static Azure.Core.HttpHeader;

namespace Sestamk.UserControl
{
    public partial class UC_ProductCategories : System.Windows.Forms.UserControl
    {
        public event EventHandler OnAddClicked;
        public event EventHandler OnDeleteClicked;

        public UC_ProductCategories()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            lblIsActive.Click += btnAdd_Click;
            guna2PictureBox1.Click += btnAdd_Click;
            guna2Panel2.Click += btnAdd_Click;
            guna2Panel4.Click += btnAdd_Click;
            CategoryColor.Click += btnAdd_Click;
            lblCategoryCode.Click += btnAdd_Click;
            lblNameAr.Click += btnAdd_Click;
            lblNameEn.Click += btnAdd_Click;
            picCategory.Click += btnAdd_Click;
            lblCountproducts.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;
            this.Click += btnAdd_Click;
        }
        public UC_ProductCategories(dynamic Categorie) : this()
        {
            if (Categorie != null)
            {
                this.CategoryID = Categorie.CategoryID;
                this.CategoryCode = Categorie.CategoryCode;
                this.CategoryNameAr = Categorie.CategoryNameAr;
                this.CategoryNameEn = Categorie.CategoryNameEn;
                this.Colorhex = ColorTranslator.FromHtml(Categorie.HexCode ?? "#FFFFFF");
                this.IsActive = Categorie.IsActive;
                this.ColorID = Categorie.ColorID;
                this.Countproducts = Categorie.Countproducts;
                this.CategoryNotes = Categorie.Notes;
                this.CreatedDate = Categorie.CreatedDate;
                this.CreatedBy = Categorie.CreatedBy;
                this.LastModified = Categorie.LastModified;
                this.CategoryTypeID = Categorie.CategoryTypeID;
                // إذا كان الحقل "Image" في الداتابيز عبارة عن Base64
                this.ProductImageBase64 = Categorie.Image;
            }
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            OnAddClicked?.Invoke(this, e);
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            OnDeleteClicked?.Invoke(this, e);
        }

        #region Private Fields

        private int _categoryID;
        private string _categoryCode;
        private string _categoryNameAr;
        private string _categoryNameEn;
        private string _categoryImageBase64;
        private string _categoryNotes;
        private string _createdDate;
        private string _createdBy;
        private string _lastModified;
        private int _categoryTypeID;
        private int _colorID;
        private bool _isActive;
        private bool _iscardselected;
        private Color _colorhex;
        private int _countproducts;

        #endregion

        #region Public Properties

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CategoryID { get => _categoryID; set => _categoryID = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CategoryNotes { get => _categoryNotes; set => _categoryNotes = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CreatedBy { get => _createdBy; set => _createdBy = value; }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CreatedDate { get => _createdDate; set => _createdDate = value; }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string LastModified { get => _lastModified; set => _lastModified = value; }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CategoryTypeID { get => _categoryTypeID; set => _categoryTypeID = value; }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Iscardselected
        {
            get => _iscardselected;
            set
            {
                _iscardselected = value;
                if (_iscardselected) 
                {
                    this.BackColor = Color.FromArgb(8, 0, 189); 
                    guna2Panel2.BorderColor = Color.FromArgb(0, 192, 0); 
                    guna2Panel2.FillColor = Color.FromArgb(27, 81, 67);
                    guna2Panel1.Dock = DockStyle.None;

                }
                else
                {
                    this.BackColor = Color.FromArgb(17, 25, 40);
                    //this.BackColor = Color.FromArgb(43, 43, 44);
                    //guna2Panel2.BorderColor = Color.FromArgb(84, 84, 84);
                    //guna2Panel2.FillColor = Color.FromArgb(43, 43, 44); 
                    guna2Panel1.Dock = DockStyle.Fill;
                }
            }
        }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CategoryCode
        {
            get => _categoryCode;
            set
            {
                _categoryCode = value; if (lblCategoryCode != null) lblCategoryCode.Text = "كود القسم : " + value;
            }
        }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (_isActive)
                {
                    lblIsActive.Text = "نشط";
                    lblIsActive.ForeColor = Color.FromArgb(71, 194, 117);
                    guna2Panel2.BorderColor = Color.FromArgb(0, 192, 0);
                    guna2Panel2.FillColor = Color.FromArgb(27, 81, 67);
                }
                else
                {
                    lblIsActive.Text = "غير نشط";
                    lblIsActive.ForeColor = Color.FromArgb(140, 140, 140);
                    guna2Panel2.BorderColor = Color.FromArgb(84, 84, 84);
                    guna2Panel2.FillColor = Color.FromArgb(43, 43, 44);
                }
            }
        }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ColorID
        {
            get => _colorID;
            set => _colorID = value;
        }
        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CategoryNameAr
        {
            get => _categoryNameAr;
            set { _categoryNameAr = value; if (lblNameAr != null) lblNameAr.Text = value; }
        }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string CategoryNameEn
        {
            get => _categoryNameEn;
            set { _categoryNameEn = value; if (lblNameEn != null) lblNameEn.Text = value; }
        }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int Countproducts
        {
            get => _countproducts;
            set { _countproducts = value; if (lblCountproducts != null) lblCountproducts.Text = Convert.ToString(value); }
        }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color Colorhex
        {
            get => _colorhex;
            set
            {
                _colorhex = value;
                if (CategoryColor != null)
                {
                    CategoryColor.FillColor = value;
                    CategoryColor.ShadowDecoration.Enabled = true;
                    CategoryColor.ShadowDecoration.Color = value;
                }
                ;
            }
        }

        [Category("Product UI")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image CategoryImage
        {
            get => picCategory?.Image;
            set { if (picCategory != null) picCategory.Image = value; }
        }

        [Category("Product Data")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ProductImageBase64
        {
            get => _categoryImageBase64;
            set
            {
                _categoryImageBase64 = value;
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        byte[] imgBytes = Convert.FromBase64String(value);
                        using (MemoryStream ms = new MemoryStream(imgBytes))
                        {
                            CategoryImage = new Bitmap(ms);
                        }
                    }
                    catch { CategoryImage = Properties.Resources._1772674733217_019cbba5_6fa6_7c10_9fce_5e3dc87bf76b; }
                }
                else
                {
                    CategoryImage = Properties.Resources._1772674733217_019cbba5_6fa6_7c10_9fce_5e3dc87bf76b; ;
                }
            }
        }

        #endregion
    }
}