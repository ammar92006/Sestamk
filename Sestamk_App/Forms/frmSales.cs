using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using Sestamk.UserControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Xml.Linq;
using System.Runtime.InteropServices;

namespace Sestamk.Forms
{
    public partial class frmSales : BaseForm
    {
        // Win32 API لإخفاء الاسكرول بار
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private const int SB_HORZ = 0;

        private BindingList<ProductCategories> ProductCategoriesList = new BindingList<ProductCategories>();
        private BindingList<Products> ProductsList = new BindingList<Products>();
        private BindingSource ProductCategoriesBinding = new BindingSource();
        private BindingSource ProductsBinding = new BindingSource();
        private Guna2Button selectedCategoryButton = null;
        private int _selectedCategoryID = -1; // -1 = الكل

        // متغيرات السحب للتمرير - المنتجات
        private bool _isDraggingScroll = false;
        private Point _dragStartPoint;
        private Point _scrollStartOffset;

        // متغيرات السحب للتمرير - الأقسام
        private bool _isCatDragging = false;
        private Point _catDragStart;
        private Point _catScrollStart;

        // متغيرات السحب للتمرير - الفاتورة
        private bool _isDraggingInvoice = false;
        private Point _invoiceDragStart;
        private Point _invoiceScrollStart;


        // الاعدادات الافتراضية
        private int _defaultbtn = 2;
        private int _Selected_invoice_type;

        // ═══════════════════════════════════════════
        //  🆕 قائمة أصناف الفاتورة في الذاكرة (بدل قراءة من UI Controls)
        // ═══════════════════════════════════════════
        private List<OrderItemModel> _invoiceItems = new List<OrderItemModel>();

        // ═══════════════════════════════════════════
        //  متغيرات العميل المحدد
        // ═══════════════════════════════════════════
        private Customer _selectedCustomer = null;
        private UC_CustomerPicker _customerPicker = null;

        // ═══════════════════════════════════════════
        //  متغيرات الطيار المحدد (دليفري)
        // ═══════════════════════════════════════════
        private DeliveryStaffModel _selectedDriver = null;
        private UC_DriverPicker _driverPicker = null;

        // ═══════════════════════════════════════════
        //  متغيرات الطاولة المحددة (صالة)
        // ═══════════════════════════════════════════
        private TableModel _selectedTable = null;
        private UC_TablePicker _tablePicker = null;

        // ═══════════════════════════════════════════
        //  هيكل بيانات الفاتورة المعلقة
        // ═══════════════════════════════════════════
        private static List<HeldInvoice> _heldInvoices = new List<HeldInvoice>();

        // ═══════════════════════════════════════════
        // Work Shift Overlay
        // ═══════════════════════════════════════════
        private UC_Work_Shift _workShiftPanel;

        // ═══════════════════════════════════════════
        // Occupied Tables Panel
        // ═══════════════════════════════════════════
        private FlowLayoutPanel flowOccupiedTables;
        private System.Windows.Forms.Timer _clockTimer;

        public frmSales()
        {
            InitializeComponent();

            // ═══════════════════════════════════════════════════════
            //  Guna2 Borderless Form — Resize + Corners + Shadow
            //  بديل احترافي لكل كود Win32 اليدوي
            // ═══════════════════════════════════════════════════════
            var borderlessForm = new Guna.UI2.WinForms.Guna2BorderlessForm();
            borderlessForm.ContainerControl = this;
            borderlessForm.BorderRadius = 20;
            borderlessForm.ResizeForm = true;
            borderlessForm.DragForm = false; // الغاء السحب التلقائي للاعتماد على Main_Methods.Attach
            //borderlessForm.ShadowColor = Color.FromArgb(0, 200, 255);  // Neon Blue shadow
            borderlessForm.AnimateWindow = true;
            borderlessForm.DockIndicatorTransparencyValue = 0.6;

            this.DoubleBuffered = true;
            this.MinimumSize = new Size(1024, 600);

            // Dock the products flow to fill available space
            flowProducts.Dock = DockStyle.Fill;

            LoadProductCategoriesAsync();
            LoadProductsAsync();
            formui();
            CenterControl();
            uC_ProductOptions1.OnAddToInvoice += UC_ProductOptions1_OnAddToInvoice;
            uC_ProductOptions1.OnCancel += (s, e) =>
            {
                uC_ProductOptions1.SelectedSize = null;
                uC_ProductOptions1.SelectedAddons.Clear();
                uC_ProductOptions1.Visible = false;
            };
            uC_ProductOptions1.OnSizeClicked += UC_ProductOptions1_OnSizeClicked;
            txtSearch.TextChanged += txtSearch_TextChanged;

            Main_Methods.Attach(pnlHeader, this);
            Main_Methods.Attach(guna2Panel9, this);
            Main_Methods.Attach(label3, this);
            Main_Methods.Attach(label5, this);
            Main_Methods.Attach(label2, this);
            Main_Methods.Attach(guna2Panel1, this);
            Main_Methods.Attach(panel1, this);
            Main_Methods.Attach(label1, this);
            Main_Methods.Attach(label6, this);
            Main_Methods.Attach(guna2Panel8, this);
            Main_Methods.Attach(guna2Panel7, this);
            Main_Methods.Attach(guna2Panel3, this);
            Main_Methods.Attach(guna2CirclePictureBox1, this);

            // 
            var Button0 = (Guna.UI2.WinForms.Guna2Button)btnSaffari;
            var Button1 = (Guna.UI2.WinForms.Guna2Button)btnTable;
            var Button2 = (Guna.UI2.WinForms.Guna2Button)btnDelivery;

            if (_defaultbtn == 0) // تيك اوي
            {
                UpdateSegmentSelection(Button0);
            }
            else if (_defaultbtn == 1) // طاوله
            {
                UpdateSegmentSelection(Button1);
            }
            else if (_defaultbtn == 2) // دليفري
            {
                UpdateSegmentSelection(Button2);
            }

            // ═══════════════════════════════════════════
            //  ربط أزرار الفورم الجديدة
            // ═══════════════════════════════════════════
            SetupCustomerPicker();
            SetupDriverPicker();
            SetupTablePicker();
            guna2Button7.Click += BtnSelectCustomer_Click;   // زر تحديد العميل
            guna2Button6.Click += BtnConfirmPayment_Click;   // زر تأكيد الدفع
            guna2Button3.Click += BtnHoldInvoice_Click;      // زر تعليق
            guna2Button5.Click += BtnClearInvoice_Click;     // زر تفريغ

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(label3, label5, guna2CirclePictureBox1);

            // ── اعداد لوحة الورديات ──
            SetupWorkShiftOverlay();

            // ── ربط أحداث أزرار الورديات في الهيدر ──
            guna2Button1.Click += BtnStartHeaderShift_Click; // بدأ الوردية بالهيدر
            guna2Button2.Click += BtnEndHeaderShift_Click;   // إنهاء الوردية بالهيدر
            
            // ── اعداد حاوية الطاولات المشغولة ──
            SetupOccupiedTablesFlow();

            guna2Panel6.Tag = "NoTheme";
            guna2Button6.Tag = "NoTheme";

            this.Load += frmSales_Load;

            // ── إعداد مؤقت الساعة ──
            _clockTimer = new System.Windows.Forms.Timer();
            _clockTimer.Interval = 1000;
            _clockTimer.Tick += (s, e) => {
                label6.Text = DateTime.Now.ToString("tt hh:mm - yyyy/MM/dd");
            };
            _clockTimer.Start();
            label6.Text = DateTime.Now.ToString("tt hh:mm - yyyy/MM/dd");
        }

        private async void frmSales_Load(object sender, EventArgs e)
        {
            await Sestamk.Classes.ShiftService.LoadCurrentShiftAsync(Sestamk.Classes.UserSession.UserId);
            
            if (!Sestamk.Classes.ShiftService.IsShiftOpen)
            {
                ShowShiftOverlay();
            }
            else
            {
                HideShiftOverlay();
            }

            await LoadOccupiedTablesAsync();
        }

        private void SetupWorkShiftOverlay()
        {
            _workShiftPanel = new UC_Work_Shift();
            _workShiftPanel.Dock = DockStyle.Fill;
            _workShiftPanel.OnShiftStarted += (s, ev) => 
            {
                HideShiftOverlay();
            };
            this.pn_container.Controls.Add(_workShiftPanel);
            _workShiftPanel.BringToFront();
            _workShiftPanel.Visible = false;
        }

