namespace Sestamk.Forms
{
    partial class frmReturns
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // ═══ Header ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlHeader;
        internal System.Windows.Forms.Label lblTitle;
        internal Guna.UI2.WinForms.Guna2ControlBox cbClose;
        internal Guna.UI2.WinForms.Guna2ControlBox cbMax;
        internal Guna.UI2.WinForms.Guna2ControlBox cbMin;

        // ═══ Search ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlSearch;
        internal System.Windows.Forms.Label lblSearchLabel;
        internal Guna.UI2.WinForms.Guna2TextBox txtInvoiceNumber;
        internal Guna.UI2.WinForms.Guna2Button btnSearchInvoice;

        // ═══ Invoice Info ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlInvoiceInfo;
        internal System.Windows.Forms.Label lblInfoInvoiceNum;
        internal System.Windows.Forms.Label lblInfoDate;
        internal System.Windows.Forms.Label lblInfoCustomer;
        internal System.Windows.Forms.Label lblInfoTotal;
        internal System.Windows.Forms.Label lblValInvoiceNum;
        internal System.Windows.Forms.Label lblValDate;
        internal System.Windows.Forms.Label lblValCustomer;
        internal System.Windows.Forms.Label lblValTotal;

        // ═══ Items Grid ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlItems;
        internal System.Windows.Forms.Label lblItemsHeader;
        internal Guna.UI2.WinForms.Guna2DataGridView dgvItems;

        // ═══ Bottom ═══
        internal Guna.UI2.WinForms.Guna2Panel pnlBottom;
        internal System.Windows.Forms.Label lblReasonLabel;
        internal Guna.UI2.WinForms.Guna2TextBox txtReason;
        internal System.Windows.Forms.Label lblRefundMethodLabel;
        internal System.Windows.Forms.RadioButton rdoCash;
        internal System.Windows.Forms.RadioButton rdoCard;
        internal System.Windows.Forms.RadioButton rdoCredit;
        internal System.Windows.Forms.Label lblTotalLabel;
        internal System.Windows.Forms.Label lblTotalValue;
        internal Guna.UI2.WinForms.Guna2Button btnSaveReturn;
        internal Guna.UI2.WinForms.Guna2Button btnCancel;

        // ═══ Status ═══
        internal System.Windows.Forms.Label lblStatus;

        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges ce1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce11 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce12 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce13 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce14 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce15 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce16 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce17 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce18 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce19 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges ce20 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dgvStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dgvStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dgvStyle3 = new DataGridViewCellStyle();

            pnlHeader = new Guna.UI2.WinForms.Guna2Panel();
            cbClose = new Guna.UI2.WinForms.Guna2ControlBox();
            cbMax = new Guna.UI2.WinForms.Guna2ControlBox();
            cbMin = new Guna.UI2.WinForms.Guna2ControlBox();
            lblTitle = new System.Windows.Forms.Label();
            pnlSearch = new Guna.UI2.WinForms.Guna2Panel();
            lblSearchLabel = new System.Windows.Forms.Label();
            txtInvoiceNumber = new Guna.UI2.WinForms.Guna2TextBox();
            btnSearchInvoice = new Guna.UI2.WinForms.Guna2Button();
            pnlInvoiceInfo = new Guna.UI2.WinForms.Guna2Panel();
            lblInfoInvoiceNum = new System.Windows.Forms.Label();
            lblValInvoiceNum = new System.Windows.Forms.Label();
            lblInfoDate = new System.Windows.Forms.Label();
            lblValDate = new System.Windows.Forms.Label();
            lblInfoCustomer = new System.Windows.Forms.Label();
            lblValCustomer = new System.Windows.Forms.Label();
            lblInfoTotal = new System.Windows.Forms.Label();
            lblValTotal = new System.Windows.Forms.Label();
            pnlItems = new Guna.UI2.WinForms.Guna2Panel();
            lblItemsHeader = new System.Windows.Forms.Label();
            dgvItems = new Guna.UI2.WinForms.Guna2DataGridView();
            pnlBottom = new Guna.UI2.WinForms.Guna2Panel();
            lblReasonLabel = new System.Windows.Forms.Label();
            txtReason = new Guna.UI2.WinForms.Guna2TextBox();
            lblRefundMethodLabel = new System.Windows.Forms.Label();
            rdoCash = new System.Windows.Forms.RadioButton();
            rdoCard = new System.Windows.Forms.RadioButton();
            rdoCredit = new System.Windows.Forms.RadioButton();
            lblTotalLabel = new System.Windows.Forms.Label();
            lblTotalValue = new System.Windows.Forms.Label();
            btnSaveReturn = new Guna.UI2.WinForms.Guna2Button();
            btnCancel = new Guna.UI2.WinForms.Guna2Button();
            lblStatus = new System.Windows.Forms.Label();

