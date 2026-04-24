using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using Sestamk.UserControl;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Timers;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmProductCategories : BaseForm
    {
        private Guna.UI2.WinForms.Guna2CirclePictureBox _selectedCircle = null;
        private System.Windows.Forms.UserControl _selectedgrid = null;
        private int _selectedColorID = 0;
        private string _selectedHexCode = "";
        private string _selectedColorName = "";

        private int _selectedCardegoryID = 0;

        private const int CIRCLE_SIZE = 46;
        private const int RING_PADDING = 6;
        private const int CIRCLE_SPACING = 66;
        private const int START_X = 12;
        private const int START_Y = 12;
        private const int MAX_PER_ROW = 7;

        private bool isEditMode = false;
        private int currentCategoryID = 0;
        private string CategoryImageBase64 = null;

        // ══════════════════════════════════════════════
        //  🔍  Search
        // ══════════════════════════════════════════════
        private System.Timers.Timer searchTimer;
        private List<ProductCategories> allCategoriesList = new List<ProductCategories>();

        private BindingSource ProductCategoriesBinding = new BindingSource();
        private BindingList<ProductCategories> ProductCategoriesList = new BindingList<ProductCategories>();

        public frmProductCategories()
        {
            InitializeComponent();

            pnlColorPicker.Paint += PnlColorPicker_Paint;

            LoadColorsFromDatabase();
            LoadCategoryAsync();

            CategoryImagePanel.AllowDrop = true;
            CategoryImagePanel.DragEnter += Panel_DragEnter;
            CategoryImagePanel.DragDrop += Panel_DragDrop;

            // ── إعداد البحث ──
            searchTimer = new System.Timers.Timer(300);
            searchTimer.AutoReset = false;
            searchTimer.Elapsed += SearchTimer_Elapsed;

            FillSearchComboBox();

            // ✅ ربط الأحداث يدوياً
            txtCategorySearch.TextChanged += txtCategorySearch_TextChanged;
            cmbCategorySearchField.SelectedIndexChanged += (s, e) =>
            {
                searchTimer.Stop();
                searchTimer.Start();
            };

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(guna2Button4, guna2Button5);
        }

        // ════════════════════════════════════════════════════
        //  🔍  ملء الـ ComboBox بحقول البحث
        // ════════════════════════════════════════════════════
        private void FillSearchComboBox()
        {
            cmbCategorySearchField.Items.Clear();
            cmbCategorySearchField.Items.Add("اسم القسم عربي");
            cmbCategorySearchField.Items.Add("اسم القسم انجليزي");
            cmbCategorySearchField.Items.Add("كود القسم");
            cmbCategorySearchField.Items.Add("نوع القسم");
            cmbCategorySearchField.Items.Add("ملاحظات");
            cmbCategorySearchField.SelectedIndex = 0;
        }

        // ════════════════════════════════════════════════════
        //  🔍  TextChanged → يشغّل الـ Timer
        // ════════════════════════════════════════════════════
        private void txtCategorySearch_TextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        // ════════════════════════════════════════════════════
        //  🔍  Timer Elapsed → ينفذ الفلترة على الـ UI Thread
        // ════════════════════════════════════════════════════
        private void SearchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                string keyword = txtCategorySearch.Text;
                string field = cmbCategorySearchField.SelectedItem?.ToString() ?? "اسم القسم عربي";
                ApplySearchFilter(keyword, field);
            }));
        }

        // ════════════════════════════════════════════════════
        //  🔍  الفلترة المحلية
        // ════════════════════════════════════════════════════
        private void ApplySearchFilter(string keyword, string field)
        {
            keyword = keyword?.Trim() ?? "";

            IEnumerable<ProductCategories> result = allCategoriesList;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                switch (field)
                {
                    case "اسم القسم عربي":
                        result = result.Where(c => (c.CategoryNameAr ?? "").Contains(keyword));
                        break;
                    case "اسم القسم انجليزي":
                        result = result.Where(c => (c.CategoryNameEn ?? "").Contains(keyword));
                        break;
                    case "كود القسم":
                        result = result.Where(c => (c.CategoryCode ?? "").Contains(keyword));
                        break;
                    case "نوع القسم":
                        result = result.Where(c => (c.CategoryTypeName ?? "").Contains(keyword));
                        break;
                    case "ملاحظات":
                        result = result.Where(c => (c.Notes ?? "").Contains(keyword));
                        break;
                    default:
                        result = result.Where(c => (c.CategoryNameAr ?? "").Contains(keyword));
                        break;
                }
            }

            RenderCategories(result.ToList());
        }

        // ════════════════════════════════════════════════════
        //  🔄  تحميل الأقسام من قاعدة البيانات
        // ════════════════════════════════════════════════════
        private async Task LoadCategoryAsync()
        {
            try
            {
                string query = @"SELECT CategoryTypeID, TypeName 
                         FROM CategoryTypes
                         WHERE IsDeleted = 0 AND IsActive = 1
                         ORDER BY CategoryTypeID";

                using (SqlConnection con = DB_Server.GetConnection())
                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmbCategoryType.DataSource = dt;
                    cmbCategoryType.DisplayMember = "TypeName";
                    cmbCategoryType.ValueMember = "CategoryTypeID";
                    cmbCategoryType.SelectedIndex = -1;
                }
            }
            catch (Exception ex) { ToastManager.ShowError("خطأ", ex.Message); }

            try
            {
                string query = @"SELECT 
                                    PC.CategoryID, PC.CategoryCode,
                                    PC.CategoryNameAr, PC.CategoryNameEn,
                                    PC.BackgroundColor, PC.Image, PC.IsActive,
                                    PC.Notes, PC.CreatedDate, PC.CreatedBy,
                                    PC.LastModified, PC.IsDeleted,
                                    PC.ColorID, PC.CategoryTypeID,
                                    CategoryTypes.TypeName, Colors.HexCode
                                FROM ProductCategories AS PC
                                LEFT JOIN Colors        ON Colors.ColorID               = PC.ColorID
                                LEFT JOIN CategoryTypes ON CategoryTypes.CategoryTypeID = PC.CategoryTypeID
                                WHERE PC.IsDeleted = 0;";

                DataTable dt = new DataTable();
                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        dt.Load(reader);
                }

                foreach (DataRow row in dt.Rows)
                {
                    int id = Convert.ToInt32(row["CategoryID"]);
                    var existing = ProductCategoriesList.FirstOrDefault(x => x.CategoryID == id);

                    string GetString(string col) => row[col] == DBNull.Value ? "" : row[col].ToString();
                    int GetInt(string col) => row[col] == DBNull.Value ? 0 : Convert.ToInt32(row[col]);
                    bool GetBool(string col) => row[col] != DBNull.Value && Convert.ToBoolean(row[col]);

                    if (existing != null)
                    {
                        existing.CategoryID = id;
                        existing.CategoryCode = GetString("CategoryCode");
                        existing.CategoryNameAr = GetString("CategoryNameAr");
                        existing.CategoryNameEn = GetString("CategoryNameEn");
                        existing.Image = GetString("Image");
                        existing.IsActive = GetBool("IsActive");
                        existing.ColorID = GetInt("ColorID");
                        existing.Notes = GetString("Notes");
                        existing.CreatedDate = GetString("CreatedDate");
                        existing.CreatedBy = GetString("CreatedBy");
                        existing.LastModified = GetString("LastModified");
                        existing.CategoryTypeID = GetInt("CategoryTypeID");
                        existing.HexCode = GetString("HexCode");
                        existing.CategoryTypeName = GetString("TypeName");
                    }
                    else
                    {
                        ProductCategoriesList.Add(new ProductCategories
                        {
                            CategoryID = id,
                            CategoryCode = GetString("CategoryCode"),
                            CategoryNameAr = GetString("CategoryNameAr"),
                            CategoryNameEn = GetString("CategoryNameEn"),
                            Image = GetString("Image"),
                            IsActive = GetBool("IsActive"),
                            ColorID = GetInt("ColorID"),
                            Notes = GetString("Notes"),
                            CreatedDate = GetString("CreatedDate"),
                            CreatedBy = GetString("CreatedBy"),
                            LastModified = GetString("LastModified"),
                            CategoryTypeID = GetInt("CategoryTypeID"),
                            HexCode = GetString("HexCode"),
                            CategoryTypeName = GetString("TypeName"),
                        });
                    }
                }

                var idsFromDb = dt.AsEnumerable()
                                  .Select(r => Convert.ToInt32(r["CategoryID"]))
                                  .ToHashSet();

                for (int i = ProductCategoriesList.Count - 1; i >= 0; i--)
                    if (!idsFromDb.Contains(ProductCategoriesList[i].CategoryID))
                        ProductCategoriesList.RemoveAt(i);

                allCategoriesList = ProductCategoriesList.ToList();
                RenderCategories();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التحميل: " + ex.Message);
            }
        }

        // ════════════════════════════════════════════════════
        //  🖼️  رسم الكاردز
        //  ✅  العداد "X من أصل Y" — مضبوط RTL
        //  ✅  رسالة "لا توجد نتائج"
        // ════════════════════════════════════════════════════
        private void RenderCategories(List<ProductCategories> sourceList = null)
        {
            var listToRender = sourceList ?? ProductCategoriesList.ToList();
            int total = allCategoriesList.Count;
            int shown = listToRender.Count;

            // ✅ \u200f = Right-to-Left Mark يمنع تشابك الأرقام مع العربي
            lblProductCategoriesCount.Text = $"\u200f{shown} من أصل {total}";

            flowProductCategories.SuspendLayout();

            try
            {
                while (flowProductCategories.Controls.Count > 0)
                {
                    var ctrl = flowProductCategories.Controls[0];
                    flowProductCategories.Controls.Remove(ctrl);
                    ctrl.Dispose();
                }

                flowProductCategories.FlowDirection = FlowDirection.LeftToRight;
                flowProductCategories.WrapContents = true;
                flowProductCategories.AutoScroll = true;
                flowProductCategories.Visible = true;
                flowProductCategories.Enabled = true;

                // ✅ رسالة "لا توجد نتائج"
                if (listToRender.Count == 0)
                {
                    ShowNoResultsMessage();
                    return;
                }

                var cardsToDisplay = new List<UC_ProductCategories>();
                foreach (var cat in listToRender)
                {
                    UC_ProductCategories card = new UC_ProductCategories(cat);
                    card.Margin = new Padding(10);
                    card.Visible = true;
                    card.OnAddClicked += Card_OnAddClicked;
                    card.OnDeleteClicked += Card_OnDeleteClicked;
                    cardsToDisplay.Add(card);
                }

                flowProductCategories.Controls.AddRange(cardsToDisplay.ToArray());
                flowProductCategories.BringToFront();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ أثناء رسم المنتجات: " + ex.Message);
            }
            finally
            {
                flowProductCategories.ResumeLayout(true);
                Application.DoEvents();
                flowProductCategories.PerformLayout();
                flowProductCategories.Refresh();
            }
        }

        // ════════════════════════════════════════════════════
        //  ❌  رسالة "لا توجد نتائج"
        // ════════════════════════════════════════════════════
        private void ShowNoResultsMessage()
        {
            var pnl = new Panel
            {
                Width = flowProductCategories.Width - 40,
                Height = 160,
                BackColor = Color.Transparent,
                Margin = new Padding(20, 40, 20, 0),
            };

            var lblIcon = new Label
            {
                Text = "🔍",
                Font = new Font("Segoe UI Emoji", 32),
                ForeColor = Color.FromArgb(80, 100, 130),
                AutoSize = false,
                Width = pnl.Width,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 10),
            };

            var lblMain = new Label
            {
                Text = "لا توجد نتائج",
                Font = new Font("Tajawal", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 190, 210),
                AutoSize = false,
                Width = pnl.Width,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 74),
            };

            string subText = !string.IsNullOrWhiteSpace(txtCategorySearch.Text)
                ? $"لا يوجد قسم يطابق \"{txtCategorySearch.Text.Trim()}\""
                : "لا توجد أقسام متاحة";

            var lblSub = new Label
            {
                Text = subText,
                Font = new Font("Tajawal", 9),
                ForeColor = Color.FromArgb(100, 120, 150),
                AutoSize = false,
                Width = pnl.Width,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 112),
            };

            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(lblMain);
            pnl.Controls.Add(lblSub);
            flowProductCategories.Controls.Add(pnl);
        }

        // ════════════════════════════════════════════════════
        //  🔁  إعادة تطبيق الفلتر الحالي
        // ════════════════════════════════════════════════════
        private void RefreshCurrentFilter()
        {
            string keyword = txtCategorySearch.Text;
            string field = cmbCategorySearchField.SelectedItem?.ToString() ?? "اسم القسم عربي";
            ApplySearchFilter(keyword, field);
        }

        // ════════════════════════════════════════════════════
        //  🗑️  حذف من الكارد مباشرة
        // ════════════════════════════════════════════════════
        private async void Card_OnDeleteClicked(object sender, EventArgs e)
        {
            UC_ProductCategories clickedCard = sender as UC_ProductCategories;
            clickedCard.CategoryID = _selectedCardegoryID;

            if (clickedCard != null)
            {
                if (!frmConfirm.Show("تأكيد حذف القسم", $"هل تريد بالتأكيد حذف القسم \"{clickedCard.CategoryNameAr}\"؟\nلا يمكن التراجع عن هذه العملية.")) return;

                try
                {
                    await SoftDeleteCategorieAsync(_selectedCardegoryID);

                    var toRemove = ProductCategoriesList.FirstOrDefault(x => x.CategoryID == _selectedCardegoryID);
                    if (toRemove != null) ProductCategoriesList.Remove(toRemove);

                    allCategoriesList = ProductCategoriesList.ToList();
                    RefreshCurrentFilter();

                    ToastManager.ShowSuccess("نجاح", "✅ تم الحذف بنجاح");
                    clearFilds();
                }
                catch (Exception ex)
                {
                    ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message);
                }
            }
        }

        // ════════════════════════════════════════════════════
        //  ✏️  تحديد الكارد وملء الفورم
        // ════════════════════════════════════════════════════
        private void Card_OnAddClicked(object sender, EventArgs e)
        {
            UC_ProductCategories clickedCard = sender as UC_ProductCategories;
            if (clickedCard == null) return;

            _selectedCardegoryID = clickedCard.CategoryID;
            txtCategoryCode.Text = clickedCard.CategoryCode;
            txtCategoryNameAr.Text = clickedCard.CategoryNameAr;
            txtCategoryNameEn.Text = clickedCard.CategoryNameEn;
            toggleCategorystatus.Checked = clickedCard.IsActive;

            CategoryImage.Image = clickedCard.CategoryImage
                ?? Properties.Resources._1772674733217_019cbba5_6fa6_7c10_9fce_5e3dc87bf76b;
            CategoryImage.Visible = true;
            CategoryImage.BringToFront();

            btn_clearimg.BringToFront();
            btn_clearimg.Visible = true;
            lblCreatedBy.Text = clickedCard.CreatedBy;
            lblLastModified.Text = clickedCard.LastModified;
            CategoryImagePanel.Visible = false;
            txtCategoryNote.Text = clickedCard.CategoryNotes;

            cmbCategoryType.SelectedIndex = clickedCard.CategoryTypeID == 0
                ? -1 : clickedCard.CategoryTypeID - 1;

            lblCreatedDate.Text = clickedCard.CreatedDate;

            if (clickedCard.ColorID == 0) ClearColorSelection();
            else SelectColorByID(clickedCard.ColorID);

            Selectactivecard(clickedCard);
        }

        // ════════════════════════════════════════════════════
        //  🎯  تمييز الكارد المختار
        // ════════════════════════════════════════════════════
        private void Selectactivecard(System.Windows.Forms.UserControl targetcard)
        {
            if (_selectedgrid == targetcard) return;
            if (_selectedgrid != null) _selectedgrid.BackColor = Color.FromArgb(17, 25, 40);
            _selectedgrid = targetcard;
            targetcard.BackColor = Color.FromArgb(51, 164, 244);
            flowProductCategories.Invalidate();
        }

        private void clearselectedactivecard()
        {
            if (_selectedgrid != null)
            {
                _selectedgrid.BackColor = Color.FromArgb(17, 25, 40);
                _selectedgrid = null;
            }
        }

        // ════════════════════════════════════════════════════
        //  🎨  رسم الحلقة على الـ Panel
        // ════════════════════════════════════════════════════
        private void PnlColorPicker_Paint(object sender, PaintEventArgs e)
        {
            if (_selectedCircle == null) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = _selectedCircle.Location.X + _selectedCircle.Width / 2;
            int cy = _selectedCircle.Location.Y + _selectedCircle.Height / 2;

            int ringDiameter = CIRCLE_SIZE + RING_PADDING * 2;
            int x = cx - ringDiameter / 2;
            int y = cy - ringDiameter / 2;

            Color ringColor = _selectedCircle.FillColor;

            int glowSize = ringDiameter + 6;
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - glowSize / 2, cy - glowSize / 2, glowSize, glowSize);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(40, ringColor.R, ringColor.G, ringColor.B);
                    brush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }

            using (var pen = new Pen(ringColor, 3f))
                g.DrawEllipse(pen, x, y, ringDiameter, ringDiameter);
        }

        // ════════════════════════════════════════════════════
        //  📦  جلب الألوان من قاعدة البيانات
        // ════════════════════════════════════════════════════
        private void LoadColorsFromDatabase()
        {
            pnlColorPicker.Controls.Clear();
            _selectedCircle = null;
            _selectedColorID = 0;
            _selectedHexCode = "";
            _selectedColorName = "";
            pnlColorPicker.Invalidate();

            DataTable dt = GetColorsFromDB();
            if (dt == null || dt.Rows.Count == 0) { ShowNoColorsMessage(); return; }
            BuildColorPalette(dt);
        }

        private DataTable GetColorsFromDB()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var conn = DB_Server.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        SELECT ColorID, ColorCode, HexCode, RGB, SortOrder
                        FROM   dbo.Colors
                        WHERE  IsActive  = 1 AND IsDeleted = 0
                        ORDER  BY SortOrder ASC";

                    using (var da = new Microsoft.Data.SqlClient.SqlDataAdapter(sql, conn))
                        da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الألوان: " + ex.Message);
                return null;
            }
            return dt;
        }

        private void BuildColorPalette(DataTable dt)
        {
            int col = 0, row = 0;

            foreach (DataRow dr in dt.Rows)
            {
                Color fillColor = HexToColor(dr["HexCode"].ToString());
                if (fillColor == Color.Empty) continue;

                var item = new ColorItem
                {
                    ColorID = Convert.ToInt32(dr["ColorID"]),
                    ColorCode = dr["ColorCode"].ToString(),
                    HexCode = dr["HexCode"].ToString(),
                    RGB = dr["RGB"].ToString(),
                    FillColor = fillColor,
                    BackColor = Color.FromArgb(30, fillColor.R, fillColor.G, fillColor.B)
                };

                AddColorCircle(item, col, row);
                col++;
                if (col >= MAX_PER_ROW) { col = 0; row++; }
            }

            AddPlusButton(col, row);
            int totalRows = (int)Math.Ceiling((double)(dt.Rows.Count + 1) / MAX_PER_ROW);
            pnlColorPicker.Height = START_Y + totalRows * CIRCLE_SPACING + 20;
        }

        private void AddColorCircle(ColorItem item, int col, int row)
        {
            int cx = START_X + col * CIRCLE_SPACING + CIRCLE_SIZE / 2;
            int cy = START_Y + row * CIRCLE_SPACING + CIRCLE_SIZE / 2;

            var circle = new Guna.UI2.WinForms.Guna2CirclePictureBox
            {
                Size = new Size(CIRCLE_SIZE, CIRCLE_SIZE),
                Location = new Point(cx - CIRCLE_SIZE / 2, cy - CIRCLE_SIZE / 2),
                FillColor = item.FillColor,
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.Transparent,
                Tag = item,
                ShadowDecoration =
                {
                    Enabled = true,
                    Color   = Color.FromArgb(60, item.FillColor.R, item.FillColor.G, item.FillColor.B),
                    Depth   = 5,
                }
            };

            new ToolTip { InitialDelay = 300 }.SetToolTip(circle, item.ColorCode);
            circle.Click += (s, e) => SelectColor(circle);
            circle.MouseEnter += (s, e) => { if (circle != _selectedCircle) circle.ShadowDecoration.Depth = 9; };
            circle.MouseLeave += (s, e) => { if (circle != _selectedCircle) circle.ShadowDecoration.Depth = 5; };

            pnlColorPicker.Controls.Add(circle);
        }

        private void SelectColor(Guna.UI2.WinForms.Guna2CirclePictureBox target)
        {
            if (_selectedCircle == target) return;

            if (_selectedCircle != null)
            {
                _selectedCircle.ShadowDecoration.Depth = 5;
                _selectedCircle.ShadowDecoration.Color =
                    Color.FromArgb(60, _selectedCircle.FillColor.R,
                                       _selectedCircle.FillColor.G,
                                       _selectedCircle.FillColor.B);
            }

            _selectedCircle = target;
            target.ShadowDecoration.Enabled = true;
            target.ShadowDecoration.Color = target.FillColor;
            target.ShadowDecoration.Depth = 10;
            pnlColorPicker.Invalidate();

            if (target.Tag is ColorItem ci)
            {
                _selectedColorID = ci.ColorID;
                _selectedHexCode = ci.HexCode;
                _selectedColorName = ci.ColorCode;
                OnColorSelected(ci);
            }
        }

        private void AddPlusButton(int col, int row)
        {
            int cx = START_X + col * CIRCLE_SPACING + CIRCLE_SIZE / 2;
            int cy = START_Y + row * CIRCLE_SPACING + CIRCLE_SIZE / 2;

            var btn = new Guna.UI2.WinForms.Guna2CirclePictureBox
            {
                Size = new Size(CIRCLE_SIZE, CIRCLE_SIZE),
                Location = new Point(cx - CIRCLE_SIZE / 2, cy - CIRCLE_SIZE / 2),
                FillColor = Color.FromArgb(45, 47, 65),
                Cursor = Cursors.Hand,
                SizeMode = PictureBoxSizeMode.CenterImage,
                ShadowDecoration = { Enabled = false }
            };

            var bmp = new Bitmap(CIRCLE_SIZE, CIRCLE_SIZE);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (var pen = new Pen(Color.FromArgb(130, 255, 255, 255), 1.8f))
                {
                    pen.DashStyle = DashStyle.Dash;
                    g.DrawEllipse(pen, 3, 3, CIRCLE_SIZE - 7, CIRCLE_SIZE - 7);
                }
                using (var pen = new Pen(Color.FromArgb(200, 255, 255, 255), 2.5f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    int mid = CIRCLE_SIZE / 2, arm = 8;
                    g.DrawLine(pen, mid - arm, mid, mid + arm, mid);
                    g.DrawLine(pen, mid, mid - arm, mid, mid + arm);
                }
            }
            btn.Image = bmp;

            btn.MouseEnter += (s, e) =>
            {
                btn.FillColor = Color.FromArgb(60, 62, 85);
                btn.ShadowDecoration.Enabled = true;
                btn.ShadowDecoration.Color = Color.FromArgb(80, 150, 150, 255);
                btn.ShadowDecoration.Depth = 6;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.FillColor = Color.FromArgb(45, 47, 65);
                btn.ShadowDecoration.Enabled = false;
            };

            btn.Click += (s, e) => OpenAddColorDialog();
            new ToolTip().SetToolTip(btn, "اضافة لون جديد");
            pnlColorPicker.Controls.Add(btn);
        }

        private void OpenAddColorDialog()
        {
            using (var dlg = new frmCustomers())
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadColorsFromDatabase();
        }

        // ════════════════════════════════════════════════════
        //  🛠️  Helpers
        // ════════════════════════════════════════════════════
        private void SelectColorByID(int colorID)
        {
            if (colorID <= 0)
            {
                if (_selectedCircle != null)
                {
                    _selectedCircle.ShadowDecoration.Depth = 5;
                    _selectedCircle = null;
                    pnlColorPicker.Invalidate();
                }
                return;
            }

            foreach (Control ctrl in pnlColorPicker.Controls)
                if (ctrl is Guna.UI2.WinForms.Guna2CirclePictureBox circle
                    && circle.Tag is ColorItem ci && ci.ColorID == colorID)
                { SelectColor(circle); return; }
        }

        private void ClearColorSelection()
        {
            if (_selectedCircle != null)
            {
                _selectedCircle.ShadowDecoration.Depth = 5;
                _selectedCircle = null;
                _selectedColorID = 0;
                _selectedHexCode = "";
                _selectedColorName = "";
                pnlColorPicker.Invalidate();
            }
        }

        private bool ValidateCategoriesInputs(out string errorMessage)
        {
            errorMessage = "";
            if (string.IsNullOrWhiteSpace(txtCategoryCode.Text)) { errorMessage = "من فضلك أدخل كود القسم"; txtCategoryCode.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtCategoryNameAr.Text)) { errorMessage = "من فضلك أدخل اسم القسم عربي"; txtCategoryNameAr.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtCategoryNameEn.Text)) { errorMessage = "من فضلك أدخل اسم القسم انجليزي"; txtCategoryNameEn.Focus(); return false; }
            if (_selectedCircle == null) { errorMessage = "من فضلك اختر لون للقسم"; pnlColorPicker.Focus(); return false; }
            if (cmbCategoryType.SelectedIndex == -1) { errorMessage = "من فضلك اختر نوع القسم"; cmbCategoryType.Focus(); return false; }
            return true;
        }

        private void clearFilds()
        {
            txtCategoryCode.Clear();
            txtCategoryNameAr.Clear();
            txtCategoryNameEn.Clear();
            toggleCategorystatus.Checked = false;
            CategoryImage.Visible = false;
            lblCreatedBy.Text = "";
            lblLastModified.Text = "";
            _selectedCardegoryID = 0;
            CategoryImagePanel.Visible = true;
            btn_clearimg.Visible = false;
            txtCategoryNote.Clear();
            lblCreatedDate.Text = "";
            cmbCategoryType.SelectedIndex = -1;
            ClearColorSelection();
            clearselectedactivecard();
        }

        private void OnColorSelected(ColorItem color) { }

        private Color HexToColor(string hex)
        {
            try
            {
                hex = hex?.Trim().TrimStart('#') ?? "";
                if (hex.Length == 6)
                    return Color.FromArgb(
                        Convert.ToInt32(hex.Substring(0, 2), 16),
                        Convert.ToInt32(hex.Substring(2, 2), 16),
                        Convert.ToInt32(hex.Substring(4, 2), 16));
            }
            catch { }
            return Color.Empty;
        }

        private void ShowNoColorsMessage()
        {
            pnlColorPicker.Controls.Add(new Label
            {
                Text = "لا توجد الوان في قاعدة البيانات",
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(10, 15),
                Font = new Font("Tajawal", 10)
            });
        }

        public int SelectedColorID => _selectedColorID;
        public string SelectedHexCode => _selectedHexCode;
        public string SelectedColorName => _selectedColorName;
        public void RefreshColors() => LoadColorsFromDatabase();

        // ════════════════════════════════════════════════════
        //  🔲  Form Buttons
        // ════════════════════════════════════════════════════
        private void btn_close_Click(object sender, EventArgs e) => this.Close();

        private void btnMax_Click(object sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        private void btnMin_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;
        private void btn_clear_Click(object sender, EventArgs e) => clearFilds();

        // ════════════════════════════════════════════════════
        //  ➕  إضافة قسم جديد
        // ════════════════════════════════════════════════════
        private async void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateCategoriesInputs(out string msg))
            { ToastManager.ShowWarning("تنبيه", msg); return; }

            if (IsCategoryExists(txtCategoryCode.Text.Trim()))
            { ToastManager.ShowWarning("تنبيه", "كود القسم الجديد مكرر!"); return; }

            try
            {
                await InsertCategorieAsync();
                await LoadCategoryAsync();
                clearFilds();
            }
            catch (Exception) { }
        }

        private bool IsCategoryExists(string categoryCode)
            => ProductCategoriesList.Any(x => x.CategoryCode == categoryCode);

        private async Task<int> InsertCategorieAsync()
        {
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO ProductCategories
                (CategoryCode,CategoryNameAr,CategoryNameEn,Image,IsActive,Notes,
                 CreatedBy,ColorID,CategoryTypeID,LastModified)
                OUTPUT INSERTED.CategoryID
                VALUES
                (@Code,@NameAr,@NameEn,@Image,@Active,@Notes,
                 @User,@ColorID,@CategoryTypeID,GETDATE())", conn))
            {
                cmd.Parameters.AddWithValue("@Code", txtCategoryCode.Text.Trim());
                cmd.Parameters.AddWithValue("@NameAr", txtCategoryNameAr.Text.Trim());
                cmd.Parameters.AddWithValue("@NameEn", txtCategoryNameEn.Text.Trim());
                cmd.Parameters.AddWithValue("@Active", toggleCategorystatus.Checked);
                cmd.Parameters.AddWithValue("@Notes", txtCategoryNote.Text.Trim());
                cmd.Parameters.AddWithValue("@User", UserSession.UserId);
                cmd.Parameters.AddWithValue("@ColorID", _selectedColorID);
                cmd.Parameters.AddWithValue("@CategoryTypeID", cmbCategoryType.SelectedIndex - 1);
                cmd.Parameters.AddWithValue("@Image", CategoryImageBase64 ?? (object)DBNull.Value);
                await conn.OpenAsync();
                return (int)await cmd.ExecuteScalarAsync();
            }
        }

        // ════════════════════════════════════════════════════
        //  🖼️  إدارة صورة القسم
        // ════════════════════════════════════════════════════
        private void SelectCategoryImage()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "اختيار صورة القسم";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    byte[] imageBytes = File.ReadAllBytes(ofd.FileName);
                    CategoryImageBase64 = Convert.ToBase64String(imageBytes);

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        CategoryImage.Image = Image.FromStream(ms);
                        CategoryImage.Visible = true;
                        btn_clearimg.BringToFront();
                        btn_clearimg.Visible = true;
                    }
                }
            }
        }

        private void Panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(DataFormats.Bitmap) ||
                e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
        }

        private async void Panel_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Image img = null;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0) img = Image.FromFile(files[0]);
                }
                else if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    img = (Bitmap)e.Data.GetData(DataFormats.Bitmap);
                }
                else if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    string url = e.Data.GetData(DataFormats.Text).ToString();
                    if (url.StartsWith("http"))
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var bytes = await client.GetByteArrayAsync(url);
                            using (MemoryStream ms = new MemoryStream(bytes))
                                img = Image.FromStream(ms, true, true);
                        }
                    }
                }

                if (img != null)
                {
                    CategoryImage.Image = new Bitmap(img);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        CategoryImage.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        CategoryImageBase64 = Convert.ToBase64String(ms.ToArray());
                    }
                    CategoryImage.Visible = true;
                    btn_clearimg.Visible = true;
                    btn_clearimg.BringToFront();
                }
                else ToastManager.ShowWarning("تنبيه", "لم يتم التعرف على الصورة.");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "حدث خطأ أثناء تحميل الصورة: " + ex.Message);
            }
        }

        private void CategoryImagePanel_Click(object sender, EventArgs e) => SelectCategoryImage();
        private void label5_Click(object sender, EventArgs e) => SelectCategoryImage();
        private void guna2PictureBox2_Click(object sender, EventArgs e) => SelectCategoryImage();

        private void btn_clearimg_Click(object sender, EventArgs e)
        {
            CategoryImagePanel.Visible = true;
            CategoryImage.Image = null;
            CategoryImage.Visible = false;
            CategoryImageBase64 = "";
        }

        // ════════════════════════════════════════════════════
        //  ✏️  تعديل قسم
        // ════════════════════════════════════════════════════
        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (!ValidateCategoriesInputs(out string msg))
            { ToastManager.ShowWarning("تنبيه", msg); return; }

            bool isDuplicate = ProductCategoriesList
                .Any(x => x.CategoryCode == txtCategoryCode.Text.Trim() && x.CategoryID != _selectedCardegoryID);

            if (isDuplicate)
            { ToastManager.ShowWarning("تنبيه", "كود القسم مكرر مع قسم آخر!"); return; }

            if (_selectedCardegoryID == 0)
            { ToastManager.ShowWarning("تنبيه", "من فضلك اختر قسماً لتعديله"); return; }

            int categoryTypeID = cmbCategoryType.SelectedValue != null
                ? Convert.ToInt32(cmbCategoryType.SelectedValue) : 0;

            string imageToSave = string.IsNullOrEmpty(CategoryImageBase64) ? null : CategoryImageBase64;

            try
            {
                await UpdateCategorieAsync(_selectedCardegoryID,
                    txtCategoryCode.Text.Trim(), txtCategoryNameAr.Text.Trim(), txtCategoryNameEn.Text.Trim(),
                    toggleCategorystatus.Checked, txtCategoryNote.Text.Trim(),
                    _selectedColorID, categoryTypeID, imageToSave);

                var existing = ProductCategoriesList.FirstOrDefault(x => x.CategoryID == _selectedCardegoryID);
                if (existing != null)
                {
                    existing.CategoryCode = txtCategoryCode.Text.Trim();
                    existing.CategoryNameAr = txtCategoryNameAr.Text.Trim();
                    existing.CategoryNameEn = txtCategoryNameEn.Text.Trim();
                    existing.IsActive = toggleCategorystatus.Checked;
                    existing.Notes = txtCategoryNote.Text.Trim();
                    existing.ColorID = _selectedColorID;
                    existing.HexCode = _selectedHexCode;
                    existing.CategoryTypeID = categoryTypeID;
                    existing.LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (!string.IsNullOrEmpty(CategoryImageBase64)) existing.Image = CategoryImageBase64;
                }

                allCategoriesList = ProductCategoriesList.ToList();
                RefreshCurrentFilter();
                clearFilds();

                ToastManager.ShowSuccess("نجاح", "✅ تم التعديل بنجاح");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في التعديل: " + ex.Message);
            }
        }

        private async Task UpdateCategorieAsync(
            int categoryID, string code, string nameAr, string nameEn,
            bool isActive, string notes, int colorID, int categoryTypeID, string imageBase64)
        {
            string query = imageBase64 != null
                ? @"UPDATE ProductCategories SET
                    CategoryCode=@Code,CategoryNameAr=@NameAr,CategoryNameEn=@NameEn,
                    IsActive=@Active,Notes=@Notes,ColorID=@ColorID,
                    CategoryTypeID=@CategoryTypeID,Image=@Image,LastModified=GETDATE()
                    WHERE CategoryID=@ID AND IsDeleted=0"
                : @"UPDATE ProductCategories SET
                    CategoryCode=@Code,CategoryNameAr=@NameAr,CategoryNameEn=@NameEn,
                    IsActive=@Active,Notes=@Notes,ColorID=@ColorID,
                    CategoryTypeID=@CategoryTypeID,LastModified=GETDATE()
                    WHERE CategoryID=@ID AND IsDeleted=0";

            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", categoryID);
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@NameAr", nameAr);
                cmd.Parameters.AddWithValue("@NameEn", nameEn);
                cmd.Parameters.AddWithValue("@Active", isActive);
                cmd.Parameters.AddWithValue("@Notes", notes);
                cmd.Parameters.AddWithValue("@ColorID", colorID);
                cmd.Parameters.AddWithValue("@CategoryTypeID", categoryTypeID);
                if (imageBase64 != null)
                    cmd.Parameters.AddWithValue("@Image", imageBase64);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ════════════════════════════════════════════════════
        //  🗑️  حذف قسم من الـ Toolbar
        // ════════════════════════════════════════════════════
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedCardegoryID == 0)
            { ToastManager.ShowWarning("تنبيه", "من فضلك اختر قسماً أولاً"); return; }

            if (!frmConfirm.Show("تأكيد الحذف", "هل أنت متأكد من حذف هذا القسم؟")) return;

            try
            {
                await SoftDeleteCategorieAsync(_selectedCardegoryID);

                var toRemove = ProductCategoriesList.FirstOrDefault(x => x.CategoryID == _selectedCardegoryID);
                if (toRemove != null) ProductCategoriesList.Remove(toRemove);

                allCategoriesList = ProductCategoriesList.ToList();
                RefreshCurrentFilter();

                ToastManager.ShowSuccess("نجاح", "✅ تم الحذف بنجاح");
                clearFilds();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في الحذف: " + ex.Message);
            }
        }

        private async Task SoftDeleteCategorieAsync(int categoryID)
        {
            const string query = @"
                UPDATE ProductCategories SET
                    IsDeleted=1, IsActive=0, LastModified=GETDATE()
                WHERE CategoryID=@ID AND IsDeleted=0";

            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", categoryID);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    // ════════════════════════════════════════════════════════
    //  📦  Model
    // ════════════════════════════════════════════════════════
    public class ColorItem
    {
        public int ColorID { get; set; }
        public string ColorCode { get; set; }
        public string HexCode { get; set; }
        public string RGB { get; set; }
        public Color FillColor { get; set; }
        public Color BackColor { get; set; }
    }
}