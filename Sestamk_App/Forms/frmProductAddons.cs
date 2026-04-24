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
    public partial class frmProductAddons : BaseForm
    {
        private int _selectedProductAddonID = 0;
        private byte[] _currentImageBytes = null;

        public frmProductAddons()
        {
            InitializeComponent();
            Main_Methods.StyleDataGridView(dgvProductAddons);

            // ── إعداد الـ SearchField ComboBox ──
            SetupSearchFieldCombo();

            // ── ربط الأحداث ──
            this.Load += FrmProductAddons_Load;
            dgvProductAddons.CellClick += DgvProductAddons_CellClick;
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
            cmbSearchField.Items.Add("اسم الإضافة");
            cmbSearchField.Items.Add("سعر البيع");
            cmbSearchField.Items.Add("حالة التفعيل");
            cmbSearchField.SelectedIndex = 0;
            cmbSearchField.SelectedIndexChanged += (s, e) => LoadProductAddonsData(txtSearch.Text.Trim());
        }

        // ════════════════════════════════════════════════════
        //  📦  تحميل البيانات عند الفتح
        // ════════════════════════════════════════════════════
        private void FrmProductAddons_Load(object sender, EventArgs e)
        {
            LoadProductsComboBox();
            LoadAddonsComboBox();
            LoadProductAddonsData();
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

        private void LoadAddonsComboBox()
        {
            try
            {
                using var con = DB_Server.GetConnection();
                var da = new SqlDataAdapter("SELECT AddonID, AddonNameAr FROM Addons WHERE IsActive = 1", con);
                var dt = new DataTable();
                da.Fill(dt);
                cmbAddon.DataSource = dt;
                cmbAddon.DisplayMember = "AddonNameAr";
                cmbAddon.ValueMember = "AddonID";
                cmbAddon.SelectedIndex = -1;
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في تحميل الإضافات: " + ex.Message); }
        }

        private void LoadProductAddonsData(string searchTerm = "")
        {
            try
            {
                string field = cmbSearchField.SelectedItem?.ToString() ?? "اسم المنتج";

                string query = @"
                    SELECT pa.ProductAddonID, pa.ProductID, p.ProductNameAr,
                           pa.AddonID, a.AddonNameAr, pa.CostPrice, pa.SalePrice,
                           pa.VAT, pa.IsActive, pa.ProductImage
                    FROM ProductAddons pa
                    INNER JOIN Products p ON pa.ProductID = p.ProductID
                    INNER JOIN Addons a ON pa.AddonID = a.AddonID
                    WHERE p.IsDeleted = 0";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string col = field switch
                    {
                        "اسم المنتج"   => "p.ProductNameAr",
                        "اسم الإضافة"  => "a.AddonNameAr",
                        "سعر البيع"    => "CAST(pa.SalePrice AS NVARCHAR)",
                        "حالة التفعيل" => "CASE WHEN pa.IsActive=1 THEN N'نشط' ELSE N'غير نشط' END",
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

                dgvProductAddons.DataSource = dt;

                if (dgvProductAddons.Columns.Contains("ProductImage"))   dgvProductAddons.Columns["ProductImage"].Visible   = false;
                if (dgvProductAddons.Columns.Contains("ProductAddonID")) dgvProductAddons.Columns["ProductAddonID"].Visible = false;
                if (dgvProductAddons.Columns.Contains("ProductID"))      dgvProductAddons.Columns["ProductID"].Visible      = false;
                if (dgvProductAddons.Columns.Contains("AddonID"))        dgvProductAddons.Columns["AddonID"].Visible        = false;

                if (dgvProductAddons.Columns.Contains("ProductNameAr")) dgvProductAddons.Columns["ProductNameAr"].HeaderText = "المنتج";
                if (dgvProductAddons.Columns.Contains("AddonNameAr"))   dgvProductAddons.Columns["AddonNameAr"].HeaderText   = "الإضافة";
                if (dgvProductAddons.Columns.Contains("CostPrice"))     dgvProductAddons.Columns["CostPrice"].HeaderText     = "سعر التكلفة";
                if (dgvProductAddons.Columns.Contains("SalePrice"))     dgvProductAddons.Columns["SalePrice"].HeaderText     = "سعر البيع";
                if (dgvProductAddons.Columns.Contains("VAT"))           dgvProductAddons.Columns["VAT"].HeaderText           = "الضريبة(%)";
                if (dgvProductAddons.Columns.Contains("IsActive"))      dgvProductAddons.Columns["IsActive"].HeaderText      = "نشط";
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في تحميل بيانات الإضافات: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  ✅  التحقق من البيانات
        // ════════════════════════════════════════════════════
        private bool ValidateInputs()
        {
            if (cmbProduct.SelectedIndex == -1) { ToastManager.ShowWarning("تنبيه", "اختر المنتج"); return false; }
            if (cmbAddon.SelectedIndex == -1)   { ToastManager.ShowWarning("تنبيه", "اختر الإضافة"); return false; }
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
                string query = @"INSERT INTO ProductAddons (ProductID, AddonID, CostPrice, SalePrice, VAT, ProductImage, IsActive)
                                 VALUES (@ProductID, @AddonID, @CostPrice, @SalePrice, @VAT, @ProductImage, @IsActive)";

                using var con = DB_Server.GetConnection();
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductID", cmbProduct.SelectedValue);
                cmd.Parameters.AddWithValue("@AddonID",   cmbAddon.SelectedValue);
                cmd.Parameters.AddWithValue("@CostPrice", Convert.ToDecimal(txtCostPrice.Text));
                cmd.Parameters.AddWithValue("@SalePrice", Convert.ToDecimal(txtSalePrice.Text));
                cmd.Parameters.AddWithValue("@VAT",       Convert.ToDecimal(txtVAT.Text));
                cmd.Parameters.AddWithValue("@IsActive",  guna2ToggleSwitch1.Checked);

                if (_currentImageBytes != null)
                    cmd.Parameters.Add("@ProductImage", SqlDbType.VarBinary).Value = _currentImageBytes;
                else
                    cmd.Parameters.Add("@ProductImage", SqlDbType.VarBinary).Value = DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();

                ToastManager.ShowSuccess("نجاح", "تمت الإضافة بنجاح ✅");
                btn_clear_Click(null, null);
                LoadProductAddonsData();
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  ✏️  تعديل
        // ════════════════════════════════════════════════════
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedProductAddonID == 0) { ToastManager.ShowWarning("تنبيه", "اختر عنصراً للتعديل"); return; }
            if (!ValidateInputs()) return;
            try
            {
                string query = @"UPDATE ProductAddons SET
                                    ProductID = @ProductID, AddonID = @AddonID,
                                    CostPrice = @CostPrice, SalePrice = @SalePrice,
                                    VAT = @VAT, IsActive = @IsActive";

                if (_currentImageBytes != null) query += ", ProductImage = @ProductImage";
                query += " WHERE ProductAddonID = @ID";

                using var con = DB_Server.GetConnection();
                using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ID",        _selectedProductAddonID);
                cmd.Parameters.AddWithValue("@ProductID", cmbProduct.SelectedValue);
                cmd.Parameters.AddWithValue("@AddonID",   cmbAddon.SelectedValue);
                cmd.Parameters.AddWithValue("@CostPrice", Convert.ToDecimal(txtCostPrice.Text));
                cmd.Parameters.AddWithValue("@SalePrice", Convert.ToDecimal(txtSalePrice.Text));
                cmd.Parameters.AddWithValue("@VAT",       Convert.ToDecimal(txtVAT.Text));
                cmd.Parameters.AddWithValue("@IsActive",  guna2ToggleSwitch1.Checked);

                if (_currentImageBytes != null)
                    cmd.Parameters.Add("@ProductImage", SqlDbType.VarBinary).Value = _currentImageBytes;

                con.Open();
                cmd.ExecuteNonQuery();

                ToastManager.ShowSuccess("نجاح", "تم التعديل بنجاح ✅");
                btn_clear_Click(null, null);
                LoadProductAddonsData();
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  🗑️  حذف
        // ════════════════════════════════════════════════════
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedProductAddonID == 0) { ToastManager.ShowWarning("تنبيه", "اختر عنصراً للحذف"); return; }

            if (frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذه الإضافة؟"))
            {
                try
                {
                    using var con = DB_Server.GetConnection();
                    using var cmd = new SqlCommand("DELETE FROM ProductAddons WHERE ProductAddonID = @ID", con);
                    cmd.Parameters.AddWithValue("@ID", _selectedProductAddonID);
                    con.Open();
                    cmd.ExecuteNonQuery();

                    ToastManager.ShowSuccess("نجاح", "تم الحذف بنجاح ✅");
                    btn_clear_Click(null, null);
                    LoadProductAddonsData();
                }
                catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ: " + ex.Message); }
            }
        }

        // ════════════════════════════════════════════════════
        //  🧹  تنظيف الحقول
        // ════════════════════════════════════════════════════
        private void btn_clear_Click(object sender, EventArgs e)
        {
            _selectedProductAddonID = 0;
            cmbProduct.SelectedIndex = -1;
            cmbAddon.SelectedIndex   = -1;
            txtCostPrice.Clear();
            txtSalePrice.Clear();
            txtVAT.Clear();
            guna2ToggleSwitch1.Checked = true;
            guna2PictureBox1.Image = Properties.Resources.mode_landscape__1_;
            _currentImageBytes = null;
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
        private void DgvProductAddons_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvProductAddons.Rows[e.RowIndex];

            _selectedProductAddonID = Convert.ToInt32(row.Cells["ProductAddonID"].Value);
            cmbProduct.SelectedValue = Convert.ToInt32(row.Cells["ProductID"].Value);
            cmbAddon.SelectedValue   = Convert.ToInt32(row.Cells["AddonID"].Value);

            txtCostPrice.Text = row.Cells["CostPrice"].Value?.ToString();
            txtSalePrice.Text = row.Cells["SalePrice"].Value?.ToString();
            txtVAT.Text       = row.Cells["VAT"].Value?.ToString();
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
            LoadProductAddonsData(txtSearch.Text.Trim());
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
