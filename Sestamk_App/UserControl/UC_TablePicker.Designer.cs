namespace Sestamk.UserControl
{
    partial class UC_TablePicker
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pnlContainer = new Guna.UI2.WinForms.Guna2Panel();
            flowTables = new FlowLayoutPanel();
            pnlSectionsBar = new Guna.UI2.WinForms.Guna2Panel();
            flowSections = new FlowLayoutPanel();
            pnlHeader = new Guna.UI2.WinForms.Guna2Panel();
            lblTitle = new Label();
            btnClose = new Guna.UI2.WinForms.Guna2Button();
            pnlContainer.SuspendLayout();
            pnlSectionsBar.SuspendLayout();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlContainer
            // 
            pnlContainer.BackColor = Color.Transparent;
            pnlContainer.BorderColor = Color.FromArgb(32, 143, 252);
            pnlContainer.BorderRadius = 20;
            pnlContainer.BorderThickness = 2;
            pnlContainer.Controls.Add(flowTables);
            pnlContainer.Controls.Add(pnlSectionsBar);
            pnlContainer.Controls.Add(pnlHeader);
            pnlContainer.CustomizableEdges = customizableEdges7;
            pnlContainer.Dock = DockStyle.Fill;
            pnlContainer.FillColor = Color.FromArgb(16, 25, 34);
            pnlContainer.Location = new Point(0, 0);
            pnlContainer.Name = "pnlContainer";
            pnlContainer.ShadowDecoration.CustomizableEdges = customizableEdges8;
            pnlContainer.Size = new Size(700, 600);
            pnlContainer.TabIndex = 0;
            // 
            // flowTables
            // 
            flowTables.AutoScroll = true;
            flowTables.BackColor = Color.Transparent;
            flowTables.Dock = DockStyle.Fill;
            flowTables.FlowDirection = FlowDirection.RightToLeft;
            flowTables.Location = new Point(0, 150);
            flowTables.Name = "flowTables";
            flowTables.Padding = new Padding(15, 10, 15, 10);
            flowTables.Size = new Size(700, 450);
            flowTables.TabIndex = 0;
            // 
            // pnlSectionsBar
            // 
            pnlSectionsBar.BackColor = Color.Transparent;
            pnlSectionsBar.Controls.Add(flowSections);
            pnlSectionsBar.CustomizableEdges = customizableEdges1;
            pnlSectionsBar.Dock = DockStyle.Top;
            pnlSectionsBar.FillColor = Color.FromArgb(20, 30, 42);
            pnlSectionsBar.Location = new Point(0, 70);
            pnlSectionsBar.Name = "pnlSectionsBar";
            pnlSectionsBar.Padding = new Padding(10, 5, 10, 5);
            pnlSectionsBar.ShadowDecoration.CustomizableEdges = customizableEdges2;
            pnlSectionsBar.Size = new Size(700, 80);
            pnlSectionsBar.TabIndex = 1;
            // 
            // flowSections
            // 
            flowSections.AutoScroll = true;
            flowSections.BackColor = Color.Transparent;
            flowSections.Dock = DockStyle.Fill;
            flowSections.FlowDirection = FlowDirection.RightToLeft;
            flowSections.Location = new Point(10, 5);
            flowSections.Name = "flowSections";
            flowSections.Padding = new Padding(5);
            flowSections.Size = new Size(680, 70);
            flowSections.TabIndex = 0;
            flowSections.WrapContents = false;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.Transparent;
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(btnClose);
            pnlHeader.CustomizableEdges = customizableEdges5;
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.FillColor = Color.FromArgb(20, 30, 42);
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.ShadowDecoration.CustomizableEdges = customizableEdges6;
            pnlHeader.Size = new Size(700, 70);
            pnlHeader.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Alexandria", 18F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(32, 143, 252);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(700, 70);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "\U0001fa91 تحديد الطاولة";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            btnClose.BorderRadius = 12;
            btnClose.CustomizableEdges = customizableEdges3;
            btnClose.FillColor = Color.FromArgb(30, 40, 55);
            btnClose.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnClose.ForeColor = Color.FromArgb(239, 68, 68);
            btnClose.Location = new Point(10, 14);
            btnClose.Name = "btnClose";
            btnClose.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnClose.Size = new Size(42, 42);
            btnClose.TabIndex = 1;
            btnClose.Text = "✕";
            // 
            // UC_TablePicker
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlContainer);
            Name = "UC_TablePicker";
            Size = new Size(700, 600);
            pnlContainer.ResumeLayout(false);
            pnlSectionsBar.ResumeLayout(false);
            pnlHeader.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ─── Control Declarations ───
        private Guna.UI2.WinForms.Guna2Panel pnlContainer;
        private Guna.UI2.WinForms.Guna2Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private Guna.UI2.WinForms.Guna2Button btnClose;
        private Guna.UI2.WinForms.Guna2Panel pnlSectionsBar;
        private System.Windows.Forms.FlowLayoutPanel flowSections;
        private System.Windows.Forms.FlowLayoutPanel flowTables;
    }
}