        private void ShowShiftOverlay()
        {
            _workShiftPanel.LoadData();
            _workShiftPanel.Visible = true;
            _workShiftPanel.BringToFront();
            
            // أزرار الهيدر
            guna2Button1.Visible = true;   // بدأ الوردية
            guna2Button2.Visible = false;  // إنهاء الوردية
            label4.Text = "الوردية : مغلقة";
            label4.ForeColor = Color.Red;
            guna2Panel6.FillColor = Color.DarkRed;
        }

        private void HideShiftOverlay()
        {
            _workShiftPanel.Visible = false;
            
            // أزرار الهيدر
            guna2Button1.Visible = false;
            guna2Button2.Visible = true;
            label4.Text = "الوردية : بدأت";
            label4.ForeColor = Color.Lime;
            guna2Panel6.FillColor = Color.DarkGreen;
        }

        private async void BtnStartHeaderShift_Click(object sender, EventArgs e)
        {
            if (Sestamk.Classes.ShiftService.IsShiftOpen) return;
            var shift = await Sestamk.Classes.ShiftService.OpenShiftAsync(Sestamk.Classes.UserSession.UserId, 0, "بدأ الوردية من القائمة");
            if (shift != null)
            {
                HideShiftOverlay();
            }
        }

        private async void BtnEndHeaderShift_Click(object sender, EventArgs e)
        {
            if (!Sestamk.Classes.ShiftService.IsShiftOpen) return;

            if (frmInputCash.Show("إنهاء الوردية", "الرجاء إدخال المبلغ الموجود في الدرج (الكاش الفعلي) لإنهاء الشيفت.", out decimal closingCash))
            {
                bool closed = await Sestamk.Classes.ShiftService.CloseShiftAsync(closingCash, "إنهاء من واجهة المبيعات");
                if (closed)
                {
                    ShowShiftOverlay();
                }
            }
        }

        private void SetupOccupiedTablesFlow()
        {
            flowOccupiedTables = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = true,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 0)
            };
            