            pnlHeader.SuspendLayout();
            pnlSearch.SuspendLayout();
            pnlInvoiceInfo.SuspendLayout();
            pnlItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
            pnlBottom.SuspendLayout();
            SuspendLayout();

            // ══ pnlHeader ══
            pnlHeader.Controls.Add(cbMin);
            pnlHeader.Controls.Add(cbMax);
            pnlHeader.Controls.Add(cbClose);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.CustomizableEdges = ce1;
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.FillColor = System.Drawing.Color.FromArgb(26, 26, 39);
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.ShadowDecoration.CustomizableEdges = ce2;
            pnlHeader.Size = new System.Drawing.Size(1100, 55);
            pnlHeader.TabIndex = 0;

            // cbClose
            cbClose.CustomizableEdges = ce3;
            cbClose.Dock = System.Windows.Forms.DockStyle.Left;
            cbClose.FillColor = System.Drawing.Color.Transparent;
            cbClose.IconColor = System.Drawing.Color.FromArgb(249, 250, 251);
            cbClose.Location = new System.Drawing.Point(0, 0);
            cbClose.Name = "cbClose";
            cbClose.ShadowDecoration.CustomizableEdges = ce4;
            cbClose.Size = new System.Drawing.Size(45, 55);
            cbClose.TabIndex = 0;

            // cbMax
            cbMax.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MaximizeBox;
            cbMax.CustomizableEdges = ce5;
            cbMax.Dock = System.Windows.Forms.DockStyle.Left;
            cbMax.FillColor = System.Drawing.Color.Transparent;
            cbMax.IconColor = System.Drawing.Color.FromArgb(249, 250, 251);
            cbMax.Location = new System.Drawing.Point(45, 0);
            cbMax.Name = "cbMax";
            cbMax.ShadowDecoration.CustomizableEdges = ce6;
            cbMax.Size = new System.Drawing.Size(45, 55);
            cbMax.TabIndex = 1;

            // cbMin
            cbMin.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            cbMin.CustomizableEdges = ce7;
            cbMin.Dock = System.Windows.Forms.DockStyle.Left;
            cbMin.FillColor = System.Drawing.Color.Transparent;
            cbMin.IconColor = System.Drawing.Color.FromArgb(249, 250, 251);
            cbMin.Location = new System.Drawing.Point(90, 0);
            cbMin.Name = "cbMin";
            cbMin.ShadowDecoration.CustomizableEdges = ce8;
            cbMin.Size = new System.Drawing.Size(45, 55);
            cbMin.TabIndex = 2;

