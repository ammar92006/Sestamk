namespace Sestamk.UserControl
{
    partial class ProductCard :  System.Windows.Forms.UserControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges9 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges10 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            guna2Panel2 = new Guna.UI2.WinForms.Guna2Panel();
            btnAdd = new Guna.UI2.WinForms.Guna2Button();
            lblDescription = new Label();
            lblName = new Label();
            picProduct = new Guna.UI2.WinForms.Guna2PictureBox();
            btn_add_product = new Guna.UI2.WinForms.Guna2PictureBox();
            guna2Panel1.SuspendLayout();
            guna2Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picProduct).BeginInit();
            ((System.ComponentModel.ISupportInitialize)btn_add_product).BeginInit();
            SuspendLayout();
            // 
            // guna2Panel1
            // 
            guna2Panel1.Controls.Add(guna2Panel2);
            guna2Panel1.CustomizableEdges = customizableEdges9;
            guna2Panel1.Dock = DockStyle.Fill;
            guna2Panel1.Location = new Point(0, 0);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges10;
            guna2Panel1.Size = new Size(220, 260);
            guna2Panel1.TabIndex = 2;
            // 
            // guna2Panel2
            // 
            guna2Panel2.BorderRadius = 20;
            guna2Panel2.Controls.Add(btnAdd);
            guna2Panel2.Controls.Add(lblDescription);
            guna2Panel2.Controls.Add(lblName);
            guna2Panel2.Controls.Add(picProduct);
            guna2Panel2.Controls.Add(btn_add_product);
            guna2Panel2.CustomizableEdges = customizableEdges7;
            guna2Panel2.Dock = DockStyle.Fill;
            guna2Panel2.FillColor = Color.FromArgb(27, 36, 47);
            guna2Panel2.Location = new Point(0, 0);
            guna2Panel2.Name = "guna2Panel2";
            guna2Panel2.ShadowDecoration.CustomizableEdges = customizableEdges8;
            guna2Panel2.Size = new Size(220, 260);
            guna2Panel2.TabIndex = 0;
            // 
            // btnAdd
            // 
            btnAdd.BackColor = Color.FromArgb(27, 36, 47);
            btnAdd.CustomizableEdges = customizableEdges1;
            btnAdd.DisabledState.BorderColor = Color.DarkGray;
            btnAdd.DisabledState.CustomBorderColor = Color.DarkGray;
            btnAdd.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnAdd.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnAdd.Dock = DockStyle.Fill;
            btnAdd.FillColor = Color.FromArgb(27, 36, 47);
            btnAdd.Font = new Font("Segoe UI", 9F);
            btnAdd.ForeColor = Color.White;
            btnAdd.Image = Properties.Resources.add_to_cart__1_;
            btnAdd.ImageSize = new Size(64, 64);
            btnAdd.Location = new Point(0, 200);
            btnAdd.Name = "btnAdd";
            btnAdd.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnAdd.Size = new Size(70, 60);
            btnAdd.TabIndex = 8;
            // 
            // lblDescription
            // 
            lblDescription.BackColor = Color.FromArgb(27, 36, 47);
            lblDescription.Dock = DockStyle.Right;
            lblDescription.Font = new Font("Alexandria", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDescription.ForeColor = Color.FromArgb(62, 144, 255);
            lblDescription.Location = new Point(70, 200);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(150, 60);
            lblDescription.TabIndex = 6;
            lblDescription.Text = "label2";
            lblDescription.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblName
            // 
            lblName.BackColor = Color.FromArgb(27, 36, 47);
            lblName.Dock = DockStyle.Top;
            lblName.Font = new Font("Alexandria", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblName.ForeColor = SystemColors.ControlLightLight;
            lblName.Location = new Point(0, 150);
            lblName.Name = "lblName";
            lblName.Size = new Size(220, 50);
            lblName.TabIndex = 5;
            lblName.Text = "label1";
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // picProduct
            // 
            picProduct.CustomizableEdges = customizableEdges3;
            picProduct.Dock = DockStyle.Top;
            picProduct.ImageRotate = 0F;
            picProduct.Location = new Point(0, 0);
            picProduct.Name = "picProduct";
            picProduct.ShadowDecoration.CustomizableEdges = customizableEdges4;
            picProduct.Size = new Size(220, 150);
            picProduct.SizeMode = PictureBoxSizeMode.StretchImage;
            picProduct.TabIndex = 4;
            picProduct.TabStop = false;
            // 
            // btn_add_product
            // 
            btn_add_product.CustomizableEdges = customizableEdges5;
            btn_add_product.Image = Properties.Resources.plus;
            btn_add_product.ImageRotate = 0F;
            btn_add_product.Location = new Point(28, 64);
            btn_add_product.Name = "btn_add_product";
            btn_add_product.ShadowDecoration.CustomizableEdges = customizableEdges6;
            btn_add_product.Size = new Size(106, 83);
            btn_add_product.SizeMode = PictureBoxSizeMode.CenterImage;
            btn_add_product.TabIndex = 7;
            btn_add_product.TabStop = false;
            // 
            // ProductCard
            // 
            BackColor = Color.Transparent;
            Controls.Add(guna2Panel1);
            Name = "ProductCard";
            RightToLeft = RightToLeft.Yes;
            Size = new Size(220, 260);
            guna2Panel1.ResumeLayout(false);
            guna2Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picProduct).EndInit();
            ((System.ComponentModel.ISupportInitialize)btn_add_product).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel2;
        private Label lblName;
        private Guna.UI2.WinForms.Guna2PictureBox picProduct;
        private Guna.UI2.WinForms.Guna2PictureBox btn_add_product;
        private Label lblDescription;
        private Guna.UI2.WinForms.Guna2Button btnAdd;
    }
}