            // إضافة الحاوية الجديدة إلى لوحة الطاولات بالأسفل (guna2Panel16)
            // قمنا بتغيير خصائص Panel16 ليكون مرتبا
            guna2Panel16.Controls.Add(flowOccupiedTables);
            flowOccupiedTables.BringToFront();
        }

        private async Task LoadOccupiedTablesAsync()
        {
            try
            {
                // جلب الطاولات المشغولة
                string query = "SELECT t.*, s.SectionName FROM Tables t LEFT JOIN Sections s ON t.SectionId = s.Id WHERE t.Status = 'occupied' AND t.IsActive = 1";
                var dt = await Sestamk.Classes.DB_Server.GetTableAsync(query, null);
                
                flowOccupiedTables.SuspendLayout();
                flowOccupiedTables.Controls.Clear();

                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        var btn = new Guna.UI2.WinForms.Guna2Button
                        {
                            Name = "btnOccupiedTable_NoTheme",
                            Text = $"طاولة {row["TableNumber"]}",
                            Width = 100,
                            Height = 45,
                            BorderRadius = 10,
                            FillColor = Color.FromArgb(231, 76, 60), // لون مميز للمشغول (مثلاً أحمر)
                            ForeColor = Color.White,
                            Font = new Font("Alexandria", 11, FontStyle.Bold),
                            Margin = new Padding(5),
                            Tag = row["Id"]
                        };
                        
                        btn.Click += async (s, ev) => 
                        {
                            // يمكن لاحقاً فتح الفاتورة الخاصة بهذه الطاولة
                            ToastManager.ShowInfo("طاولة مشغولة", $"تم اختيار {btn.Text}");
                        };

                        flowOccupiedTables.Controls.Add(btn);
                    }
                }
                else
                {
                    var lbl = new Label
                    {
                        Text = "لا توجد طاولات مشغولة",
                        ForeColor = Color.Gray,
                        AutoSize = true,
                        Font = new Font("Alexandria", 11, FontStyle.Regular),
                        Margin = new Padding(20)
                    };
                    flowOccupiedTables.Controls.Add(lbl);
                }
                flowOccupiedTables.ResumeLayout();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading tables: " + ex.Message);
            }
        }



        private void CenterControl()
        {
            uC_ProductOptions1.Left = (this.ClientSize.Width - uC_ProductOptions1.Width) / 2;
            uC_ProductOptions1.Top = (this.ClientSize.Height - uC_ProductOptions1.Height) / 2;
        }
        private void formui()
        {
            flowProducts.AutoScroll = true;
            flowProducts.WrapContents = true;
            flowProducts.FlowDirection = FlowDirection.RightToLeft;
            flowProducts.Padding = new Padding(20);

            // تفعيل السحب بالماوس/التاتش للتمرير - المنتجات
            flowProducts.MouseDown += FlowProducts_MouseDown;
            flowProducts.MouseMove += FlowProducts_MouseMove;
            flowProducts.MouseUp += FlowProducts_MouseUp;

            // تفعيل السحب بالماوس/التاتش للتمرير - الأقسام
            flowLayoutPanelCategories.MouseDown += FlowCategories_MouseDown;
            flowLayoutPanelCategories.MouseMove += FlowCategories_MouseMove;
            flowLayoutPanelCategories.MouseUp += FlowCategories_MouseUp;

            // إخفاء الاسكرول الأفقي في الفاتورة
            flowInvoiceItems.AutoScroll = true;
            flowInvoiceItems.WrapContents = false;
            flowInvoiceItems.ControlAdded += (s, ev) =>
            {
                // إخفاء الاسكرول الأفقي بعد كل إضافة
                ShowScrollBar(flowInvoiceItems.Handle, SB_HORZ, false);
            };
            flowInvoiceItems.ControlRemoved += (s, ev) =>
            {
                ShowScrollBar(flowInvoiceItems.Handle, SB_HORZ, false);
            };
        }
        private void Card_OnAddClicked(object sender, EventArgs e)
        {
            //// 'sender' هو كائن الـ ProductCard الذي تم النقر عليه
            ProductCard clickedCard = sender as ProductCard;

            if (clickedCard != null)
            {
                // الآن يمكنك الوصول لبيانات المنتج من البطاقة التي نُقر عليها
                string code = clickedCard.ProductCode;
                string name = clickedCard.ProductNameAr;
                //decimal price = clickedCard.Price;
                if (sender is ProductCard card)
                {
                    uC_ProductOptions1.ProductID = clickedCard.ProductID;
                    uC_ProductOptions1.ProductNameAr = name;
                    uC_ProductOptions1.Visible = true;
                    //uC_ProductOptions1.OnSizeClicked += onsizeclick(sender,e);   
                    //MessageBox.Show($"إضافة {card.ProductNameAr} بسعر {card.Price:N2}");
                }
                // مثال: إضافة المنتج لشبكة المبيعات (DataGridView) أو السلة
                // AddToCart(code, name, price);

                //MessageBox.Show($"تم اختيار: {name}");
            }
        }

        private void UC_ProductOptions1_OnSizeClicked(object? sender, EventArgs e)
        {
            var size = uC_ProductOptions1.SelectedSize;
            if (size != null)
            {
                // Optional: You can add logic here to update the UI when a size is selected
                // For now, we'll keep it simple as the user requested linking the UI clicks.
                //MessageBox.Show($"تم اختيار الحجم: {size.SizeNameAr} بسعر {size.SalePrice:N2}");
            }
        }
        private async Task LoadProductCategoriesAsync()
        {
            try
            {
                string query = @"SELECT 
	                                CategoryID,
	                                CategoryCode,
	                                CategoryNameAr,
	                                CategoryNameEn,
	                                BackgroundColor,
	                                Image,
	                                IsActive,
	                                Notes,
	                                CreatedDate,
	                                CreatedBy,
	                                LastModified,
                                    CategoryTypeID,
	                                IsDeleted
                                FROM ProductCategories
                                Where IsDeleted = 0 AND IsActive = 1";
                DataTable dt = new DataTable();
                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }
                // تحديث / إضافة
                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["CategoryID"]);

                    var existing = ProductCategoriesList.FirstOrDefault(x => x.CategoryID == id);

                    // مساعدات صغيرة لتفادي DBNull
                    string GetString(string col) => row[col] == DBNull.Value ? "" : row[col].ToString();
                    int GetInt(string col) => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);
                    bool GetBool(string col) => row[col] != DBNull.Value && Convert.ToBoolean(row[col]);
                    decimal GetDecimal(string col) => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);
                    float GetFloat(string col)
                    {
                        if (row[col] == DBNull.Value) return 0f;
                        return Convert.ToSingle(row[col]);
                    }

                    if (existing != null)
                    {
                        existing.CategoryID = id;
                        existing.CategoryCode = GetString("CategoryCode");
                        existing.CategoryNameAr = GetString("CategoryNameAr");
                        existing.CategoryNameEn = GetString("CategoryNameEn");
                        existing.BackgroundColor = GetString("BackgroundColor");
                        existing.Image = GetString("Image");
                        existing.IsActive = GetBool("IsActive");

                        existing.Notes = GetString("Notes");
                        existing.CreatedDate = GetString("CreatedDate");
                        existing.CreatedBy = GetString("CreatedBy");
                        existing.LastModified = GetString("LastModified");
                        existing.CategoryTypeID = GetInt("CategoryTypeID");
                    }
                    else
                    {
                        ProductCategoriesList.Add(new ProductCategories
                        {
                            CategoryID = id,
                            CategoryCode = GetString("CategoryCode"),
                            CategoryNameAr = GetString("CategoryNameAr"),
                            CategoryNameEn = GetString("CategoryNameEn"),
                            BackgroundColor = GetString("BackgroundColor"),
                            Image = GetString("Image"),
                            IsActive = GetBool("IsActive"),

                            Notes = GetString("Notes"),
                            CreatedDate = GetString("CreatedDate"),
                            CreatedBy = GetString("CreatedBy"),

                            LastModified = GetString("LastModified"),
                            CategoryTypeID = GetInt("CategoryTypeID")
                        });
                    }
                }

                // حذف اللي مش موجودين
                var idsFromDb = dt.AsEnumerable()
                                  .Select(r => Convert.ToInt32(r["CategoryID"]))
                                  .ToHashSet();

                for (int i = ProductCategoriesList.Count - 1; i >= 0; i--)
                {
                    if (!idsFromDb.Contains(ProductCategoriesList[i].CategoryID))
                        ProductCategoriesList.RemoveAt(i);
                }
                RenderCategories();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التحميل: " + ex.Message);
            }
        }
        private async Task LoadProductsAsync()
        {
            try
            {
                string query = @"SELECT      [ProductID]
                                            ,[ProductCode]
                                            ,[ProductNameAr]
                                            ,[ProductNameEn]
                                            ,[CategoryID]
                                            ,[Image]
                                            ,[Description]
                                            ,[DiscountPercent]
                                            ,[TaxPercent]
                                            ,[IsActive]
                                            ,[IsDeleted]
                                            ,[PreparationTime]
                                            ,[Notes]
                                            ,[CreatedDate]
                                            ,[CreatedByUserID]
                                            ,[LastModified]
                                            ,(SELECT MIN(SalePrice) FROM ProductSizes WHERE ProductID = Products.ProductID AND IsActive = 1) as Price
                                FROM [DB_Sestamk].[dbo].[Products]
                                Where [IsDeleted] = 0 AND [IsActive] = 1";
                DataTable dt = new DataTable();
                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                        //MessageBox.Show("عدد المنتجات :" + dt.Rows.Count);
                    }
                }
                // تحديث / إضافة
                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["ProductID"]);

                    var existing = ProductsList.FirstOrDefault(x => x.ProductID == id);

                    // مساعدات صغيرة لتفادي DBNull
                    string GetString(string col) => row[col] == DBNull.Value ? "" : row[col].ToString();
                    int GetInt(string col) => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);
                    bool GetBool(string col) => row[col] != DBNull.Value && Convert.ToBoolean(row[col]);
                    decimal GetDecimal(string col) => row[col] == DBNull.Value ? 0m : Convert.ToDecimal(row[col]);
                    float GetFloat(string col)
                    {
                        if (row[col] == DBNull.Value) return 0f;
                        return Convert.ToSingle(row[col]);
                    }

                    if (existing != null)
                    {
                        existing.ProductID = id;
                        existing.ProductCode = GetString("ProductCode");
                        //existing.Barcode = GetString("Barcode");
                        existing.ProductNameAr = GetString("ProductNameAr");
                        existing.ProductNameEn = GetString("ProductNameEn");
                        existing.CategoryID = GetInt("CategoryID");
                        existing.Image = GetString("Image");
                        existing.Description = GetString("Description");
                        existing.DiscountPercent = GetFloat("DiscountPercent");
                        existing.IsActive = GetBool("IsActive");

                        existing.PreparationTime = GetString("PreparationTime");
                        existing.Notes = GetString("Notes");
                        existing.CreatedDate = GetString("CreatedDate");
                        existing.CreatedByUserID = GetInt("CreatedByUserID");
                        existing.LastModified = GetString("LastModified");
                        existing.Price = GetDecimal("Price");
                    }
                    else
                    {
                        ProductsList.Add(new Products
                        {
                            ProductID = id,
                            ProductCode = GetString("ProductCode"),
                            ProductNameAr = GetString("ProductNameAr"),
                            ProductNameEn = GetString("ProductNameEn"),
                            CategoryID = GetInt("CategoryID"),
                            Image = GetString("Image"),
                            Description = GetString("Description"),
                            DiscountPercent = GetFloat("DiscountPercent"),
                            IsActive = GetBool("IsActive"),

                            PreparationTime = GetString("PreparationTime"),
                            Notes = GetString("Notes"),
                            CreatedDate = GetString("CreatedDate"),
                            CreatedByUserID = GetInt("CreatedByUserID"),
                            LastModified = GetString("LastModified"),
                            Price = GetDecimal("Price"),
                        });
                    }
                }

                // حذف اللي مش موجودين
                var idsFromDb = dt.AsEnumerable()
                                  .Select(r => Convert.ToInt32(r["ProductID"]))
                                  .ToHashSet();

                for (int i = ProductsList.Count - 1; i >= 0; i--)
                {
                    if (!idsFromDb.Contains(ProductsList[i].ProductID))
                        ProductsList.RemoveAt(i);
                }
                RenderProducts();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التحميل: " + ex.Message);
            }
        }

        private void RenderCategories()
        {
            flowLayoutPanelCategories.SuspendLayout();

            flowLayoutPanelCategories.Controls.Clear();

            flowLayoutPanelCategories.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanelCategories.WrapContents = false; // 🔥 مهم جداً

            // زر "الكل" أولاً
            var allBtn = new Guna2Button
            {
                Height = 112,
                Text = "الكل",
                Tag = "ALL",
                BorderRadius = 15,
                BorderThickness = 1,
                Font = new Font("Alexandria", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Tile = true,
                Width = flowLayoutPanelCategories.ClientSize.Width - 34,
                Margin = new Padding(7, 3, 7, 3),
                FillColor = Color.FromArgb(45, 55, 72)
            };
            allBtn.Click += AllCategory_Click;
            flowLayoutPanelCategories.Controls.Add(allBtn);

            // أزرار الأقسام الفعلية
            foreach (var cat in ProductCategoriesList.Where(x => x.IsActive))
            {
                Guna2Button btn = CreateCategoryButton(cat);
                flowLayoutPanelCategories.Controls.Add(btn);
            }

            // تمييز زر "الكل" افتراضياً وعرض كل المنتجات
            selectedCategoryButton = allBtn;
            _selectedCategoryID = -1;
            HighlightCategoryButton(allBtn);

            flowLayoutPanelCategories.ResumeLayout();
        }

        private void AllCategory_Click(object sender, EventArgs e)
        {
            if (sender is Guna2Button btn)
            {
                ResetCategoryButtons();
                // إعادة لون زر "الكل" يدوياً لأنه ليس Tag من نوع ProductCategories
                foreach (Control c in flowLayoutPanelCategories.Controls)
                {
                    if (c is Guna2Button b && b.Tag is string s && s == "ALL")
                        b.FillColor = Color.FromArgb(45, 55, 72);
                }
                HighlightCategoryButton(btn);
                selectedCategoryButton = btn;
                _selectedCategoryID = -1;
                txtSearch.Text = "";
                RenderProducts(-1);
            }
        }
        private void RenderProducts(int selectedCategoryid = 0, string searchText = "")
        {
            // إيقاف التحديث المؤقت لتحسين الأداء ومنع الوميض
            flowProducts.SuspendLayout();

            try
            {
                // 1. تنظيف العناصر الحالية تماماً مع التخلص من مواردها
                while (flowProducts.Controls.Count > 0)
                {
                    var ctrl = flowProducts.Controls[0];
                    flowProducts.Controls.Remove(ctrl);
                    ctrl.Dispose();
                }

                // 2. ضبط خصائص الحاوية برمجياً
                flowProducts.FlowDirection = FlowDirection.LeftToRight;
                flowProducts.WrapContents = true;
                flowProducts.AutoScroll = true;
                flowProducts.Visible = true;
                flowProducts.Enabled = true;

                if (ProductsList == null || ProductsList.Count == 0)
                    return;

                // فلترة المنتجات حسب القسم والبحث
                IEnumerable<Products> filtered = ProductsList;

                // -1 = الكل، غير ذلك = قسم معين
                if (selectedCategoryid != -1)
                    filtered = filtered.Where(x => x.CategoryID == selectedCategoryid);

                // فلتر البحث
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string search = searchText.Trim().ToLower();
                    filtered = filtered.Where(x =>
                        (x.ProductNameAr != null && x.ProductNameAr.ToLower().Contains(search)) ||
                        (x.ProductNameEn != null && x.ProductNameEn.ToLower().Contains(search)) ||
                        (x.ProductCode != null && x.ProductCode.ToLower().Contains(search)));
                }

                // مصفوفة مؤقتة لإضافة العناصر دفعة واحدة
                var cardsToDisplay = new List<ProductCard>();
                foreach (var product in filtered)
                {
                    ProductCard card = new ProductCard(product);
                    card.Size = new Size(220, 260);
                    card.Margin = new Padding(5);
                    card.Visible = true;
                    card.OnAddClicked += Card_OnAddClicked;
                    cardsToDisplay.Add(card);
                }

                flowProducts.Controls.AddRange(cardsToDisplay.ToArray());
                flowProducts.BringToFront();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ أثناء رسم المنتجات: " + ex.Message);
            }
            finally
            {
                flowProducts.ResumeLayout(true);
                Application.DoEvents();
                flowProducts.PerformLayout();
                flowProducts.Refresh();
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RenderProducts(_selectedCategoryID, txtSearch.Text);
        }
        private Guna2Button CreateCategoryButton(ProductCategories cat)
        {
            Guna2Button btn = new Guna2Button();
            btn.Name = "btnCategory_NoTheme";

            //btn.Width = 190;
            btn.Height = 112;

            btn.Text = cat.CategoryNameAr;
            btn.Tag = cat;

            btn.BorderRadius = 15;
            btn.BorderThickness = 1;

            btn.Font = new Font("Alexandria", 14, FontStyle.Bold);
            btn.ForeColor = Color.White;

            btn.Tile = true;
            //btn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btn.Width = flowLayoutPanelCategories.ClientSize.Width - 34;

            btn.Margin = new Padding(7, 3, 7, 3);

            // اللون
            if (!string.IsNullOrEmpty(cat.BackgroundColor))
                btn.FillColor = ColorTranslator.FromHtml(cat.BackgroundColor);
            else
                btn.FillColor = Color.FromArgb(62, 144, 255);

            // صورة
            if (!string.IsNullOrEmpty(cat.Image))
            {
                try
                {
                    byte[] imgBytes = Convert.FromBase64String(cat.Image);
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        btn.Image = Image.FromStream(ms);
                        btn.ImageSize = new Size(64, 64);
                        btn.ImageOffset = new Point(0, 13);
                        btn.TextOffset = new Point(0, 9);
                    }
                }
                catch
                {
                }
            }

            btn.Click += Category_Click;

            return btn;
        }
        private void Category_Click(object sender, EventArgs e)
        {
            if (sender is Guna2Button btn && btn.Tag is ProductCategories cat)
            {
                ResetCategoryButtons();
                // إعادة لون زر "الكل"
                foreach (Control c in flowLayoutPanelCategories.Controls)
                {
                    if (c is Guna2Button b && b.Tag is string s && s == "ALL")
                        b.FillColor = Color.FromArgb(45, 55, 72);
                }
                HighlightCategoryButton(btn);
                selectedCategoryButton = btn;
                _selectedCategoryID = cat.CategoryID;
                txtSearch.Text = "";
                RenderProducts(cat.CategoryID);
            }
        }
        private async void HighlightCategoryButton(Guna2Button btn)
        {
            btn.BorderThickness = 5;
            btn.BorderColor = Color.AliceBlue;

            for (int i = 0; i < 3; i++)
            {
                btn.FillColor = ControlPaint.Light(btn.FillColor);
                await Task.Delay(30);
            }
        }
        private void ResetCategoryButtons()
        {
            foreach (Control c in flowLayoutPanelCategories.Controls)
            {
                if (c is Guna2Button b)
                {
                    b.BorderThickness = 1;
                    b.BorderColor = Color.Transparent;
                    if (b.Tag is ProductCategories cat)
                    {
                        if (!string.IsNullOrEmpty(cat.BackgroundColor))
                            b.FillColor = ColorTranslator.FromHtml(cat.BackgroundColor);
                        else
                            b.FillColor = Color.FromArgb(62, 144, 255);
                    }
                }
            }
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            // Guna2BorderlessForm يتعامل مع الزوايا والبوردر تلقائياً
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void UC_ProductOptions1_OnAddToInvoice(object sender, EventArgs e)
        {
            var size = uC_ProductOptions1.SelectedSize;
            var addons = uC_ProductOptions1.SelectedAddons;

            if (size == null && (addons == null || addons.Count == 0))
            {
                return;
            }

            uC_ProductOptions1.Visible = false;

            flowInvoiceItems.SuspendLayout();

            Control lastAdded = null;

            // 1. إضافة الصنف الرئيسي (المنتج + الحجم)
            if (size != null)
            {
                var orderItem = new OrderItemModel
                {
                    ProductID = uC_ProductOptions1.ProductID,
                    ProductSizeID = size.ProductSizeID,
                    ProductName = uC_ProductOptions1.ProductNameAr,
                    SizeName = size.SizeNameAr,
                    UnitPrice = size.SalePrice,
                    Quantity = 1,
                    LineTotal = size.SalePrice
                };

                _invoiceItems.Add(orderItem);

                // رسم في الـ UI
                string productWithSize = $"{uC_ProductOptions1.ProductNameAr} ({size.SizeNameAr})";
                var itemPanel = CreateInvoiceItemPanel(productWithSize, size.SalePrice, _invoiceItems.Count - 1);
                flowInvoiceItems.Controls.Add(itemPanel);
                lastAdded = itemPanel;
            }

            // 2. إضافة الإضافات كأصناف منفصلة
            if (addons != null && addons.Count > 0)
            {
                foreach (var addon in addons)
                {
                    var addonItem = new OrderItemModel
                    {
                        ProductID = -1, // معرف خاص للإضافات أو يمكن ربطه بجدول الإضافات لاحقاً
                        ProductName = $" + {addon.AddonNameAr}",
                        UnitPrice = addon.SalePrice,
                        Quantity = 1,
                        LineTotal = addon.SalePrice
                    };

                    _invoiceItems.Add(addonItem);

                    // رسم في الـ UI (كصنف منفصل تماماً مع أزرار التحكم)
                    var addonPanel = CreateInvoiceItemPanel(addonItem.ProductName, addon.SalePrice, _invoiceItems.Count - 1, true);
                    flowInvoiceItems.Controls.Add(addonPanel);
                    lastAdded = addonPanel;
                }
            }

            flowInvoiceItems.ResumeLayout(true);

            UpdateInvoiceTotal();

            // النزول لآخر عنصر مضاف
            if (lastAdded != null)
            {
                flowInvoiceItems.ScrollControlIntoView(lastAdded);
            }

            // صوت تأكيد
            System.Media.SystemSounds.Asterisk.Play();

            ToastManager.ShowSuccess("اضافة المنتج", $"تم اضافة ({uC_ProductOptions1.ProductNameAr}) الي الفاتورة");
        }


        /// <summary>
        /// إنشاء عنصر فاتورة ديناميكي بنفس تصميم العنصر التجريبي
        /// </summary>
        private Guna.UI2.WinForms.Guna2Panel CreateInvoiceItemPanel(string itemName, decimal unitPrice, int itemIndex = -1, bool isAddon = false)
        {
            var panel = new Guna.UI2.WinForms.Guna2Panel
            {
                BackColor = Color.Transparent,
                BorderRadius = 10,
                FillColor = isAddon ? Color.FromArgb(40, 45, 55) : Color.FromArgb(26, 36, 47), // لون مختلف قليلاً للإضافات
                Size = new Size(430, 95), // نفس الحجم للكل لضمان ظهور الأزرار
                Margin = new Padding(5),
                Tag = unitPrice // نحفظ سعر الوحدة في Tag
            };

            // اسم الصنف
            var lblName = new Label
            {
                Text = itemName,
                ForeColor = Color.FromArgb(241, 245, 249),
                Font = new Font("Alexandria", isAddon ? 10F : 11F, isAddon ? FontStyle.Regular : FontStyle.Bold),
                Location = new Point(137, 10),
                Size = new Size(292, 27),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // سعر الصنف
            var lblPrice = new Label
            {
                Text = $"{unitPrice} جنية",
                ForeColor = Color.FromArgb(241, 245, 249),
                Location = new Point(4, 8),
                Size = new Size(119, 29),
                TextAlign = ContentAlignment.MiddleCenter,
                Name = "lblItemPrice"
            };

            // عدد الكمية
            var lblCount = new Label
            {
                Text = "1",
                ForeColor = Color.FromArgb(241, 245, 249),
                Location = new Point(289, 50),
                Size = new Size(64, 29),
                TextAlign = ContentAlignment.MiddleCenter,
                Name = "lblItemCount"
            };

            // زر نقصان
            var btnMinus = new Guna.UI2.WinForms.Guna2Button
            {
                BorderColor = Color.FromArgb(32, 52, 79),
                BorderRadius = 10,
                BorderThickness = 1,
                FillColor = Color.FromArgb(31, 41, 59),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Image = Properties.Resources.minus__4_,
                ImageSize = new Size(30, 32),
                Location = new Point(230, 48),
                Size = new Size(56, 33)
            };

            // زر زيادة
            var btnPlus = new Guna.UI2.WinForms.Guna2Button
            {
                BorderColor = Color.FromArgb(32, 52, 79),
                BorderRadius = 10,
                BorderThickness = 1,
                FillColor = Color.FromArgb(31, 41, 59),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                Image = Properties.Resources.add__3_,
                Location = new Point(358, 48),
                Size = new Size(56, 33)
            };

            // زر حذف
            var btnDelete = new Guna.UI2.WinForms.Guna2Button
            {
                BorderColor = Color.DodgerBlue,
                BorderRadius = 15,
                FillColor = Color.Empty,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White,
                HoverState = { FillColor = Color.Silver },
                Image = Properties.Resources.delete__2_,
                ImageSize = new Size(32, 32),
                Location = new Point(35, 42),
                Size = new Size(57, 45)
            };

            // أحداث الأزرار
            btnPlus.Click += (s, ev) =>
            {
                int count = Convert.ToInt32(lblCount.Text) + 1;
                lblCount.Text = count.ToString();
                lblPrice.Text = $"{unitPrice * count} جنية";
                UpdateInvoiceTotal();
            };

            btnMinus.Click += (s, ev) =>
            {
                int count = Convert.ToInt32(lblCount.Text);
                count--;
                if (count < 1)
                {
                    // حذف العنصر لما يوصل لصفر
                    if (frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذا الصنف؟"))
                    {
                        int idx = flowInvoiceItems.Controls.GetChildIndex(panel);
                        if (idx >= 0 && idx < _invoiceItems.Count) _invoiceItems.RemoveAt(idx);

                        flowInvoiceItems.Controls.Remove(panel);
                        panel.Dispose();
                        UpdateInvoiceTotal();
                    }
                }
                else
                {
                    lblCount.Text = count.ToString();
                    lblPrice.Text = $"{unitPrice * count} جنية";
                    UpdateInvoiceTotal();
                }
            };

            btnDelete.Click += (s, ev) =>
            {
                if (frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذا الصنف؟"))
                {
                    int idx = flowInvoiceItems.Controls.GetChildIndex(panel);
                    if (idx >= 0 && idx < _invoiceItems.Count) _invoiceItems.RemoveAt(idx);

                    flowInvoiceItems.Controls.Remove(panel);
                    panel.Dispose();
                    UpdateInvoiceTotal();
                }
            };

            AttachScrollForwarding(panel);
            AttachScrollForwarding(lblName);
            AttachScrollForwarding(lblPrice);
            AttachScrollForwarding(lblCount);

            panel.Controls.Add(lblName);
            panel.Controls.Add(lblPrice);
            panel.Controls.Add(lblCount);
            panel.Controls.Add(btnMinus);
            panel.Controls.Add(btnPlus);
            panel.Controls.Add(btnDelete);

            return panel;
        }


        /// <summary>
        /// تمرير أحداث الماوس من الكنترول للـ flowInvoiceItems لتفعيل السحب
        /// </summary>
        private void AttachScrollForwarding(Control ctrl)
        {
            ctrl.MouseDown += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Left)
                {
                    _isDraggingInvoice = true;
                    _invoiceDragStart = flowInvoiceItems.PointToClient(ctrl.PointToScreen(ev.Location));
                    _invoiceScrollStart = flowInvoiceItems.AutoScrollPosition;
                    flowInvoiceItems.Cursor = Cursors.SizeNS;
                }
            };
            ctrl.MouseMove += (s, ev) =>
            {
                if (!_isDraggingInvoice) return;
                var curPos = flowInvoiceItems.PointToClient(ctrl.PointToScreen(ev.Location));
                int deltaY = curPos.Y - _invoiceDragStart.Y;
                flowInvoiceItems.AutoScrollPosition = new Point(0, Math.Abs(_invoiceScrollStart.Y) - deltaY);
            };
            ctrl.MouseUp += (s, ev) =>
            {
                _isDraggingInvoice = false;
                flowInvoiceItems.Cursor = Cursors.Default;
            };
        }

        /// <summary>
        /// حساب المجموع الكلي وتحديث الليبل
        /// </summary>
        private void UpdateInvoiceTotal()
        {
            // 🆕 حساب من القائمة بدل الـ UI Controls
            decimal total = 0;
            int itemCount = 0;
            int quantityCount = 0;

            foreach (var item in _invoiceItems)
            {
                if (!item.IsVoided)
                {
                    total += item.UnitPrice * item.Quantity;
                    itemCount++;
                    quantityCount += item.Quantity;

                    // ملاحظة: بما أننا قمنا بتبسيط الهيكل، لم تعد الإضافات تضاف مرتين
                    // لأن كل إضافة أصبحت Item مستقل في _invoiceItems
                }
            }

            // تحديث UI
            string currency = SettingsService.CurrencyName;
            label12.Text = $"{total} {currency}";

            // أيضاً نحدّث من الـ UI Controls (للتوافق مع +/-)
            decimal uiTotal = 0;
            foreach (Control ctrl in flowInvoiceItems.Controls)
            {
                if (ctrl is Guna.UI2.WinForms.Guna2Panel itemPanel && ctrl.Tag is decimal unitPrice)
                {
                    var lblCount = itemPanel.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Name == "lblItemCount");
                    if (lblCount != null)
                    {
                        int count = Convert.ToInt32(lblCount.Text);
                        uiTotal += unitPrice * count;
                    }
                }
            }
            if (uiTotal > 0) label12.Text = $"{uiTotal} {currency}";
        }
        // دالة لتحديث حالة الأزرار
        private void UpdateSegmentSelection(Guna.UI2.WinForms.Guna2Button selectedButton)
        {
            // مصفوفة تضم الأزرار الثلاثة (استبدل الأسماء بأسماء أزرارك)
            Guna.UI2.WinForms.Guna2Button[] buttons = { btnSaffari, btnDelivery, btnTable };

            foreach (var btn in buttons)
            {
                // إذا كان الزر هو الذي تم الضغط عليه، اجعله Checked
                // وإلا اجعله Unchecked
                btn.Checked = (btn == selectedButton);

                // اختيار اختياري: تغيير لون النص ليكون باهتًا في الأزرار غير النشطة
                //if (btn.Checked)
                //    btn.ForeColor = Color.White;
                //else
                //    btn.ForeColor = Color.Gray;
            }
            string currency = SettingsService.CurrencyName;
            if (selectedButton == btnSaffari)
            {
                _Selected_invoice_type = 0;
                lbl_Service.Visible = false;
                lbl_Service_Price.Visible = false;
                // إخفاء الـ Pickers
                HideDriverPicker();
                HideTablePicker();
            }
            else if (selectedButton == btnTable)
            {
                _Selected_invoice_type = 1;
                lbl_Service.Visible = true;
                lbl_Service_Price.Visible = true;
                lbl_Service.Text = "خدمة صالة";
                lbl_Service_Price.Text = $"{SettingsService.ServicePriceTable} {currency}";
                // إخفاء picker الطيار وإظهار picker الطاولة
                HideDriverPicker();
                ShowTablePicker();
            }
            else if (selectedButton == btnDelivery)
            {
                _Selected_invoice_type = 2;
                lbl_Service.Visible = true;
                lbl_Service_Price.Visible = true;
                lbl_Service.Text = "خدمة توصيل";
                lbl_Service_Price.Text = $"{SettingsService.ServicePriceDelivery} {currency}";
                // إخفاء picker الطاولة وإظهار picker الطيار
                HideTablePicker();
                ShowDriverPicker();
            }
        }

        private void btnSaffari_Click(object sender, EventArgs e)
        {
            // تحويل الـ sender إلى Guna2Button لمعرفة أي زر ضُغط
            var clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;

            // استدعاء دالة التحديث
            UpdateSegmentSelection(clickedButton);

            // هنا يمكنك وضع Logic إضافي بناءً على نوع الطلب
            string orderType = clickedButton.Text;
            //MessageBox.Show("Selected Mode: " + orderType);
        }

        private void btnTable_Click(object sender, EventArgs e)
        {
            // تحويل الـ sender إلى Guna2Button لمعرفة أي زر ضُغط
            var clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;

            // استدعاء دالة التحديث
            UpdateSegmentSelection(clickedButton);

            // هنا يمكنك وضع Logic إضافي بناءً على نوع الطلب
            string orderType = clickedButton.Text;
            //MessageBox.Show("Selected Mode: " + orderType);
        }

        private void btnDelivery_Click(object sender, EventArgs e)
        {
            // تحويل الـ sender إلى Guna2Button لمعرفة أي زر ضُغط
            var clickedButton = (Guna.UI2.WinForms.Guna2Button)sender;

            // استدعاء دالة التحديث
            UpdateSegmentSelection(clickedButton);

            // هنا يمكنك وضع Logic إضافي بناءً على نوع الطلب
            string orderType = clickedButton.Text;
            //MessageBox.Show("Selected Mode: " + orderType);
        }

        // ====== سحب بالماوس/التاتش للتمرير ======
        private void FlowProducts_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDraggingScroll = true;
                _dragStartPoint = e.Location;
                _scrollStartOffset = flowProducts.AutoScrollPosition;
                flowProducts.Cursor = Cursors.SizeAll;
            }
        }

        private void FlowProducts_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingScroll) return;

            int deltaX = e.X - _dragStartPoint.X;
            int deltaY = e.Y - _dragStartPoint.Y;

            // AutoScrollPosition يرجع قيم سالبة لكن SetAutoScrollPosition يحتاج موجبة
            flowProducts.AutoScrollPosition = new Point(
                Math.Abs(_scrollStartOffset.X) - deltaX,
                Math.Abs(_scrollStartOffset.Y) - deltaY
            );
        }

        private void FlowProducts_MouseUp(object sender, MouseEventArgs e)
        {
            _isDraggingScroll = false;
            flowProducts.Cursor = Cursors.Default;
        }

        // ====== سحب بالماوس/التاتش للتمرير - الأقسام ======
        private void FlowCategories_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isCatDragging = true;
                _catDragStart = e.Location;
                _catScrollStart = flowLayoutPanelCategories.AutoScrollPosition;
                flowLayoutPanelCategories.Cursor = Cursors.SizeAll;
            }
        }

        private void FlowCategories_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isCatDragging) return;

            int deltaY = e.Y - _catDragStart.Y;

            flowLayoutPanelCategories.AutoScrollPosition = new Point(
                0,
                Math.Abs(_catScrollStart.Y) - deltaY
            );
        }

        private void FlowCategories_MouseUp(object sender, MouseEventArgs e)
        {
            _isCatDragging = false;
            flowLayoutPanelCategories.Cursor = Cursors.Default;
        }

        // ═══════════════════════════════════════════════════════════
        //  تحديد العميل — UC_CustomerPicker
        // ═══════════════════════════════════════════════════════════
        private void SetupCustomerPicker()
        {
            _customerPicker = new UC_CustomerPicker();
            _customerPicker.Size = new Size(600, 750);
            _customerPicker.Visible = false;
            _customerPicker.Anchor = AnchorStyles.None;

            _customerPicker.OnCustomerSelected += CustomerPicker_OnCustomerSelected;
            _customerPicker.OnCancel += (s, ev) =>
            {
                _customerPicker.Visible = false;
            };

            this.Controls.Add(_customerPicker);
            _customerPicker.BringToFront();
        }

        private void BtnSelectCustomer_Click(object sender, EventArgs e)
        {
            // إظهار UC_CustomerPicker في منتصف الشاشة
            _customerPicker.Left = (this.ClientSize.Width - _customerPicker.Width) / 2;
            _customerPicker.Top = (this.ClientSize.Height - _customerPicker.Height) / 2;
            _customerPicker.Visible = true;
            _customerPicker.BringToFront();
        }

        private void CustomerPicker_OnCustomerSelected(object sender, Customer customer)
        {
            _selectedCustomer = customer;
            _customerPicker.Visible = false;

            // تحديث اسم العميل في خانة العميل
            guna2TextBox1.Text = customer.CustomerName;

            ToastManager.ShowSuccess("تحديد العميل", $"تم تحديد العميل: {customer.CustomerName}");
        }

        // ═══════════════════════════════════════════════════════════
        //  تحديد الطيار — UC_DriverPicker
        // ═══════════════════════════════════════════════════════════
        private void SetupDriverPicker()
        {
            _driverPicker = new UC_DriverPicker();
            _driverPicker.Size = new Size(600, 750);
            _driverPicker.Visible = false;
            _driverPicker.Anchor = AnchorStyles.None;

            _driverPicker.OnDriverSelected += DriverPicker_OnDriverSelected;
            _driverPicker.OnCancel += (s, ev) =>
            {
                _driverPicker.Visible = false;
            };

            this.Controls.Add(_driverPicker);
            _driverPicker.BringToFront();
        }

        private void ShowDriverPicker()
        {
            if (_driverPicker == null) return;
            _driverPicker.Left = (this.ClientSize.Width - _driverPicker.Width) / 2;
            _driverPicker.Top = (this.ClientSize.Height - _driverPicker.Height) / 2;
            _driverPicker.Visible = true;
            _driverPicker.BringToFront();
        }

        private void HideDriverPicker()
        {
            if (_driverPicker != null && _driverPicker.Visible)
                _driverPicker.Visible = false;
        }

        private void DriverPicker_OnDriverSelected(object sender, DeliveryStaffModel driver)
        {
            _selectedDriver = driver;
            _driverPicker.Visible = false;

            // تحديث سعر التوصيلة إذا الطيار عنده سعر مخصص
            if (driver.DeliveryFee > 0)
            {
                string currency = SettingsService.CurrencyName;
                lbl_Service_Price.Text = $"{driver.DeliveryFee} {currency}";
            }

            ToastManager.ShowSuccess("تحديد الطيار", $"🛵 تم تحديد الطيار: {driver.FullName}");
        }

        // ═══════════════════════════════════════════════════════════
        //  تحديد الطاولة — UC_TablePicker
        // ═══════════════════════════════════════════════════════════
        private void SetupTablePicker()
        {
            _tablePicker = new UC_TablePicker();
            _tablePicker.Size = new Size(700, 600);
            _tablePicker.Visible = false;
            _tablePicker.Anchor = AnchorStyles.None;

            _tablePicker.OnTableSelected += TablePicker_OnTableSelected;
            _tablePicker.OnCancel += (s, ev) =>
            {
                _tablePicker.Visible = false;
            };

            this.Controls.Add(_tablePicker);
            _tablePicker.BringToFront();
        }

        private void ShowTablePicker()
        {
            if (_tablePicker == null) return;
            _tablePicker.Left = (this.ClientSize.Width - _tablePicker.Width) / 2;
            _tablePicker.Top = (this.ClientSize.Height - _tablePicker.Height) / 2;
            _tablePicker.Visible = true;
            _tablePicker.BringToFront();
        }

        private void HideTablePicker()
        {
            if (_tablePicker != null && _tablePicker.Visible)
                _tablePicker.Visible = false;
        }

        private void TablePicker_OnTableSelected(object sender, TableModel table)
        {
            _selectedTable = table;
            _tablePicker.Visible = false;
            ToastManager.ShowSuccess("تحديد الطاولة", $"🪑 تم تحديد: {table.DisplayName} — {table.SectionName}");
        }

        // ═══════════════════════════════════════════════════════════
        //  تأكيد الدفع — فتح frmPayment
        // ═══════════════════════════════════════════════════════════
        private async void BtnConfirmPayment_Click(object sender, EventArgs e)
        {
            // التحقق من وجود عناصر في الفاتورة
            if (_invoiceItems.Count == 0 && flowInvoiceItems.Controls.Count == 0)
            {
                ToastManager.ShowWarning("تنبيه", "لا يوجد أصناف في الفاتورة");
                return;
            }

            // 🆕 مزامنة الكميات من الـ UI إلى القائمة
            SyncQuantitiesFromUI();

            // حساب المجموع
            decimal total = GetInvoiceTotal();

            // حساب مبلغ الخدمة
            decimal serviceAmount = 0;
            if (_Selected_invoice_type == 1)
                serviceAmount = SettingsService.ServicePriceTable;
            else if (_Selected_invoice_type == 2)
                serviceAmount = SettingsService.ServicePriceDelivery;

            // الحصول على رقم الفاتورة مبكراً لتمريره لفورم الدفع
            string nextOrderNumber = await OrderService.GetNextOrderNumberAsync();

            // فتح فورم الدفع
            using (var paymentForm = new frmPayment())
            {
                paymentForm.TotalAmount = total;
                paymentForm.ServiceAmount = serviceAmount;
                paymentForm.InvoiceType = _Selected_invoice_type;
                paymentForm.SelectedCustomer = _selectedCustomer;
                paymentForm.InvoiceNumber = nextOrderNumber;

                var result = paymentForm.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    try
                    {
                        // 🆕 بناء OrderModel كامل وحفظه في DB
                        var order = new OrderModel
                        {
                            OrderNumber = nextOrderNumber,
                            ShiftID = ShiftService.IsShiftOpen ? ShiftService.CurrentShift.ShiftID : 0,
                            CustomerID = _selectedCustomer?.CustomerID,
                            CustomerName = _selectedCustomer?.CustomerName ?? "عميل نقدي",
                            UserID = UserSession.UserId,
                            CashierName = UserSession.Full_Name ?? UserSession.UserName ?? "",
                            OrderType = _Selected_invoice_type,
                            // ═══ ربط الطيار والطاولة ═══
                            DriverID = _selectedDriver?.DeliveryId,
                            DriverName = _selectedDriver?.FullName ?? "",
                            TableID = (int?)_selectedTable?.Id,
                            SubTotal = total,
                            DiscountAmount = paymentForm.DiscountAmount,
                            ServiceAmount = serviceAmount,
                            TaxPercent = SettingsService.TaxPercent,
                            TaxAmount = Math.Round(total * SettingsService.TaxPercent / 100m, 2),
                            TotalAmount = paymentForm.NetTotal,
                            PaidAmount = paymentForm.PaidAmount,
                            ChangeAmount = paymentForm.ChangeAmount,
                            RemainingAmount = paymentForm.NetTotal - paymentForm.PaidAmount > 0 
                                ? paymentForm.NetTotal - paymentForm.PaidAmount : 0,
                            Status = 3, // مُسلَّم
                            Items = new List<OrderItemModel>(_invoiceItems)
                        };

                        // إضافة بيانات الدفع
                        order.Payments.Add(new PaymentModel
                        {
                            PaymentMethod = paymentForm.PaymentMethod,
                            Amount = paymentForm.PaidAmount,
                            PaymentDate = DateTime.Now
                        });

                        // ═══ حفظ في قاعدة البيانات ═══
                        int orderId = await OrderService.SaveOrderAsync(order);

                        if (orderId > 0)
                        {
                            // ═══ طباعة الإيصال ═══
                            if (SettingsService.PrintReceiptOnPayment)
                            {
                                try
                                {
                                    var printer = new ReceiptPrinter();
                                    printer.PrintReceipt(order);
                                }
                                catch (Exception printEx)
                                {
                                    ToastManager.ShowWarning("تنبيه", "تم حفظ الفاتورة لكن فشلت الطباعة: " + printEx.Message);
                                }
                            }

                            // ═══ فتح درج النقد ═══
                            if (SettingsService.OpenDrawerOnPayment && paymentForm.PaymentMethod == 0)
                            {
                                ReceiptPrinter.OpenCashDrawer();
                            }

                            string paymentMethodText = paymentForm.PaymentMethod switch
                            {
                                0 => "نقدي",
                                1 => "بطاقة",
                                2 => "آجل",
                                _ => "غير محدد"
                            };

                            decimal change = paymentForm.ChangeAmount;
                            string changeText = change > 0 ? $"\nالباقي: {change:N2} {SettingsService.CurrencySymbol}" : "";

                            ToastManager.ShowSuccess("تم حفظ الفاتورة",
                                $"فاتورة {order.OrderNumber} ✓\n" +
                                $"طريقة الدفع: {paymentMethodText}\n" +
                                $"المدفوع: {paymentForm.PaidAmount:N2} {SettingsService.CurrencySymbol}{changeText}");

                            // 🆕 تحديث الطاولات المشغولة فوراً
                            await LoadOccupiedTablesAsync();
                        }
                        else
                        {
                            ToastManager.ShowError("خطأ", "فشل حفظ الفاتورة في قاعدة البيانات!");
                            return; // لا تنظّف الفاتورة
                        }
                    }
                    catch (Exception ex)
                    {
                        ToastManager.ShowError("خطأ", "خطأ في حفظ الفاتورة: " + ex.Message);
                        return; // لا تنظّف الفاتورة
                    }

                    // تنظيف الفاتورة
                    ClearInvoice();
                }
            }
        }

        /// <summary>
        /// مزامنة الكميات من الـ UI Controls إلى قائمة _invoiceItems
        /// </summary>
        private void SyncQuantitiesFromUI()
        {
            int itemIndex = 0;
            foreach (Control ctrl in flowInvoiceItems.Controls)
            {
                if (ctrl is Guna.UI2.WinForms.Guna2Panel itemPanel && ctrl.Tag is decimal)
                {
                    var lblCount = itemPanel.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Name == "lblItemCount");
                    if (lblCount != null && itemIndex < _invoiceItems.Count)
                    {
                        int qty = 1;
                        int.TryParse(lblCount.Text, out qty);
                        _invoiceItems[itemIndex].Quantity = qty;
                        _invoiceItems[itemIndex].LineTotal = _invoiceItems[itemIndex].UnitPrice * qty;
                    }
                    itemIndex++;
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  حساب مجموع الفاتورة
        // ═══════════════════════════════════════════════════════════
        private decimal GetInvoiceTotal()
        {
            decimal total = 0;
            foreach (Control ctrl in flowInvoiceItems.Controls)
            {
                if (ctrl is Guna.UI2.WinForms.Guna2Panel itemPanel && ctrl.Tag is decimal unitPrice)
                {
                    var lblCount = itemPanel.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Name == "lblItemCount");
                    if (lblCount != null)
                    {
                        int count = Convert.ToInt32(lblCount.Text);
                        total += unitPrice * count;
                    }
                }
            }
            return total;
        }

        // ═══════════════════════════════════════════════════════════
        //  تفريغ الفاتورة
        // ═══════════════════════════════════════════════════════════
        private void BtnClearInvoice_Click(object sender, EventArgs e)
        {
            if (flowInvoiceItems.Controls.Count == 0)
            {
                ToastManager.ShowInfo("تنبيه", "الفاتورة فارغة بالفعل");
                return;
            }

            if (frmConfirm.Show("تفريغ الفاتورة", "هل أنت متأكد من تفريغ جميع الأصناف من الفاتورة؟"))
            {
                ClearInvoice();
                ToastManager.ShowSuccess("تفريغ", "تم تفريغ الفاتورة بنجاح");
            }
        }

        private void ClearInvoice()
        {
            // 🆕 تنظيف القائمة والواجهة
            _invoiceItems.Clear();

            flowInvoiceItems.SuspendLayout();
            while (flowInvoiceItems.Controls.Count > 0)
            {
                var ctrl = flowInvoiceItems.Controls[0];
                flowInvoiceItems.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
            flowInvoiceItems.ResumeLayout(true);
            UpdateInvoiceTotal();

            // 1. إعادة ضبط العميل
            _selectedCustomer = null;
            guna2TextBox1.Text = "";

            // 2. إعادة ضبط الطيار والطاولة
            _selectedDriver = null;
            _selectedTable = null;

            // 3. إعادة ضبط نوع الفاتورة إلى "تيك اوي" (الوضع الافتراضي)
            UpdateSegmentSelection(btnSaffari);

            // 4. تصفير أي نصوص إضافية
            lbl_Service_Price.Text = "0 جنية";
            
            // صوت تأكيد بسيط
            System.Media.SystemSounds.Exclamation.Play();
        }

        // ═══════════════════════════════════════════════════════════
        //  تعليق الفاتورة
        // ═══════════════════════════════════════════════════════════
        private void BtnHoldInvoice_Click(object sender, EventArgs e)
        {
            if (flowInvoiceItems.Controls.Count == 0)
            {
                ToastManager.ShowWarning("تنبيه", "لا يوجد أصناف لتعليقها");
                return;
            }

            // جمع بيانات الفاتورة الحالية
            var heldInvoice = new HeldInvoice
            {
                HoldTime = DateTime.Now,
                InvoiceType = _Selected_invoice_type,
                CustomerName = _selectedCustomer?.CustomerName ?? "عميل نقدي",
                CustomerId = _selectedCustomer?.CustomerID ?? 0,
                Items = new List<HeldInvoiceItem>()
            };

            foreach (Control ctrl in flowInvoiceItems.Controls)
            {
                if (ctrl is Guna.UI2.WinForms.Guna2Panel itemPanel && ctrl.Tag is decimal unitPrice)
                {
                    var lblName = itemPanel.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Name != "lblItemCount" && l.Name != "lblItemPrice" && l.Text != null && !l.Text.Contains("جنية"));
                    var lblCount = itemPanel.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Name == "lblItemCount");

                    if (lblName != null && lblCount != null)
                    {
                        heldInvoice.Items.Add(new HeldInvoiceItem
                        {
                            ItemName = lblName.Text,
                            UnitPrice = unitPrice,
                            Quantity = Convert.ToInt32(lblCount.Text)
                        });
                    }
                }
            }

            heldInvoice.TotalAmount = GetInvoiceTotal();

            // إضافة للقائمة
            _heldInvoices.Add(heldInvoice);

            // تنظيف الفاتورة
            ClearInvoice();

            string invoiceTypeText = _Selected_invoice_type switch
            {
                0 => "تيك اوي",
                1 => "صالة",
                2 => "دليفري",
                _ => ""
            };

            ToastManager.ShowSuccess("تعليق الفاتورة",
                $"تم تعليق الفاتورة بنجاح ⏸\n" +
                $"عدد الأصناف: {heldInvoice.Items.Count}\n" +
                $"المجموع: {heldInvoice.TotalAmount:N2} ج\n" +
                $"إجمالي الفواتير المعلقة: {_heldInvoices.Count}");
        }

        // ═══════════════════════════════════════════════════════════
        //  استرجاع فاتورة معلقة (يمكن استدعاؤها من frmHold_Invoices)
        // ═══════════════════════════════════════════════════════════
        public void RestoreHeldInvoice(HeldInvoice invoice)
        {
            if (invoice == null) return;

            // تنظيف الفاتورة الحالية أولاً
            ClearInvoice();

            // استعادة نوع الفاتورة
            if (invoice.InvoiceType == 0)
                UpdateSegmentSelection(btnSaffari);
            else if (invoice.InvoiceType == 1)
                UpdateSegmentSelection(btnTable);
            else if (invoice.InvoiceType == 2)
                UpdateSegmentSelection(btnDelivery);

            // استعادة العناصر
            flowInvoiceItems.SuspendLayout();
            foreach (var item in invoice.Items)
            {
                var panel = CreateInvoiceItemPanel(item.ItemName, item.UnitPrice);

                // تحديث الكمية
                var lblCount = panel.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblItemCount");
                var lblPrice = panel.Controls.OfType<Label>()
                    .FirstOrDefault(l => l.Name == "lblItemPrice");

                if (lblCount != null)
                    lblCount.Text = item.Quantity.ToString();
                if (lblPrice != null)
                    lblPrice.Text = $"{item.UnitPrice * item.Quantity} جنية";

                flowInvoiceItems.Controls.Add(panel);
            }
            flowInvoiceItems.ResumeLayout(true);

            UpdateInvoiceTotal();

            // حذف الفاتورة المعلقة من القائمة
            _heldInvoices.Remove(invoice);

            ToastManager.ShowSuccess("استرجاع", "تم استرجاع الفاتورة المعلقة بنجاح ▶");
        }

        /// <summary>
        /// الحصول على قائمة الفواتير المعلقة
        /// </summary>
        public static List<HeldInvoice> GetHeldInvoices() => _heldInvoices;

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            if (_heldInvoices == null || _heldInvoices.Count == 0)
            {
                ToastManager.ShowWarning("تنبيه", "لا توجد فواتير معلقة حالياً.");
                return;
            }

            // فتح فورم الطلبات المعلقة كمهدّأ
            using (var holdForm = new frmHold_Invoices())
            {
                var result = holdForm.ShowDialog(this);
                if (result == DialogResult.OK && holdForm.SelectedInvoiceToRestore != null)
                {
                    this.RestoreHeldInvoice(holdForm.SelectedInvoiceToRestore);
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        //  🆕 اختصارات لوحة المفاتيح
        // ═══════════════════════════════════════════════════════════
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F2:
                    txtSearch.Focus();
                    txtSearch.SelectAll();
                    return true;

                case Keys.F3:
                    BtnSelectCustomer_Click(null, EventArgs.Empty);
                    return true;

                case Keys.F4:
                    // تبديل نوع الطلب
                    if (_Selected_invoice_type == 0)
                        UpdateSegmentSelection(btnTable);
                    else if (_Selected_invoice_type == 1)
                        UpdateSegmentSelection(btnDelivery);
                    else
                        UpdateSegmentSelection(btnSaffari);
                    return true;

                case Keys.F5:
                    BtnHoldInvoice_Click(null, EventArgs.Empty);
                    return true;

                case Keys.F9:
                    BtnClearInvoice_Click(null, EventArgs.Empty);
                    return true;

                case Keys.F12:
                    BtnConfirmPayment_Click(null, EventArgs.Empty);
                    return true;

                case Keys.Escape:
                    if (uC_ProductOptions1.Visible)
                    {
                        uC_ProductOptions1.Visible = false;
                        return true;
                    }
                    if (_customerPicker != null && _customerPicker.Visible)
                    {
                        _customerPicker.Visible = false;
                        return true;
                    }
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        // ═══════════════════════════════════════════════════════════
        //  هيكل بيانات الفاتورة المعلقة
        // ═══════════════════════════════════════════════════════════
        public class HeldInvoice
        {
            public DateTime HoldTime { get; set; }
            public int InvoiceType { get; set; }
            public string CustomerName { get; set; }
            public int CustomerId { get; set; }
            public decimal TotalAmount { get; set; }
            public List<HeldInvoiceItem> Items { get; set; } = new List<HeldInvoiceItem>();

            public string InvoiceTypeText => InvoiceType switch
            {
                0 => "تيك اوي",
                1 => "صالة",
                2 => "دليفري",
                _ => "غير محدد"
            };
        }

        public class HeldInvoiceItem
        {
            public string ItemName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }
    }
}