            // lblTitle
            lblTitle.AutoSize = true;
            lblTitle.BackColor = System.Drawing.Color.Transparent;
            lblTitle.Font = new System.Drawing.Font("Alexandria", 16F, System.Drawing.FontStyle.Bold);
            lblTitle.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            lblTitle.Location = new System.Drawing.Point(450, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(200, 38);
            lblTitle.TabIndex = 3;
            lblTitle.Text = "↩  فورم المرتجع";

            // ══ pnlSearch ══
            pnlSearch.Controls.Add(btnSearchInvoice);
            pnlSearch.Controls.Add(txtInvoiceNumber);
            pnlSearch.Controls.Add(lblSearchLabel);
            pnlSearch.CustomizableEdges = ce9;
            pnlSearch.Dock = System.Windows.Forms.DockStyle.Top;
            pnlSearch.FillColor = System.Drawing.Color.FromArgb(30, 30, 47);
            pnlSearch.Location = new System.Drawing.Point(0, 55);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            pnlSearch.ShadowDecoration.CustomizableEdges = ce10;
            pnlSearch.Size = new System.Drawing.Size(1100, 60);
            pnlSearch.TabIndex = 1;

            // lblSearchLabel
            lblSearchLabel.AutoSize = true;
            lblSearchLabel.BackColor = System.Drawing.Color.Transparent;
            lblSearchLabel.Font = new System.Drawing.Font("Alexandria", 11F);
            lblSearchLabel.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblSearchLabel.Location = new System.Drawing.Point(940, 16);
            lblSearchLabel.Name = "lblSearchLabel";
            lblSearchLabel.Size = new System.Drawing.Size(130, 26);
            lblSearchLabel.TabIndex = 0;
            lblSearchLabel.Text = "رقم الفاتورة:";

            // txtInvoiceNumber
            txtInvoiceNumber.BorderRadius = 10;
            txtInvoiceNumber.CustomizableEdges = ce11;
            txtInvoiceNumber.FillColor = System.Drawing.Color.FromArgb(18, 18, 30);
            txtInvoiceNumber.Font = new System.Drawing.Font("Alexandria", 11F);
            txtInvoiceNumber.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            txtInvoiceNumber.Location = new System.Drawing.Point(530, 13);
            txtInvoiceNumber.Name = "txtInvoiceNumber";
            txtInvoiceNumber.PlaceholderText = "INV-20260512-001";
            txtInvoiceNumber.ShadowDecoration.CustomizableEdges = ce12;
            txtInvoiceNumber.Size = new System.Drawing.Size(400, 36);
            txtInvoiceNumber.TabIndex = 1;

            // btnSearchInvoice
            btnSearchInvoice.BorderRadius = 10;
            btnSearchInvoice.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSearchInvoice.CustomizableEdges = ce13;
            btnSearchInvoice.FillColor = System.Drawing.Color.FromArgb(59, 130, 246);
            btnSearchInvoice.Font = new System.Drawing.Font("Alexandria", 11F, System.Drawing.FontStyle.Bold);
            btnSearchInvoice.ForeColor = System.Drawing.Color.White;
            btnSearchInvoice.Location = new System.Drawing.Point(390, 13);
            btnSearchInvoice.Name = "btnSearchInvoice";
            btnSearchInvoice.ShadowDecoration.CustomizableEdges = ce14;
            btnSearchInvoice.Size = new System.Drawing.Size(130, 36);
            btnSearchInvoice.TabIndex = 2;
            btnSearchInvoice.Text = "🔍 بحث";

            // ══ pnlInvoiceInfo ══
            pnlInvoiceInfo.Controls.Add(lblInfoInvoiceNum);
            pnlInvoiceInfo.Controls.Add(lblValInvoiceNum);
            pnlInvoiceInfo.Controls.Add(lblInfoDate);
            pnlInvoiceInfo.Controls.Add(lblValDate);
            pnlInvoiceInfo.Controls.Add(lblInfoCustomer);
            pnlInvoiceInfo.Controls.Add(lblValCustomer);
            pnlInvoiceInfo.Controls.Add(lblInfoTotal);
            pnlInvoiceInfo.Controls.Add(lblValTotal);
            pnlInvoiceInfo.CustomizableEdges = ce15;
            pnlInvoiceInfo.Dock = System.Windows.Forms.DockStyle.Top;
            pnlInvoiceInfo.FillColor = System.Drawing.Color.FromArgb(22, 22, 35);
            pnlInvoiceInfo.Location = new System.Drawing.Point(0, 115);
            pnlInvoiceInfo.Name = "pnlInvoiceInfo";
            pnlInvoiceInfo.Padding = new System.Windows.Forms.Padding(20, 8, 20, 8);
            pnlInvoiceInfo.ShadowDecoration.CustomizableEdges = ce16;
            pnlInvoiceInfo.Size = new System.Drawing.Size(1100, 75);
            pnlInvoiceInfo.TabIndex = 2;

            // info labels — رقم الفاتورة
            lblInfoInvoiceNum.AutoSize = true;
            lblInfoInvoiceNum.BackColor = System.Drawing.Color.Transparent;
            lblInfoInvoiceNum.Font = new System.Drawing.Font("Alexandria", 10F);
            lblInfoInvoiceNum.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblInfoInvoiceNum.Location = new System.Drawing.Point(900, 12);
            lblInfoInvoiceNum.Name = "lblInfoInvoiceNum";
            lblInfoInvoiceNum.Text = "رقم الفاتورة:";

            lblValInvoiceNum.AutoSize = true;
            lblValInvoiceNum.BackColor = System.Drawing.Color.Transparent;
            lblValInvoiceNum.Font = new System.Drawing.Font("Alexandria", 10F, System.Drawing.FontStyle.Bold);
            lblValInvoiceNum.ForeColor = System.Drawing.Color.FromArgb(59, 130, 246);
            lblValInvoiceNum.Location = new System.Drawing.Point(900, 36);
            lblValInvoiceNum.Name = "lblValInvoiceNum";
            lblValInvoiceNum.Text = "—";

            // التاريخ
            lblInfoDate.AutoSize = true;
            lblInfoDate.BackColor = System.Drawing.Color.Transparent;
            lblInfoDate.Font = new System.Drawing.Font("Alexandria", 10F);
            lblInfoDate.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblInfoDate.Location = new System.Drawing.Point(670, 12);
            lblInfoDate.Name = "lblInfoDate";
            lblInfoDate.Text = "التاريخ:";

            lblValDate.AutoSize = true;
            lblValDate.BackColor = System.Drawing.Color.Transparent;
            lblValDate.Font = new System.Drawing.Font("Alexandria", 10F, System.Drawing.FontStyle.Bold);
            lblValDate.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            lblValDate.Location = new System.Drawing.Point(670, 36);
            lblValDate.Name = "lblValDate";
            lblValDate.Text = "—";

            // العميل
            lblInfoCustomer.AutoSize = true;
            lblInfoCustomer.BackColor = System.Drawing.Color.Transparent;
            lblInfoCustomer.Font = new System.Drawing.Font("Alexandria", 10F);
            lblInfoCustomer.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblInfoCustomer.Location = new System.Drawing.Point(400, 12);
            lblInfoCustomer.Name = "lblInfoCustomer";
            lblInfoCustomer.Text = "العميل:";

            lblValCustomer.AutoSize = true;
            lblValCustomer.BackColor = System.Drawing.Color.Transparent;
            lblValCustomer.Font = new System.Drawing.Font("Alexandria", 10F, System.Drawing.FontStyle.Bold);
            lblValCustomer.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            lblValCustomer.Location = new System.Drawing.Point(400, 36);
            lblValCustomer.Name = "lblValCustomer";
            lblValCustomer.Text = "—";

            // إجمالي الفاتورة الأصلية
            lblInfoTotal.AutoSize = true;
            lblInfoTotal.BackColor = System.Drawing.Color.Transparent;
            lblInfoTotal.Font = new System.Drawing.Font("Alexandria", 10F);
            lblInfoTotal.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblInfoTotal.Location = new System.Drawing.Point(100, 12);
            lblInfoTotal.Name = "lblInfoTotal";
            lblInfoTotal.Text = "إجمالي الفاتورة:";

            lblValTotal.AutoSize = true;
            lblValTotal.BackColor = System.Drawing.Color.Transparent;
            lblValTotal.Font = new System.Drawing.Font("Alexandria", 13F, System.Drawing.FontStyle.Bold);
            lblValTotal.ForeColor = System.Drawing.Color.FromArgb(16, 185, 129);
            lblValTotal.Location = new System.Drawing.Point(100, 34);
            lblValTotal.Name = "lblValTotal";
            lblValTotal.Text = "—";

            // ══ pnlItems (Fill — يأخذ المساحة الباقية) ══
            pnlItems.Controls.Add(dgvItems);
            pnlItems.Controls.Add(lblItemsHeader);
            pnlItems.CustomizableEdges = ce17;
            pnlItems.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlItems.FillColor = System.Drawing.Color.FromArgb(18, 18, 24);
            pnlItems.Location = new System.Drawing.Point(0, 190);
            pnlItems.Name = "pnlItems";
            pnlItems.Padding = new System.Windows.Forms.Padding(15, 5, 15, 5);
            pnlItems.ShadowDecoration.CustomizableEdges = ce18;
            pnlItems.TabIndex = 3;

            // lblItemsHeader
            lblItemsHeader.AutoSize = true;
            lblItemsHeader.BackColor = System.Drawing.Color.Transparent;
            lblItemsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            lblItemsHeader.Font = new System.Drawing.Font("Alexandria", 10F, System.Drawing.FontStyle.Bold);
            lblItemsHeader.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblItemsHeader.Location = new System.Drawing.Point(15, 5);
            lblItemsHeader.Name = "lblItemsHeader";
            lblItemsHeader.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            lblItemsHeader.Size = new System.Drawing.Size(300, 32);
            lblItemsHeader.TabIndex = 1;
            lblItemsHeader.Text = "📦 حدد الأصناف المراد إرجاعها وحدد الكمية";

            // dgvItems
            dgvStyle1.BackColor = System.Drawing.Color.White;
            dgvItems.AlternatingRowsDefaultCellStyle = dgvStyle1;
            dgvStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvStyle2.BackColor = System.Drawing.Color.FromArgb(100, 88, 255);
            dgvStyle2.Font = new System.Drawing.Font("Alexandria", 10F, System.Drawing.FontStyle.Bold);
            dgvStyle2.ForeColor = System.Drawing.Color.White;
            dgvStyle2.WrapMode = DataGridViewTriState.True;
            dgvItems.ColumnHeadersDefaultCellStyle = dgvStyle2;
            dgvItems.ColumnHeadersHeight = 45;
            dgvStyle3.BackColor = System.Drawing.Color.White;
            dgvStyle3.Font = new System.Drawing.Font("Alexandria", 10F);
            dgvStyle3.ForeColor = System.Drawing.Color.FromArgb(71, 69, 94);
            dgvStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(220, 215, 255);
            dgvStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(71, 69, 94);
            dgvItems.DefaultCellStyle = dgvStyle3;
            dgvItems.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvItems.GridColor = System.Drawing.Color.FromArgb(231, 229, 255);
            dgvItems.Location = new System.Drawing.Point(15, 42);
            dgvItems.Name = "dgvItems";
            dgvItems.RowHeadersVisible = false;
            dgvItems.RowTemplate.Height = 42;
            dgvItems.TabIndex = 0;
            dgvItems.ThemeStyle.BackColor = System.Drawing.Color.White;
            dgvItems.ThemeStyle.GridColor = System.Drawing.Color.FromArgb(231, 229, 255);
            dgvItems.ThemeStyle.HeaderStyle.BackColor = System.Drawing.Color.FromArgb(100, 88, 255);
            dgvItems.ThemeStyle.HeaderStyle.BorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvItems.ThemeStyle.HeaderStyle.ForeColor = System.Drawing.Color.White;
            dgvItems.ThemeStyle.HeaderStyle.Height = 45;
            dgvItems.ThemeStyle.RowsStyle.BackColor = System.Drawing.Color.White;
            dgvItems.ThemeStyle.RowsStyle.BorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvItems.ThemeStyle.RowsStyle.Height = 42;
            dgvItems.ThemeStyle.RowsStyle.SelectionBackColor = System.Drawing.Color.FromArgb(220, 215, 255);

            // ══ pnlBottom ══
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Controls.Add(btnSaveReturn);
            pnlBottom.Controls.Add(lblTotalValue);
            pnlBottom.Controls.Add(lblTotalLabel);
            pnlBottom.Controls.Add(rdoCredit);
            pnlBottom.Controls.Add(rdoCard);
            pnlBottom.Controls.Add(rdoCash);
            pnlBottom.Controls.Add(lblRefundMethodLabel);
            pnlBottom.Controls.Add(txtReason);
            pnlBottom.Controls.Add(lblReasonLabel);
            pnlBottom.CustomizableEdges = ce19;
            pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlBottom.FillColor = System.Drawing.Color.FromArgb(26, 26, 39);
            pnlBottom.Location = new System.Drawing.Point(0, 580);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new System.Windows.Forms.Padding(20, 12, 20, 12);
            pnlBottom.ShadowDecoration.CustomizableEdges = ce20;
            pnlBottom.Size = new System.Drawing.Size(1100, 145);
            pnlBottom.TabIndex = 4;

            // lblReasonLabel
            lblReasonLabel.AutoSize = true;
            lblReasonLabel.BackColor = System.Drawing.Color.Transparent;
            lblReasonLabel.Font = new System.Drawing.Font("Alexandria", 10F);
            lblReasonLabel.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblReasonLabel.Location = new System.Drawing.Point(930, 14);
            lblReasonLabel.Name = "lblReasonLabel";
            lblReasonLabel.Text = "سبب الإرجاع:";

            // txtReason
            txtReason.BorderRadius = 10;
            txtReason.FillColor = System.Drawing.Color.FromArgb(18, 18, 30);
            txtReason.Font = new System.Drawing.Font("Alexandria", 10F);
            txtReason.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            txtReason.Location = new System.Drawing.Point(480, 10);
            txtReason.Name = "txtReason";
            txtReason.PlaceholderText = "مثال: منتج تالف / طلب خاطئ ...";
            txtReason.Size = new System.Drawing.Size(440, 36);
            txtReason.TabIndex = 0;

            // lblRefundMethodLabel
            lblRefundMethodLabel.AutoSize = true;
            lblRefundMethodLabel.BackColor = System.Drawing.Color.Transparent;
            lblRefundMethodLabel.Font = new System.Drawing.Font("Alexandria", 10F);
            lblRefundMethodLabel.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblRefundMethodLabel.Location = new System.Drawing.Point(930, 60);
            lblRefundMethodLabel.Name = "lblRefundMethodLabel";
            lblRefundMethodLabel.Text = "طريقة الاسترداد:";

            // rdoCash
            rdoCash.AutoSize = true;
            rdoCash.BackColor = System.Drawing.Color.Transparent;
            rdoCash.Checked = true;
            rdoCash.Font = new System.Drawing.Font("Alexandria", 10F);
            rdoCash.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            rdoCash.Location = new System.Drawing.Point(830, 58);
            rdoCash.Name = "rdoCash";
            rdoCash.TabIndex = 1;
            rdoCash.TabStop = true;
            rdoCash.Text = "نقدي";

            // rdoCard
            rdoCard.AutoSize = true;
            rdoCard.BackColor = System.Drawing.Color.Transparent;
            rdoCard.Font = new System.Drawing.Font("Alexandria", 10F);
            rdoCard.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            rdoCard.Location = new System.Drawing.Point(740, 58);
            rdoCard.Name = "rdoCard";
            rdoCard.TabIndex = 2;
            rdoCard.Text = "بطاقة";

            // rdoCredit
            rdoCredit.AutoSize = true;
            rdoCredit.BackColor = System.Drawing.Color.Transparent;
            rdoCredit.Font = new System.Drawing.Font("Alexandria", 10F);
            rdoCredit.ForeColor = System.Drawing.Color.FromArgb(249, 250, 251);
            rdoCredit.Location = new System.Drawing.Point(620, 58);
            rdoCredit.Name = "rdoCredit";
            rdoCredit.TabIndex = 3;
            rdoCredit.Text = "رصيد آجل";

            // lblTotalLabel
            lblTotalLabel.AutoSize = true;
            lblTotalLabel.BackColor = System.Drawing.Color.Transparent;
            lblTotalLabel.Font = new System.Drawing.Font("Alexandria", 11F);
            lblTotalLabel.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblTotalLabel.Location = new System.Drawing.Point(380, 55);
            lblTotalLabel.Name = "lblTotalLabel";
            lblTotalLabel.Text = "إجمالي المرتجع:";

            // lblTotalValue
            lblTotalValue.AutoSize = true;
            lblTotalValue.BackColor = System.Drawing.Color.Transparent;
            lblTotalValue.Font = new System.Drawing.Font("Alexandria", 20F, System.Drawing.FontStyle.Bold);
            lblTotalValue.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68);
            lblTotalValue.Location = new System.Drawing.Point(100, 45);
            lblTotalValue.Name = "lblTotalValue";
            lblTotalValue.Text = "0.00";

