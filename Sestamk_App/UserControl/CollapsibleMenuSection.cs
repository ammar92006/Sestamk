using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    /// <summary>
    /// قسم قائمة قابل للطي - يمكن إضافته من الـ Toolbox مباشرة
    /// Collapsible Menu Section - A reusable sidebar menu item with expandable children
    ///
    /// الاستخدام:
    /// 1. قم بتجميع المشروع (Build)
    /// 2. سيظهر الكنترول في الـ Toolbox تلقائياً
    /// 3. اسحبه على أي Form أو Panel
    /// 4. من الـ Properties عدّل: Title, ChildItems, الألوان, الخطوط, إلخ
    /// 5. تعامل مع الحدث ChildItemClicked للتنقل
    ///
    /// مثال برمجي:
    ///   var section = new CollapsibleMenuSection();
    ///   section.Title = "المبيعات";
    ///   section.SetChildItems(new[] { "نقاط البيع", "الطلبات", "الطاولات" });
    ///   section.ChildItemClicked += (s, e) => { MessageBox.Show(e.ItemName); };
    /// </summary>
    [DefaultEvent("ChildItemClicked")]
    [DefaultProperty("Title")]
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Panel))]
    [Description("A collapsible sidebar menu section with expandable child items - قسم قائمة قابل للطي")]
    [Docking(DockingBehavior.AutoDock)]
    public partial class CollapsibleMenuSection : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private string _title = "Menu Section";
        private bool _isExpanded = false;
        private int _headerHeight = 48;
        private int _childItemHeight = 42;
        private int _childItemSpacing = 2;
        private int _animationSpeed = 8;
        private int _animationSteps = 15;
        private int _borderRadius = 8;

        // ---- Colors ----
        private Color _headerBackColor = Color.FromArgb(31, 41, 55);
        private Color _headerHoverColor = Color.FromArgb(40, 52, 70);
        private Color _headerForeColor = Color.FromArgb(229, 231, 235);
        private Color _childBackColor = Color.FromArgb(24, 33, 48);
        private Color _childHoverColor = Color.FromArgb(55, 65, 81);
        private Color _childForeColor = Color.FromArgb(200, 210, 220);
        private Color _activeChildBackColor = Color.FromArgb(240, 38, 63);
        private Color _activeChildForeColor = Color.White;
        private Color _arrowColor = Color.FromArgb(156, 163, 175);
        private Color _accentColor = Color.FromArgb(240, 38, 63);
        private Color _separatorColor = Color.FromArgb(45, 55, 72);

        // ---- Fonts ----
        private Font _headerFont = new Font("Alexandria", 11F, FontStyle.Bold);
        private Font _childFont = new Font("Alexandria", 9.5F, FontStyle.Regular);

        // ---- Icon ----
        private Image? _headerIcon = null;
        private Size _headerIconSize = new Size(22, 22);

        // ---- Internal Controls ----
        private Panel pnlHeader = null!;
        private Panel pnlChildren = null!;
        private Label lblTitle = null!;
        private Panel pnlArrow = null!;
        private PictureBox picIcon = null!;

        // ---- Data ----
        private List<string> _childItems = new List<string>();
        private string _activeChildItem = "";

        // ---- Animation ----
        private System.Windows.Forms.Timer? _animationTimer;
        private int _targetHeight;
        private bool _isAnimating = false;

        // ---- State ----
        private bool _isHeaderHovered = false;
        private Dictionary<Panel, bool> _childHoverStates = new Dictionary<Panel, bool>();

        // ---- Layout ----
        private int _bottomMargin = 2;

        #endregion

        #region Events

        /// <summary>
        /// يُطلق عند النقر على عنصر فرعي
        /// Fires when a child menu item is clicked
        /// </summary>
        [Category("Collapsible Menu")]
        [Description("Fires when a child menu item is clicked")]
        public event EventHandler<ChildItemClickEventArgs>? ChildItemClicked;

        /// <summary>
        /// يُطلق عند فتح أو إغلاق القسم
        /// Fires when the section is expanded or collapsed
        /// </summary>
        [Category("Collapsible Menu")]
        [Description("Fires when the section is expanded or collapsed")]
        public event EventHandler<ExpandedChangedEventArgs>? ExpandedChanged;

        /// <summary>
        /// يُطلق عند النقر على الهيدر
        /// Fires when the header is clicked
        /// </summary>
        [Category("Collapsible Menu")]
        [Description("Fires when the header is clicked")]
        public event EventHandler? HeaderClicked;

        #endregion

        #region Constructor

        public CollapsibleMenuSection()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            this.BackColor = Color.Transparent;
            BuildUI();
        }

        #endregion

        #region Properties - Appearance

        [Category("Collapsible Menu")]
        [Description("العنوان المعروض على الهيدر - The title text displayed on the header")]
        [DefaultValue("Menu Section")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if (lblTitle != null) lblTitle.Text = value;
                Invalidate();
            }
        }

        [Category("Collapsible Menu")]
        [Description("هل القسم مفتوح - Whether the section is expanded")]
        [DefaultValue(false)]
        [Browsable(true)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (!DesignMode)
                        AnimateExpand(value);
                    else
                        ApplyExpandState(value);
                    Invalidate();
                }
            }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون خلفية الهيدر - Background color of the header")]
        [DefaultValue(typeof(Color), "31, 41, 55")]
        public Color HeaderBackColor
        {
            get => _headerBackColor;
            set { _headerBackColor = value; if (pnlHeader != null) pnlHeader.Invalidate(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون الهيدر عند التمرير - Hover color of the header")]
        [DefaultValue(typeof(Color), "40, 52, 70")]
        public Color HeaderHoverColor
        {
            get => _headerHoverColor;
            set => _headerHoverColor = value;
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون نص الهيدر - Text color of the header")]
        [DefaultValue(typeof(Color), "229, 231, 235")]
        public Color HeaderForeColor
        {
            get => _headerForeColor;
            set { _headerForeColor = value; if (lblTitle != null) lblTitle.ForeColor = value; }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون خلفية العناصر الفرعية - Background color of child items")]
        [DefaultValue(typeof(Color), "24, 33, 48")]
        public Color ChildBackColor
        {
            get => _childBackColor;
            set { _childBackColor = value; RefreshChildren(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون العناصر الفرعية عند التمرير - Hover color of child items")]
        [DefaultValue(typeof(Color), "55, 65, 81")]
        public Color ChildHoverColor
        {
            get => _childHoverColor;
            set => _childHoverColor = value;
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون نص العناصر الفرعية - Text color of child items")]
        [DefaultValue(typeof(Color), "200, 210, 220")]
        public Color ChildForeColor
        {
            get => _childForeColor;
            set { _childForeColor = value; RefreshChildren(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون خلفية العنصر النشط - Background color of the active/selected child item")]
        [DefaultValue(typeof(Color), "240, 38, 63")]
        public Color ActiveChildBackColor
        {
            get => _activeChildBackColor;
            set { _activeChildBackColor = value; RefreshChildren(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون نص العنصر النشط - Text color of the active/selected child item")]
        [DefaultValue(typeof(Color), "255, 255, 255")]
        public Color ActiveChildForeColor
        {
            get => _activeChildForeColor;
            set { _activeChildForeColor = value; RefreshChildren(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون السهم - Color of the arrow indicator")]
        [DefaultValue(typeof(Color), "156, 163, 175")]
        public Color ArrowColor
        {
            get => _arrowColor;
            set { _arrowColor = value; if (pnlArrow != null) pnlArrow.Invalidate(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("اللون المميز - Accent color used for active indicators")]
        [DefaultValue(typeof(Color), "240, 38, 63")]
        public Color AccentColor
        {
            get => _accentColor;
            set { _accentColor = value; Invalidate(); }
        }

        [Category("Collapsible Menu - Colors")]
        [Description("لون الفاصل - Color of the separator line")]
        [DefaultValue(typeof(Color), "45, 55, 72")]
        public Color SeparatorColor
        {
            get => _separatorColor;
            set { _separatorColor = value; Invalidate(); }
        }

        #endregion

        #region Properties - Layout

        [Category("Collapsible Menu - Layout")]
        [Description("ارتفاع الهيدر - Height of the header panel")]
        [DefaultValue(48)]
        public int HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = value; RebuildUI(); }
        }

        [Category("Collapsible Menu - Layout")]
        [Description("ارتفاع كل عنصر فرعي - Height of each child item")]
        [DefaultValue(42)]
        public int ChildItemHeight
        {
            get => _childItemHeight;
            set { _childItemHeight = value; RebuildUI(); }
        }

        [Category("Collapsible Menu - Layout")]
        [Description("المسافة بين العناصر الفرعية - Spacing between child items")]
        [DefaultValue(2)]
        public int ChildItemSpacing
        {
            get => _childItemSpacing;
            set { _childItemSpacing = value; RebuildUI(); }
        }

        [Category("Collapsible Menu - Layout")]
        [Description("الهامش السفلي - Bottom margin for spacing between sections")]
        [DefaultValue(2)]
        public int BottomMargin
        {
            get => _bottomMargin;
            set { _bottomMargin = value; this.Margin = new Padding(0, 0, 0, value); }
        }

        [Category("Collapsible Menu - Layout")]
        [Description("نصف قطر الحواف الدائرية - Border radius for rounded corners")]
        [DefaultValue(8)]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = value; Invalidate(); }
        }

        #endregion

        #region Properties - Fonts

        [Category("Collapsible Menu - Fonts")]
        [Description("خط عنوان الهيدر - Font used for the header title")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Font HeaderFont
        {
            get => _headerFont;
            set { _headerFont = value; if (lblTitle != null) lblTitle.Font = value; }
        }

        [Category("Collapsible Menu - Fonts")]
        [Description("خط العناصر الفرعية - Font used for child items")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Font ChildFont
        {
            get => _childFont;
            set { _childFont = value; RefreshChildren(); }
        }

        #endregion

        #region Properties - Icon

        [Category("Collapsible Menu - Icon")]
        [Description("أيقونة الهيدر - Icon displayed on the header")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image? HeaderIcon
        {
            get => _headerIcon;
            set
            {
                _headerIcon = value;
                if (picIcon != null)
                {
                    picIcon.Image = value;
                    picIcon.Visible = value != null;
                    PositionHeaderControls();
                }
            }
        }

        [Category("Collapsible Menu - Icon")]
        [Description("حجم أيقونة الهيدر - Size of the header icon")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Size HeaderIconSize
        {
            get => _headerIconSize;
            set
            {
                _headerIconSize = value;
                if (picIcon != null)
                {
                    picIcon.Size = value;
                    PositionHeaderControls();
                }
            }
        }

        #endregion

        #region Properties - Animation

        [Category("Collapsible Menu - Animation")]
        [Description("سرعة الأنيميشن بالمللي ثانية - Speed of the expand/collapse animation in ms per step")]
        [DefaultValue(12)]
        public int AnimationSpeed
        {
            get => _animationSpeed;
            set => _animationSpeed = Math.Max(1, value);
        }

        [Category("Collapsible Menu - Animation")]
        [Description("عدد خطوات الأنيميشن - Number of animation steps")]
        [DefaultValue(10)]
        public int AnimationSteps
        {
            get => _animationSteps;
            set => _animationSteps = Math.Max(1, value);
        }

        #endregion

        #region Properties - Data

        [Category("Collapsible Menu - Data")]
        [Description("قائمة العناصر الفرعية - List of child menu item names")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(System.Drawing.Design.UITypeEditor))]
        public List<string> ChildItems
        {
            get => _childItems;
            set
            {
                _childItems = value ?? new List<string>();
                RebuildChildren();
            }
        }

        [Category("Collapsible Menu - Data")]
        [Description("العنصر النشط حالياً - The currently active/selected child item name")]
        [DefaultValue("")]
        public string ActiveChildItem
        {
            get => _activeChildItem;
            set
            {
                _activeChildItem = value ?? "";
                RefreshChildren();
            }
        }

        #endregion

        #region UI Build

        private void BuildUI()
        {
            this.SuspendLayout();
            this.Width = 260;
            this.Height = _headerHeight;
            this.Padding = Padding.Empty;
            this.Margin = new Padding(0, 0, 0, 2);

            // ═══════════════════════════════════════
            //  Header Panel
            // ═══════════════════════════════════════
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = _headerHeight,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent,
            };
            pnlHeader.Paint += PnlHeader_Paint;
            pnlHeader.MouseEnter += (s, e) => { _isHeaderHovered = true; pnlHeader.Invalidate(); };
            pnlHeader.MouseLeave += (s, e) => { _isHeaderHovered = false; pnlHeader.Invalidate(); };
            pnlHeader.Click += (s, e) => OnHeaderClick();

            // Icon
            picIcon = new PictureBox
            {
                Size = _headerIconSize,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Visible = _headerIcon != null,
                Image = _headerIcon,
                Cursor = Cursors.Hand,
            };
            picIcon.Click += (s, e) => OnHeaderClick();
            picIcon.MouseEnter += (s, e) => { _isHeaderHovered = true; pnlHeader.Invalidate(); };
            picIcon.MouseLeave += (s, e) => { _isHeaderHovered = false; pnlHeader.Invalidate(); };

            // Title label
            lblTitle = new Label
            {
                Text = _title,
                ForeColor = _headerForeColor,
                Font = _headerFont,
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
            };
            lblTitle.Click += (s, e) => OnHeaderClick();
            lblTitle.MouseEnter += (s, e) => { _isHeaderHovered = true; pnlHeader.Invalidate(); };
            lblTitle.MouseLeave += (s, e) => { _isHeaderHovered = false; pnlHeader.Invalidate(); };

            // Arrow panel (custom painted chevron)
            pnlArrow = new Panel
            {
                Size = new Size(24, 24),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
            };
            pnlArrow.Paint += PnlArrow_Paint;
            pnlArrow.Click += (s, e) => OnHeaderClick();
            pnlArrow.MouseEnter += (s, e) => { _isHeaderHovered = true; pnlHeader.Invalidate(); };
            pnlArrow.MouseLeave += (s, e) => { _isHeaderHovered = false; pnlHeader.Invalidate(); };

            pnlHeader.Controls.Add(pnlArrow);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(picIcon);

            // ═══════════════════════════════════════
            //  Children Panel
            // ═══════════════════════════════════════
            pnlChildren = new Panel
            {
                Dock = DockStyle.Top,
                Height = 0,
                BackColor = Color.Transparent,
                Visible = false,
            };

            // الترتيب مهم: Children ثم Header عشان الـ Dock يشتغل صح
            this.Controls.Add(pnlChildren);
            this.Controls.Add(pnlHeader);

            PositionHeaderControls();

            this.ResumeLayout(false);
        }

        private void PositionHeaderControls()
        {
            if (pnlHeader == null || lblTitle == null || pnlArrow == null) return;

            int rightPadding = 12;
            int leftPadding = 12;
            int centerY = (_headerHeight - lblTitle.PreferredHeight) / 2;

            bool isRtl = (this.RightToLeft == RightToLeft.Yes);

            if (isRtl)
            {
                // RTL: الأيقونة على اليمين، السهم على اليسار
                int x = pnlHeader.Width - rightPadding;

                if (_headerIcon != null && picIcon != null)
                {
                    picIcon.Location = new Point(x - _headerIconSize.Width, (_headerHeight - _headerIconSize.Height) / 2);
                    x -= (_headerIconSize.Width + 10);
                }

                lblTitle.Location = new Point(x - lblTitle.PreferredWidth, centerY);
                pnlArrow.Location = new Point(leftPadding, (_headerHeight - 24) / 2);
            }
            else
            {
                // LTR: الأيقونة على اليسار، السهم على اليمين
                int x = leftPadding;

                if (_headerIcon != null && picIcon != null)
                {
                    picIcon.Location = new Point(x, (_headerHeight - _headerIconSize.Height) / 2);
                    x += (_headerIconSize.Width + 10);
                }

                lblTitle.Location = new Point(x, centerY);
                pnlArrow.Location = new Point(pnlHeader.Width - rightPadding - 24, (_headerHeight - 24) / 2);
            }
        }

        private void RebuildUI()
        {
            if (pnlHeader == null) return;
            pnlHeader.Height = _headerHeight;
            PositionHeaderControls();
            RebuildChildren();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PositionHeaderControls();

            if (pnlChildren != null)
            {
                foreach (Control ctrl in pnlChildren.Controls)
                {
                    if (ctrl is Panel childPanel)
                    {
                        childPanel.Width = this.Width - 8;
                    }
                }
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PositionHeaderControls();
            RebuildChildren();
        }

        #endregion

        #region Child Items

        private void RebuildChildren()
        {
            if (pnlChildren == null) return;

            pnlChildren.SuspendLayout();
            pnlChildren.Controls.Clear();
            _childHoverStates.Clear();

            int y = 4;
            for (int i = 0; i < _childItems.Count; i++)
            {
                Panel childPanel = CreateChildItem(_childItems[i], i);
                childPanel.Location = new Point(4, y);
                pnlChildren.Controls.Add(childPanel);
                y += _childItemHeight + _childItemSpacing;
            }

            int totalChildrenHeight = y + 4;

            if (_isExpanded)
            {
                pnlChildren.Height = totalChildrenHeight;
                pnlChildren.Visible = true;
                this.Height = _headerHeight + totalChildrenHeight;
            }
            else
            {
                pnlChildren.Height = 0;
                pnlChildren.Visible = false;
                this.Height = _headerHeight;
            }

            pnlChildren.ResumeLayout(true);
        }

        private Panel CreateChildItem(string text, int index)
        {
            bool isActive = text == _activeChildItem;

            Panel itemPanel = new Panel
            {
                Width = this.Width - 8,
                Height = _childItemHeight,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = text,
            };

            // تسجيل حالة الـ hover لهذا العنصر
            _childHoverStates[itemPanel] = false;

            itemPanel.Paint += (s, e) =>
            {
                if (s is not Panel panel) return;

                bool active = (string)panel.Tag! == _activeChildItem;
                bool hovered = _childHoverStates.ContainsKey(panel) && _childHoverStates[panel];

                // تحديد لون الخلفية بناءً على الحالة
                Color bgColor;
                if (active)
                    bgColor = _activeChildBackColor;
                else if (hovered)
                    bgColor = _childHoverColor;
                else
                    bgColor = _childBackColor;

                using (GraphicsPath path = CreateRoundedRectPath(new Rectangle(0, 0, panel.Width, panel.Height), 6))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush brush = new SolidBrush(bgColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // شريط مؤشر العنصر النشط
                    if (active)
                    {
                        bool isRtl = (this.RightToLeft == RightToLeft.Yes);
                        Rectangle barRect = isRtl
                            ? new Rectangle(panel.Width - 4, 8, 3, panel.Height - 16)
                            : new Rectangle(1, 8, 3, panel.Height - 16);

                        using (SolidBrush barBrush = new SolidBrush(_accentColor))
                        {
                            e.Graphics.FillRectangle(barBrush, barRect);
                        }
                    }
                }
            };

            Label childLabel = new Label
            {
                Text = text,
                ForeColor = isActive ? _activeChildForeColor : _childForeColor,
                Font = _childFont,
                AutoSize = false,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TextAlign = (this.RightToLeft == RightToLeft.Yes) ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = (this.RightToLeft == RightToLeft.Yes) ? new Padding(0, 0, 20, 0) : new Padding(20, 0, 0, 0),
                Tag = text,
            };

            childLabel.Click += (s, e) => OnChildItemClick(text);
            itemPanel.Click += (s, e) => OnChildItemClick(text);

            // ---- Hover Effects (بدون تعديل الـ field المشترك) ----
            Action<bool> setHover = (hover) =>
            {
                bool active = (string)itemPanel.Tag! == _activeChildItem;
                if (!active)
                {
                    _childHoverStates[itemPanel] = hover;
                    itemPanel.Invalidate();
                    childLabel.ForeColor = hover ? _headerForeColor : _childForeColor;
                }
            };

            childLabel.MouseEnter += (s, e) => setHover(true);
            childLabel.MouseLeave += (s, e) => setHover(false);
            itemPanel.MouseEnter += (s, e) => setHover(true);
            itemPanel.MouseLeave += (s, e) => setHover(false);

            itemPanel.Controls.Add(childLabel);
            return itemPanel;
        }

        private void RefreshChildren()
        {
            RebuildChildren();
        }

        /// <summary>
        /// أضف عنصر فرعي جديد برمجياً
        /// Add a new child item programmatically
        /// </summary>
        public void AddChildItem(string itemName)
        {
            _childItems.Add(itemName);
            RebuildChildren();
        }

        /// <summary>
        /// احذف عنصر فرعي
        /// Remove a child item
        /// </summary>
        public void RemoveChildItem(string itemName)
        {
            _childItems.Remove(itemName);
            RebuildChildren();
        }

        /// <summary>
        /// امسح جميع العناصر الفرعية
        /// Clear all child items
        /// </summary>
        public void ClearChildItems()
        {
            _childItems.Clear();
            RebuildChildren();
        }

        #endregion

        #region Painting

        private void PnlHeader_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height);
            Color bgColor = _isHeaderHovered ? _headerHoverColor : _headerBackColor;

            using (GraphicsPath path = CreateRoundedRectPath(rect, _borderRadius))
            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                g.FillPath(brush, path);
            }

            // شريط Accent عند التوسيع
            if (_isExpanded)
            {
                bool isRtl = (this.RightToLeft == RightToLeft.Yes);
                Rectangle barRect = isRtl
                    ? new Rectangle(pnlHeader.Width - 4, 10, 3, pnlHeader.Height - 20)
                    : new Rectangle(1, 10, 3, pnlHeader.Height - 20);

                using (SolidBrush barBrush = new SolidBrush(_accentColor))
                {
                    g.FillRectangle(barBrush, barRect);
                }
            }
        }

        private void PnlArrow_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // رسم سهم Chevron
            using (Pen pen = new Pen(_arrowColor, 2.5f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                int cx = 12, cy = 12;
                int size = 5;

                if (_isExpanded)
                {
                    // سهم لأعلى ^
                    g.DrawLine(pen, cx - size, cy + 2, cx, cy - 3);
                    g.DrawLine(pen, cx, cy - 3, cx + size, cy + 2);
                }
                else
                {
                    // سهم لأسفل v
                    g.DrawLine(pen, cx - size, cy - 2, cx, cy + 3);
                    g.DrawLine(pen, cx, cy + 3, cx + size, cy - 2);
                }
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        #endregion

        #region Animation

        /// <summary>
        /// Animate expand/collapse with smooth transition
        /// </summary>
        private void AnimateExpand(bool expand)
        {
            _isExpanded = expand;

            if (_childItems.Count == 0)
            {
                pnlChildren.Visible = false;
                pnlChildren.Height = 0;
                this.Height = _headerHeight;
                pnlArrow?.Invalidate();
                OnLayoutUpdated();
                ExpandedChanged?.Invoke(this, new ExpandedChangedEventArgs(expand));
                return;
            }

            int totalChildrenHeight = CalculateTotalChildrenHeight();
            _targetHeight = expand ? totalChildrenHeight : 0;

            // Stop any ongoing animation
            StopAnimation();

            pnlChildren.Visible = true;
            _isAnimating = true;

            int currentH = pnlChildren.Height;
            int diff = _targetHeight - currentH;
            
            // Use easing function for smoother animation
            int stepsRemaining = _animationSteps;
            int startHeight = currentH;

            _animationTimer = new System.Windows.Forms.Timer { Interval = _animationSpeed };
            int currentStep = 0;
            
            _animationTimer.Tick += (s, e) =>
            {
                currentStep++;
                
                // Use ease-in-out cubic for smooth transition
                float t = (float)currentStep / _animationSteps;
                float easedT = t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2;
                
                int newHeight = startHeight + (int)(diff * easedT);
                pnlChildren.Height = Math.Max(0, newHeight);
                this.Height = _headerHeight + pnlChildren.Height;

                if (currentStep >= _animationSteps)
                {
                    // Animation complete
                    pnlChildren.Height = _targetHeight;
                    this.Height = _headerHeight + _targetHeight;
                    StopAnimation();
                    _isAnimating = false;

                    if (!expand)
                    {
                        pnlChildren.Visible = false;
                    }

                    pnlArrow?.Invalidate();
                    OnLayoutUpdated();
                    ExpandedChanged?.Invoke(this, new ExpandedChangedEventArgs(expand));
                }
            };
            _animationTimer.Start();

            pnlArrow?.Invalidate();
        }

        /// <summary>
        /// Stop any ongoing animation
        /// </summary>
        private void StopAnimation()
        {
            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer.Dispose();
                _animationTimer = null;
            }
            _isAnimating = false;
        }

        /// <summary>
        /// Notify parent about layout changes for proper repositioning
        /// </summary>
        private void OnLayoutUpdated()
        {
            // Trigger layout recalculation in parent container
            this.Parent?.Invalidate();
            this.Parent?.Update();
        }

        private void ApplyExpandState(bool expand)
        {
            if (_childItems.Count == 0)
            {
                pnlChildren.Visible = false;
                pnlChildren.Height = 0;
                this.Height = _headerHeight;
                return;
            }

            int totalChildrenHeight = CalculateTotalChildrenHeight();

            if (expand)
            {
                pnlChildren.Visible = true;
                pnlChildren.Height = totalChildrenHeight;
                this.Height = _headerHeight + totalChildrenHeight;
            }
            else
            {
                pnlChildren.Visible = false;
                pnlChildren.Height = 0;
                this.Height = _headerHeight;
            }

            pnlArrow?.Invalidate();
        }

        private int CalculateTotalChildrenHeight()
        {
            int total = 4; // Top padding
            for (int i = 0; i < _childItems.Count; i++)
            {
                total += _childItemHeight + _childItemSpacing;
            }
            total += 4; // Bottom padding
            return total;
        }

        #endregion

        #region Event Handlers

        private void OnHeaderClick()
        {
            HeaderClicked?.Invoke(this, EventArgs.Empty);
            IsExpanded = !IsExpanded;
        }

        private void OnChildItemClick(string itemName)
        {
            ActiveChildItem = itemName;
            ChildItemClicked?.Invoke(this, new ChildItemClickEventArgs(itemName));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// فتح/إغلاق القسم
        /// Toggle the expand/collapse state
        /// </summary>
        public void Toggle()
        {
            IsExpanded = !IsExpanded;
        }

        /// <summary>
        /// فتح القسم
        /// Expand the section
        /// </summary>
        public void Expand()
        {
            IsExpanded = true;
        }

        /// <summary>
        /// إغلاق القسم
        /// Collapse the section
        /// </summary>
        public void Collapse()
        {
            IsExpanded = false;
        }

        /// <summary>
        /// تعيين العناصر الفرعية من مصفوفة نصوص
        /// Set up child items from a string array
        /// مثال: section.SetChildItems(new[] { "POS", "Orders", "Tables" });
        /// </summary>
        public void SetChildItems(string[] items)
        {
            _childItems = new List<string>(items);
            RebuildChildren();
        }

        /// <summary>
        /// إعادة ضبط العنصر النشط
        /// Reset the active child item
        /// </summary>
        public void ResetActiveItem()
        {
            _activeChildItem = "";
            RefreshChildren();
        }

        /// <summary>
        /// إغلاق جميع الأقسام في الكنتينر الأب (مفيد لـ Accordion)
        /// Collapse all sibling sections in the parent container (useful for accordion behavior)
        /// </summary>
        public void CollapseAllSiblings()
        {
            if (this.Parent == null) return;

            foreach (Control ctrl in this.Parent.Controls)
            {
                if (ctrl is CollapsibleMenuSection section && section != this)
                {
                    section.Collapse();
                }
            }
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Stop();
                _animationTimer?.Dispose();
                _headerFont?.Dispose();
                _childFont?.Dispose();
                _childHoverStates.Clear();
            }
            base.Dispose(disposing);
        }

        #endregion


    }

    #region Event Args Classes

    /// <summary>
    /// بيانات الحدث عند النقر على عنصر فرعي
    /// Event data when a child item is clicked
    /// </summary>
    public class ChildItemClickEventArgs : EventArgs
    {
        /// <summary>
        /// اسم العنصر الذي تم النقر عليه
        /// </summary>
        public string ItemName { get; }

        /// <summary>
        /// فهرس العنصر في القائمة
        /// </summary>
        public int ItemIndex { get; }

        public ChildItemClickEventArgs(string itemName, int itemIndex = -1)
        {
            ItemName = itemName;
            ItemIndex = itemIndex;
        }
    }

    /// <summary>
    /// بيانات الحدث عند تغيير حالة التوسيع
    /// Event data when expanded state changes
    /// </summary>
    public class ExpandedChangedEventArgs : EventArgs
    {
        /// <summary>
        /// هل القسم مفتوح الآن
        /// </summary>
        public bool IsExpanded { get; }

        public ExpandedChangedEventArgs(bool isExpanded)
        {
            IsExpanded = isExpanded;
        }
    }

    #endregion
}
