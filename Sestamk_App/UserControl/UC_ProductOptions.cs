using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    public partial class UC_ProductOptions : System.Windows.Forms.UserControl
    {
        private int _productid;
        private string _productCode;
        private string _productNameAr;
        private string _productNameEn;
        private bool _isDragging = false;
        private Point _mouseOffset;
        private Point _targetLocation;          // الموقع المستهدف
        private System.Windows.Forms.Timer _smoothTimer;
        private const float LERP_SPEED = 0.2f; // كلما كان أقل كلما كان أنعم (0.1 ~ 0.3)
        private List<ProductSizeModel> ProductSizesList = new List<ProductSizeModel>();
        private List<ProductAddonModel> ProductAddonsList = new List<ProductAddonModel>();
        
        public event EventHandler OnAddToInvoice;
        public event EventHandler OnCancel;

        public event EventHandler OnSizeClicked;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProductSizeModel SelectedSize { get; internal set; }
        public List<ProductAddonModel> SelectedAddons { get; private set; } = new List<ProductAddonModel>();

        public UC_ProductOptions()
        {
            InitializeComponent();
            // إعداد Timer للحركة الناعمة
            _smoothTimer = new System.Windows.Forms.Timer();
            _smoothTimer.Interval = 10; // ~100fps
            _smoothTimer.Tick += SmoothTimer_Tick;

            lblName.MouseDown += DragHandle_MouseDown;
            lblName.MouseMove += DragHandle_MouseMove;
            lblName.MouseUp += DragHandle_MouseUp;
            lblName.MouseHover += LblName_MouseHover;
            lblName.MouseLeave += LblName_MouseLeave;
            label2.MouseDown += DragHandle_MouseDown;
            label2.MouseMove += DragHandle_MouseMove;
            label2.MouseUp += DragHandle_MouseUp;
            label2.MouseHover += LblName_MouseHover;
            label2.MouseLeave += LblName_MouseLeave;
            
            btn_close.Click += (s, e) => OnCancel?.Invoke(this, e);
            btn_cancel.Click += (s, e) => OnCancel?.Invoke(this, e);
            guna2Button1.Click += Guna2Button1_Click; // إضافة للفاتورة
        }

        private void Guna2Button1_Click(object? sender, EventArgs e)
        {
            // السماح بالإضافة حتى بدون اختيار حجم (لو فيه إضافات مختارة)
            if (SelectedSize == null && ProductSizesList.Count > 0 && SelectedAddons.Count == 0)
            {
                ToastManager.ShowWarning("تنبيه", "يرجى اختيار الحجم أو إضافة على الأقل.");
                return;
            }
            OnAddToInvoice?.Invoke(this, e);
        }

        private void LblName_MouseLeave(object? sender, EventArgs e)
        {
            // ✅ رجّع شكل الماوس لما كان عليه
            Cursor.Current = Cursors.Default;
            this.Cursor = Cursors.Default; 
        }

        private void LblName_MouseHover(object? sender, EventArgs e)
        {
            Cursor.Current = Cursors.SizeAll;
            this.Cursor = Cursors.SizeAll;
        }

        private void DragHandle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _targetLocation = this.Location;

                // ✅ غيّر شكل الماوس لـ SizeAll (السهم الرباعي)
                Cursor.Current = Cursors.SizeAll;
                this.Cursor = Cursors.SizeAll;

                Point cursorInParent = this.Parent.PointToClient(Cursor.Position);
                _mouseOffset = new Point(
                    cursorInParent.X - this.Left,
                    cursorInParent.Y - this.Top
                );

                ((Control)sender).Capture = true;
                _smoothTimer.Start();
            }
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            // ✅ نفس التحويل في الحركة
            Point cursorInParent = this.Parent.PointToClient(Cursor.Position);

            int newX = cursorInParent.X - _mouseOffset.X;
            int newY = cursorInParent.Y - _mouseOffset.Y;

            // منع الخروج من حدود الـ Parent
            if (this.Parent != null)
            {
                newX = Math.Max(0, Math.Min(newX, this.Parent.ClientSize.Width - this.Width));
                newY = Math.Max(0, Math.Min(newY, this.Parent.ClientSize.Height - this.Height));
            }

            // ✅ نحدث الهدف بس، مش الموقع مباشرة (الـ Timer هو اللي بيحرك)
            _targetLocation = new Point(newX, newY);
        }

        private void DragHandle_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            ((Control)sender).Capture = false;

            // ✅ رجّع شكل الماوس لما كان عليه
            Cursor.Current = Cursors.Default;
            this.Cursor = Cursors.Default;
        }
        private void SmoothTimer_Tick(object sender, EventArgs e)
        {
            // Lerp: تحريك تدريجي ناعم نحو الهدف
            float currentX = this.Left;
            float currentY = this.Top;

            float newX = currentX + (_targetLocation.X - currentX) * LERP_SPEED;
            float newY = currentY + (_targetLocation.Y - currentY) * LERP_SPEED;

            this.Location = new Point((int)newX, (int)newY);

            // وقف التايمر لما يوصل للهدف تقريباً
            if (!_isDragging &&
                Math.Abs(this.Left - _targetLocation.X) < 1 &&
                Math.Abs(this.Top - _targetLocation.Y) < 1)
            {
                this.Location = _targetLocation; // snap للموقع النهائي
                _smoothTimer.Stop();
            }
        }

        [Category("Product Data")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int ProductID
        { 
            get => _productid; 
            set 
            { 
                if (DesignMode) return;
                _productid = value; 
                SelectedSize = null;
                SelectedAddons.Clear();
                LoadAndRenderOptions(value); 
            }
        }
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
        private void btn_close_Click(object sender, EventArgs e)
        {
            //this.Hide();
            this.Visible = false;
        }

        private async void LoadAndRenderOptions(int productID)
        {
            await LoadOptionsAsync(productID);
            RenderSizePanels(productID);
            RenderAddonPanels(productID);
        }

        private async Task LoadOptionsAsync(int productID)
        {
            try
            {
                ProductSizesList.Clear();
                ProductAddonsList.Clear();

                using (SqlConnection conn = DB_Server.GetConnection())
                {
                    await conn.OpenAsync();

                    // Load Sizes
                    string sizeQuery = @"SELECT ps.*, s.SizeNameAr 
                                       FROM ProductSizes ps
                                       JOIN Sizes s ON ps.SizeID = s.SizeID
                                       WHERE ps.IsActive = 1 AND ps.ProductID = @PID";
                    using (SqlCommand cmd = new SqlCommand(sizeQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PID", productID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ProductSizesList.Add(new ProductSizeModel
                                {
                                    ProductSizeID = Convert.ToInt32(reader["ProductSizeID"]),
                                    ProductID = Convert.ToInt32(reader["ProductID"]),
                                    SizeID = Convert.ToInt32(reader["SizeID"]),
                                    SizeNameAr = reader["SizeNameAr"].ToString() ?? "",
                                    SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                    VAT = Convert.ToDecimal(reader["VAT"]),
                                    Barcode = reader["Barcode"].ToString() ?? "",
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
                                });
                            }
                        }
                    }

                    // Load Addons
                    string addonQuery = @"SELECT pa.*, a.AddonNameAr 
                                        FROM ProductAddons pa
                                        JOIN Addons a ON pa.AddonID = a.AddonID
                                        WHERE pa.IsActive = 1 AND pa.ProductID = @PID";
                    using (SqlCommand cmd = new SqlCommand(addonQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PID", productID);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ProductAddonsList.Add(new ProductAddonModel
                                {
                                    ProductAddonID = Convert.ToInt32(reader["ProductAddonID"]),
                                    ProductID = Convert.ToInt32(reader["ProductID"]),
                                    AddonID = Convert.ToInt32(reader["AddonID"]),
                                    AddonNameAr = reader["AddonNameAr"].ToString() ?? "",
                                    SalePrice = Convert.ToDecimal(reader["SalePrice"]),
                                    VAT = Convert.ToDecimal(reader["VAT"]),
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الخيارات: " + ex.Message);
            }
        }

        private void RenderSizePanels(int productID)
        {
            pn_variants_container_main.SuspendLayout();

            // امسح كل الـ panels القديمة (الديناميكية + الديمو)
            var oldPanelsSize = pn_variants_container_main.Controls
                .OfType<Guna.UI2.WinForms.Guna2Panel>()
                .Where(p => p.Name.StartsWith("pn_size_"))
                .ToList();

            foreach (var p in oldPanelsSize)
            {
                p.Controls.Clear();
                pn_variants_container_main.Controls.Remove(p);
                p.Dispose();
            }

            // إخفاء البانل التجريبي (الديمو)
            pn_variants_container.Visible = false;

            var sizes = ProductSizesList.Where(s => s.ProductID == productID).ToList();

            if (sizes.Count == 0)
            {
                pn_variants_container_main.BackgroundImage = Properties.Resources.لا_يوجد_احجام_لهذا_المنتج;
                pn_variants_container_main.AutoScrollMinSize = Size.Empty;
                pn_variants_container_main.ResumeLayout(true);
                return;
            }
            else
                pn_variants_container_main.BackgroundImage = null;

            const int panelWidth = 200;
            const int panelHeight = 250;
            const int spacing = 10;

            // حساب العرض الكلي للمحتوى الداخلي
            int totalContentWidth = sizes.Count * (panelWidth + spacing) + spacing;

            // ← إصلاح: استخدام AutoScrollMinSize بدل تغيير Width للحاوية المربوطة بـ Dock
            pn_variants_container_main.AutoScrollMinSize = new Size(totalContentWidth, 0);

            for (int index = 0; index < sizes.Count; index++)
            {
                var size = sizes[index];
                int xOffset = spacing + index * (panelWidth + spacing);

                var rb = new Guna.UI2.WinForms.Guna2CustomRadioButton
                {
                    Name = $"rb_size_{size.ProductSizeID}",
                    Dock = DockStyle.Top,
                    Size = new Size(panelWidth, 22),
                    Checked = false,
                    Tag = size
                };
                rb.CheckedChanged += (s, e) => { if (rb.Checked) SelectedSize = size; };
                
                // Style RadioButton
                rb.CheckedState.BorderColor = Color.FromArgb(60, 130, 246);
                rb.CheckedState.BorderThickness = 5;
                rb.CheckedState.FillColor = Color.FromArgb(60, 130, 246);
                rb.CheckedState.InnerColor = Color.White;
                rb.UncheckedState.BorderColor = Color.FromArgb(112, 120, 132);
                rb.UncheckedState.BorderThickness = 5;

                var pic = new Guna.UI2.WinForms.Guna2PictureBox
                {
                    Name = $"pic_size_{size.ProductSizeID}",
                    Dock = DockStyle.Top,
                    Size = new Size(panelWidth, 150),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Properties.Resources.burger
                };

                var lblNameSize = new Label
                {
                    Name = $"lbl_size_name_{size.ProductSizeID}",
                    Dock = DockStyle.Top,
                    Size = new Size(panelWidth, 35),
                    Font = new Font("Alexandria", 14.25F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(236, 240, 245),
                    Text = size.SizeNameAr,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var lblPriceSize = new Label
                {
                    Name = $"lbl_size_price_{size.ProductSizeID}",
                    Dock = DockStyle.Top,
                    Size = new Size(panelWidth, 30),
                    Font = new Font("Alexandria", 14.25F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(236, 240, 245),
                    Text = $"{size.SalePrice} ج.م",
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var panel = new Guna.UI2.WinForms.Guna2Panel
                {
                    Name = $"pn_size_{size.ProductSizeID}",
                    Size = new Size(panelWidth, panelHeight),
                    Location = new Point(xOffset, spacing),
                    BorderColor = Color.FromArgb(51, 65, 85),
                    BorderRadius = 15,
                    BorderThickness = 5,
                    Tag = size
                };

                panel.Controls.Add(rb);
                panel.Controls.Add(new Label { Dock = DockStyle.Top, Height = 5 });
                panel.Controls.Add(lblPriceSize);
                panel.Controls.Add(lblNameSize);
                panel.Controls.Add(pic);

                pic.Click += (s, e) => eventclickpanel(s, e);
                lblNameSize.Click += (s, e) => eventclickpanel(s, e);
                lblPriceSize.Click += (s, e) => eventclickpanel(s, e);
                panel.Click += (s, e) => eventclickpanel(s, e);
                rb.Click += (s, e) => eventclickpanel(s, e);
                
                pn_variants_container_main.Controls.Add(panel);
            }

            pn_variants_container_main.ResumeLayout(true);
        }
        private void eventclickpanel(object sender, EventArgs e)
        {
            Control ctrl = sender as Control;
            if (ctrl == null) return;

            // Find the panel (either sender is the panel or it's a child of the panel)
            Guna.UI2.WinForms.Guna2Panel targetPanel = ctrl as Guna.UI2.WinForms.Guna2Panel;
            if (targetPanel == null)
            {
                // Traverse up to find the Guna2Panel named "pn_size_..."
                Control current = ctrl.Parent;
                while (current != null && !(current is Guna.UI2.WinForms.Guna2Panel && current.Name.StartsWith("pn_size_")))
                {
                    current = current.Parent;
                }
                targetPanel = current as Guna.UI2.WinForms.Guna2Panel;
            }

            if (targetPanel != null && targetPanel.Tag is ProductSizeModel size)
            {
                SelectedSize = size;

                // Update RadioButtons and visual state for all size panels
                foreach (Control c in pn_variants_container_main.Controls)
                {
                    if (c is Guna.UI2.WinForms.Guna2Panel p && p.Name.StartsWith("pn_size_"))
                    {
                        bool isSelected = (p == targetPanel);
                        
                        // Update border color for visual feedback
                        p.BorderColor = isSelected ? Color.FromArgb(94, 148, 255) : Color.FromArgb(51, 65, 85);
                        
                        // Update RadioButton state
                        var rb = p.Controls.OfType<Guna.UI2.WinForms.Guna2CustomRadioButton>().FirstOrDefault();
                        if (rb != null)
                        {
                            rb.Checked = isSelected;
                        }
                    }
                }

                OnSizeClicked?.Invoke(this, e);
            }
        }
        private void RenderAddonPanels(int productID)
        {
            guna2Panel6.Controls.Clear();
            SelectedAddons.Clear();
            var addons = ProductAddonsList.Where(a => a.ProductID == productID).ToList();

            if (addons.Count == 0 || ProductAddonsList.Count == 0)
            {
                guna2Panel6.Visible = false;
                guna2Button3.Visible = false;
                return;
            }

            guna2Panel6.Visible = true;
            guna2Button3.Visible = true;

            // Iterate in reverse if using DockStyle.Top to maintain correct order from top to bottom
            for (int i = addons.Count - 1; i >= 0; i--)
            {
                var addon = addons[i];
                var panel = new Guna.UI2.WinForms.Guna2Panel
                {
                    Name = $"pn_addon_{addon.ProductAddonID}",
                    Size = new Size(601, 64),
                    Dock = DockStyle.Top,
                    Padding = new Padding(0, 0, 0, 5),
                    Tag = addon
                };

                var checkBox = new Guna.UI2.WinForms.Guna2CustomCheckBox
                {
                    Name = $"cb_addon_{addon.ProductAddonID}",
                    Size = new Size(32, 32),
                    Location = new Point(553, 16),
                    CheckedState = { 
                        BorderColor = Color.FromArgb(94, 148, 255), 
                        BorderRadius = 2, 
                        BorderThickness = 0, 
                        FillColor = Color.FromArgb(94, 148, 255) 
                    },
                    UncheckedState = { 
                        BorderColor = Color.FromArgb(125, 137, 149), 
                        BorderRadius = 2, 
                        BorderThickness = 0, 
                        FillColor = Color.FromArgb(125, 137, 149) 
                    },
                    Tag = addon
                };

                var lblName = new Label
                {
                    Name = $"lbl_addon_name_{addon.ProductAddonID}",
                    Text = addon.AddonNameAr,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(119, 16),
                    Size = new Size(428, 32),
                    TextAlign = ContentAlignment.MiddleLeft,
                    RightToLeft = RightToLeft.Yes
                };

                var lblPrice = new Label
                {
                    Name = $"lbl_addon_price_{addon.ProductAddonID}",
                    Text = $"{addon.SalePrice} جنية",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(3, 16),
                    Size = new Size(110, 32),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                panel.Controls.Add(checkBox);
                panel.Controls.Add(lblName);
                panel.Controls.Add(lblPrice);

                // Checkbox state logic
                checkBox.CheckedChanged += (s, e) =>
                {
                    if (checkBox.Checked)
                    {
                        if (!SelectedAddons.Contains(addon))
                            SelectedAddons.Add(addon);
                    }
                    else
                    {
                        SelectedAddons.Remove(addon);
                    }
                };

                // UX: Toggle checkbox on clicking the panel or text labels
                EventHandler toggleAction = (s, e) => { checkBox.Checked = !checkBox.Checked; };
                panel.Click += toggleAction;
                lblName.Click += toggleAction;
                lblPrice.Click += toggleAction;

                guna2Panel6.Controls.Add(panel);
            }
        }

    }
}