            // btnSaveReturn
            btnSaveReturn.BorderRadius = 12;
            btnSaveReturn.Cursor = System.Windows.Forms.Cursors.Hand;
            btnSaveReturn.Enabled = false;
            btnSaveReturn.FillColor = System.Drawing.Color.FromArgb(239, 68, 68);
            btnSaveReturn.Font = new System.Drawing.Font("Alexandria", 12F, System.Drawing.FontStyle.Bold);
            btnSaveReturn.ForeColor = System.Drawing.Color.White;
            btnSaveReturn.Location = new System.Drawing.Point(20, 95);
            btnSaveReturn.Name = "btnSaveReturn";
            btnSaveReturn.Size = new System.Drawing.Size(200, 40);
            btnSaveReturn.TabIndex = 5;
            btnSaveReturn.Text = "✅ حفظ المرتجع";

            // btnCancel
            btnCancel.BorderRadius = 12;
            btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            btnCancel.FillColor = System.Drawing.Color.FromArgb(107, 114, 128);
            btnCancel.Font = new System.Drawing.Font("Alexandria", 12F, System.Drawing.FontStyle.Bold);
            btnCancel.ForeColor = System.Drawing.Color.White;
            btnCancel.Location = new System.Drawing.Point(230, 95);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(140, 40);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "إلغاء";

            // ══ lblStatus ══
            lblStatus.BackColor = System.Drawing.Color.FromArgb(26, 26, 39);
            lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblStatus.Font = new System.Drawing.Font("Alexandria", 9F);
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            lblStatus.Location = new System.Drawing.Point(0, 725);
            lblStatus.Name = "lblStatus";
            lblStatus.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            lblStatus.Size = new System.Drawing.Size(1100, 25);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "ابحث عن الفاتورة للبدء";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // ══ frmReturns ══
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(18, 18, 24);
            ClientSize = new System.Drawing.Size(1100, 750);
            Controls.Add(pnlItems);
            Controls.Add(pnlInvoiceInfo);
            Controls.Add(pnlSearch);
            Controls.Add(pnlHeader);
            Controls.Add(pnlBottom);
            Controls.Add(lblStatus);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            MinimumSize = new System.Drawing.Size(900, 650);
            Name = "frmReturns";
            RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            RightToLeftLayout = true;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "فورم المرتجع";

            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            pnlInvoiceInfo.ResumeLayout(false);
            pnlInvoiceInfo.PerformLayout();
            pnlItems.ResumeLayout(false);
            pnlItems.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
