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

        #endregion

        #region ── Constructor ────────────────────────────────────────────────

        public Mainform()
        {
            InitializeComponent();

            // Load user UserControl dashboard
            UC_Dashboard dashboard = new UC_Dashboard();
            dashboard.Dock = DockStyle.Fill;

            // Setup user profile in sidebar
            SetupUserProfile();

            // Setup the sales chart with dark mode theme
            ConfigureSalesChart();

            // Setup the collapsible sidebar navigation
            SetupSidebarNavigation();

            // Load user info and apply permissions
            LoadUserInfoAndApplyPermissions();
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

        #region ── Sales Chart Configuration ──────────────────────────────────

        /// <summary>
        /// Configures the sales chart with a professional dark mode theme.
        /// Sets up colors, axes, grid lines, tooltips, and sample data.
        /// </summary>
        private void ConfigureSalesChart()
        {
            // Clear previous data to prevent duplicate rendering on re-init
            salesChart.Datasets.Clear();

            // ─── Background ───
            salesChart.BackColor = CHART_BACKGROUND;

            // ─── Y-Axis Configuration ───
            salesChart.YAxes.GridLines.Color = CHART_GRID;
            salesChart.YAxes.GridLines.Display = true;
            salesChart.YAxes.Ticks.Font.FontName = "Segoe UI";
            salesChart.YAxes.Ticks.Font.Size = 11;

            // ─── X-Axis Configuration ───
            salesChart.XAxes.GridLines.Display = false;
            salesChart.XAxes.Ticks.Font.FontName = "Segoe UI";
            salesChart.XAxes.Ticks.Font.Size = 11;

            // ─── Legend ───
            salesChart.Legend.LabelFont.FontName = "Segoe UI";
            salesChart.Legend.LabelFont.Size = 12;

            // ─── Title ───
            salesChart.Title.Font.FontName = "Segoe UI";
            salesChart.Title.Font.Size = 14;
            salesChart.Title.Font.Style = ChartFontStyle.Bold;
            salesChart.Title.ForeColor = CHART_TEXT;

            // ─── Tooltips ───
            salesChart.Tooltips.TitleFont.FontName = "Segoe UI";
            salesChart.Tooltips.TitleFont.Size = 11;
            salesChart.Tooltips.TitleFont.Style = ChartFontStyle.Bold;
            salesChart.Tooltips.BodyFont.FontName = "Segoe UI";
            salesChart.Tooltips.BodyFont.Size = 10;
            salesChart.Tooltips.BodyForeColor = CHART_TEXT;

            // ─── Primary Dataset: Sales Line ───
            var salesDataset = new GunaSplineDataset();
            salesDataset.Label = "المبيعات";
            salesDataset.BorderColor = CHART_ACCENT_BLUE;
            salesDataset.BorderWidth = 3;
            salesDataset.PointRadius = 5;
            salesDataset.FillColor = CHART_FILL_BLUE;

            // Sample weekly sales data
            salesDataset.DataPoints.Add("السبت", 420);
            salesDataset.DataPoints.Add("الاحد", 520);
            salesDataset.DataPoints.Add("الاثنين", 480);
            salesDataset.DataPoints.Add("الثلاثاء", 580);
            salesDataset.DataPoints.Add("الاربعاء", 450);
            salesDataset.DataPoints.Add("الخميس", 700);
            salesDataset.DataPoints.Add("الجمعة", 900);

            salesChart.Datasets.Add(salesDataset);

            // ─── Secondary Dataset: Orders Line ───
            var ordersDataset = new GunaSplineDataset();
            ordersDataset.Label = "الطلبات";
            ordersDataset.BorderColor = CHART_ACCENT_PURPLE;
            ordersDataset.BorderWidth = 2;
            ordersDataset.PointRadius = 4;
            ordersDataset.FillColor = Color.Transparent;

            ordersDataset.DataPoints.Add("السبت", 32);
            ordersDataset.DataPoints.Add("الاحد", 38);
            ordersDataset.DataPoints.Add("الاثنين", 35);
            ordersDataset.DataPoints.Add("الثلاثاء", 42);
            ordersDataset.DataPoints.Add("الاربعاء", 30);
            ordersDataset.DataPoints.Add("الخميس", 55);
            ordersDataset.DataPoints.Add("الجمعة", 68);

            salesChart.Datasets.Add(ordersDataset);

            // Force the chart to re-render
            salesChart.Update();
        }

        #endregion

        #region ── Sidebar Navigation Setup ───────────────────────────────────

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

        #endregion

        #region ── Collapsible Section Event Handlers ─────────────────────────

        private void Coll_Sales_ChildItemClicked(object sender, ChildItemClickEventArgs e)
        {
            switch (e.ItemName)
            {
                case "المبيعات":
                    Main_Methods.OpenForm<frmSales>();
                    break;
                case "المرتجع":
                    ToastManager.ShowInfo("قريباً", "هذه الميزة ستتوفر في التحديث القادم");
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
            // Reports
            ToastManager.ShowInfo("التقارير", "سيتم فتح شاشة التقارير");
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
            // Show confirmation dialog
            DialogResult result = MessageBox.Show(
                "هل أنت متأكد من تسجيل الخروج؟",
                "تسجيل الخروج",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

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

                // Clear user session
                UserSession.Logout();

                // Show toast notification
                ToastManager.ShowInfo("تم تسجيل الخروج", "تم تسجيل خروجك بنجاح");

                // إغلاق جميع التنبيهات المفتوحة فوراً لتجنب الأخطاء
                ToastManager.CloseAll();

                // Find the existing login form (since it's the main application context)
                Login loginForm = Application.OpenForms.OfType<Login>().FirstOrDefault();
                if (loginForm != null)
                {
                    // Attempt to clear text boxes
                    var txtUser = loginForm.Controls.Find("txt_username", true).FirstOrDefault() as Guna.UI2.WinForms.Guna2TextBox;
                    var txtPass = loginForm.Controls.Find("txt_password", true).FirstOrDefault() as Guna.UI2.WinForms.Guna2TextBox;
                    
                    if (txtUser != null) txtUser.Clear();
                    if (txtPass != null) txtPass.Clear();

                    loginForm.Show();
                    if (txtUser != null) txtUser.Focus();
                }
                else
                {
                    loginForm = new Login();
                    loginForm.Show();
                }

                // Close all other forms except the login form
                var formsToClose = Application.OpenForms.Cast<Form>()
                    .Where(f => f != loginForm)
                    .ToList();

                foreach (Form form in formsToClose)
                {
                    form.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                ToastManager.ShowError("خطأ", "حدث خطأ أثناء تسجيل الخروج");
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
            ToastManager.ShowWarning("Low Stock", "Chicken is running low.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ToastManager.ShowError("Payment Failed", "Card declined for Table 8.");
        }

        #endregion
    }
}
