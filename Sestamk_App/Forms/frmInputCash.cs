using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public class frmInputCash : Form
    {
        private Guna2Panel pnlOverlay;
        private Guna2Panel pnlCard;
        private Label lblTitle;
        private Label lblMessage;
        private Guna2TextBox txtCashAmount;
        private Guna2Button btnConfirm;
        private Guna2Button btnCancel;
        private System.Windows.Forms.Timer _fadeTimer;
        private bool _isClosing = false;
        private double _targetOpacity = 0.95;
        
        public decimal CashValue { get; private set; } = 0;

        public frmInputCash(string title, string message)
        {
            InitializeComponent();

            lblTitle.Text = title;
            lblMessage.Text = message;

            btnConfirm.Click += BtnConfirm_Click;
            btnCancel.Click += BtnCancel_Click;
            pnlOverlay.Click += BtnCancel_Click;

            this.KeyPreview = true;
            this.KeyDown += FrmConfirm_KeyDown;
            txtCashAmount.KeyDown += TxtCashAmount_KeyDown;
            txtCashAmount.KeyPress += TxtCashAmount_KeyPress;

            this.Opacity = 0;
            _fadeTimer = new System.Windows.Forms.Timer();
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += FadeTimer_Tick;

            this.Load += FrmConfirm_Load;
            this.Resize += FrmConfirm_Resize;
        }

        private void InitializeComponent()
        {
            this.pnlOverlay = new Guna.UI2.WinForms.Guna2Panel();
            this.pnlCard = new Guna.UI2.WinForms.Guna2Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblMessage = new System.Windows.Forms.Label();
            this.txtCashAmount = new Guna.UI2.WinForms.Guna2TextBox();
            this.btnConfirm = new Guna.UI2.WinForms.Guna2Button();
            this.btnCancel = new Guna.UI2.WinForms.Guna2Button();

            this.SuspendLayout();
            
            // pnlOverlay
            this.pnlOverlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.pnlOverlay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlOverlay.Location = new System.Drawing.Point(0, 0);
            this.pnlOverlay.Name = "pnlOverlay";
            this.pnlOverlay.Size = new System.Drawing.Size(1200, 800);
            this.pnlOverlay.TabIndex = 0;
            
            // pnlCard
            this.pnlCard.BackColor = System.Drawing.Color.Transparent;
            this.pnlCard.BorderRadius = 15;
            this.pnlCard.Controls.Add(this.btnCancel);
            this.pnlCard.Controls.Add(this.btnConfirm);
            this.pnlCard.Controls.Add(this.txtCashAmount);
            this.pnlCard.Controls.Add(this.lblMessage);
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(25)))), ((int)(((byte)(35)))));
            this.pnlCard.Location = new System.Drawing.Point(350, 250);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(500, 310);
            this.pnlCard.TabIndex = 1;
            
            // lblTitle
            this.lblTitle.Font = new System.Drawing.Font("Alexandria", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(460, 40);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "إنهاء الوردية";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblTitle.RightToLeft = RightToLeft.Yes;
            
            // lblMessage
            this.lblMessage.Font = new System.Drawing.Font("Alexandria", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMessage.ForeColor = System.Drawing.Color.LightGray;
            this.lblMessage.Location = new System.Drawing.Point(20, 70);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(460, 70);
            this.lblMessage.TabIndex = 1;
            this.lblMessage.Text = "الرجاء إدخال المبلغ الموجود في الدرج (الكاش الفعلي) لإنهاء الشيفت.";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblMessage.RightToLeft = RightToLeft.Yes;

            // txtCashAmount
            this.txtCashAmount.BorderRadius = 10;
            this.txtCashAmount.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCashAmount.DefaultText = "";
            this.txtCashAmount.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.txtCashAmount.ForeColor = System.Drawing.Color.White;
            this.txtCashAmount.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.txtCashAmount.Location = new System.Drawing.Point(50, 150);
            this.txtCashAmount.Name = "txtCashAmount";
            this.txtCashAmount.Size = new System.Drawing.Size(400, 50);
            this.txtCashAmount.TabIndex = 2;
            this.txtCashAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtCashAmount.PlaceholderText = "0.00";

            // btnConfirm
            this.btnConfirm.BorderRadius = 10;
            this.btnConfirm.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(204)))), ((int)(((byte)(113)))));
            this.btnConfirm.Font = new System.Drawing.Font("Alexandria", 14.25F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.Location = new System.Drawing.Point(50, 230);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(190, 50);
            this.btnConfirm.TabIndex = 3;
            this.btnConfirm.Text = "تأكيد";
            
            // btnCancel
            this.btnCancel.BorderRadius = 10;
            this.btnCancel.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnCancel.Font = new System.Drawing.Font("Alexandria", 14.25F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(260, 230);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(190, 50);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "إلغاء";
            
            // frmInputCash
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Controls.Add(this.pnlCard);
            this.Controls.Add(this.pnlOverlay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmInputCash";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "إدخال النقدية";
            this.TransparencyKey = System.Drawing.Color.Black;
            this.ResumeLayout(false);
        }

        private void TxtCashAmount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as Guna2TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtCashAmount.Text, out decimal val))
            {
                CashValue = val;
                this.DialogResult = DialogResult.Yes;
                _isClosing = true;
                _fadeTimer.Start();
            }
            else
            {
                MessageBox.Show("الرجاء إدخال مبلغ صحيح", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            _isClosing = true;
            _fadeTimer.Start();
        }

        private void FrmConfirm_Load(object sender, EventArgs e)
        {
            CenterCard();
            _fadeTimer.Start();
            txtCashAmount.Focus();
        }

        private void FrmConfirm_Resize(object sender, EventArgs e)
        {
            CenterCard();
        }

        private void CenterCard()
        {
            pnlCard.Left = (this.ClientSize.Width - pnlCard.Width) / 2;
            pnlCard.Top = (this.ClientSize.Height - pnlCard.Height) / 2;
        }

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
                    this.Close();
                }
            }
        }

        private void FrmConfirm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                BtnCancel_Click(sender, e);
                e.Handled = true;
            }
        }

        private void TxtCashAmount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnConfirm_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Static helper
        /// </summary>
        public static bool Show(string title, string message, out decimal outValue)
        {
            outValue = 0;
            using (var frm = new frmInputCash(title, message))
            {
                Form owner = null;
                if (Application.OpenForms.Count > 0)
                    owner = Application.OpenForms[0];

                if (owner != null)
                {
                    Screen screen = Screen.FromControl(owner);
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Bounds = screen.WorkingArea;
                }

                bool result = frm.ShowDialog(owner) == DialogResult.Yes;
                if (result)
                {
                    outValue = frm.CashValue;
                }
                return result;
            }
        }
    }
}
