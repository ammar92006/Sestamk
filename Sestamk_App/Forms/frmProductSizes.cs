using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmProductSizes : BaseForm
    {
        private int _selectedProductSizeID = 0;
        private byte[] _currentImageBytes = null;

        public frmProductSizes()
        {
            InitializeComponent();
            Main_Methods.StyleDataGridView(dgvProductSizes);

            // ── إعداد الـ SearchField ComboBox ──
            SetupSearchFieldCombo();

            // ── ربط الأحداث ──
            this.Load += FrmProductSizes_Load;
            dgvProductSizes.CellClick += DgvProductSizes_CellClick;
            txtSearch.TextChanged += txtSearch_TextChanged;

            // ── Drag & Drop للصورة ──
            pnlImageDrop.AllowDrop = true;
            pnlImageDrop.DragEnter += Panel_DragEnter;
            pnlImageDrop.DragDrop += Panel_DragDrop;
            pnlImageDrop.Click += (s, e) => SelectImage();
            label4.Click += (s, e) => SelectImage();
            guna2PictureBox1.Click += (s, e) => SelectImage();

            // ── Window Controls ──
            btn_close.Click += (s, e) => this.Close();
            btnMax.Click += btnMax_Click;
            btnMin.Click += btnMin_Click;

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(lblUserName, lblUserRole);
        }

        // ════════════════════════════════════════════════════
        //  🔍  إعداد حقول البحث
        // ════════════════════════════════════════════════════
        private void SetupSearchFieldCombo()
        {
            cmbSearchField.Items.Clear();
            cmbSearchField.Items.Add("اسم المنتج");
            cmbSearchField.Items.Add("اسم الحجم");
            cmbSearchField.Items.Add("الباركود");
            cmbSearchField.Items.Add("حالة التفعيل");
            cmbSearchField.SelectedIndex = 0;
            cmbSearchField.SelectedIndexChanged += (s, e) => LoadProductSizesData(txtSearch.Text.Trim());
        }

        // ════════════════════════════════════════════════════
        //  📦  تحميل البيانات عند الفتح
        // ════════════════════════════════════════════════════
        private void FrmProductSizes_Load(object sender, EventArgs e)
        {
            LoadProductsComboBox();
            LoadSizesComboBox();
            LoadProductSizesData();
            dgvProductSizes.ClearSelection();
        }

        private void LoadProductsComboBox()
        {
            try
            {
                using var con = DB_Server.GetConnection();
                var da = new SqlDataAdapter("SELECT ProductID, ProductNameAr FROM Products WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY ProductNameAr", con);
                var dt = new DataTable();
                da.Fill(dt);
                cmbProduct.DataSource = dt;
                cmbProduct.DisplayMember = "ProductNameAr";
                cmbProduct.ValueMember = "ProductID";
                cmbProduct.SelectedIndex = -1;
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في تحميل المنتجات: " + ex.Message); }
        }

        private void LoadSizesComboBox()
        {
            try
            {
                using var con = DB_Server.GetConnection();
                var da = new SqlDataAdapter("SELECT SizeID, SizeNameAr FROM Sizes WHERE IsActive = 1", con);
                var dt = new DataTable();
                da.Fill(dt);
                cmbSize.DataSource = dt;
                cmbSize.DisplayMember = "SizeNameAr";
                cmbSize.ValueMember = "SizeID";
                cmbSize.SelectedIndex = -1;
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في تحميل الأحجام: " + ex.Message); }
        }

        private void LoadProductSizesData(string searchTerm = "")
        {
            try
            {
                string field = cmbSearchField.SelectedItem?.ToString() ?? "اسم المنتج";

                string query = @"
                    SELECT ps.ProductSizeID, ps.ProductID, p.ProductNameAr, 
                           ps.SizeID, s.SizeNameAr, ps.CostPrice, ps.SalePrice, 
                           ps.VAT, ps.Barcode, ps.IsActive, ps.ProductImage
                    FROM ProductSizes ps
                    INNER JOIN Products p ON ps.ProductID = p.ProductID
                    INNER JOIN Sizes s ON ps.SizeID = s.SizeID
                    WHERE p.IsDeleted = 0";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string col = field switch
                    {
                        "اسم المنتج"   => "p.ProductNameAr",
                        "اسم الحجم"    => "s.SizeNameAr",
                        "الباركود"      => "ps.Barcode",
                        "حالة التفعيل" => "CASE WHEN ps.IsActive=1 THEN N'نشط' ELSE N'غير نشط' END",
                        _              => "p.ProductNameAr"
                    };
                    query += $" AND ({col} LIKE @search)";
                }

                using var con = DB_Server.GetConnection();
                using var cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(searchTerm))
                    cmd.Parameters.AddWithValue("@search", "%" + searchTerm + "%");

                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                dgvProductSizes.DataSource = dt;

                if (dgvProductSizes.Columns.Contains("ProductImage"))   dgvProductSizes.Columns["ProductImage"].Visible   = false;
                if (dgvProductSizes.Columns.Contains("ProductSizeID"))  dgvProductSizes.Columns["ProductSizeID"].Visible  = false;
                if (dgvProductSizes.Columns.Contains("ProductID"))      dgvProductSizes.Columns["ProductID"].Visible      = false;
                if (dgvProductSizes.Columns.Contains("SizeID"))         dgvProductSizes.Columns["SizeID"].Visible         = false;

                if (dgvProductSizes.Columns.Contains("ProductNameAr")) dgvProductSizes.Columns["ProductNameAr"].HeaderText = "المنتج";
                if (dgvProductSizes.Columns.Contains("SizeNameAr"))    dgvProductSizes.Columns["SizeNameAr"].HeaderText    = "الحجم";
                if (dgvProductSizes.Columns.Contains("CostPrice"))     dgvProductSizes.Columns["CostPrice"].HeaderText     = "سعر التكلفة";
                if (dgvProductSizes.Columns.Contains("SalePrice"))     dgvProductSizes.Columns["SalePrice"].HeaderText     = "سعر البيع";
                if (dgvProductSizes.Columns.Contains("VAT"))           dgvProductSizes.Columns["VAT"].HeaderText           = "الضريبة(%)";
                if (dgvProductSizes.Columns.Contains("Barcode"))       dgvProductSizes.Columns["Barcode"].HeaderText       = "الباركود";
                if (dgvProductSizes.Columns.Contains("IsActive"))      dgvProductSizes.Columns["IsActive"].HeaderText      = "نشط";
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في تحميل بيانات الأحجام: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  ✅  التحقق من البيانات
        // ════════════════════════════════════════════════════
        private bool ValidateInputs()
        {
            if (cmbProduct.SelectedIndex == -1) { ToastManager.ShowWarning("تنبيه", "اختر المنتج"); return false; }
            if (cmbSize.SelectedIndex == -1)    { ToastManager.ShowWarning("تنبيه", "اختر الحجم"); return false; }
            if (!decimal.TryParse(txtCostPrice.Text, out _)) { ToastManager.ShowWarning("تنبيه", "سعر التكلفة غير صالح"); return false; }
            if (!decimal.TryParse(txtSalePrice.Text, out _)) { ToastManager.ShowWarning("تنبيه", "سعر البيع غير صالح"); return false; }
            if (!decimal.TryParse(txtVAT.Text, out _))       { ToastManager.ShowWarning("تنبيه", "الضريبة غير صالحة"); return false; }
            return true;
        }

        // ════════════════════════════════════════════════════
        //  ➕  إضافة
        // ════════════════════════════════════════════════════
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;
            try
            {
                string query = @"INSERT INTO ProductSizes (ProductID, SizeID, CostPrice, SalePrice, VAT, Barcode, ProductImage, IsActive) 
                                 VALUES (@ProductID, @SizeID, @CostPrice, @SalePrice, @VAT, @Barcode, @ProductImage, @IsActive)";

                using var con = DB_Server.GetConnection();
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductID", cmbProduct.SelectedValue);
                cmd.Parameters.AddWithValue("@SizeID",    cmbSize.SelectedValue);
                cmd.Parameters.AddWithValue("@CostPrice", Convert.ToDecimal(txtCostPrice.Text));
                cmd.Parameters.AddWithValue("@SalePrice", Convert.ToDecimal(txtSalePrice.Text));
                cmd.Parameters.AddWithValue("@VAT",       Convert.ToDecimal(txtVAT.Text));
                cmd.Parameters.AddWithValue("@Barcode",   txtBarcode.Text.Trim());
                cmd.Parameters.AddWithValue("@IsActive",  guna2ToggleSwitch1.Checked);

                if (_currentImageBytes != null)
                    cmd.Parameters.Add("@ProductImage", SqlDbType.VarBinary).Value = _currentImageBytes;
                else
                    cmd.Parameters.Add("@ProductImage", SqlDbType.VarBinary).Value = DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();

                ToastManager.ShowSuccess("نجاح", "تمت الإضافة بنجاح ✅");
                btn_clear_Click(null, null);
                LoadProductSizesData();
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  ✏️  تعديل
        // ════════════════════════════════════════════════════
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedProductSizeID == 0) { ToastManager.ShowWarning("تنبيه", "اختر عنصراً للتعديل"); return; }
            if (!ValidateInputs()) return;
            try
            {
                string query = @"UPDATE ProductSizes SET 
                                    ProductID = @ProductID, SizeID = @SizeID, 
                                    CostPrice = @CostPrice, SalePrice = @SalePrice, 
                                    VAT = @VAT, Barcode = @Barcode, 
                                    IsActive = @IsActive";

                if (_currentImageBytes != null) query += ", ProductImage = @ProductImage";
                query += " WHERE ProductSizeID = @ID";

                using var con = DB_Server.GetConnection();
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ID",        _selectedProductSizeID);
                cmd.Parameters.AddWithValue("@ProductID", cmbProduct.SelectedValue);
                cmd.Parameters.AddWithValue("@SizeID",    cmbSize.SelectedValue);
                cmd.Parameters.AddWithValue("@CostPrice", Convert.ToDecimal(txtCostPrice.Text));
                cmd.Parameters.AddWithValue("@SalePrice", Convert.ToDecimal(txtSalePrice.Text));
                cmd.Parameters.AddWithValue("@VAT",       Convert.ToDecimal(txtVAT.Text));
                cmd.Parameters.AddWithValue("@Barcode",   txtBarcode.Text.Trim());
                cmd.Parameters.AddWithValue("@IsActive",  guna2ToggleSwitch1.Checked);

                if (_currentImageBytes != null)
                    cmd.Parameters.Add("@ProductImage", SqlDbType.VarBinary).Value = _currentImageBytes;

                con.Open();
                cmd.ExecuteNonQuery();

                ToastManager.ShowSuccess("نجاح", "تم التعديل بنجاح ✅");
                btn_clear_Click(null, null);
                LoadProductSizesData();
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  🗑️  حذف
        // ════════════════════════════════════════════════════
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedProductSizeID == 0) { ToastManager.ShowWarning("تنبيه", "اختر عنصراً للحذف"); return; }

            if (frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذا الحجم؟"))
            {
                try
                {
                    using var con = DB_Server.GetConnection();
                    using var cmd = new SqlCommand("DELETE FROM ProductSizes WHERE ProductSizeID = @ID", con);
                    cmd.Parameters.AddWithValue("@ID", _selectedProductSizeID);
                    con.Open();
                    cmd.ExecuteNonQuery();

                    ToastManager.ShowSuccess("نجاح", "تم الحذف بنجاح ✅");
                    btn_clear_Click(null, null);
                    LoadProductSizesData();
                }
                catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ: " + ex.Message); }
            }
        }

        // ════════════════════════════════════════════════════
        //  🧹  تنظيف الحقول
        // ════════════════════════════════════════════════════
        private void btn_clear_Click(object sender, EventArgs e)
        {
            _selectedProductSizeID = 0;
            cmbProduct.SelectedIndex = -1;
            cmbSize.SelectedIndex    = -1;
            txtCostPrice.Clear();
            txtSalePrice.Clear();
            txtVAT.Text= "0";
            txtBarcode.Clear();
            guna2ToggleSwitch1.Checked = true;
            guna2PictureBox1.Image = Properties.Resources.mode_landscape__1_;
            _currentImageBytes = null;
            dgvProductSizes.ClearSelection();
        }

        // ════════════════════════════════════════════════════
        //  🖼️  الصورة
        // ════════════════════════════════════════════════════
        private void SelectImage()
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(ofd.FileName);
                    guna2PictureBox1.Image = img;
                    guna2PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                    using var ms = new MemoryStream();
                    img.Save(ms, img.RawFormat);
                    _currentImageBytes = ms.ToArray();
                }
                catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في قراءة الصورة: " + ex.Message); }
            }
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0)
            {
                try
                {
                    Image img = Image.FromFile(files[0]);
                    guna2PictureBox1.Image = img;
                    guna2PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    using var ms = new MemoryStream();
                    img.Save(ms, img.RawFormat);
                    _currentImageBytes = ms.ToArray();
                }
                catch { }
            }
        }

        // ════════════════════════════════════════════════════
        //  📋  Grid Cell Click
        // ════════════════════════════════════════════════════
        private void DgvProductSizes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvProductSizes.Rows[e.RowIndex];

            _selectedProductSizeID = Convert.ToInt32(row.Cells["ProductSizeID"].Value);
            cmbProduct.SelectedValue = Convert.ToInt32(row.Cells["ProductID"].Value);
            cmbSize.SelectedValue    = Convert.ToInt32(row.Cells["SizeID"].Value);

            txtCostPrice.Text = row.Cells["CostPrice"].Value?.ToString();
            txtSalePrice.Text = row.Cells["SalePrice"].Value?.ToString();
            txtVAT.Text       = row.Cells["VAT"].Value?.ToString();
            txtBarcode.Text   = row.Cells["Barcode"].Value?.ToString();
            guna2ToggleSwitch1.Checked = Convert.ToBoolean(row.Cells["IsActive"].Value);

            if (row.Cells["ProductImage"].Value != DBNull.Value && row.Cells["ProductImage"].Value != null)
            {
                _currentImageBytes = (byte[])row.Cells["ProductImage"].Value;
                using var ms = new MemoryStream(_currentImageBytes);
                guna2PictureBox1.Image = Image.FromStream(ms);
                guna2PictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                _currentImageBytes = null;
                guna2PictureBox1.Image = Properties.Resources.mode_landscape__1_;
            }
        }

        // ════════════════════════════════════════════════════
        //  🔍  البحث
        // ════════════════════════════════════════════════════
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProductSizesData(txtSearch.Text.Trim());
        }

        // ════════════════════════════════════════════════════
        //  🪟  Window Controls
        // ════════════════════════════════════════════════════
        private void btn_close_Click(object sender, EventArgs e)  => this.Close();
        private void btnMax_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal : FormWindowState.Maximized;
        }
        private void btnMin_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
    }
}
