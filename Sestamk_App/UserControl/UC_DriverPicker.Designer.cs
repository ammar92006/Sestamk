namespace Sestamk.UserControl
{
    partial class UC_DriverPicker
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.pnlContainer = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlHeader = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new Guna.UI2.WinForms.Guna2Button();
            this.txtSearch = new Guna.UI2.WinForms.Guna2TextBox();
            this.flowDrivers = new System.Windows.Forms.FlowLayoutPanel();

            this.pnlContainer.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();

            // ═══════════════════════════════════
            //  pnlContainer
            // ═══════════════════════════════════
            this.pnlContainer.BackColor = System.Drawing.Color.Transparent;
            this.pnlContainer.BorderColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.pnlContainer.BorderRadius = 20;
            this.pnlContainer.BorderThickness = 2;
            this.pnlContainer.Controls.Add(this.flowDrivers);
            this.pnlContainer.Controls.Add(this.pnlHeader);
            this.pnlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContainer.FillColor = System.Drawing.Color.FromArgb(16, 25, 34);
            this.pnlContainer.Location = new System.Drawing.Point(0, 0);
            this.pnlContainer.Name = "pnlContainer";
            this.pnlContainer.Size = new System.Drawing.Size(600, 750);

            // ═══════════════════════════════════
            //  pnlHeader
            // ═══════════════════════════════════
            this.pnlHeader.BackColor = System.Drawing.Color.Transparent;
            this.pnlHeader.Controls.Add(this.txtSearch);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.btnClose);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.FillColor = System.Drawing.Color.FromArgb(20, 30, 42);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(600, 130);

            // ═══════════════════════════════════
            //  lblTitle
            // ═══════════════════════════════════
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Alexandria", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(600, 50);
            this.lblTitle.Text = "🛵 تحديد الطيار";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // ═══════════════════════════════════
            //  btnClose
            // ═══════════════════════════════════
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            this.btnClose.BorderRadius = 12;
            this.btnClose.FillColor = System.Drawing.Color.FromArgb(30, 40, 55);
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnClose.Location = new System.Drawing.Point(10, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(42, 42);
            this.btnClose.Text = "✕";

            // ═══════════════════════════════════
            //  txtSearch
            // ═══════════════════════════════════
            this.txtSearch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.txtSearch.BackColor = System.Drawing.Color.FromArgb(20, 30, 42);
            this.txtSearch.BorderColor = System.Drawing.Color.FromArgb(40, 55, 72);
            this.txtSearch.BorderRadius = 12;
            this.txtSearch.BorderThickness = 2;
            this.txtSearch.FillColor = System.Drawing.Color.FromArgb(24, 36, 48);
            this.txtSearch.FocusedState.BorderColor = System.Drawing.Color.FromArgb(245, 158, 11);
            this.txtSearch.HoverState.BorderColor = System.Drawing.Color.FromArgb(94, 148, 255);
            this.txtSearch.Font = new System.Drawing.Font("Alexandria", 12F, System.Drawing.FontStyle.Bold);
            this.txtSearch.ForeColor = System.Drawing.Color.White;
            this.txtSearch.Location = new System.Drawing.Point(20, 60);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "بحث بالاسم أو رقم التلفون...";
            this.txtSearch.Size = new System.Drawing.Size(560, 45);
            this.txtSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSearch.TextOffset = new System.Drawing.Point(5, 0);

            // ═══════════════════════════════════
            //  flowDrivers
            // ═══════════════════════════════════
            this.flowDrivers.AutoScroll = true;
            this.flowDrivers.BackColor = System.Drawing.Color.Transparent;
            this.flowDrivers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowDrivers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowDrivers.Location = new System.Drawing.Point(0, 130);
            this.flowDrivers.Name = "flowDrivers";
            this.flowDrivers.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.flowDrivers.Size = new System.Drawing.Size(600, 620);
            this.flowDrivers.WrapContents = false;

            // ═══════════════════════════════════
            //  UC_DriverPicker
            // ═══════════════════════════════════
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.pnlContainer);
            this.Name = "UC_DriverPicker";
            this.Size = new System.Drawing.Size(600, 750);

            this.pnlHeader.ResumeLayout(false);
            this.pnlContainer.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        // ─── Control Declarations ───
        private Guna.UI2.WinForms.Guna2Panel pnlContainer;
        private Guna.UI2.WinForms.Guna2Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private Guna.UI2.WinForms.Guna2Button btnClose;
        private Guna.UI2.WinForms.Guna2TextBox txtSearch;
        private System.Windows.Forms.FlowLayoutPanel flowDrivers;
    }
}
