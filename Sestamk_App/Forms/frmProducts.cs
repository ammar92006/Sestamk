using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmProducts : BaseForm
    {
        protected override Size DesignClientSize => new Size(1650, 1000);

        // ══════════════════════════════════════════════
        //  📊  Data
        // ══════════════════════════════════════════════
        private BindingList<Products> ProductsList = new BindingList<Products>();
        private BindingSource productsBinding = new BindingSource();
        private List<Products> allProducts = new List<Products>();

        private int _selectedProductID = 0;
        private string ProductImageBase64 = null;

        // ══════════════════════════════════════════════
        //  🔍  Search Timer
        // ══════════════════════════════════════════════
        private System.Timers.Timer searchTimer;

        // ══════════════════════════════════════════════
        //  🖼️  Image cache (prevents GC disposal)
        // ══════════════════════════════════════════════
        private Image _currentProductImage = null;

        // ══════════════════════════════════════════════
        //  📦  Category cache for filter
        // ══════════════════════════════════════════════
        private DataTable _categoriesTable = null;

        public frmProducts()
        {
            InitializeComponent();

            // ═══ حل مشكلة تكرار الأعمدة ═══
            dgvProducts.AutoGenerateColumns = false;

            InitDataGridView();
            Main_Methods.StyleDataGridView(dgvProducts);
            BindGridColumns();
            Main_Methods.FillComboBoxWithGridHeaders(dgvProducts, cmbSearchField);

            // ── إعداد البحث ──
            searchTimer = new System.Timers.Timer(300);
            searchTimer.AutoReset = false;
            searchTimer.Elapsed += SearchTimer_Elapsed;

            txtSearch.TextChanged += txtSearch_TextChanged;
            cmbSearchField.SelectedIndexChanged += (s, e) => ApplyAllFilters();

            // ── تحميل البيانات ──
            LoadCategoriesComboBox();
            SetupFilterCombos();
            _ = LoadProductsAsync();

            // ── إعداد الصورة (drag & drop) ──
            pnlImageDrop.AllowDrop = true;
            pnlImageDrop.DragEnter += Panel_DragEnter;
            pnlImageDrop.DragDrop += Panel_DragDrop;
            pnlImageDrop.Click += (s, e) => SelectProductImage();
            label4.Click += (s, e) => SelectProductImage();
            picProductImage.Click += (s, e) => SelectProductImage();

            // ── ربط أحداث الأزرار ──
            btnSave.Click += btnSave_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btn_clear.Click += btn_clear_Click;

            dgvProducts.CellClick += dgvProducts_CellClick;
            dgvProducts.CellFormatting += dgvProducts_CellFormatting;

            // ── أزرار وقت التحضير +/- ──
            btnPrepPlus.Click += btnPrepPlus_Click;
            btnPrepMinus.Click += btnPrepMinus_Click;

            // ── إعداد الـ PictureBox ──
            picProductImage.SizeMode = PictureBoxSizeMode.Zoom;

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(lblUserName, lblUserRole);
        }

        // ════════════════════════════════════════════════════
        //  🔄  تهيئة الـ DataGridView
        // ════════════════════════════════════════════════════
        private void InitDataGridView()
        {
            productsBinding.DataSource = ProductsList;
            dgvProducts.DataSource = productsBinding;
        }

        private void BindGridColumns()
        {
            var mappings = new Dictionary<string, string>
            {
                { "colProductID", "ProductID" },
                { "colProductCode", "ProductCode" },
                { "colProductNameAr", "ProductNameAr" },
                { "colProductNameEn", "ProductNameEn" },
                { "colCategoryID", "CategoryID" },
                { "colImage", "Image" },
                { "colDescription", "Description" },
                { "colDiscountPercent", "DiscountPercent" },
                { "colIsActive", "IsActive" },
                { "colPreparationTime", "PreparationTime" },
                { "colNotes", "Notes" },
                { "colCreatedDate", "CreatedDate" },
                { "colCreatedByUserID", "CreatedByUserID" },
                { "colLastModified", "LastModified" },
            };

            foreach (var map in mappings)
            {
                if (dgvProducts.Columns.Contains(map.Key))
                    dgvProducts.Columns[map.Key].DataPropertyName = map.Value;
            }
        }

        // ════════════════════════════════════════════════════
        //  🎨  تنسيق خلايا الجدول
        // ════════════════════════════════════════════════════
        private void dgvProducts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;
            string colName = dgvProducts.Columns[e.ColumnIndex].Name;

            if (colName == "colIsActive")
            {
                bool isActive = e.Value is bool b ? b : bool.TryParse(e.Value.ToString(), out bool r) && r;
                e.Value = isActive ? "نشط ✅" : "غير نشط ❌";
                e.CellStyle.ForeColor = isActive ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 68, 68);
                e.FormattingApplied = true;
            }
            else if (colName == "colDiscountPercent")
            {
                if (float.TryParse(e.Value.ToString(), out float disc))
                {
                    e.Value = disc > 0 ? $"{disc:0.##}%" : "—";
                    e.FormattingApplied = true;
                }
            }
            else if (colName == "colPreparationTime")
            {
                if (e.Value != null)
                {
                    string val = e.Value.ToString();
                    if (TimeSpan.TryParse(val, out TimeSpan ts))
                    {
                        if (ts.TotalMinutes > 0)
                        {
                            e.Value = $"{(int)ts.TotalMinutes} د";
                            e.FormattingApplied = true;
                        }
                    }
                    else if (int.TryParse(val, out int mins) && mins > 0)
                    {
                        e.Value = $"{mins} د";
                        e.FormattingApplied = true;
                    }
                }
            }
        }

        // ════════════════════════════════════════════════════
        //  📦  تحميل الأقسام في ComboBox + الفلتر
        // ════════════════════════════════════════════════════
        private void LoadCategoriesComboBox()
        {
            try
            {
                string query = @"SELECT CategoryID, CategoryNameAr 
                                 FROM ProductCategories 
                                 WHERE IsDeleted = 0 AND IsActive = 1
                                 ORDER BY CategoryNameAr";

                using (SqlConnection con = DB_Server.GetConnection())
                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    _categoriesTable = new DataTable();
                    da.Fill(_categoriesTable);

                    // ── ComboBox الأساسي (بيانات المنتج) ──
                    cmbCategory.DataSource = _categoriesTable.Copy();
                    cmbCategory.DisplayMember = "CategoryNameAr";
                    cmbCategory.ValueMember = "CategoryID";
                    cmbCategory.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في تحميل الأقسام: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  🔽  إعداد كومبو بوكس الفلترة
        // ════════════════════════════════════════════════════
        private void SetupFilterCombos()
        {
            // ── فلتر حالة النشاط ──
            cmbFilterStatus.Items.Clear();
            cmbFilterStatus.Items.Add("الكل");
            cmbFilterStatus.Items.Add("نشط");
            cmbFilterStatus.Items.Add("غير نشط");
            cmbFilterStatus.SelectedIndex = 0;
            cmbFilterStatus.SelectedIndexChanged += (s, e) => ApplyAllFilters();

            // ── فلتر القسم ──
            if (_categoriesTable != null)
            {
                DataTable filterCatTable = _categoriesTable.Copy();
                DataRow allRow = filterCatTable.NewRow();
                allRow["CategoryID"] = 0;
                allRow["CategoryNameAr"] = "كل الأقسام";
                filterCatTable.Rows.InsertAt(allRow, 0);

                cmbFilterCategory.DataSource = filterCatTable;
                cmbFilterCategory.DisplayMember = "CategoryNameAr";
                cmbFilterCategory.ValueMember = "CategoryID";
                cmbFilterCategory.SelectedIndex = 0;
            }
            cmbFilterCategory.SelectedIndexChanged += (s, e) => ApplyAllFilters();
        }

        // ════════════════════════════════════════════════════
        //  🔄  تحميل المنتجات من قاعدة البيانات
        // ════════════════════════════════════════════════════
        private async Task LoadProductsAsync()
        {
            try
            {
                string query = @"SELECT p.ProductID, p.ProductCode, p.ProductNameAr, p.ProductNameEn,
                                        p.CategoryID, p.Image, p.Description, p.DiscountPercent,
                                        p.IsActive, p.PreparationTime, p.Notes,
                                        p.CreatedDate, p.CreatedByUserID, p.LastModified
                                 FROM Products p
                                 WHERE p.IsDeleted = 0
                                 ORDER BY p.ProductID DESC";

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

                dgvProducts.SuspendLayout();
                productsBinding.RaiseListChangedEvents = false;

                ProductsList.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    string GetString(string col) => row[col] == DBNull.Value ? "" : row[col].ToString();
                    int GetInt(string col) => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);
                    bool GetBool(string col) => row[col] != DBNull.Value && Convert.ToBoolean(row[col]);
                    float GetFloat(string col) => row[col] == DBNull.Value ? 0f : Convert.ToSingle(row[col]);

                    ProductsList.Add(new Products
                    {
                        ProductID = Convert.ToInt32(row["ProductID"]),
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
                    });
                }

                allProducts = ProductsList.ToList();

                productsBinding.RaiseListChangedEvents = true;
                productsBinding.ResetBindings(false);
                dgvProducts.ResumeLayout();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التحميل: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        //  🔍  البحث + الفلترة المدمجة
        // ════════════════════════════════════════════════════
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        private void SearchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke(new Action(() => ApplyAllFilters()));
        }

        /// <summary>
        /// يطبق جميع الفلاتر: البحث + حالة النشاط + القسم
        /// </summary>
        private void ApplyAllFilters()
        {
            string keyword = (txtSearch.Text ?? "").Trim();
            string searchField = cmbSearchField.SelectedItem?.ToString() ?? "اسم المنتج (عربي)";

            IEnumerable<Products> result = allProducts;

            // ═══ فلتر حالة النشاط ═══
            string statusFilter = cmbFilterStatus.SelectedItem?.ToString() ?? "الكل";
            if (statusFilter == "نشط")
                result = result.Where(p => p.IsActive);
            else if (statusFilter == "غير نشط")
                result = result.Where(p => !p.IsActive);

            // ═══ فلتر القسم ═══
            if (cmbFilterCategory.SelectedValue != null)
            {
                int catId = Convert.ToInt32(cmbFilterCategory.SelectedValue);
                if (catId > 0)
                    result = result.Where(p => p.CategoryID == catId);
            }

            // ═══ فلتر البحث النصي ═══
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = keyword;
                result = searchField switch
                {
                    "كود المنتج" => result.Where(p => (p.ProductCode ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)),
                    "اسم المنتج (عربي)" => result.Where(p => (p.ProductNameAr ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)),
                    "اسم المنتج (إنجليزي)" => result.Where(p => (p.ProductNameEn ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)),
                    "الوصف" => result.Where(p => (p.Description ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)),
                    "نسبة الخصم" => result.Where(p => p.DiscountPercent.ToString().Contains(kw)),
                    "حالة التفعيل" => result.Where(p => (p.IsActive ? "نشط" : "غير نشط").Contains(kw)),
                    "وقت التحضير" => result.Where(p => (p.PreparationTime ?? "").Contains(kw)),
                    "ملاحظات" => result.Where(p => (p.Notes ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)),
                    "تاريخ الإنشاء" => result.Where(p => (p.CreatedDate ?? "").Contains(kw)),
                    "آخر تعديل" => result.Where(p => (p.LastModified ?? "").Contains(kw)),
                    _ => result.Where(p => (p.ProductNameAr ?? "").Contains(kw, StringComparison.OrdinalIgnoreCase)),
                };
            }

            // ═══ تحديث الجدول ═══
            dgvProducts.SuspendLayout();
            productsBinding.RaiseListChangedEvents = false;

            ProductsList.Clear();
            foreach (var p in result)
                ProductsList.Add(p);

            productsBinding.RaiseListChangedEvents = true;
            productsBinding.ResetBindings(false);
            dgvProducts.ResumeLayout();
        }

        // ════════════════════════════════════════════════════
        //  🔍  تحويل أسماء الأعمدة
        // ════════════════════════════════════════════════════
        private string GetDatabaseColumnName(string field)
        {
            if (string.IsNullOrWhiteSpace(field)) return "";
            return field.Trim() switch
            {
                "كود المنتج" => "ProductCode",
                "اسم المنتج (عربي)" => "ProductNameAr",
                "اسم المنتج (إنجليزي)" => "ProductNameEn",
                "الوصف" => "Description",
                "نسبة الخصم" => "DiscountPercent",
                "حالة التفعيل" => "IsActive",
                "وقت التحضير" => "PreparationTime",
                "ملاحظات" => "Notes",
                "تاريخ الإنشاء" => "CreatedDate",
                "آخر تعديل" => "LastModified",
                _ => ""
            };
        }

        // ════════════════════════════════════════════════════
        //  📋  ملء الفورم عند الضغط على صف
        // ════════════════════════════════════════════════════
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
            if (row.Cells["colProductID"].Value == null) return;

            _selectedProductID = Convert.ToInt32(row.Cells["colProductID"].Value);

            txtProductCode.Text = row.Cells["colProductCode"].Value?.ToString() ?? "";
            txtNameAr.Text = row.Cells["colProductNameAr"].Value?.ToString() ?? "";
            txtNameEn.Text = row.Cells["colProductNameEn"].Value?.ToString() ?? "";
            txtDescription.Text = row.Cells["colDescription"].Value?.ToString() ?? "";
            txtDiscount.Text = row.Cells["colDiscountPercent"].Value?.ToString() ?? "";
            
            var prepVal = row.Cells["colPreparationTime"].Value?.ToString();
            if (TimeSpan.TryParse(prepVal, out TimeSpan prepTs))
                txtPrepTime.Text = ((int)prepTs.TotalMinutes).ToString();
            else if (int.TryParse(prepVal, out int pMins))
                txtPrepTime.Text = pMins.ToString();
            else
                txtPrepTime.Text = "15";

            txtNotes.Text = row.Cells["colNotes"].Value?.ToString() ?? "";
            lblCreatedAt.Text = row.Cells["colCreatedDate"].Value?.ToString() ?? "";
            lblLastActivity.Text = row.Cells["colLastModified"].Value?.ToString() ?? "";
            lblAddedBy.Text = row.Cells["colCreatedByUserID"].Value?.ToString() ?? "";

            // ── حالة التفعيل ──
            var isActiveVal = row.Cells["colIsActive"].Value;
            tglIsActive.Checked = isActiveVal != null && isActiveVal != DBNull.Value && Convert.ToBoolean(isActiveVal);

            // ── تحديد القسم ──
            var catVal = row.Cells["colCategoryID"].Value;
            if (catVal != null && catVal != DBNull.Value && Convert.ToInt32(catVal) > 0)
                cmbCategory.SelectedValue = Convert.ToInt32(catVal);
            else
                cmbCategory.SelectedIndex = -1;

            // ── تحميل الصورة ──
            LoadProductImage(_selectedProductID);

            // ── تحميل السعر والباركود والضريبة ──
            LoadVariantAndTaxData(_selectedProductID);
        }

        // ════════════════════════════════════════════════════
        //  🖼️  تحميل صورة المنتج (نسخة مستقلة)
        // ════════════════════════════════════════════════════
        private void LoadProductImage(int productID)
        {
            // ── تنظيف الصورة القديمة ──
            DisposeCurrentImage();
            ProductImageBase64 = null;

            var product = ProductsList.FirstOrDefault(p => p.ProductID == productID);
            if (product != null && !string.IsNullOrEmpty(product.Image))
            {
                try
                {
                    byte[] imgBytes = Convert.FromBase64String(product.Image);
                    // ── إنشاء Bitmap مستقل لا يعتمد على Stream ──
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        using (Image tempImg = Image.FromStream(ms, true, true))
                        {
                            // Bitmap مستقل — الصورة هتفضل ظاهرة حتى بعد ما الـ Stream يتقفل
                            _currentProductImage = new Bitmap(tempImg.Width, tempImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(_currentProductImage))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                g.DrawImage(tempImg, 0, 0, tempImg.Width, tempImg.Height);
                            }
                        }
                    }
                    picProductImage.Image = _currentProductImage;
                    ProductImageBase64 = product.Image;
                }
                catch
                {
                    picProductImage.Image = Properties.Resources.mode_landscape__1_;
                    ProductImageBase64 = null;
                }
            }
            else
            {
                picProductImage.Image = Properties.Resources.mode_landscape__1_;
            }
        }

        private void DisposeCurrentImage()
        {
            if (_currentProductImage != null)
            {
                picProductImage.Image = null;
                _currentProductImage.Dispose();
                _currentProductImage = null;
            }
        }

        // ════════════════════════════════════════════════════
        //  💰  تحميل بيانات الـ Variant + الضريبة (اتصال واحد)
        // ════════════════════════════════════════════════════
        private void LoadVariantAndTaxData(int productID)
        {
            try
            {
                string query = @"
                    SELECT 
                        (SELECT TOP 1 Price FROM Variants WHERE ProductID = @PID ORDER BY IsDefault DESC, VariantID ASC) AS Price,
                        (SELECT TOP 1 Barcode FROM Variants WHERE ProductID = @PID ORDER BY IsDefault DESC, VariantID ASC) AS Barcode,
                        (SELECT TaxPercent FROM Products WHERE ProductID = @PID) AS TaxPercent";

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PID", productID);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtPrice.Text = reader["Price"] != DBNull.Value
                                ? Convert.ToDecimal(reader["Price"]).ToString("0.00") : "0.00";
                            txtBarcode.Text = reader["Barcode"] != DBNull.Value
                                ? reader["Barcode"].ToString() : "";
                            txtTax.Text = reader["TaxPercent"] != DBNull.Value
                                ? Convert.ToSingle(reader["TaxPercent"]).ToString("0.00") : "0.00";
                        }
                        else
                        {
                            txtPrice.Text = "0.00";
                            txtBarcode.Text = "";
                            txtTax.Text = "0.00";
                        }
                    }
                }
            }
            catch
            {
                txtPrice.Text = "0.00";
                txtBarcode.Text = "";
                txtTax.Text = "0.00";
            }
        }

        // ════════════════════════════════════════════════════
        //  ✅  التحقق من المدخلات
        // ════════════════════════════════════════════════════
        private bool ValidateProductInputs(out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrWhiteSpace(txtProductCode.Text))
            { errorMessage = "من فضلك أدخل كود المنتج"; txtProductCode.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtNameAr.Text))
            { errorMessage = "من فضلك أدخل اسم المنتج (عربي)"; txtNameAr.Focus(); return false; }
            if (cmbCategory.SelectedIndex < 0 || cmbCategory.SelectedValue == null)
            { errorMessage = "من فضلك اختر القسم"; cmbCategory.Focus(); return false; }
            return true;
        }

        // ════════════════════════════════════════════════════
        //  🔍  التحقق من تكرار الكود
        // ════════════════════════════════════════════════════
        private async Task<bool> IsProductCodeExists(string code, int currentId = 0)
        {
            const string query = "SELECT COUNT(*) FROM Products WHERE ProductCode = @Code AND ProductID <> @ID AND IsDeleted = 0";
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@ID", currentId);
                await conn.OpenAsync();
                return (int)await cmd.ExecuteScalarAsync() > 0;
            }
        }

        // ════════════════════════════════════════════════════
        //  ➕  إضافة منتج جديد
        // ════════════════════════════════════════════════════
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateProductInputs(out string msg))
            { ToastManager.ShowWarning("تنبيه", msg); return; }

            string productCode = txtProductCode.Text.Trim();

            if (await IsProductCodeExists(productCode))
            { ToastManager.ShowWarning("تنبيه", "كود المنتج مسجل مسبقاً، يرجى اختيار كود آخر."); return; }

            SetButtonsEnabled(false);

            try
            {
                int newProductId = await InsertProductAsync();
                await InsertVariantAsync(newProductId);

                string nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var newProduct = new Products
                {
                    ProductID = newProductId,
                    ProductCode = productCode,
                    ProductNameAr = txtNameAr.Text.Trim(),
                    ProductNameEn = txtNameEn.Text.Trim(),
                    CategoryID = Convert.ToInt32(cmbCategory.SelectedValue ?? 0),
                    Image = ProductImageBase64 ?? "",
                    Description = txtDescription.Text.Trim(),
                    DiscountPercent = float.TryParse(txtDiscount.Text, out float disc) ? disc : 0f,
                    IsActive = tglIsActive.Checked,
                    PreparationTime = TimeSpan.FromMinutes(int.TryParse(txtPrepTime.Text, out int tMins) ? tMins : 0).ToString(@"hh\:mm\:ss"),
                    Notes = txtNotes.Text.Trim(),
                    CreatedDate = nowStr,
                    CreatedByUserID = UserSession.UserId,
                    LastModified = nowStr,
                };

                ProductsList.Add(newProduct);
                allProducts = ProductsList.ToList();

                ToastManager.ShowSuccess("نجاح", "تمت الإضافة بنجاح ✅");
                ClearProductForm();
            }
            catch (Exception ex)
            { ToastManager.ShowError("خطأ", "خطأ في الإضافة: " + ex.Message); }
            finally { SetButtonsEnabled(true); }
        }

        private async Task<int> InsertProductAsync()
        {
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Products 
                (ProductCode, ProductNameAr, ProductNameEn, CategoryID, Image, 
                 Description, DiscountPercent, TaxPercent, IsActive, PreparationTime, 
                 Notes, CreatedByUserID, LastModified)
                OUTPUT INSERTED.ProductID
                VALUES 
                (@Code, @NameAr, @NameEn, @CategoryID, @Image,
                 @Description, @Discount, @Tax, @Active, @PrepTime,
                 @Notes, @User, GETDATE())", conn))
            {
                cmd.Parameters.AddWithValue("@Code", txtProductCode.Text.Trim());
                cmd.Parameters.AddWithValue("@NameAr", txtNameAr.Text.Trim());
                cmd.Parameters.AddWithValue("@NameEn", txtNameEn.Text.Trim());
                cmd.Parameters.AddWithValue("@CategoryID", Convert.ToInt32(cmbCategory.SelectedValue ?? 0));
                cmd.Parameters.AddWithValue("@Image", (object)ProductImageBase64 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@Discount", float.TryParse(txtDiscount.Text, out float d) ? d : 0f);
                cmd.Parameters.AddWithValue("@Tax", float.TryParse(txtTax.Text, out float t) ? t : 0f);
                cmd.Parameters.AddWithValue("@Active", tglIsActive.Checked);
                int prepMins = int.TryParse(txtPrepTime.Text.Trim(), out int val) ? val : 0;
                cmd.Parameters.AddWithValue("@PrepTime", TimeSpan.FromMinutes(prepMins));
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text.Trim());
                cmd.Parameters.AddWithValue("@User", UserSession.UserId);

                await conn.OpenAsync();
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        private async Task InsertVariantAsync(int productID)
        {
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Variants 
                (ProductID, VariantCode, VariantNameAr, VariantNameEn, Price, Cost, Barcode, DisplayOrder, IsDefault)
                VALUES 
                (@ProductID, @VCode, @VNameAr, @VNameEn, @Price, @Cost, @Barcode, 1, 1)", conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);
                cmd.Parameters.AddWithValue("@VCode", txtProductCode.Text.Trim());
                cmd.Parameters.AddWithValue("@VNameAr", txtNameAr.Text.Trim());
                cmd.Parameters.AddWithValue("@VNameEn", txtNameEn.Text.Trim());
                cmd.Parameters.AddWithValue("@Price", decimal.TryParse(txtPrice.Text, out decimal p) ? p : 0m);
                cmd.Parameters.AddWithValue("@Cost", 0m);
                cmd.Parameters.AddWithValue("@Barcode", txtBarcode.Text.Trim());

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ════════════════════════════════════════════════════
        //  ✏️  تعديل منتج
        // ════════════════════════════════════════════════════
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_selectedProductID == 0)
            { ToastManager.ShowWarning("تنبيه", "من فضلك اختر منتجاً لتعديله"); return; }

            if (!ValidateProductInputs(out string msg))
            { ToastManager.ShowWarning("تنبيه", msg); return; }

            if (await IsProductCodeExists(txtProductCode.Text.Trim(), _selectedProductID))
            { ToastManager.ShowWarning("تنبيه", "كود المنتج مكرر مع منتج آخر!"); return; }

            SetButtonsEnabled(false);

            try
            {
                await UpdateProductAsync();
                await UpdateVariantAsync(_selectedProductID);

                var existing = ProductsList.FirstOrDefault(x => x.ProductID == _selectedProductID);
                if (existing != null)
                {
                    existing.ProductCode = txtProductCode.Text.Trim();
                    existing.ProductNameAr = txtNameAr.Text.Trim();
                    existing.ProductNameEn = txtNameEn.Text.Trim();
                    existing.CategoryID = Convert.ToInt32(cmbCategory.SelectedValue ?? 0);
                    existing.Description = txtDescription.Text.Trim();
                    existing.DiscountPercent = float.TryParse(txtDiscount.Text, out float disc) ? disc : 0f;
                    existing.IsActive = tglIsActive.Checked;
                    existing.PreparationTime = TimeSpan.FromMinutes(int.TryParse(txtPrepTime.Text, out int tMins) ? tMins : 0).ToString(@"hh\:mm\:ss");
                    existing.Notes = txtNotes.Text.Trim();
                    existing.LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (ProductImageBase64 != null) existing.Image = ProductImageBase64;
                }

                allProducts = ProductsList.ToList();
                productsBinding.ResetBindings(false);

                ToastManager.ShowSuccess("نجاح", "تم التعديل بنجاح ✅");
                ClearProductForm();
            }
            catch (Exception ex)
            { ToastManager.ShowError("خطأ", "خطأ في التعديل: " + ex.Message); }
            finally { SetButtonsEnabled(true); }
        }

        private async Task UpdateProductAsync()
        {
            string imageClause = ProductImageBase64 != null ? ", Image=@Image" : "";

            string query = $@"UPDATE Products SET
                    ProductCode=@Code, ProductNameAr=@NameAr, ProductNameEn=@NameEn,
                    CategoryID=@CategoryID, Description=@Description,
                    DiscountPercent=@Discount, TaxPercent=@Tax,
                    IsActive=@Active, PreparationTime=@PrepTime,
                    Notes=@Notes, LastModified=GETDATE(){imageClause}
                    WHERE ProductID=@ID AND IsDeleted=0";

            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", _selectedProductID);
                cmd.Parameters.AddWithValue("@Code", txtProductCode.Text.Trim());
                cmd.Parameters.AddWithValue("@NameAr", txtNameAr.Text.Trim());
                cmd.Parameters.AddWithValue("@NameEn", txtNameEn.Text.Trim());
                cmd.Parameters.AddWithValue("@CategoryID", Convert.ToInt32(cmbCategory.SelectedValue ?? 0));
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@Discount", float.TryParse(txtDiscount.Text, out float d) ? d : 0f);
                cmd.Parameters.AddWithValue("@Tax", float.TryParse(txtTax.Text, out float t) ? t : 0f);
                cmd.Parameters.AddWithValue("@Active", tglIsActive.Checked);
                int prepMins = int.TryParse(txtPrepTime.Text.Trim(), out int val) ? val : 0;
                cmd.Parameters.AddWithValue("@PrepTime", TimeSpan.FromMinutes(prepMins));
                cmd.Parameters.AddWithValue("@Notes", txtNotes.Text.Trim());
                if (ProductImageBase64 != null)
                    cmd.Parameters.AddWithValue("@Image", ProductImageBase64);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task UpdateVariantAsync(int productID)
        {
            const string query = @"
                IF EXISTS (SELECT 1 FROM Variants WHERE ProductID = @ProductID)
                    UPDATE TOP(1) Variants SET Price=@Price, Barcode=@Barcode WHERE ProductID=@ProductID
                ELSE
                    INSERT INTO Variants (ProductID, VariantCode, VariantNameAr, VariantNameEn, Price, Cost, Barcode, DisplayOrder, IsDefault)
                    VALUES (@ProductID, @VCode, @VNameAr, @VNameEn, @Price, 0, @Barcode, 1, 1)";

            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ProductID", productID);
                cmd.Parameters.AddWithValue("@Price", decimal.TryParse(txtPrice.Text, out decimal p) ? p : 0m);
                cmd.Parameters.AddWithValue("@Barcode", txtBarcode.Text.Trim());
                cmd.Parameters.AddWithValue("@VCode", txtProductCode.Text.Trim());
                cmd.Parameters.AddWithValue("@VNameAr", txtNameAr.Text.Trim());
                cmd.Parameters.AddWithValue("@VNameEn", txtNameEn.Text.Trim());

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ════════════════════════════════════════════════════
        //  🗑️  حذف منتج (Soft Delete)
        // ════════════════════════════════════════════════════
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedProductID == 0)
            { ToastManager.ShowWarning("تنبيه", "من فضلك اختر منتجاً أولاً"); return; }

            var product = ProductsList.FirstOrDefault(p => p.ProductID == _selectedProductID);
            string productName = product?.ProductNameAr ?? "هذا المنتج";

            if (!frmConfirm.Show("تأكيد حذف المنتج", $"هل تريد بالتأكيد حذف المنتج \"{productName}\"؟\nلا يمكن التراجع عن هذه العملية.")) return;

            SetButtonsEnabled(false);

            try
            {
                const string query = "UPDATE Products SET IsDeleted=1, IsActive=0, LastModified=GETDATE() WHERE ProductID=@ID AND IsDeleted=0";
                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", _selectedProductID);
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                if (product != null)
                {
                    ProductsList.Remove(product);
                    allProducts = ProductsList.ToList();
                }

                ToastManager.ShowSuccess("نجاح", "✅ تم الحذف بنجاح");
                ClearProductForm();
            }
            catch (Exception ex)
            { ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message); }
            finally { SetButtonsEnabled(true); }
        }

        // ════════════════════════════════════════════════════
        //  🧹  تفريغ الحقول
        // ════════════════════════════════════════════════════
        private void btn_clear_Click(object sender, EventArgs e) => ClearProductForm();

        private void ClearProductForm()
        {
            txtProductCode.Clear();
            txtNameAr.Clear();
            txtNameEn.Clear();
            txtBarcode.Clear();
            txtPrice.Clear();
            txtDiscount.Clear();
            txtTax.Clear();
            txtNotes.Clear();
            txtDescription.Clear();
            txtPrepTime.Text = "15";
            tglIsActive.Checked = true;
            cmbCategory.SelectedIndex = -1;

            DisposeCurrentImage();
            picProductImage.Image = Properties.Resources.mode_landscape__1_;
            ProductImageBase64 = null;

            lblCreatedAt.Text = "";
            lblLastActivity.Text = "";
            lblAddedBy.Text = "";

            _selectedProductID = 0;
            dgvProducts.ClearSelection();
        }

        // ════════════════════════════════════════════════════
        //  🖼️  إدارة صورة المنتج
        // ════════════════════════════════════════════════════
        private void SelectProductImage()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "اختيار صورة المنتج";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(ofd.FileName);
                        ProductImageBase64 = Convert.ToBase64String(imageBytes);
                        SetProductImageFromBytes(imageBytes);
                    }
                    catch (Exception ex) { ToastManager.ShowError("خطأ", "خطأ في فتح الصورة: " + ex.Message); }
                }
            }
        }

        /// <summary>
        /// يعرض الصورة من byte[] مع إنشاء Bitmap مستقل عالي الجودة
        /// </summary>
        private void SetProductImageFromBytes(byte[] imageBytes)
        {
            DisposeCurrentImage();

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (Image tempImg = Image.FromStream(ms, true, true))
                {
                    _currentProductImage = new Bitmap(tempImg.Width, tempImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(_currentProductImage))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.DrawImage(tempImg, 0, 0, tempImg.Width, tempImg.Height);
                    }
                }
            }
            picProductImage.Image = _currentProductImage;
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(DataFormats.Bitmap))
                e.Effect = DragDropEffects.Copy;
        }

        private void Panel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0)
                    {
                        byte[] imageBytes = File.ReadAllBytes(files[0]);
                        ProductImageBase64 = Convert.ToBase64String(imageBytes);
                        SetProductImageFromBytes(imageBytes);
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    var bmp = (Bitmap)e.Data.GetData(DataFormats.Bitmap);
                    DisposeCurrentImage();
                    _currentProductImage = new Bitmap(bmp);
                    picProductImage.Image = _currentProductImage;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        _currentProductImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ProductImageBase64 = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", "حدث خطأ أثناء تحميل الصورة: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════
        //  ⏱️  أزرار وقت التحضير +/-
        // ════════════════════════════════════════════════════
        private void btnPrepPlus_Click(object sender, EventArgs e)
        {
            int val = int.TryParse(txtPrepTime.Text, out int v) ? v : 0;
            txtPrepTime.Text = (val + 5).ToString();
        }

        private void btnPrepMinus_Click(object sender, EventArgs e)
        {
            int val = int.TryParse(txtPrepTime.Text, out int v) ? v : 0;
            txtPrepTime.Text = Math.Max(0, val - 5).ToString();
        }

        // ════════════════════════════════════════════════════
        //  🔘  تعطيل/تفعيل الأزرار أثناء العمليات
        // ════════════════════════════════════════════════════
        private void SetButtonsEnabled(bool enabled)
        {
            btnSave.Enabled = enabled;
            btnUpdate.Enabled = enabled;
            btnDelete.Enabled = enabled;
        }

        // ════════════════════════════════════════════════════
        //  🔲  أزرار النافذة
        // ════════════════════════════════════════════════════
        private void btn_close_Click(object sender, EventArgs e)
        {
            searchTimer?.Stop();
            searchTimer?.Dispose();
            DisposeCurrentImage();
            this.Close();
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
