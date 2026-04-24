using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmHold_Invoices : BaseForm
    {
        private FlowLayoutPanel flowInvoices;
        private List<frmSales.HeldInvoice> _allHeldInvoices;
        
        public frmSales.HeldInvoice SelectedInvoiceToRestore { get; private set; }

        public frmHold_Invoices()
        {
            InitializeComponent();
            SetupUI();
            this.Load += FrmHold_Invoices_Load;
            
            // إضافة زر الإغلاق كخيار للعودة بدون اختيار
            var closeBtn = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "رجوع",
                Font = new Font("Alexandria", 12F, FontStyle.Bold),
                FillColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                BorderRadius = 10,
                Size = new Size(118, 45),
                Location = new Point(12, 17)
            };
            closeBtn.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            guna2Panel1.Controls.Add(closeBtn);
        }

        private void SetupUI()
        {
            flowInvoices = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.LeftToRight
            };
            this.Controls.Add(flowInvoices);
            flowInvoices.BringToFront(); // Ensure it's above the background but below panels if they are docked top/bottom
            
            guna2TextBox1.TextChanged += Guna2TextBox1_TextChanged;
        }

        private void FrmHold_Invoices_Load(object sender, EventArgs e)
        {
            _allHeldInvoices = frmSales.GetHeldInvoices();
            RenderInvoices(_allHeldInvoices);
        }

        private void Guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            string search = guna2TextBox1.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(search))
            {
                RenderInvoices(_allHeldInvoices);
                return;
            }

            var filtered = _allHeldInvoices.FindAll(inv =>
                (inv.CustomerName != null && inv.CustomerName.ToLower().Contains(search))
            );
            RenderInvoices(filtered);
        }

        private void RenderInvoices(List<frmSales.HeldInvoice> invoices)
        {
            flowInvoices.SuspendLayout();
            
            while (flowInvoices.Controls.Count > 0)
            {
                var ctrl = flowInvoices.Controls[0];
                flowInvoices.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            decimal totalAmounts = 0;

            foreach (var inv in invoices)
            {
                totalAmounts += inv.TotalAmount;
                var card = CreateInvoiceCard(inv);
                flowInvoices.Controls.Add(card);
            }

            label4.Text = invoices.Count.ToString();
            label3.Text = $"{totalAmounts:N2} ج م";

            flowInvoices.ResumeLayout(true);
        }

        private Guna.UI2.WinForms.Guna2Panel CreateInvoiceCard(frmSales.HeldInvoice inv)
        {
            var card = new Guna.UI2.WinForms.Guna2Panel
            {
                Size = new Size(300, 160),
                FillColor = Color.FromArgb(31, 41, 55),
                BorderRadius = 15,
                Margin = new Padding(15),
                Cursor = Cursors.Hand
            };

            var lblName = new Label
            {
                Text = string.IsNullOrEmpty(inv.CustomerName) ? "عميل غير محدد" : inv.CustomerName,
                Font = new Font("Alexandria", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 15),
                Size = new Size(280, 30),
                TextAlign = ContentAlignment.MiddleRight
            };

            var lblType = new Label
            {
                Text = inv.InvoiceType == 0 ? "تيك اوي" : (inv.InvoiceType == 1 ? "صالة" : "توصيل"),
                Font = new Font("Alexandria", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 159, 165),
                Location = new Point(10, 50),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            var lblTime = new Label
            {
                Text = inv.HoldTime.ToString("hh:mm tt"),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(148, 159, 165),
                Location = new Point(10, 75),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            var lblTotal = new Label
            {
                Text = $"{inv.TotalAmount:N2} ج م",
                Font = new Font("Alexandria", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 204, 113),
                Location = new Point(10, 110),
                Size = new Size(280, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            var btnRestore = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "استرجاع",
                Font = new Font("Alexandria", 10F, FontStyle.Bold),
                FillColor = Color.FromArgb(32, 143, 252),
                ForeColor = Color.White,
                BorderRadius = 8,
                Size = new Size(100, 35),
                Location = new Point(180, 110)
            };

            EventHandler clickHandler = (s, e) =>
            {
                SelectedInvoiceToRestore = inv;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            card.Click += clickHandler;
            lblName.Click += clickHandler;
            lblType.Click += clickHandler;
            lblTime.Click += clickHandler;
            lblTotal.Click += clickHandler;
            btnRestore.Click += clickHandler;

            card.Controls.Add(lblName);
            card.Controls.Add(lblType);
            card.Controls.Add(lblTime);
            card.Controls.Add(lblTotal);
            card.Controls.Add(btnRestore);

            return card;
        }
    }
}
