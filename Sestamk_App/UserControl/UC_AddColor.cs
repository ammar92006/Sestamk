using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    // ════════════════════════════════════════════════════════
    //  🎨  UC_AddColor — Premium Add-Color Dialog
    //  نفس نمط frmConfirm: Full-screen overlay + centered card
    // ════════════════════════════════════════════════════════
    public class UC_AddColor : Form
    {
        // ═══════════════════════════════════════
        //  📦  Controls
        // ═══════════════════════════════════════
        private Panel pnlOverlay;
        private Guna2Panel pnlCard;
        private Panel pnlAccent;

        // ── Header ──
        private Label lblIcon;
        private Label lblTitle;
        private Label lblSubtitle;

        // ── Color Name ──
        private Label lblColorName;
        private Guna2TextBox txtColorName;

        // ── Hex Code + Pick ──
        private Label lblHexCode;
        private Guna2TextBox txtHexCode;
        private Guna2Button btnPickColor;

        // ── Preview Area ──
        private Guna2Panel pnlPreviewContainer;
        private Guna2CirclePictureBox picPreview;
        private Label lblPreviewName;
        private Label lblPreviewHex;
        private Label lblPreviewRGB;

        // ── Buttons ──
        private Panel pnlButtons;
        private Guna2Button btnSave;
        private Guna2Button btnCancel;

        // ── Separator ──
        private Panel pnlSeparator;

        // ═══════════════════════════════════════
        //  📡  Events
        // ═══════════════════════════════════════
        public event EventHandler ColorAdded;

        // ═══════════════════════════════════════
        //  🎨  State
        // ═══════════════════════════════════════
        private Color _selectedColor = Color.Empty;
        private System.Windows.Forms.Timer _fadeTimer;
        private bool _isClosing = false;
        private double _targetOpacity = 0.95;

        // ═══════════════════════════════════════
        //  🏗️  Constructor
        // ═══════════════════════════════════════
        public UC_AddColor()
        {
            InitializeDialog();
            BuildUI();
            WireEvents();
        }

        // ═══════════════════════════════════════
        //  ⚙️  Form Setup (same as frmConfirm)
        // ═══════════════════════════════════════
        private void InitializeDialog()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.KeyPreview = true;
            this.Opacity = 0;
            this.DoubleBuffered = true;

            _fadeTimer = new System.Windows.Forms.Timer { Interval = 12 };
            _fadeTimer.Tick += FadeTimer_Tick;

            this.Load += (s, e) =>
            {
                CenterCard();
                _fadeTimer.Start();
            };
            this.Resize += (s, e) => CenterCard();
        }

        // ═══════════════════════════════════════
        //  🏗️  Build UI
        // ═══════════════════════════════════════
        private void BuildUI()
        {
            // ══════════════════════════════════
            //  📐  Overlay
            // ══════════════════════════════════
            pnlOverlay = new Panel
            {
                BackColor = Color.FromArgb(0, 0, 0),
                Dock = DockStyle.Fill,
            };
            this.Controls.Add(pnlOverlay);

            // ══════════════════════════════════
            //  🃏  Card
            // ══════════════════════════════════
            pnlCard = new Guna2Panel
            {
                Size = new Size(480, 560),
                FillColor = Color.FromArgb(15, 23, 42),
                BorderColor = Color.FromArgb(45, 55, 72),
                BorderRadius = 24,
                BorderThickness = 1,
                ShadowDecoration =
                {
                    Enabled = true,
                    BorderRadius = 24,
                    Color = Color.FromArgb(40, 0, 0, 0),
                    Depth = 20,
                },
            };
            pnlOverlay.Controls.Add(pnlCard);

            // ── Accent bar ──
            pnlAccent = new Panel
            {
                BackColor = Color.FromArgb(51, 164, 244),
                Location = new Point(0, 0),
                Size = new Size(480, 4),
            };
            pnlCard.Controls.Add(pnlAccent);

            // ══════════════════════════════════
            //  🎨  Header Section
            // ══════════════════════════════════
            lblIcon = new Label
            {
                Text = "🎨",
                Font = new Font("Segoe UI Emoji", 34),
                ForeColor = Color.FromArgb(51, 164, 244),
                Location = new Point(0, 14),
                Size = new Size(480, 58),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            pnlCard.Controls.Add(lblIcon);

            lblTitle = new Label
            {
                Text = "إضافة لون جديد",
                Font = new Font("Alexandria", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 245, 249),
                Location = new Point(40, 72),
                Size = new Size(400, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                RightToLeft = RightToLeft.Yes,
            };
            pnlCard.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "أدخل اسم اللون واختر اللون المطلوب لإضافته",
                Font = new Font("Alexandria", 9.5f),
                ForeColor = Color.FromArgb(120, 140, 165),
                Location = new Point(40, 106),
                Size = new Size(400, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                RightToLeft = RightToLeft.Yes,
            };
            pnlCard.Controls.Add(lblSubtitle);

            // ══════════════════════════════════
            //  📝  Color Name Field
            // ══════════════════════════════════
            lblColorName = new Label
            {
                Text = "اسم اللون",
                Font = new Font("Alexandria", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(157, 176, 185),
                Location = new Point(30, 145),
                Size = new Size(420, 28),
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes,
            };
            pnlCard.Controls.Add(lblColorName);

            txtColorName = new Guna2TextBox
            {
                Location = new Point(30, 175),
                Size = new Size(420, 48),
                BorderRadius = 12,
                FillColor = Color.FromArgb(2, 6, 23),
                BorderColor = Color.FromArgb(51, 65, 85),
                BorderThickness = 2,
                ForeColor = Color.White,
                Font = new Font("Alexandria", 12, FontStyle.Bold),
                PlaceholderText = "مثال: أزرق سماوي ، أحمر غامق ...",
                RightToLeft = RightToLeft.Yes,
                TextOffset = new Point(5, 0),
            };
            txtColorName.FocusedState.BorderColor = Color.FromArgb(51, 164, 244);
            txtColorName.HoverState.BorderColor = Color.FromArgb(80, 100, 140);
            pnlCard.Controls.Add(txtColorName);

            // ══════════════════════════════════
            //  🔢  Hex Code + Pick Button
            // ══════════════════════════════════
            lblHexCode = new Label
            {
                Text = "كود اللون (HEX)",
                Font = new Font("Alexandria", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(157, 176, 185),
                Location = new Point(30, 235),
                Size = new Size(420, 28),
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes,
            };
            pnlCard.Controls.Add(lblHexCode);

            txtHexCode = new Guna2TextBox
            {
                Location = new Point(30, 265),
                Size = new Size(275, 48),
                BorderRadius = 12,
                FillColor = Color.FromArgb(2, 6, 23),
                BorderColor = Color.FromArgb(51, 65, 85),
                BorderThickness = 2,
                ForeColor = Color.FromArgb(51, 164, 244),
                Font = new Font("Consolas", 15, FontStyle.Bold),
                PlaceholderText = "#3B82F6",
                MaxLength = 7,
                TextOffset = new Point(5, 0),
            };
            txtHexCode.FocusedState.BorderColor = Color.FromArgb(51, 164, 244);
            txtHexCode.HoverState.BorderColor = Color.FromArgb(80, 100, 140);
            pnlCard.Controls.Add(txtHexCode);

            btnPickColor = new Guna2Button
            {
                Text = "🎯  اختيار لون",
                Size = new Size(155, 48),
                Location = new Point(315, 265),
                BorderRadius = 12,
                FillColor = Color.FromArgb(30, 64, 115),
                BorderColor = Color.FromArgb(51, 164, 244),
                BorderThickness = 2,
                ForeColor = Color.FromArgb(51, 164, 244),
                Font = new Font("Alexandria", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnPickColor.HoverState.FillColor = Color.FromArgb(40, 80, 140);
            btnPickColor.HoverState.BorderColor = Color.FromArgb(80, 180, 255);
            pnlCard.Controls.Add(btnPickColor);

            // ══════════════════════════════════
            //  👁️  Preview Area
            // ══════════════════════════════════
            pnlPreviewContainer = new Guna2Panel
            {
                Location = new Point(30, 330),
                Size = new Size(420, 120),
                BorderRadius = 16,
                FillColor = Color.FromArgb(7, 12, 28),
                BorderColor = Color.FromArgb(35, 45, 65),
                BorderThickness = 1,
            };
            pnlCard.Controls.Add(pnlPreviewContainer);

            // ── Color circle preview ──
            picPreview = new Guna2CirclePictureBox
            {
                Size = new Size(72, 72),
                Location = new Point(24, 24),
                FillColor = Color.FromArgb(40, 45, 65),
                ShadowDecoration =
                {
                    Enabled = true,
                    Depth = 8,
                    Color = Color.FromArgb(50, 0, 0, 0),
                },
            };
            pnlPreviewContainer.Controls.Add(picPreview);

            // ── Preview labels ──
            lblPreviewName = new Label
            {
                Text = "لم يتم اختيار لون",
                Font = new Font("Alexandria", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 115, 140),
                Location = new Point(110, 22),
                Size = new Size(290, 28),
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes,
            };
            pnlPreviewContainer.Controls.Add(lblPreviewName);

            lblPreviewHex = new Label
            {
                Text = "HEX: ---",
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 90, 115),
                Location = new Point(250, 54),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleRight,
            };
            pnlPreviewContainer.Controls.Add(lblPreviewHex);

            lblPreviewRGB = new Label
            {
                Text = "RGB: ---",
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(75, 90, 115),
                Location = new Point(250, 78),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleRight,
            };
            pnlPreviewContainer.Controls.Add(lblPreviewRGB);

            // ── Separator ──
            pnlSeparator = new Panel
            {
                BackColor = Color.FromArgb(35, 45, 65),
                Location = new Point(40, 468),
                Size = new Size(400, 1),
            };
            pnlCard.Controls.Add(pnlSeparator);

            // ══════════════════════════════════
            //  🔘  Buttons
            // ══════════════════════════════════
            pnlButtons = new Panel
            {
                Location = new Point(30, 486),
                Size = new Size(420, 54),
                BackColor = Color.Transparent,
            };
            pnlCard.Controls.Add(pnlButtons);

            btnSave = new Guna2Button
            {
                Text = "💾  حفظ اللون",
                Size = new Size(200, 50),
                Location = new Point(0, 0),
                BorderRadius = 14,
                FillColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Alexandria", 13, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnSave.HoverState.FillColor = Color.FromArgb(5, 150, 105);
            pnlButtons.Controls.Add(btnSave);

            btnCancel = new Guna2Button
            {
                Text = "إلغاء",
                Size = new Size(200, 50),
                Location = new Point(220, 0),
                BorderRadius = 14,
                FillColor = Color.FromArgb(30, 41, 59),
                BorderColor = Color.FromArgb(51, 65, 85),
                BorderThickness = 2,
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Alexandria", 13, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnCancel.HoverState.FillColor = Color.FromArgb(51, 65, 85);
            pnlButtons.Controls.Add(btnCancel);
        }

        // ═══════════════════════════════════════
        //  🔌  Wire Events
        // ═══════════════════════════════════════
        private void WireEvents()
        {
            btnCancel.Click += (s, e) => { _isClosing = true; _fadeTimer.Start(); };
            pnlOverlay.Click += (s, e) => { _isClosing = true; _fadeTimer.Start(); };
            btnPickColor.Click += BtnPickColor_Click;
            btnSave.Click += BtnSave_Click;
            txtHexCode.TextChanged += TxtHexCode_TextChanged;
            txtColorName.TextChanged += TxtColorName_TextChanged;

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    _isClosing = true;
                    _fadeTimer.Start();
                    e.Handled = true;
                }
            };

            // ── Custom paint for preview ring ──
            pnlPreviewContainer.Paint += PnlPreview_Paint;
        }

        // ═══════════════════════════════════════
        //  🎬  Fade Animation (same as frmConfirm)
        // ═══════════════════════════════════════
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (!_isClosing)
            {
                if (this.Opacity < _targetOpacity)
                {
                    this.Opacity += 0.08;
                    if (this.Opacity >= _targetOpacity)
                    {
                        this.Opacity = _targetOpacity;
                        _fadeTimer.Stop();
                    }
                }
            }
            else
            {
                this.Opacity -= 0.1;
                if (this.Opacity <= 0)
                {
                    _fadeTimer.Stop();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        // ═══════════════════════════════════════
        //  📐  Center Card
        // ═══════════════════════════════════════
        private void CenterCard()
        {
            if (pnlCard == null) return;
            pnlCard.Left = (this.ClientSize.Width - pnlCard.Width) / 2;
            pnlCard.Top = (this.ClientSize.Height - pnlCard.Height) / 2;
        }

        // ═══════════════════════════════════════
        //  🎨  Paint preview ring glow
        // ═══════════════════════════════════════
        private void PnlPreview_Paint(object sender, PaintEventArgs e)
        {
            if (_selectedColor == Color.Empty) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cx = picPreview.Left + picPreview.Width / 2;
            int cy = picPreview.Top + picPreview.Height / 2;
            int glowSize = picPreview.Width + 20;

            using (var path = new GraphicsPath())
            {
                path.AddEllipse(cx - glowSize / 2, cy - glowSize / 2, glowSize, glowSize);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.FromArgb(35, _selectedColor.R, _selectedColor.G, _selectedColor.B);
                    brush.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(brush, path);
                }
            }
        }

        // ═══════════════════════════════════════
        //  🎯  Open Color Dialog
        // ═══════════════════════════════════════
        private void BtnPickColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.FullOpen = true;
                dlg.AnyColor = true;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedColor = dlg.Color;
                    string hex = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
                    txtHexCode.Text = hex;
                    UpdatePreview();
                }
            }
        }

        // ═══════════════════════════════════════
        //  ✏️  Live preview from hex input
        // ═══════════════════════════════════════
        private void TxtHexCode_TextChanged(object sender, EventArgs e)
        {
            string hex = txtHexCode.Text?.Trim().TrimStart('#') ?? "";
            if (hex.Length == 6)
            {
                try
                {
                    _selectedColor = Color.FromArgb(
                        Convert.ToInt32(hex.Substring(0, 2), 16),
                        Convert.ToInt32(hex.Substring(2, 2), 16),
                        Convert.ToInt32(hex.Substring(4, 2), 16));

                    txtHexCode.BorderColor = Color.FromArgb(51, 65, 85);
                    UpdatePreview();
                }
                catch
                {
                    txtHexCode.BorderColor = Color.FromArgb(220, 50, 50);
                }
            }
            else
            {
                ResetPreview();
            }
        }

        // ═══════════════════════════════════════
        //  ✏️  Update preview name live
        // ═══════════════════════════════════════
        private void TxtColorName_TextChanged(object sender, EventArgs e)
        {
            string name = txtColorName.Text?.Trim() ?? "";
            if (_selectedColor != Color.Empty)
            {
                lblPreviewName.Text = string.IsNullOrEmpty(name) ? "لون مخصص" : name;
                lblPreviewName.ForeColor = Color.FromArgb(220, 230, 240);
            }
        }

        // ═══════════════════════════════════════
        //  🔄  Update Preview Area
        // ═══════════════════════════════════════
        private void UpdatePreview()
        {
            picPreview.FillColor = _selectedColor;
            picPreview.ShadowDecoration.Color = Color.FromArgb(80, _selectedColor.R, _selectedColor.G, _selectedColor.B);
            picPreview.ShadowDecoration.Depth = 12;

            string name = txtColorName.Text?.Trim() ?? "";
            lblPreviewName.Text = string.IsNullOrEmpty(name) ? "لون مخصص" : name;
            lblPreviewName.ForeColor = Color.FromArgb(220, 230, 240);

            string hexStr = $"#{_selectedColor.R:X2}{_selectedColor.G:X2}{_selectedColor.B:X2}";
            lblPreviewHex.Text = $"HEX: {hexStr}";
            lblPreviewHex.ForeColor = Color.FromArgb(51, 164, 244);

            lblPreviewRGB.Text = $"RGB: {_selectedColor.R}, {_selectedColor.G}, {_selectedColor.B}";
            lblPreviewRGB.ForeColor = Color.FromArgb(120, 140, 170);

            // ── Update accent bar color ──
            pnlAccent.BackColor = _selectedColor;

            pnlPreviewContainer.BorderColor = Color.FromArgb(60, _selectedColor.R, _selectedColor.G, _selectedColor.B);
            pnlPreviewContainer.Invalidate();
        }

        private void ResetPreview()
        {
            picPreview.FillColor = Color.FromArgb(40, 45, 65);
            picPreview.ShadowDecoration.Color = Color.FromArgb(50, 0, 0, 0);
            picPreview.ShadowDecoration.Depth = 8;

            lblPreviewName.Text = "لم يتم اختيار لون";
            lblPreviewName.ForeColor = Color.FromArgb(100, 115, 140);
            lblPreviewHex.Text = "HEX: ---";
            lblPreviewHex.ForeColor = Color.FromArgb(75, 90, 115);
            lblPreviewRGB.Text = "RGB: ---";
            lblPreviewRGB.ForeColor = Color.FromArgb(75, 90, 115);

            pnlAccent.BackColor = Color.FromArgb(51, 164, 244);
            pnlPreviewContainer.BorderColor = Color.FromArgb(35, 45, 65);
            pnlPreviewContainer.Invalidate();
        }

        // ═══════════════════════════════════════
        //  💾  Save Color to Database
        // ═══════════════════════════════════════
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            // ── Validation ──
            if (string.IsNullOrWhiteSpace(txtColorName.Text))
            {
                ToastManager.ShowWarning("تنبيه", "من فضلك أدخل اسم اللون");
                txtColorName.Focus();
                return;
            }

            string hex = txtHexCode.Text?.Trim() ?? "";
            if (!hex.StartsWith("#")) hex = "#" + hex;
            hex = hex.TrimStart('#');
            if (hex.Length != 6)
            {
                ToastManager.ShowWarning("تنبيه", "من فضلك أدخل كود لون صحيح (مثال: #FF5733)");
                txtHexCode.Focus();
                return;
            }

            if (_selectedColor == Color.Empty)
            {
                ToastManager.ShowWarning("تنبيه", "من فضلك اختر لوناً أولاً");
                return;
            }

            string hexCode = $"#{hex.ToUpper()}";
            string colorName = txtColorName.Text.Trim();
            string rgb = $"{_selectedColor.R},{_selectedColor.G},{_selectedColor.B}";

            // ── Disable button to prevent double-click ──
            btnSave.Enabled = false;
            btnSave.Text = "⏳  جاري الحفظ...";

            try
            {
                // ── Check duplicate hex ──
                string checkQuery = "SELECT COUNT(*) FROM Colors WHERE HexCode = @Hex AND IsDeleted = 0";
                object result = await DB_Server.ScalarAsync(checkQuery, new[]
                {
                    new SqlParameter("@Hex", hexCode)
                });

                if (result != null && Convert.ToInt32(result) > 0)
                {
                    ToastManager.ShowWarning("تنبيه", "هذا اللون موجود بالفعل في قاعدة البيانات");
                    btnSave.Enabled = true;
                    btnSave.Text = "💾  حفظ اللون";
                    return;
                }

                // ── Get next SortOrder ──
                string maxQuery = "SELECT ISNULL(MAX(SortOrder), 0) + 1 FROM Colors WHERE IsDeleted = 0";
                object maxResult = await DB_Server.ScalarAsync(maxQuery);
                int nextSort = maxResult != null && maxResult != DBNull.Value ? Convert.ToInt32(maxResult) : 1;

                // ── Insert ──
                string insertQuery = @"
                    INSERT INTO Colors (ColorCode, HexCode, RGB, SortOrder, IsActive, IsDeleted)
                    VALUES (@ColorCode, @HexCode, @RGB, @SortOrder, 1, 0)";

                await DB_Server.ExecuteAsync(insertQuery, new[]
                {
                    new SqlParameter("@ColorCode", colorName),
                    new SqlParameter("@HexCode", hexCode),
                    new SqlParameter("@RGB", rgb),
                    new SqlParameter("@SortOrder", nextSort),
                });

                ToastManager.ShowSuccess("نجاح", $"✅ تم إضافة اللون \"{colorName}\" بنجاح");

                // ── Notify parent ──
                ColorAdded?.Invoke(this, EventArgs.Empty);

                _isClosing = true;
                _fadeTimer.Start();
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في حفظ اللون: " + ex.Message);
                btnSave.Enabled = true;
                btnSave.Text = "💾  حفظ اللون";
            }
        }

        // ═══════════════════════════════════════
        //  📌  Static Show helper (like frmConfirm)
        // ═══════════════════════════════════════
        public static void ShowDialog(Form owner, Action onColorAdded)
        {
            using (var dlg = new UC_AddColor())
            {
                if (onColorAdded != null)
                    dlg.ColorAdded += (s, e) => onColorAdded();

                if (owner != null)
                {
                    Screen screen = Screen.FromControl(owner);
                    dlg.StartPosition = FormStartPosition.Manual;
                    dlg.Bounds = screen.WorkingArea;
                }

                dlg.ShowDialog(owner);
            }
        }
    }
}
