namespace Sestamk.Forms
{
    partial class frmSalesReports
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing) { if (disposing && (components != null)) components.Dispose(); base.Dispose(disposing); }

        // ═══ Header ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlHeader;
        internal System.Windows.Forms.Label lblTitle;
        internal Guna.UI2.WinForms.Guna2Button btnPrint;
        internal Guna.UI2.WinForms.Guna2Button btnExport;
        internal Guna.UI2.WinForms.Guna2ControlBox cbClose;
        internal Guna.UI2.WinForms.Guna2ControlBox cbMax;
        internal Guna.UI2.WinForms.Guna2ControlBox cbMin;
        // ═══ Filters ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlFilters;
        internal System.Windows.Forms.FlowLayoutPanel flowFilters;
        internal System.Windows.Forms.Label lblFrom;
        internal System.Windows.Forms.Label lblTo;
        internal Guna.UI2.WinForms.Guna2DateTimePicker dtpFrom;
        internal Guna.UI2.WinForms.Guna2DateTimePicker dtpTo;
        internal Guna.UI2.WinForms.Guna2Button btnToday;
        internal Guna.UI2.WinForms.Guna2Button btnYesterday;
        internal Guna.UI2.WinForms.Guna2Button btnWeek;
        internal Guna.UI2.WinForms.Guna2Button btnMonth;
        internal Guna.UI2.WinForms.Guna2ComboBox cboOrderType;
        internal Guna.UI2.WinForms.Guna2ComboBox cboPaymentMethod;
        internal Guna.UI2.WinForms.Guna2ComboBox cboCashier;
        internal Guna.UI2.WinForms.Guna2Button btnSearch;
        internal Guna.UI2.WinForms.Guna2Button btnReset;
        // ═══ Summary ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlSummary;
        internal System.Windows.Forms.FlowLayoutPanel flowCards;
        // ═══ Content ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlContent;
        internal Guna.UI2.WinForms.Guna2TabControl tabReports;
        internal System.Windows.Forms.TabPage tabOrders;
        internal System.Windows.Forms.TabPage tabProducts;
        internal System.Windows.Forms.TabPage tabShifts;
        internal System.Windows.Forms.TabPage tabPayments;
        internal System.Windows.Forms.TabPage tabCustomers;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvOrders;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvProducts;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvShifts;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvPayments;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvCustomers;
        // ═══ Returns Tab ═══
        internal System.Windows.Forms.TabPage tabReturns;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvReturns;
        // ═══ Status ═══
        internal System.Windows.Forms.Label lblStatus;

        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges35 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges36 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges17 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges18 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges19 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges20 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges21 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges22 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges23 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges24 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges25 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges26 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges27 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges28 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges29 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges30 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges31 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges32 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges33 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges34 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges37 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges38 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges39 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges40 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle9 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle10 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle11 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle12 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle13 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle14 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle15 = new DataGridViewCellStyle();
            pnlHeader = new Guna.UI2.WinForms.Guna2Panel();
            cbClose = new Guna.UI2.WinForms.Guna2ControlBox();
            cbMax = new Guna.UI2.WinForms.Guna2ControlBox();
            cbMin = new Guna.UI2.WinForms.Guna2ControlBox();
            lblTitle = new Label();
            btnPrint = new Guna.UI2.WinForms.Guna2Button();
            btnExport = new Guna.UI2.WinForms.Guna2Button();
            pnlFilters = new Guna.UI2.WinForms.Guna2Panel();
            flowFilters = new FlowLayoutPanel();
            lblFrom = new Label();
            dtpFrom = new Guna.UI2.WinForms.Guna2DateTimePicker();
            lblTo = new Label();
            dtpTo = new Guna.UI2.WinForms.Guna2DateTimePicker();
            btnToday = new Guna.UI2.WinForms.Guna2Button();
            btnYesterday = new Guna.UI2.WinForms.Guna2Button();
            btnWeek = new Guna.UI2.WinForms.Guna2Button();
            btnMonth = new Guna.UI2.WinForms.Guna2Button();
            cboOrderType = new Guna.UI2.WinForms.Guna2ComboBox();
            cboPaymentMethod = new Guna.UI2.WinForms.Guna2ComboBox();
            cboCashier = new Guna.UI2.WinForms.Guna2ComboBox();
            btnSearch = new Guna.UI2.WinForms.Guna2Button();
            btnReset = new Guna.UI2.WinForms.Guna2Button();
            pnlSummary = new Guna.UI2.WinForms.Guna2Panel();
            flowCards = new FlowLayoutPanel();
            pnlContent = new Guna.UI2.WinForms.Guna2Panel();
            tabReports = new Guna.UI2.WinForms.Guna2TabControl();
            tabOrders = new TabPage();
            dgvOrders = new Guna.UI2.WinForms.Guna2DataGridView();
            tabProducts = new TabPage();
            dgvProducts = new Guna.UI2.WinForms.Guna2DataGridView();
            tabShifts = new TabPage();
            dgvShifts = new Guna.UI2.WinForms.Guna2DataGridView();
            tabPayments = new TabPage();
            dgvPayments = new Guna.UI2.WinForms.Guna2DataGridView();
            tabCustomers = new TabPage();
            dgvCustomers = new Guna.UI2.WinForms.Guna2DataGridView();
            tabReturns = new TabPage();
            dgvReturns = new Guna.UI2.WinForms.Guna2DataGridView();
            lblStatus = new Label();
            pnlHeader.SuspendLayout();
            pnlFilters.SuspendLayout();
            flowFilters.SuspendLayout();
            pnlSummary.SuspendLayout();
            pnlContent.SuspendLayout();
            tabReports.SuspendLayout();
            tabOrders.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOrders).BeginInit();
            tabProducts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProducts).BeginInit();
            tabShifts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvShifts).BeginInit();
            tabPayments.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPayments).BeginInit();
            tabCustomers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).BeginInit();
            tabReturns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReturns).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Controls.Add(cbMin);
            pnlHeader.Controls.Add(cbMax);
            pnlHeader.Controls.Add(cbClose);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnPrint);
            pnlHeader.Controls.Add(btnExport);
            pnlHeader.CustomizableEdges = customizableEdges11;
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.FillColor = Color.FromArgb(26, 26, 39);
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.ShadowDecoration.CustomizableEdges = customizableEdges12;
            pnlHeader.Size = new Size(1300, 55);
            pnlHeader.TabIndex = 3;
            // 
            // cbClose
            // 
            cbClose.CustomizableEdges = customizableEdges5;
            cbClose.Dock = DockStyle.Left;
            cbClose.FillColor = Color.Transparent;
            cbClose.IconColor = Color.FromArgb(249, 250, 251);
            cbClose.Location = new Point(0, 0);
            cbClose.Name = "cbClose";
            cbClose.ShadowDecoration.CustomizableEdges = customizableEdges6;
            cbClose.Size = new Size(45, 55);
            cbClose.TabIndex = 0;
            // 
            // cbMax
            // 
            cbMax.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MaximizeBox;
            cbMax.CustomizableEdges = customizableEdges3;
            cbMax.Dock = DockStyle.Left;
            cbMax.FillColor = Color.Transparent;
            cbMax.IconColor = Color.FromArgb(249, 250, 251);
            cbMax.Location = new Point(45, 0);
            cbMax.Name = "cbMax";
            cbMax.ShadowDecoration.CustomizableEdges = customizableEdges4;
            cbMax.Size = new Size(45, 55);
            cbMax.TabIndex = 1;
            // 
            // cbMin
            // 
            cbMin.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            cbMin.CustomizableEdges = customizableEdges1;
            cbMin.Dock = DockStyle.Left;
            cbMin.FillColor = Color.Transparent;
            cbMin.IconColor = Color.FromArgb(249, 250, 251);
            cbMin.Location = new Point(90, 0);
            cbMin.Name = "cbMin";
            cbMin.ShadowDecoration.CustomizableEdges = customizableEdges2;
            cbMin.Size = new Size(45, 55);
            cbMin.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Alexandria", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(249, 250, 251);
            lblTitle.Location = new Point(585, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(221, 38);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "📊  تقارير المبيعات";
            // 
            // btnPrint
            // 
            btnPrint.BorderRadius = 10;
            btnPrint.Cursor = Cursors.Hand;
            btnPrint.CustomizableEdges = customizableEdges7;
            btnPrint.FillColor = Color.FromArgb(107, 114, 128);
            btnPrint.Font = new Font("Alexandria", 10F, FontStyle.Bold);
            btnPrint.ForeColor = Color.White;
            btnPrint.Location = new Point(1050, 10);
            btnPrint.Name = "btnPrint";
            btnPrint.ShadowDecoration.CustomizableEdges = customizableEdges8;
            btnPrint.Size = new Size(100, 35);
            btnPrint.TabIndex = 4;
            btnPrint.Text = "🖨️ طباعة";
            // 
            // btnExport
            // 
            btnExport.BorderRadius = 10;
            btnExport.Cursor = Cursors.Hand;
            btnExport.CustomizableEdges = customizableEdges9;
            btnExport.FillColor = Color.FromArgb(16, 185, 129);
            btnExport.Font = new Font("Alexandria", 10F, FontStyle.Bold);
            btnExport.ForeColor = Color.White;
            btnExport.Location = new Point(1160, 10);
            btnExport.Name = "btnExport";
            btnExport.ShadowDecoration.CustomizableEdges = customizableEdges10;
            btnExport.Size = new Size(130, 35);
            btnExport.TabIndex = 5;
            btnExport.Text = "📤 تصدير CSV";
            // 
            // pnlFilters
            // 
            pnlFilters.Controls.Add(flowFilters);
            pnlFilters.CustomizableEdges = customizableEdges35;
            pnlFilters.Dock = DockStyle.Top;
            pnlFilters.FillColor = Color.FromArgb(30, 30, 47);
            pnlFilters.Location = new Point(0, 55);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Padding = new Padding(15, 10, 15, 10);
            pnlFilters.ShadowDecoration.CustomizableEdges = customizableEdges36;
            pnlFilters.Size = new Size(1300, 70);
            pnlFilters.TabIndex = 2;
            // 
            // flowFilters
            // 
            flowFilters.BackColor = Color.Transparent;
            flowFilters.Controls.Add(lblFrom);
            flowFilters.Controls.Add(dtpFrom);
            flowFilters.Controls.Add(lblTo);
            flowFilters.Controls.Add(dtpTo);
            flowFilters.Controls.Add(btnToday);
            flowFilters.Controls.Add(btnYesterday);
            flowFilters.Controls.Add(btnWeek);
            flowFilters.Controls.Add(btnMonth);
            flowFilters.Controls.Add(cboOrderType);
            flowFilters.Controls.Add(cboPaymentMethod);
            flowFilters.Controls.Add(cboCashier);
            flowFilters.Controls.Add(btnSearch);
            flowFilters.Controls.Add(btnReset);
            flowFilters.Dock = DockStyle.Fill;
            flowFilters.FlowDirection = FlowDirection.RightToLeft;
            flowFilters.Location = new Point(15, 10);
            flowFilters.Name = "flowFilters";
            flowFilters.Size = new Size(1270, 50);
            flowFilters.TabIndex = 0;
            flowFilters.WrapContents = false;
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.BackColor = Color.Transparent;
            lblFrom.Font = new Font("Alexandria", 10F);
            lblFrom.ForeColor = Color.FromArgb(156, 163, 175);
            lblFrom.Location = new Point(3, 12);
            lblFrom.Margin = new Padding(3, 12, 3, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(34, 24);
            lblFrom.TabIndex = 0;
            lblFrom.Text = "من:";
            // 
            // dtpFrom
            // 
            dtpFrom.BorderRadius = 8;
            dtpFrom.Checked = true;
            dtpFrom.CustomFormat = "yyyy/MM/dd";
            dtpFrom.CustomizableEdges = customizableEdges13;
            dtpFrom.FillColor = Color.FromArgb(30, 30, 47);
            dtpFrom.Font = new Font("Alexandria", 10F);
            dtpFrom.ForeColor = Color.FromArgb(249, 250, 251);
            dtpFrom.Format = DateTimePickerFormat.Custom;
            dtpFrom.Location = new Point(43, 3);
            dtpFrom.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            dtpFrom.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.ShadowDecoration.CustomizableEdges = customizableEdges14;
            dtpFrom.Size = new Size(140, 36);
            dtpFrom.TabIndex = 1;
            dtpFrom.Value = new DateTime(2026, 4, 29, 0, 0, 0, 0);
            // 
            // lblTo
            // 
            lblTo.AutoSize = true;
            lblTo.BackColor = Color.Transparent;
            lblTo.Font = new Font("Alexandria", 10F);
            lblTo.ForeColor = Color.FromArgb(156, 163, 175);
            lblTo.Location = new Point(189, 12);
            lblTo.Margin = new Padding(3, 12, 3, 0);
            lblTo.Name = "lblTo";
            lblTo.Size = new Size(33, 24);
            lblTo.TabIndex = 2;
            lblTo.Text = "إلى:";
            // 
            // dtpTo
            // 
            dtpTo.BorderRadius = 8;
            dtpTo.Checked = true;
            dtpTo.CustomFormat = "yyyy/MM/dd";
            dtpTo.CustomizableEdges = customizableEdges15;
            dtpTo.FillColor = Color.FromArgb(30, 30, 47);
            dtpTo.Font = new Font("Alexandria", 10F);
            dtpTo.ForeColor = Color.FromArgb(249, 250, 251);
            dtpTo.Format = DateTimePickerFormat.Custom;
            dtpTo.Location = new Point(228, 3);
            dtpTo.MaxDate = new DateTime(9998, 12, 31, 0, 0, 0, 0);
            dtpTo.MinDate = new DateTime(1753, 1, 1, 0, 0, 0, 0);
            dtpTo.Name = "dtpTo";
            dtpTo.ShadowDecoration.CustomizableEdges = customizableEdges16;
            dtpTo.Size = new Size(140, 36);
            dtpTo.TabIndex = 3;
            dtpTo.Value = new DateTime(2026, 4, 29, 0, 0, 0, 0);
            // 
            // btnToday
            // 
            btnToday.BorderColor = Color.FromArgb(55, 65, 81);
            btnToday.BorderRadius = 8;
            btnToday.BorderThickness = 1;
            btnToday.Cursor = Cursors.Hand;
            btnToday.CustomizableEdges = customizableEdges17;
            btnToday.FillColor = Color.Transparent;
            btnToday.Font = new Font("Alexandria", 9F);
            btnToday.ForeColor = Color.FromArgb(156, 163, 175);
            btnToday.Location = new Point(373, 2);
            btnToday.Margin = new Padding(2);
            btnToday.Name = "btnToday";
            btnToday.ShadowDecoration.CustomizableEdges = customizableEdges18;
            btnToday.Size = new Size(65, 36);
            btnToday.TabIndex = 4;
            btnToday.Text = "اليوم";
            // 
            // btnYesterday
            // 
            btnYesterday.BorderColor = Color.FromArgb(55, 65, 81);
            btnYesterday.BorderRadius = 8;
            btnYesterday.BorderThickness = 1;
            btnYesterday.Cursor = Cursors.Hand;
            btnYesterday.CustomizableEdges = customizableEdges19;
            btnYesterday.FillColor = Color.Transparent;
            btnYesterday.Font = new Font("Alexandria", 9F);
            btnYesterday.ForeColor = Color.FromArgb(156, 163, 175);
            btnYesterday.Location = new Point(442, 2);
            btnYesterday.Margin = new Padding(2);
            btnYesterday.Name = "btnYesterday";
            btnYesterday.ShadowDecoration.CustomizableEdges = customizableEdges20;
            btnYesterday.Size = new Size(65, 36);
            btnYesterday.TabIndex = 5;
            btnYesterday.Text = "أمس";
            // 
            // btnWeek
            // 
            btnWeek.BorderColor = Color.FromArgb(55, 65, 81);
            btnWeek.BorderRadius = 8;
            btnWeek.BorderThickness = 1;
            btnWeek.Cursor = Cursors.Hand;
            btnWeek.CustomizableEdges = customizableEdges21;
            btnWeek.FillColor = Color.Transparent;
            btnWeek.Font = new Font("Alexandria", 9F);
            btnWeek.ForeColor = Color.FromArgb(156, 163, 175);
            btnWeek.Location = new Point(511, 2);
            btnWeek.Margin = new Padding(2);
            btnWeek.Name = "btnWeek";
            btnWeek.ShadowDecoration.CustomizableEdges = customizableEdges22;
            btnWeek.Size = new Size(76, 36);
            btnWeek.TabIndex = 6;
            btnWeek.Text = "الأسبوع";
            // 
            // btnMonth
            // 
            btnMonth.BorderColor = Color.FromArgb(55, 65, 81);
            btnMonth.BorderRadius = 8;
            btnMonth.BorderThickness = 1;
            btnMonth.Cursor = Cursors.Hand;
            btnMonth.CustomizableEdges = customizableEdges23;
            btnMonth.FillColor = Color.Transparent;
            btnMonth.Font = new Font("Alexandria", 9F);
            btnMonth.ForeColor = Color.FromArgb(156, 163, 175);
            btnMonth.Location = new Point(591, 2);
            btnMonth.Margin = new Padding(2);
            btnMonth.Name = "btnMonth";
            btnMonth.ShadowDecoration.CustomizableEdges = customizableEdges24;
            btnMonth.Size = new Size(65, 36);
            btnMonth.TabIndex = 7;
            btnMonth.Text = "الشهر";
            // 
            // cboOrderType
            // 
            cboOrderType.BackColor = Color.Transparent;
            cboOrderType.BorderRadius = 8;
            cboOrderType.CustomizableEdges = customizableEdges25;
            cboOrderType.DrawMode = DrawMode.OwnerDrawFixed;
            cboOrderType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOrderType.FillColor = Color.FromArgb(30, 30, 47);
            cboOrderType.FocusedColor = Color.Empty;
            cboOrderType.Font = new Font("Alexandria", 10F);
            cboOrderType.ForeColor = Color.FromArgb(249, 250, 251);
            cboOrderType.ItemHeight = 30;
            cboOrderType.Items.AddRange(new object[] { "الكل", "تيك اوي", "صالة", "دليفري" });
            cboOrderType.Location = new Point(661, 3);
            cboOrderType.Name = "cboOrderType";
            cboOrderType.ShadowDecoration.CustomizableEdges = customizableEdges26;
            cboOrderType.Size = new Size(110, 36);
            cboOrderType.TabIndex = 8;
            // 
            // cboPaymentMethod
            // 
            cboPaymentMethod.BackColor = Color.Transparent;
            cboPaymentMethod.BorderRadius = 8;
            cboPaymentMethod.CustomizableEdges = customizableEdges27;
            cboPaymentMethod.DrawMode = DrawMode.OwnerDrawFixed;
            cboPaymentMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPaymentMethod.FillColor = Color.FromArgb(30, 30, 47);
            cboPaymentMethod.FocusedColor = Color.Empty;
            cboPaymentMethod.Font = new Font("Alexandria", 10F);
            cboPaymentMethod.ForeColor = Color.FromArgb(249, 250, 251);
            cboPaymentMethod.ItemHeight = 30;
            cboPaymentMethod.Items.AddRange(new object[] { "الكل", "نقدي", "بطاقة", "آجل", "تحويل" });
            cboPaymentMethod.Location = new Point(777, 3);
            cboPaymentMethod.Name = "cboPaymentMethod";
            cboPaymentMethod.ShadowDecoration.CustomizableEdges = customizableEdges28;
            cboPaymentMethod.Size = new Size(110, 36);
            cboPaymentMethod.TabIndex = 9;
            // 
            // cboCashier
            // 
            cboCashier.BackColor = Color.Transparent;
            cboCashier.BorderRadius = 8;
            cboCashier.CustomizableEdges = customizableEdges29;
            cboCashier.DrawMode = DrawMode.OwnerDrawFixed;
            cboCashier.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCashier.FillColor = Color.FromArgb(30, 30, 47);
            cboCashier.FocusedColor = Color.Empty;
            cboCashier.Font = new Font("Alexandria", 10F);
            cboCashier.ForeColor = Color.FromArgb(249, 250, 251);
            cboCashier.ItemHeight = 30;
            cboCashier.Items.AddRange(new object[] { "الكل" });
            cboCashier.Location = new Point(893, 3);
            cboCashier.Name = "cboCashier";
            cboCashier.ShadowDecoration.CustomizableEdges = customizableEdges30;
            cboCashier.Size = new Size(110, 36);
            cboCashier.TabIndex = 10;
            // 
            // btnSearch
            // 
            btnSearch.BorderRadius = 10;
            btnSearch.Cursor = Cursors.Hand;
            btnSearch.CustomizableEdges = customizableEdges31;
            btnSearch.FillColor = Color.FromArgb(59, 130, 246);
            btnSearch.Font = new Font("Alexandria", 10F, FontStyle.Bold);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(1009, 3);
            btnSearch.Name = "btnSearch";
            btnSearch.ShadowDecoration.CustomizableEdges = customizableEdges32;
            btnSearch.Size = new Size(101, 36);
            btnSearch.TabIndex = 11;
            btnSearch.Text = "🔍 بحث";
            // 
            // btnReset
            // 
            btnReset.BorderRadius = 10;
            btnReset.Cursor = Cursors.Hand;
            btnReset.CustomizableEdges = customizableEdges33;
            btnReset.FillColor = Color.FromArgb(107, 114, 128);
            btnReset.Font = new Font("Alexandria", 10F, FontStyle.Bold);
            btnReset.ForeColor = Color.White;
            btnReset.Location = new Point(1116, 3);
            btnReset.Name = "btnReset";
            btnReset.ShadowDecoration.CustomizableEdges = customizableEdges34;
            btnReset.Size = new Size(141, 36);
            btnReset.TabIndex = 12;
            btnReset.Text = "↺ إعادة";
            // 
            // pnlSummary
            // 
            pnlSummary.Controls.Add(flowCards);
            pnlSummary.CustomizableEdges = customizableEdges37;
            pnlSummary.Dock = DockStyle.Top;
            pnlSummary.FillColor = Color.FromArgb(18, 18, 24);
            pnlSummary.Location = new Point(0, 125);
            pnlSummary.Name = "pnlSummary";
            pnlSummary.Padding = new Padding(10, 5, 10, 5);
            pnlSummary.ShadowDecoration.CustomizableEdges = customizableEdges38;
            pnlSummary.Size = new Size(1300, 115);
            pnlSummary.TabIndex = 1;
            // 
            // flowCards
            // 
            flowCards.BackColor = Color.Transparent;
            flowCards.Dock = DockStyle.Fill;
            flowCards.FlowDirection = FlowDirection.RightToLeft;
            flowCards.Location = new Point(10, 5);
            flowCards.Name = "flowCards";
            flowCards.Size = new Size(1280, 105);
            flowCards.TabIndex = 0;
            flowCards.WrapContents = false;
            // 
            // pnlContent
            // 
            pnlContent.Controls.Add(tabReports);
            pnlContent.CustomizableEdges = customizableEdges39;
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.FillColor = Color.FromArgb(18, 18, 24);
            pnlContent.Location = new Point(0, 240);
            pnlContent.Name = "pnlContent";
            pnlContent.Padding = new Padding(10);
            pnlContent.ShadowDecoration.CustomizableEdges = customizableEdges40;
            pnlContent.Size = new Size(1300, 512);
            pnlContent.TabIndex = 0;
            // 
            // tabReports
            // 
            tabReports.Alignment = TabAlignment.Left;
            tabReports.Controls.Add(tabOrders);
            tabReports.Controls.Add(tabProducts);
            tabReports.Controls.Add(tabShifts);
            tabReports.Controls.Add(tabPayments);
            tabReports.Controls.Add(tabCustomers);
            tabReports.Controls.Add(tabReturns);
            tabReports.Dock = DockStyle.Fill;
            tabReports.ItemSize = new Size(130, 40);
            tabReports.Location = new Point(10, 10);
            tabReports.Name = "tabReports";
            tabReports.SelectedIndex = 0;
            tabReports.Size = new Size(1280, 492);
            tabReports.TabButtonHoverState.BorderColor = Color.Empty;
            tabReports.TabButtonHoverState.FillColor = Color.FromArgb(40, 40, 60);
            tabReports.TabButtonHoverState.Font = new Font("Segoe UI Semibold", 10F);
            tabReports.TabButtonHoverState.ForeColor = Color.FromArgb(249, 250, 251);
            tabReports.TabButtonHoverState.InnerColor = Color.FromArgb(40, 52, 70);
            tabReports.TabButtonIdleState.BorderColor = Color.Empty;
            tabReports.TabButtonIdleState.FillColor = Color.FromArgb(30, 30, 47);
            tabReports.TabButtonIdleState.Font = new Font("Segoe UI Semibold", 10F);
            tabReports.TabButtonIdleState.ForeColor = Color.FromArgb(156, 163, 175);
            tabReports.TabButtonIdleState.InnerColor = Color.FromArgb(33, 42, 57);
            tabReports.TabButtonSelectedState.BorderColor = Color.Empty;
            tabReports.TabButtonSelectedState.FillColor = Color.FromArgb(59, 130, 246);
            tabReports.TabButtonSelectedState.Font = new Font("Segoe UI Semibold", 10F);
            tabReports.TabButtonSelectedState.ForeColor = Color.White;
            tabReports.TabButtonSelectedState.InnerColor = Color.FromArgb(76, 132, 255);
            tabReports.TabButtonSize = new Size(130, 40);
            tabReports.TabIndex = 0;
            tabReports.TabMenuBackColor = Color.FromArgb(33, 42, 57);
            // 
            // tabOrders
            // 
            tabOrders.BackColor = Color.FromArgb(18, 18, 24);
            tabOrders.Controls.Add(dgvOrders);
            tabOrders.Location = new Point(134, 4);
            tabOrders.Name = "tabOrders";
            tabOrders.Size = new Size(1142, 484);
            tabOrders.TabIndex = 0;
            tabOrders.Text = "تفاصيل الفواتير";
            // 
            // dgvOrders
            // 
            dataGridViewCellStyle1.BackColor = Color.White;
            dgvOrders.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.White;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvOrders.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvOrders.ColumnHeadersHeight = 50;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvOrders.DefaultCellStyle = dataGridViewCellStyle3;
            dgvOrders.Dock = DockStyle.Fill;
            dgvOrders.GridColor = Color.FromArgb(231, 229, 255);
            dgvOrders.Location = new Point(0, 0);
            dgvOrders.Name = "dgvOrders";
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.RowTemplate.Height = 40;
            dgvOrders.Size = new Size(1142, 484);
            dgvOrders.TabIndex = 0;
            dgvOrders.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvOrders.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgvOrders.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dgvOrders.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dgvOrders.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dgvOrders.ThemeStyle.BackColor = Color.White;
            dgvOrders.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            dgvOrders.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            dgvOrders.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvOrders.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvOrders.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvOrders.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOrders.ThemeStyle.HeaderStyle.Height = 50;
            dgvOrders.ThemeStyle.ReadOnly = false;
            dgvOrders.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvOrders.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOrders.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvOrders.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvOrders.ThemeStyle.RowsStyle.Height = 40;
            dgvOrders.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvOrders.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // tabProducts
            // 
            tabProducts.BackColor = Color.FromArgb(18, 18, 24);
            tabProducts.Controls.Add(dgvProducts);
            tabProducts.Location = new Point(264, 4);
            tabProducts.Name = "tabProducts";
            tabProducts.Size = new Size(0, 72);
            tabProducts.TabIndex = 1;
            tabProducts.Text = "مبيعات المنتجات";
            // 
            // dgvProducts
            // 
            dataGridViewCellStyle4.BackColor = Color.White;
            dgvProducts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle4;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle5.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle5.ForeColor = Color.White;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            dgvProducts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dgvProducts.ColumnHeadersHeight = 50;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.White;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle6.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle6.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle6.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            dgvProducts.DefaultCellStyle = dataGridViewCellStyle6;
            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.GridColor = Color.FromArgb(231, 229, 255);
            dgvProducts.Location = new Point(0, 0);
            dgvProducts.Name = "dgvProducts";
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.RowTemplate.Height = 40;
            dgvProducts.Size = new Size(0, 72);
            dgvProducts.TabIndex = 0;
            dgvProducts.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvProducts.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgvProducts.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dgvProducts.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dgvProducts.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dgvProducts.ThemeStyle.BackColor = Color.White;
            dgvProducts.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            dgvProducts.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            dgvProducts.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvProducts.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvProducts.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvProducts.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvProducts.ThemeStyle.HeaderStyle.Height = 50;
            dgvProducts.ThemeStyle.ReadOnly = false;
            dgvProducts.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvProducts.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvProducts.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvProducts.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvProducts.ThemeStyle.RowsStyle.Height = 40;
            dgvProducts.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvProducts.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // tabShifts
            // 
            tabShifts.BackColor = Color.FromArgb(18, 18, 24);
            tabShifts.Controls.Add(dgvShifts);
            tabShifts.Location = new Point(394, 4);
            tabShifts.Name = "tabShifts";
            tabShifts.Size = new Size(0, 72);
            tabShifts.TabIndex = 2;
            tabShifts.Text = "الورديات";
            // 
            // dgvShifts
            // 
            dataGridViewCellStyle7.BackColor = Color.White;
            dgvShifts.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle7;
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle8.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle8.ForeColor = Color.White;
            dataGridViewCellStyle8.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = DataGridViewTriState.True;
            dgvShifts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle8;
            dgvShifts.ColumnHeadersHeight = 50;
            dataGridViewCellStyle9.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = Color.White;
            dataGridViewCellStyle9.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle9.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle9.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle9.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle9.WrapMode = DataGridViewTriState.False;
            dgvShifts.DefaultCellStyle = dataGridViewCellStyle9;
            dgvShifts.Dock = DockStyle.Fill;
            dgvShifts.GridColor = Color.FromArgb(231, 229, 255);
            dgvShifts.Location = new Point(0, 0);
            dgvShifts.Name = "dgvShifts";
            dgvShifts.RowHeadersVisible = false;
            dgvShifts.RowTemplate.Height = 40;
            dgvShifts.Size = new Size(0, 72);
            dgvShifts.TabIndex = 0;
            dgvShifts.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvShifts.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgvShifts.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dgvShifts.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dgvShifts.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dgvShifts.ThemeStyle.BackColor = Color.White;
            dgvShifts.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            dgvShifts.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            dgvShifts.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvShifts.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvShifts.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvShifts.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvShifts.ThemeStyle.HeaderStyle.Height = 50;
            dgvShifts.ThemeStyle.ReadOnly = false;
            dgvShifts.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvShifts.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvShifts.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvShifts.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvShifts.ThemeStyle.RowsStyle.Height = 40;
            dgvShifts.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvShifts.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // tabPayments
            // 
            tabPayments.BackColor = Color.FromArgb(18, 18, 24);
            tabPayments.Controls.Add(dgvPayments);
            tabPayments.Location = new Point(524, 4);
            tabPayments.Name = "tabPayments";
            tabPayments.Size = new Size(0, 72);
            tabPayments.TabIndex = 3;
            tabPayments.Text = "طرق الدفع";
            // 
            // dgvPayments
            // 
            dataGridViewCellStyle10.BackColor = Color.White;
            dgvPayments.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle10;
            dataGridViewCellStyle11.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle11.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle11.ForeColor = Color.White;
            dataGridViewCellStyle11.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = DataGridViewTriState.True;
            dgvPayments.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle11;
            dgvPayments.ColumnHeadersHeight = 50;
            dataGridViewCellStyle12.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = Color.White;
            dataGridViewCellStyle12.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle12.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle12.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle12.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle12.WrapMode = DataGridViewTriState.False;
            dgvPayments.DefaultCellStyle = dataGridViewCellStyle12;
            dgvPayments.Dock = DockStyle.Fill;
            dgvPayments.GridColor = Color.FromArgb(231, 229, 255);
            dgvPayments.Location = new Point(0, 0);
            dgvPayments.Name = "dgvPayments";
            dgvPayments.RowHeadersVisible = false;
            dgvPayments.RowTemplate.Height = 40;
            dgvPayments.Size = new Size(0, 72);
            dgvPayments.TabIndex = 0;
            dgvPayments.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvPayments.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgvPayments.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dgvPayments.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dgvPayments.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dgvPayments.ThemeStyle.BackColor = Color.White;
            dgvPayments.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            dgvPayments.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            dgvPayments.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvPayments.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvPayments.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvPayments.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvPayments.ThemeStyle.HeaderStyle.Height = 50;
            dgvPayments.ThemeStyle.ReadOnly = false;
            dgvPayments.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvPayments.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvPayments.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvPayments.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvPayments.ThemeStyle.RowsStyle.Height = 40;
            dgvPayments.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvPayments.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            // 
            // tabCustomers
            // 
            tabCustomers.BackColor = Color.FromArgb(18, 18, 24);
            tabCustomers.Controls.Add(dgvCustomers);
            tabCustomers.Location = new Point(654, 4);
            tabCustomers.Name = "tabCustomers";
            tabCustomers.Size = new Size(0, 72);
            tabCustomers.TabIndex = 4;
            tabCustomers.Text = "العملاء";
            // 
            // dgvCustomers
            // 
            dataGridViewCellStyle13.BackColor = Color.White;
            dgvCustomers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle13;
            dataGridViewCellStyle14.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle14.BackColor = Color.FromArgb(100, 88, 255);
            dataGridViewCellStyle14.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle14.ForeColor = Color.White;
            dataGridViewCellStyle14.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle14.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle14.WrapMode = DataGridViewTriState.True;
            dgvCustomers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle14;
            dgvCustomers.ColumnHeadersHeight = 50;
            dataGridViewCellStyle15.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle15.BackColor = Color.White;
            dataGridViewCellStyle15.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle15.ForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle15.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dataGridViewCellStyle15.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dataGridViewCellStyle15.WrapMode = DataGridViewTriState.False;
            dgvCustomers.DefaultCellStyle = dataGridViewCellStyle15;
            dgvCustomers.Dock = DockStyle.Fill;
            dgvCustomers.GridColor = Color.FromArgb(231, 229, 255);
            dgvCustomers.Location = new Point(0, 0);
            dgvCustomers.Name = "dgvCustomers";
            dgvCustomers.RowHeadersVisible = false;
            dgvCustomers.RowTemplate.Height = 40;
            dgvCustomers.Size = new Size(0, 72);
            dgvCustomers.TabIndex = 0;
            dgvCustomers.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvCustomers.ThemeStyle.AlternatingRowsStyle.Font = null;
            dgvCustomers.ThemeStyle.AlternatingRowsStyle.ForeColor = Color.Empty;
            dgvCustomers.ThemeStyle.AlternatingRowsStyle.SelectionBackColor = Color.Empty;
            dgvCustomers.ThemeStyle.AlternatingRowsStyle.SelectionForeColor = Color.Empty;
            dgvCustomers.ThemeStyle.BackColor = Color.White;
            dgvCustomers.ThemeStyle.GridColor = Color.FromArgb(231, 229, 255);
            dgvCustomers.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(100, 88, 255);
            dgvCustomers.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvCustomers.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvCustomers.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvCustomers.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvCustomers.ThemeStyle.HeaderStyle.Height = 50;
            dgvCustomers.ThemeStyle.ReadOnly = false;
            dgvCustomers.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvCustomers.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvCustomers.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvCustomers.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvCustomers.ThemeStyle.RowsStyle.Height = 40;
            dgvCustomers.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvCustomers.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            //
            // tabReturns
            //
            tabReturns.BackColor = Color.FromArgb(18, 18, 24);
            tabReturns.Controls.Add(dgvReturns);
            tabReturns.Location = new Point(784, 4);
            tabReturns.Name = "tabReturns";
            tabReturns.Size = new Size(0, 72);
            tabReturns.TabIndex = 5;
            tabReturns.Text = "المرتجعات";
            //
            // dgvReturns
            //
            dgvReturns.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            dgvReturns.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvReturns.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 68, 68);
            dgvReturns.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvReturns.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvReturns.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvReturns.ColumnHeadersHeight = 50;
            dgvReturns.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvReturns.DefaultCellStyle.BackColor = Color.White;
            dgvReturns.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvReturns.DefaultCellStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvReturns.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 220, 220);
            dgvReturns.DefaultCellStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            dgvReturns.Dock = DockStyle.Fill;
            dgvReturns.GridColor = Color.FromArgb(255, 200, 200);
            dgvReturns.Location = new Point(0, 0);
            dgvReturns.Name = "dgvReturns";
            dgvReturns.RowHeadersVisible = false;
            dgvReturns.RowTemplate.Height = 40;
            dgvReturns.Size = new Size(0, 72);
            dgvReturns.TabIndex = 0;
            dgvReturns.ThemeStyle.BackColor = Color.White;
            dgvReturns.ThemeStyle.GridColor = Color.FromArgb(255, 200, 200);
            dgvReturns.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(239, 68, 68);
            dgvReturns.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvReturns.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9F);
            dgvReturns.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvReturns.ThemeStyle.HeaderStyle.HeaightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvReturns.ThemeStyle.HeaderStyle.Height = 50;
            dgvReturns.ThemeStyle.ReadOnly = false;
            dgvReturns.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvReturns.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvReturns.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 9F);
            dgvReturns.ThemeStyle.RowsStyle.ForeColor = Color.FromArgb(71, 69, 94);
            dgvReturns.ThemeStyle.RowsStyle.Height = 40;
            dgvReturns.ThemeStyle.RowsStyle.SelectionBackColor = Color.FromArgb(255, 220, 220);
            dgvReturns.ThemeStyle.RowsStyle.SelectionForeColor = Color.FromArgb(71, 69, 94);
            //
            // lblStatus
            //
            lblStatus.BackColor = Color.FromArgb(26, 26, 39);
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Font = new Font("Alexandria", 9F);
            lblStatus.ForeColor = Color.FromArgb(156, 163, 175);
            lblStatus.Location = new Point(0, 752);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new Padding(10, 0, 10, 0);
            lblStatus.Size = new Size(1300, 28);
            lblStatus.TabIndex = 4;
            lblStatus.Text = "جاهز";
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            // 
            // frmSalesReports
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(18, 18, 24);
            ClientSize = new Size(1300, 780);
            Controls.Add(pnlContent);
            Controls.Add(pnlSummary);
            Controls.Add(pnlFilters);
            Controls.Add(pnlHeader);
            Controls.Add(lblStatus);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(1200, 700);
            Name = "frmSalesReports";
            RightToLeft = RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "تقارير المبيعات";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlFilters.ResumeLayout(false);
            flowFilters.ResumeLayout(false);
            flowFilters.PerformLayout();
            pnlSummary.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            tabReports.ResumeLayout(false);
            tabOrders.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOrders).EndInit();
            tabProducts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvProducts).EndInit();
            tabShifts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvShifts).EndInit();
            tabPayments.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPayments).EndInit();
            tabCustomers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCustomers).EndInit();
            tabReturns.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvReturns).EndInit();
            ResumeLayout(false);
        }
    }
}
