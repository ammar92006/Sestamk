namespace Sestamk.Forms
{
    partial class Mainform
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pnl_Sidebar = new Guna.UI2.WinForms.Guna2Panel();
            btn_Users = new ReaLTaiizor.Controls.Button();
            btn_Logout = new ReaLTaiizor.Controls.Button();
            btn_Menu6 = new ReaLTaiizor.Controls.Button();
            btn_Menu5 = new ReaLTaiizor.Controls.Button();
            btn_Menu4 = new ReaLTaiizor.Controls.Button();
            btn_Menu3 = new ReaLTaiizor.Controls.Button();
            btn_Suppliers = new ReaLTaiizor.Controls.Button();
            btn_Customers = new ReaLTaiizor.Controls.Button();
            lbl_AppTitle = new Guna.UI2.WinForms.Guna2HtmlLabel();
            pic_Logo = new Guna.UI2.WinForms.Guna2PictureBox();
            pnl_Main = new Guna.UI2.WinForms.Guna2Panel();
            pnl_Sidebar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pic_Logo).BeginInit();
            SuspendLayout();
            // 
            // pnl_Sidebar
            // 
            pnl_Sidebar.Controls.Add(btn_Users);
            pnl_Sidebar.Controls.Add(btn_Logout);
            pnl_Sidebar.Controls.Add(btn_Menu6);
            pnl_Sidebar.Controls.Add(btn_Menu5);
            pnl_Sidebar.Controls.Add(btn_Menu4);
            pnl_Sidebar.Controls.Add(btn_Menu3);
            pnl_Sidebar.Controls.Add(btn_Suppliers);
            pnl_Sidebar.Controls.Add(btn_Customers);
            pnl_Sidebar.Controls.Add(lbl_AppTitle);
            pnl_Sidebar.Controls.Add(pic_Logo);
            pnl_Sidebar.CustomizableEdges = customizableEdges3;
            pnl_Sidebar.Dock = DockStyle.Left;
            pnl_Sidebar.Location = new Point(0, 0);
            pnl_Sidebar.Name = "pnl_Sidebar";
            pnl_Sidebar.ShadowDecoration.CustomizableEdges = customizableEdges4;
            pnl_Sidebar.Size = new Size(273, 900);
            pnl_Sidebar.TabIndex = 0;
            // 
            // btn_Users
            // 
            btn_Users.BackColor = Color.Transparent;
            btn_Users.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Users.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Users.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Users.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Users.Image = Properties.Resources.house;
            btn_Users.ImageAlign = ContentAlignment.MiddleRight;
            btn_Users.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Users.Location = new Point(3, 795);
            btn_Users.Name = "btn_Users";
            btn_Users.Padding = new Padding(0, 0, 15, 0);
            btn_Users.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Users.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Users.Size = new Size(267, 40);
            btn_Users.TabIndex = 9;
            btn_Users.Text = "المستخدمين";
            btn_Users.TextAlignment = StringAlignment.Center;
            // 
            // btn_Logout
            // 
            btn_Logout.BackColor = Color.Transparent;
            btn_Logout.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Logout.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Logout.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Logout.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Logout.Image = Properties.Resources.house;
            btn_Logout.ImageAlign = ContentAlignment.MiddleRight;
            btn_Logout.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Logout.Location = new Point(3, 851);
            btn_Logout.Name = "btn_Logout";
            btn_Logout.Padding = new Padding(0, 0, 15, 0);
            btn_Logout.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Logout.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Logout.Size = new Size(267, 40);
            btn_Logout.TabIndex = 8;
            btn_Logout.Text = "تسجيل الخروج";
            btn_Logout.TextAlignment = StringAlignment.Center;
            btn_Logout.Click += btn_logout_Click;
            // 
            // btn_Menu6
            // 
            btn_Menu6.BackColor = Color.Transparent;
            btn_Menu6.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Menu6.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Menu6.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Menu6.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Menu6.Image = Properties.Resources.house;
            btn_Menu6.ImageAlign = ContentAlignment.MiddleRight;
            btn_Menu6.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Menu6.Location = new Point(3, 421);
            btn_Menu6.Name = "btn_Menu6";
            btn_Menu6.Padding = new Padding(0, 0, 15, 0);
            btn_Menu6.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Menu6.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Menu6.Size = new Size(267, 40);
            btn_Menu6.TabIndex = 7;
            btn_Menu6.Text = "المستخدمين";
            btn_Menu6.TextAlignment = StringAlignment.Center;
            // 
            // btn_Menu5
            // 
            btn_Menu5.BackColor = Color.Transparent;
            btn_Menu5.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Menu5.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Menu5.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Menu5.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Menu5.Image = Properties.Resources.house;
            btn_Menu5.ImageAlign = ContentAlignment.MiddleRight;
            btn_Menu5.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Menu5.Location = new Point(3, 370);
            btn_Menu5.Name = "btn_Menu5";
            btn_Menu5.Padding = new Padding(0, 0, 15, 0);
            btn_Menu5.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Menu5.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Menu5.Size = new Size(267, 40);
            btn_Menu5.TabIndex = 6;
            btn_Menu5.Text = "المستخدمين";
            btn_Menu5.TextAlignment = StringAlignment.Center;
            // 
            // btn_Menu4
            // 
            btn_Menu4.BackColor = Color.Transparent;
            btn_Menu4.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Menu4.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Menu4.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Menu4.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Menu4.Image = Properties.Resources.house;
            btn_Menu4.ImageAlign = ContentAlignment.MiddleRight;
            btn_Menu4.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Menu4.Location = new Point(3, 319);
            btn_Menu4.Name = "btn_Menu4";
            btn_Menu4.Padding = new Padding(0, 0, 15, 0);
            btn_Menu4.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Menu4.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Menu4.Size = new Size(267, 40);
            btn_Menu4.TabIndex = 5;
            btn_Menu4.Text = "المستخدمين";
            btn_Menu4.TextAlignment = StringAlignment.Center;
            btn_Menu4.Click += btn_Menu4_Click;
            // 
            // btn_Menu3
            // 
            btn_Menu3.BackColor = Color.Transparent;
            btn_Menu3.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Menu3.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Menu3.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Menu3.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Menu3.Image = Properties.Resources.house;
            btn_Menu3.ImageAlign = ContentAlignment.MiddleRight;
            btn_Menu3.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Menu3.Location = new Point(3, 268);
            btn_Menu3.Name = "btn_Menu3";
            btn_Menu3.Padding = new Padding(0, 0, 15, 0);
            btn_Menu3.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Menu3.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Menu3.Size = new Size(267, 40);
            btn_Menu3.TabIndex = 4;
            btn_Menu3.Text = "المستخدمين";
            btn_Menu3.TextAlignment = StringAlignment.Center;
            btn_Menu3.Click += button3_Click;
            // 
            // btn_Suppliers
            // 
            btn_Suppliers.BackColor = Color.Transparent;
            btn_Suppliers.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Suppliers.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Suppliers.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Suppliers.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Suppliers.Image = Properties.Resources.house;
            btn_Suppliers.ImageAlign = ContentAlignment.MiddleRight;
            btn_Suppliers.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Suppliers.Location = new Point(3, 217);
            btn_Suppliers.Name = "btn_Suppliers";
            btn_Suppliers.Padding = new Padding(0, 0, 15, 0);
            btn_Suppliers.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Suppliers.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Suppliers.Size = new Size(267, 40);
            btn_Suppliers.TabIndex = 3;
            btn_Suppliers.Text = "الموردين";
            btn_Suppliers.TextAlignment = StringAlignment.Center;
            btn_Suppliers.Click += btn_Suppliers_Click;
            // 
            // btn_Customers
            // 
            btn_Customers.BackColor = Color.Transparent;
            btn_Customers.BorderColor = Color.FromArgb(32, 34, 37);
            btn_Customers.EnteredBorderColor = Color.FromArgb(165, 37, 37);
            btn_Customers.EnteredColor = Color.FromArgb(55, 53, 62);
            btn_Customers.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Customers.Image = Properties.Resources.house;
            btn_Customers.ImageAlign = ContentAlignment.MiddleRight;
            btn_Customers.InactiveColor = Color.FromArgb(32, 34, 37);
            btn_Customers.Location = new Point(3, 166);
            btn_Customers.Name = "btn_Customers";
            btn_Customers.Padding = new Padding(0, 0, 15, 0);
            btn_Customers.PressedBorderColor = Color.FromArgb(113, 90, 90);
            btn_Customers.PressedColor = Color.FromArgb(113, 90, 90);
            btn_Customers.Size = new Size(267, 40);
            btn_Customers.TabIndex = 2;
            btn_Customers.Text = "العملاء";
            btn_Customers.TextAlignment = StringAlignment.Center;
            btn_Customers.Click += btn_Customers_Click;
            // 
            // lbl_AppTitle
            // 
            lbl_AppTitle.BackColor = Color.Transparent;
            lbl_AppTitle.Font = new Font("Alexandria", 20F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_AppTitle.ForeColor = Color.White;
            lbl_AppTitle.Location = new Point(62, 10);
            lbl_AppTitle.Name = "lbl_AppTitle";
            lbl_AppTitle.Size = new Size(133, 48);
            lbl_AppTitle.TabIndex = 1;
            lbl_AppTitle.Text = "سيستمك";
            lbl_AppTitle.TextAlignment = ContentAlignment.TopCenter;
            // 
            // pic_Logo
            // 
            pic_Logo.CustomizableEdges = customizableEdges1;
            pic_Logo.FillColor = Color.Transparent;
            pic_Logo.Image = Properties.Resources.Logo_Min_1_;
            pic_Logo.ImageRotate = 0F;
            pic_Logo.Location = new Point(217, 8);
            pic_Logo.Name = "pic_Logo";
            pic_Logo.ShadowDecoration.CustomizableEdges = customizableEdges2;
            pic_Logo.Size = new Size(50, 50);
            pic_Logo.SizeMode = PictureBoxSizeMode.Zoom;
            pic_Logo.TabIndex = 0;
            pic_Logo.TabStop = false;
            // 
            // pnl_Main
            // 
            pnl_Main.BackColor = Color.FromArgb(18, 26, 33);
            pnl_Main.CustomizableEdges = customizableEdges5;
            pnl_Main.Dock = DockStyle.Fill;
            pnl_Main.Location = new Point(273, 0);
            pnl_Main.Name = "pnl_Main";
            pnl_Main.ShadowDecoration.CustomizableEdges = customizableEdges6;
            pnl_Main.Size = new Size(1327, 900);
            pnl_Main.TabIndex = 1;
            // 
            // Mainform
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(16, 22, 27);
            ClientSize = new Size(1600, 900);
            Controls.Add(pnl_Main);
            Controls.Add(pnl_Sidebar);
            Font = new Font("Alexandria", 17.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Mainform";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            FormClosed += Mainform_FormClosed;
            pnl_Sidebar.ResumeLayout(false);
            pnl_Sidebar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pic_Logo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel pnl_Sidebar;
        private Guna.UI2.WinForms.Guna2PictureBox pic_Logo;
        private Guna.UI2.WinForms.Guna2HtmlLabel lbl_AppTitle;
        private ReaLTaiizor.Controls.Button btn_Customers;
        private ReaLTaiizor.Controls.Button btn_Suppliers;
        private ReaLTaiizor.Controls.Button btn_Menu3;
        private ReaLTaiizor.Controls.Button btn_Menu4;
        private ReaLTaiizor.Controls.Button btn_Menu5;
        private ReaLTaiizor.Controls.Button btn_Menu6;
        private ReaLTaiizor.Controls.Button btn_Logout;
        private ReaLTaiizor.Controls.Button btn_Users;
        private Guna.UI2.WinForms.Guna2Panel pnl_Main;
    }
}