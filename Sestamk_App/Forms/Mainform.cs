using Google.GenAI;
using Google.GenAI.Types;
using Guna.Charts.WinForms;
using Guna.UI2.WinForms;
using ReaLTaiizor.Forms;
using Sestamk.Classes;
using Sestamk.UserControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class Mainform : BaseForm
    {
        protected override Size DesignClientSize => new Size(1600, 900);

        #region ── Constants ──────────────────────────────────────────────────

        // ─── Dark Mode Color Palette ───
        private static readonly Color CHART_BACKGROUND = Color.FromArgb(30, 30, 46);     // #1E1E2E
        private static readonly Color CHART_GRID = Color.FromArgb(42, 42, 64);      // #2A2A40
        private static readonly Color CHART_TEXT = Color.FromArgb(234, 234, 240);    // #EAEAF0
        private static readonly Color CHART_TEXT_MUTED = Color.FromArgb(156, 163, 175);    // #9CA3AF
        private static readonly Color CHART_ACCENT_BLUE = Color.FromArgb(99, 132, 255);    // Soft blue
        private static readonly Color CHART_ACCENT_PURPLE = Color.FromArgb(167, 106, 255);   // Soft purple
        private static readonly Color CHART_ACCENT_GREEN = Color.FromArgb(72, 219, 163);    // Soft teal-green
        private static readonly Color CHART_FILL_BLUE = Color.FromArgb(30, 99, 132, 255); // Blue with transparency

        // ─── Menu Button Colors ───
        private static readonly Color MENU_BTN_ACTIVE_COLOR = Color.FromArgb(240, 38, 63);
        private static readonly Color MENU_BTN_HOVER_COLOR = Color.FromArgb(56, 65, 82);

        // ─── User Profile Colors ───
        private static readonly Color PROFILE_BG = Color.FromArgb(31, 41, 55);
        private static readonly Color PROFILE_NAME_COLOR = Color.FromArgb(229, 231, 235);
        private static readonly Color PROFILE_ROLE_COLOR = Color.FromArgb(156, 163, 175);
        private static readonly Color PROFILE_AVATAR_BG = Color.FromArgb(54, 100, 239);

        #endregion

        #region ── Fields ─────────────────────────────────────────────────────

        private bool _isLoggingOut = false;

        // ─── User Profile Controls ───
        private Guna2Panel pnlUserProfile = null!;
        private Guna2PictureBox picUserAvatar = null!;
        private Label lblUserName = null!;
        private Label lblUserRole = null!;

        // ─── Dashboard KPI Labels ───
        private Label lblTodaySalesValue = null!;
        private Label lblTodaySalesTrend = null!;
        private Label lblOrdersValue = null!;
        private Label lblOrdersTrend = null!;
        private Label lblTablesValue = null!;
        private Label lblTablesTrend = null!;

        // ─── Dashboard Grids ───
        private Guna.UI2.WinForms.Guna2DataGridView dgvRecentOrders = null!;
        private Guna.UI2.WinForms.Guna2DataGridView dgvTables = null!;

        // ─── Dashboard bottom panels ───
        private Guna.UI2.WinForms.Guna2Panel pnlBottomRow = null!;

        #endregion

        #region ── Constructor ────────────────────────────────────────────────

        public Mainform()
        {
            InitializeComponent();

            // Setup user profile in sidebar
            SetupUserProfile();

            // Setup the collapsible sidebar navigation
            SetupSidebarNavigation();

            // Build the live dashboard widgets inside the existing content panel
            BuildDashboardWidgets();

            // Load user info and apply permissions
            LoadUserInfoAndApplyPermissions();

            // Load live data asynchronously
            _ = LoadDashboardDataAsync();
        }

        #endregion

        #region ── User Profile Setup ─────────────────────────────────────────

        /// <summary>
        /// Builds the user profile panel in the sidebar with avatar, name, and role
        /// </summary>
        private void SetupUserProfile()
        {
            // ─── Setup Logo Panel (guna2Panel3) ───
            SetupLogoPanel();

            // ─── Setup User Profile Panel (guna2Panel4) ───
            guna2Panel4.Controls.Clear();
            guna2Panel4.Height = 180;
            guna2Panel4.Padding = new Padding(15, 10, 15, 10);

            // ─── Avatar ───
            picUserAvatar = new Guna2PictureBox
            {
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderRadius = 40,
                FillColor = PROFILE_AVATAR_BG,
                ImageRotate = 0F,
                Cursor = Cursors.Hand,
                Location = new Point((guna2Panel4.Width - 80) / 2, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            // Set default user icon from resources if available
            if (Properties.Resources.profile__1_ != null)
                picUserAvatar.Image = Properties.Resources.profile__1_;

            // ─── User Name Label ───
            lblUserName = new Label
            {
                Name = "lblUserName",
                Text = "...",
                Font = new Font("Alexandria", 14F, FontStyle.Bold),
                ForeColor = PROFILE_NAME_COLOR,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(15, 105),
                Size = new Size(guna2Panel4.Width - 30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };

            // ─── Role Label ───
            lblUserRole = new Label
            {
                Name = "lblUserRole",
                Text = "...",
                Font = new Font("Alexandria", 10F, FontStyle.Regular),
                ForeColor = PROFILE_ROLE_COLOR,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(15, 138),
                Size = new Size(guna2Panel4.Width - 30, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            guna2Panel4.Controls.Add(picUserAvatar);
            guna2Panel4.Controls.Add(lblUserName);
            guna2Panel4.Controls.Add(lblUserRole);

            // ─── Setup Logout Button Panel (guna2Panel6) ───
            SetupLogoutPanel();
        }

        /// <summary>
        /// Setup the top logo panel with app title
        /// </summary>
        private void SetupLogoPanel()
        {
            guna2Panel3.Controls.Clear();
            guna2Panel3.Height = 75;
            guna2Panel3.Padding = new Padding(10, 5, 10, 5);

            Label lblAppTitle = new Label
            {
                Text = "سيستمك",
                Font = new Font("Alexandria", 28F, FontStyle.Bold),
                ForeColor = Color.FromArgb(229, 231, 235),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            guna2Panel3.Controls.Add(lblAppTitle);
        }

        /// <summary>
        /// Setup the bottom logout button panel
        /// </summary>
        private void SetupLogoutPanel()
        {
            guna2Panel6.Controls.Clear();
            guna2Panel6.Height = 70;
            guna2Panel6.Padding = new Padding(15, 10, 15, 10);

            // Separator line
            Guna2Panel pnlSeparator = new Guna2Panel
            {
                Height = 2,
                Dock = DockStyle.Top,
                FillColor = Color.FromArgb(55, 65, 81),
                Location = new Point(15, 0)
            };

            // Logout button
            guna2Button9.BorderRadius = 12;
            guna2Button9.FillColor = Color.FromArgb(192, 0, 0);
            guna2Button9.Font = new Font("Alexandria", 13F, FontStyle.Bold);
            guna2Button9.ForeColor = Color.FromArgb(248, 255, 255);
            guna2Button9.Size = new Size(guna2Panel6.Width - 30, 45);
            guna2Button9.Text = "تسجيل الخروج";
            guna2Button9.TextAlign = HorizontalAlignment.Right;
            guna2Button9.Padding = new Padding(0, 0, 15, 0);
            guna2Button9.Image = Properties.Resources.logout;
            guna2Button9.ImageAlign = HorizontalAlignment.Right;
            guna2Button9.ImageOffset = new Point(15, 0);
            guna2Button9.ImageSize = new Size(28, 28);
            guna2Button9.HoverState.FillColor = Color.FromArgb(160, 0, 0);
            guna2Button9.PressedColor = Color.FromArgb(140, 0, 0);
            guna2Button9.Cursor = Cursors.Hand;
            guna2Button9.Location = new Point(15, 12);
            //guna2Button9.Anchor = AnchorStyle.Top | AnchorStyle.Left | AnchorStyle.Right;
            guna2Button9.Click += btn_logout_Click;

            guna2Panel6.Controls.Add(pnlSeparator);
            guna2Panel6.Controls.Add(guna2Button9);
        }

        /// <summary>
        /// Load user info from session and display it, then apply permissions
        /// </summary>
        private async void LoadUserInfoAndApplyPermissions()
        {
            try
            {
                // Set user name
                string displayName = string.IsNullOrEmpty(UserSession.Full_Name)
                    ? UserSession.UserName
                    : UserSession.Full_Name;

                lblUserName.Text = displayName ?? "مستخدم";

                // Load and set role name
                if (UserSession.Role_Id > 0 && string.IsNullOrEmpty(UserSession.RoleName))
                {
                    await UserSession.LoadRoleNameAsync(UserSession.Role_Id);
                }

                lblUserRole.Text = !string.IsNullOrEmpty(UserSession.RoleName)
                    ? UserSession.RoleName
                    : "مدير النظام";

                // Apply permissions to sidebar
                ApplyUserPermissions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user info: {ex.Message}");
                lblUserName.Text = UserSession.UserName ?? "مستخدم";
                lblUserRole.Text = "مدير النظام";
            }
        }

        #endregion

        #region ── User Permissions ───────────────────────────────────────────

        /// <summary>
        /// Apply user permissions to show/hide sidebar navigation items
        /// </summary>
        private void ApplyUserPermissions()
        {
            try
            {
                // If no permissions loaded or empty permissions table, show all (admin mode)
                if (UserSession.Permissions == null || UserSession.Permissions.Rows.Count == 0)
                {
                    // Admin or no restrictions - show everything
                    ShowAllSidebarItems();
                    return;
                }

                // ─── Check each sidebar item ───
                // Sales section
                bool canSales = UserSession.HasPermission("Sales", "CanViewSales") ||
                               UserSession.HasPermission("المبيعات", "CanViewSales");
                collapsibleMenuSection_Sales.Visible = canSales;

                // Products section
                bool canProducts = UserSession.HasPermission("Products", "CanViewProducts") ||
                                  UserSession.HasPermission("المنتجات", "CanViewProducts");
                collapsibleMenuSection_Products.Visible = canProducts;

                // Purchases
                bool canPurchases = UserSession.HasPermission("Purchases", "CanViewPurchases") ||
                                   UserSession.HasPermission("المشتريات", "CanViewPurchases");
                guna2Button7.Visible = canPurchases;

                // Suppliers
                bool canSuppliers = UserSession.HasPermission("Suppliers", "CanViewSuppliers") ||
                                   UserSession.HasPermission("الموردين", "CanViewSuppliers");
                btn_Supplier.Visible = canSuppliers;

                // Customers
                bool canCustomers = UserSession.HasPermission("Customers", "CanViewCustomers") ||
                                   UserSession.HasPermission("العملاء", "CanViewCustomers");
                btn_Customers.Visible = canCustomers;

                // Reports
                bool canReports = UserSession.HasPermission("Reports", "CanViewReports") ||
                                 UserSession.HasPermission("التقارير", "CanViewReports");
                guna2Button11.Visible = canReports;

                // Users (admin only)
                bool canUsers = UserSession.HasPermission("Users", "CanViewUsers") ||
                               UserSession.HasPermission("المستخدمين", "CanViewUsers");
                guna2Button12.Visible = canUsers;

                // Settings
                bool canSettings = UserSession.HasPermission("Settings", "CanViewSettings") ||
                                  UserSession.HasPermission("الاعدادات", "CanViewSettings");
                guna2Button13.Visible = canSettings;

                // Backup
                bool canBackup = UserSession.HasPermission("Backup", "CanViewBackup") ||
                                UserSession.HasPermission("النسخ الاحتياطي", "CanViewBackup");
                guna2Button14.Visible = canBackup;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying permissions: {ex.Message}");
                ShowAllSidebarItems();
            }
        }

        /// <summary>
        /// Show all sidebar items (default for admin or when no restrictions)
        /// </summary>
        private void ShowAllSidebarItems()
        {
            collapsibleMenuSection_Sales.Visible = true;
            collapsibleMenuSection_Products.Visible = true;
            guna2Button7.Visible = true;
            btn_Supplier.Visible = true;
            btn_Customers.Visible = true;
            guna2Button11.Visible = true;
            guna2Button12.Visible = true;
            guna2Button13.Visible = true;
            guna2Button14.Visible = true;
        }

        #endregion

        #region ── Dashboard: Widget Setup ────────────────────────────────────

        /// <summary>
        /// Wires the existing designer KPI labels to tracked fields,
        /// then builds the chart panel and the bottom Orders + Tables row.
        /// </summary>
        private void BuildDashboardWidgets()
        {
            // ─── Point tracked fields to existing designer labels ───
            // guna2Panel8 = مبيعات اليوم  (label6=value, label8=trend)
            lblTodaySalesValue = label6;
            lblTodaySalesTrend = label8;

            // guna2Panel9 = الطلبات  (label11=value, label9=trend)
            lblOrdersValue = label11;
            lblOrdersTrend = label9;

            // guna2Panel10 = الطاولات  (label15=value, label13=trend)
            lblTablesValue = label15;
            lblTablesTrend = label13;

            // ─── Apply consistent fonts to values (designer uses no explicit font) ───
            foreach (Label lbl in new[] { lblTodaySalesValue, lblOrdersValue, lblTablesValue })
            {
                lbl.Font = new Font("Alexandria", 26F, FontStyle.Bold);
                lbl.TextAlign = ContentAlignment.MiddleCenter;
            }

            // ─── Configure the chart with dark theme ───
            ConfigureSalesChart();

            // ─── Build the bottom row: Recent Orders | Tables Status ───
            BuildBottomRow();
        }

        /// <summary>
        /// Creates the bottom panel row directly inside guna2Panel1 (the content area).
        /// Left half = Recent Orders grid; Right half = Tables Status grid.
        /// </summary>
        private void BuildBottomRow()
        {
            // Container row panel placed below the chart (guna2Panel11 ends at y=285+529=814, add 20 gap)
            pnlBottomRow = new Guna.UI2.WinForms.Guna2Panel
            {
                Location = new Point(140, 830),
                Size = new Size(1092, 400),
                FillColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ─── Left card: Recent Orders ───
            var pnlOrders = BuildCardPanel("آخر الطلبات اليوم", 0, 0, 536, 400);
            dgvRecentOrders = BuildDashboardDgv();
            dgvRecentOrders.Location = new Point(10, 55);
            dgvRecentOrders.Size = new Size(516, 330);
            SetupOrdersColumns();
            pnlOrders.Controls.Add(dgvRecentOrders);

            // ─── Right card: Tables Status ───
            var pnlTablesCard = BuildCardPanel("حالة الطاولات", 556, 0, 536, 400);
            dgvTables = BuildDashboardDgv();
            dgvTables.Location = new Point(10, 55);
            dgvTables.Size = new Size(516, 330);
            SetupTablesColumns();
            pnlTablesCard.Controls.Add(dgvTables);

            pnlBottomRow.Controls.Add(pnlOrders);
            pnlBottomRow.Controls.Add(pnlTablesCard);
            guna2Panel1.Controls.Add(pnlBottomRow);
        }

        /// <summary>Creates a styled card panel matching the KPI cards.</summary>
        private Guna.UI2.WinForms.Guna2Panel BuildCardPanel(string title, int x, int y, int w, int h)
        {
            var card = new Guna.UI2.WinForms.Guna2Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                FillColor = Color.FromArgb(31, 41, 55),
                BorderColor = Color.FromArgb(55, 65, 81),
                BorderRadius = 20,
                BorderThickness = 3
            };

            var lbl = new Label
            {
                Text = title,
                Font = new Font("Alexandria", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(229, 231, 235),
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(w - 20, 42),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleRight,
                RightToLeft = RightToLeft.Yes
            };
            card.Controls.Add(lbl);
            return card;
        }

        /// <summary>Creates a Guna2DataGridView styled with Main_Methods dark-mode theme.</summary>
        private Guna.UI2.WinForms.Guna2DataGridView BuildDashboardDgv()
        {
            var dgv = new Guna.UI2.WinForms.Guna2DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.FromArgb(31, 41, 55),
                GridColor = Color.FromArgb(30, 41, 59),
                BorderStyle = BorderStyle.None,
                RightToLeft = RightToLeft.Yes,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersVisible = false,
                ScrollBars = ScrollBars.Vertical
            };
            Main_Methods.StyleDataGridView(dgv);
            return dgv;
        }

        private void SetupOrdersColumns()
        {
            dgvRecentOrders.Columns.Clear();
            AddColumn(dgvRecentOrders, "رقم الفاتورة", 0.15f);
            AddColumn(dgvRecentOrders, "الوقت", 0.12f);
            AddColumn(dgvRecentOrders, "العميل", 0.22f);
            AddColumn(dgvRecentOrders, "النوع", 0.15f);
            AddColumn(dgvRecentOrders, "الإجمالي", 0.18f);
            AddColumn(dgvRecentOrders, "الحالة", 0.18f);
        }

        private void SetupTablesColumns()
        {
            dgvTables.Columns.Clear();
            AddColumn(dgvTables, "رقم الطاولة", 0.20f);
            AddColumn(dgvTables, "الاسم", 0.22f);
            AddColumn(dgvTables, "القسم", 0.20f);
            AddColumn(dgvTables, "السعة", 0.13f);
            AddColumn(dgvTables, "الحالة", 0.25f);
        }

        private static void AddColumn(DataGridView dgv, string name, float fillWeight)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = name,
                FillWeight = fillWeight * 100,
                MinimumWidth = 50,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        #endregion

        #region ── Dashboard: Data Loading ────────────────────────────────────

        /// <summary>
        /// Loads all live dashboard data asynchronously and updates the UI.
        /// </summary>
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Run the four queries in parallel
                var summaryTask = DashboardService.GetTodaySummaryAsync();
                var tablesTask = DashboardService.GetTablesCountAsync();
                var weeklyTask = DashboardService.GetWeeklySalesAsync();
                var ordersTask = DashboardService.GetRecentOrdersAsync();
                var tablesGridTask = DashboardService.GetTablesStatusAsync();

                await Task.WhenAll(summaryTask, tablesTask, weeklyTask, ordersTask, tablesGridTask);

                if (IsDisposed) return;

                // Marshal to UI thread
                Invoke(() =>
                {
                    UpdateKpiCards(summaryTask.Result, tablesTask.Result);
                    UpdateSalesChart(weeklyTask.Result);
                    UpdateOrdersGrid(ordersTask.Result);
                    UpdateTablesGrid(tablesGridTask.Result);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadDashboardDataAsync: {ex.Message}");
            }
        }

        // ─── KPI Cards ───────────────────────────────────────────────────────

        private void UpdateKpiCards(
            (decimal TodaySales, int TodayOrders, decimal YesterdaySales, int YesterdayOrders) summary,
            (int Available, int Occupied, int Total) tables)
        {
            // مبيعات اليوم
            lblTodaySalesValue.Text = summary.TodaySales.ToString("N0");
            lblTodaySalesTrend.Text = BuildTrend(summary.TodaySales, summary.YesterdaySales, "عن أمس");
            lblTodaySalesTrend.ForeColor = TrendColor(summary.TodaySales, summary.YesterdaySales);

            // الطلبات
            lblOrdersValue.Text = summary.TodayOrders.ToString("N0");
            lblOrdersTrend.Text = BuildTrend(summary.TodayOrders, summary.YesterdayOrders, "عن أمس");
            lblOrdersTrend.ForeColor = TrendColor(summary.TodayOrders, summary.YesterdayOrders);

            // الطاولات
            lblTablesValue.Text = tables.Available.ToString();
            int tableDiff = tables.Available - (tables.Total - tables.Occupied - tables.Available);
            if (tables.Total > 0)
            {
                string tablesTrend = $"{tables.Available} متاحة من {tables.Total}";
                lblTablesTrend.Text = tablesTrend;
                lblTablesTrend.ForeColor = tables.Available > 0
                    ? Color.FromArgb(0, 217, 111)
                    : Color.Red;
            }
            else
            {
                lblTablesTrend.Text = "لا توجد طاولات";
                lblTablesTrend.ForeColor = CHART_TEXT_MUTED;
            }
        }

        private static string BuildTrend(decimal today, decimal yesterday, string suffix)
        {
            if (yesterday == 0)
                return today > 0 ? $"+100% {suffix}" : $"0% {suffix}";

            decimal pct = Math.Round((today - yesterday) / yesterday * 100, 1);
            string sign = pct >= 0 ? "+" : "";
            return $"{sign}{pct}% {suffix}";
        }

        private static string BuildTrend(int today, int yesterday, string suffix)
            => BuildTrend((decimal)today, (decimal)yesterday, suffix);

        private static Color TrendColor(decimal today, decimal yesterday)
            => today >= yesterday ? Color.FromArgb(0, 217, 111) : Color.Red;

        private static Color TrendColor(int today, int yesterday)
            => TrendColor((decimal)today, (decimal)yesterday);

        // ─── Chart ───────────────────────────────────────────────────────────

        private void UpdateSalesChart(DataTable dt)
        {
            salesChart.Datasets.Clear();

            var salesDs = new Guna.Charts.WinForms.GunaSplineDataset
            {
                Label = "المبيعات",
                BorderColor = CHART_ACCENT_BLUE,
                BorderWidth = 3,
                PointRadius = 5,
                FillColor = CHART_FILL_BLUE
            };

            var ordersDs = new Guna.Charts.WinForms.GunaSplineDataset
            {
                Label = "الطلبات",
                BorderColor = CHART_ACCENT_PURPLE,
                BorderWidth = 2,
                PointRadius = 4,
                FillColor = Color.Transparent
            };

            // Build a map of the last 7 days so missing days show as 0
            var dayMap = new System.Collections.Generic.Dictionary<DateTime, (decimal Sales, int Orders)>();
            for (int i = 6; i >= 0; i--)
                dayMap[DateTime.Today.AddDays(-i)] = (0, 0);

            foreach (DataRow row in dt.Rows)
            {
                var day = Convert.ToDateTime(row["OrderDay"]).Date;
                if (dayMap.ContainsKey(day))
                    dayMap[day] = (Convert.ToDecimal(row["TotalSales"]), Convert.ToInt32(row["TotalOrders"]));
            }

            foreach (var kvp in dayMap)
            {
                string label = FormatDayLabel(kvp.Key);
                salesDs.DataPoints.Add(label, (double)kvp.Value.Sales);
                ordersDs.DataPoints.Add(label, kvp.Value.Orders);
            }

            salesChart.Datasets.Add(salesDs);
            salesChart.Datasets.Add(ordersDs);
            salesChart.Update();
        }

        private static string FormatDayLabel(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => "السبت",
                DayOfWeek.Sunday => "الاحد",
                DayOfWeek.Monday => "الاثنين",
                DayOfWeek.Tuesday => "الثلاثاء",
                DayOfWeek.Wednesday => "الاربعاء",
                DayOfWeek.Thursday => "الخميس",
                DayOfWeek.Friday => "الجمعة",
                _ => date.ToString("dd/MM")
            };
        }

        // ─── Orders Grid ─────────────────────────────────────────────────────

        private void UpdateOrdersGrid(DataTable dt)
        {
            dgvRecentOrders.Rows.Clear();

            string[] columns = { "رقم الفاتورة", "الوقت", "العميل", "النوع", "الإجمالي", "الحالة" };

            foreach (DataRow row in dt.Rows)
            {
                int idx = dgvRecentOrders.Rows.Add(
                    row["رقم الفاتورة"],
                    row["الوقت"],
                    row["العميل"],
                    row["النوع"],
                    Convert.ToDecimal(row["الإجمالي"]).ToString("N2"),
                    row["الحالة"]
                );

                // Color-code status cell
                int statusCode = row["StatusCode"] != DBNull.Value ? Convert.ToInt32(row["StatusCode"]) : -1;
                var statusCell = dgvRecentOrders.Rows[idx].Cells["الحالة"];
                statusCell.Style.ForeColor = StatusCodeToColor(statusCode);
                statusCell.Style.Font = new Font("Alexandria", 10F, FontStyle.Bold);
            }
        }

        private static Color StatusCodeToColor(int code) => code switch
        {
            0 => Color.FromArgb(99, 132, 255),    // جديد — blue
            1 => Color.FromArgb(255, 193, 7),     // قيد التحضير — amber
            2 => Color.FromArgb(72, 219, 163),    // جاهز — teal
            3 => Color.FromArgb(0, 217, 111),     // مُسلَّم — green
            4 => Color.Red,                        // ملغي
            5 => Color.FromArgb(167, 106, 255),   // مرتجع — purple
            _ => Color.FromArgb(229, 231, 235)
        };

        // ─── Tables Grid ─────────────────────────────────────────────────────

        private void UpdateTablesGrid(DataTable dt)
        {
            dgvTables.Rows.Clear();

            foreach (DataRow row in dt.Rows)
            {
                int idx = dgvTables.Rows.Add(
                    row["رقم الطاولة"],
                    row["الاسم"],
                    row["القسم"],
                    row["السعة"],
                    row["الحالة"]
                );

                string rawStatus = row["StatusRaw"]?.ToString()?.ToLower() ?? "";
                var statusCell = dgvTables.Rows[idx].Cells["الحالة"];
                statusCell.Style.ForeColor = TableStatusToColor(rawStatus);
                statusCell.Style.Font = new Font("Alexandria", 10F, FontStyle.Bold);
            }
        }

        private static Color TableStatusToColor(string status) => status switch
        {
            "available" => Color.FromArgb(0, 217, 111),
            "occupied" => Color.Red,
            "reserved" => Color.FromArgb(255, 193, 7),
            "cleaning" => Color.FromArgb(99, 132, 255),
            "maintenance" => Color.FromArgb(167, 106, 255),
            _ => Color.FromArgb(229, 231, 235)
        };

        #endregion

        #region ── Sales Chart Configuration ──────────────────────────────────

        /// <summary>
        /// Applies dark-mode theme to the chart axes, fonts, and background.
        /// Data is loaded separately via UpdateSalesChart().
        /// </summary>
        private void ConfigureSalesChart()
        {
            salesChart.BackColor = CHART_BACKGROUND;

            salesChart.YAxes.GridLines.Color = CHART_GRID;
            salesChart.YAxes.GridLines.Display = true;
            salesChart.YAxes.Ticks.Font.FontName = "Segoe UI";
            salesChart.YAxes.Ticks.Font.Size = 11;

            salesChart.XAxes.GridLines.Display = false;
            salesChart.XAxes.Ticks.Font.FontName = "Segoe UI";
            salesChart.XAxes.Ticks.Font.Size = 11;

            salesChart.Legend.LabelFont.FontName = "Segoe UI";
            salesChart.Legend.LabelFont.Size = 12;

            salesChart.Title.Font.FontName = "Segoe UI";
            salesChart.Title.Font.Size = 14;
            salesChart.Title.Font.Style = ChartFontStyle.Bold;
            salesChart.Title.ForeColor = CHART_TEXT;

            salesChart.Tooltips.TitleFont.FontName = "Segoe UI";
            salesChart.Tooltips.TitleFont.Size = 11;
            salesChart.Tooltips.TitleFont.Style = ChartFontStyle.Bold;
            salesChart.Tooltips.BodyFont.FontName = "Segoe UI";
            salesChart.Tooltips.BodyFont.Size = 10;
            salesChart.Tooltips.BodyForeColor = CHART_TEXT;
        }

        #endregion

        /// <summary>
        /// Initializes the collapsible sidebar navigation system.
        /// Configures collapsible menu sections and sidebar buttons.
        /// </summary>
        private void SetupSidebarNavigation()
        {
            // ─── Configure Collapsible Sections ───
            SetupCollapsibleSection(collapsibleMenuSection_Sales, "المبيعات", Properties.Resources.market_analysis,
                new[] { "المبيعات", "المرتجع", "الطيارين", "الطاولات", "الطلبات" },
                Coll_Sales_ChildItemClicked);

            SetupCollapsibleSection(collapsibleMenuSection_Products, "المنتجات", Properties.Resources.invoice,
                new[] { "المنتجات", "الاقسام", "الاحجام", "الاضافات" },
                Coll_Products_ChildItemClicked);

            // ─── Configure Single Buttons ───
            ConfigureSidebarButton(guna2Button7, "المشتريات", guna2Button7_Click);
            ConfigureSidebarButton(btn_Supplier, "الموردين", btn_Supplier_Click);
            ConfigureSidebarButton(btn_Customers, "العملاء", btn_Customers_Click);
            ConfigureSidebarButton(guna2Button11, "التقارير", guna2Button11_Click);
            ConfigureSidebarButton(guna2Button12, "المستخدمين", guna2Button12_Click);
            ConfigureSidebarButton(guna2Button13, "الاعدادات", guna2Button13_Click);
            ConfigureSidebarButton(guna2Button14, "النسخ الاحتياطي", guna2Button14_Click);
        }

        /// <summary>
        /// Sets up a collapsible menu section with title, icon, child items, and click handler.
        /// </summary>
        private void SetupCollapsibleSection(CollapsibleMenuSection section, string title, System.Drawing.Image? icon,
            string[] childItems, EventHandler<ChildItemClickEventArgs> clickHandler)
        {
            section.Title = title;
            section.HeaderIcon = icon;
            section.SetChildItems(childItems);
            section.ChildItemClicked += clickHandler;
        }

        /// <summary>
        /// Configures a standard sidebar button with consistent styling.
        /// </summary>
        private void ConfigureSidebarButton(Guna2Button btn, string text, EventHandler clickHandler)
        {
            btn.Text = text;
            btn.TextAlign = HorizontalAlignment.Right;
            btn.Padding = new Padding(0, 0, 20, 0);
            btn.ForeColor = Color.FromArgb(229, 231, 235);
            btn.Click += clickHandler;
        }


        #region ── Collapsible Section Event Handlers ─────────────────────────

        private void Coll_Sales_ChildItemClicked(object sender, ChildItemClickEventArgs e)
        {
            switch (e.ItemName)
            {
                case "المبيعات":
                    Main_Methods.OpenForm<frmSales>();
                    break;
                case "المرتجع":
                    Main_Methods.OpenForm<frmReturns>();
                    break;
                case "الطيارين":
                    Main_Methods.OpenForm<frmDeliveryStaff>();
                    break;
                case "الطاولات":
                    Main_Methods.OpenForm<frmTables>();
                    break;
                case "الطلبات":
                    Main_Methods.OpenForm<frmHold_Invoices>();
                    break;
            }
        }

        private void Coll_Products_ChildItemClicked(object sender, ChildItemClickEventArgs e)
        {
            switch (e.ItemName)
            {
                case "المنتجات":
                    Main_Methods.OpenForm<frmProducts>();
                    break;
                case "الاقسام":
                    Main_Methods.OpenForm<frmProductCategories>();
                    break;
                case "الاحجام":
                    Main_Methods.OpenForm<frmProductSizes>();
                    break;
                case "الاضافات":
                    Main_Methods.OpenForm<frmProductAddons>();
                    break;
            }
        }

        #endregion

        #region ── Sidebar Button Event Handlers ──────────────────────────────

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            // Purchases
            ToastManager.ShowInfo("المشتريات", "سيتم فتح شاشة المشتريات");
        }

        private void btn_Supplier_Click(object sender, EventArgs e)
        {
            btn_Supplier.Checked = true;
            Main_Methods.OpenForm<frmSuppliers>(onClosed: () => btn_Supplier.Checked = false);
        }

        private void btn_Customers_Click(object sender, EventArgs e)
        {
            btn_Customers.Checked = true;
            Main_Methods.OpenForm<frmCustomers>(onClosed: () => btn_Customers.Checked = false);
        }

        private void guna2Button11_Click(object sender, EventArgs e)
        {
            guna2Button11.Checked = true;
            Main_Methods.OpenForm<frmSalesReports>(onClosed: () => guna2Button11.Checked = false);
        }

        private void guna2Button12_Click(object sender, EventArgs e)
        {
            // Users
            guna2Button12.Checked = true;
            Main_Methods.OpenForm<frmUsers>(onClosed: () => guna2Button12.Checked = false);
        }

        private void guna2Button13_Click(object sender, EventArgs e)
        {
            // Settings
            guna2Button13.Checked = true;
            Main_Methods.OpenForm<frmSettings>(onClosed: () => guna2Button13.Checked = false);
        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            // Backup
            guna2Button14.Checked = true;
            Main_Methods.OpenForm<frmBackup>(onClosed: () => guna2Button14.Checked = false);
        }

        #endregion

        #region ── Event Handlers: Navigation ─────────────────────────────────

        /// <summary>
        /// Handle logout button click with confirmation dialog
        /// </summary>
        private void btn_logout_Click(object sender, EventArgs e)
        {
            bool confirmed = frmConfirm.Show("تسجيل الخروج", "هل أنت متأكد من تسجيل الخروج؟");
            if (!confirmed) return;

            // User confirmed - proceed with logout
            PerformLogout();
        }

        /// <summary>
        /// Perform the actual logout process
        /// </summary>
        private void PerformLogout()
        {
            try
            {
                _isLoggingOut = true;
                this.Enabled = false;
                this.Opacity = 0.5;

                // 1) مسح بيانات الجلسة — لازم يحصل الأول علشان handler الـ FormClosed بتاع
                //    Login يعرف إنه ميقفلش نفسه
                UserSession.Logout();

                // 2) إغلاق التنبيهات المفتوحة
                ToastManager.CloseAll();

                // 3) إيجاد فورم Login الموجودة (هي ApplicationContext الرئيسي ومخفية فقط)
                Login loginForm = Application.OpenForms.OfType<Login>().FirstOrDefault();

                if (loginForm == null)
                {
                    // حالة استثنائية — مفيش Login مفتوح: نعمل restart للأمان
                    Application.Restart();
                    return;
                }

                // 4) تفريغ خانات الـ Login
                var txtUser = loginForm.Controls.Find("txt_username", true).FirstOrDefault() as Guna.UI2.WinForms.Guna2TextBox;
                var txtPass = loginForm.Controls.Find("txt_password", true).FirstOrDefault() as Guna.UI2.WinForms.Guna2TextBox;
                if (txtUser != null) txtUser.Clear();
                if (txtPass != null) txtPass.Clear();

                // 5) إغلاق كل الفورمز الفرعية المفتوحة (ما عدا Login و Mainform نفسها)
                //    نعمل snapshot قبل اللوب لتجنب Collection modified
                var formsToClose = Application.OpenForms.Cast<Form>()
                    .Where(f => f != loginForm && f != this)
                    .ToList();

                foreach (Form form in formsToClose)
                {
                    try
                    {
                        form.Hide();
                        form.Close();
                    }
                    catch (Exception exClose)
                    {
                        Console.WriteLine($"Error closing form {form?.Name}: {exClose.Message}");
                    }
                }

                // 6) إظهار Login وتفعيلها
                loginForm.Show();
                loginForm.WindowState = FormWindowState.Normal;
                loginForm.BringToFront();
                loginForm.Activate();
                if (txtUser != null) txtUser.Focus();

                // 7) إشعار بعد ما الواجهة جاهزة
                ToastManager.ShowInfo("تم تسجيل الخروج", "تم تسجيل خروجك بنجاح");

                // 8) إغلاق Mainform نفسها في الآخر — handler الـ FormClosed بتاع Login
                //    هيلاقي UserSession.UserId == 0 فمش هيقفلها
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                ToastManager.ShowError("خطأ", "حدث خطأ أثناء تسجيل الخروج");
                _isLoggingOut = false;
                this.Enabled = true;
                this.Opacity = 1.0;
            }
        }

        #endregion

        #region ── Test / Debug Buttons ────────────────────────────────────────

        private async void button3_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                string q = "how are you";
                string result = await GeminiAI.GenerateText(q);
                ToastManager.ShowInfo("رد Gemini AI", result);
            }
            catch (ArgumentException ex)
            {
                ToastManager.ShowWarning("خطأ", $"خطأ في البيانات المدخلة: {ex.Message}");
            }
            catch (TimeoutException)
            {
                ToastManager.ShowWarning("انتهت المهلة", "انتهت مهلة الاتصال بالخادم. يرجى المحاولة مرة أخرى.");
            }
            catch (HttpRequestException ex)
            {
                ToastManager.ShowError("خطأ في الاتصال", $"خطأ في الاتصال بالإنترنت: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                ToastManager.ShowError("خطأ", $"خطأ في معالجة البيانات: {ex.Message}");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", $"حدث خطأ غير متوقع: {ex.Message}");
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btn_Menu4_Click(object sender, EventArgs e)
        {
            ToastManager.ShowInfo("New Request", "Table 4 requested the bill.");
        }

        private void btn_Menu5_Click(object sender, EventArgs e)
        {
            ToastManager.ShowSuccess("Order Sent", "Order #1042 sent to kitchen.");
        }

        private void btn_Menu6_Click(object sender, EventArgs e)
        {
            NotificationHelper.NotifyLowStock("دجاج", 5);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ToastManager.ShowError("Payment Failed", "Card declined for Table 8.");
        }

        #endregion

        private void Mainform_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
