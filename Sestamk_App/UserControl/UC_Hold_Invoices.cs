using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sestamk.Classes;

namespace Sestamk.UserControl
{
    public partial class UC_Hold_Invoices : System.Windows.Forms.UserControl
    {
        public event EventHandler OnRestore;
        public event EventHandler OnDelete;
        public event EventHandler OnView;

        public HeldInvoiceDto Invoice { get; private set; }

        public UC_Hold_Invoices()
        {
            InitializeComponent();
        }

        public UC_Hold_Invoices(HeldInvoiceDto invoice) : this()
        {
            Invoice = invoice;
            BuildCard();
        }

        private void BuildCard()
        {
            if (Invoice == null) return;

            this.SuspendLayout();

            foreach (Control c in guna2Panel2.Controls.OfType<Control>().ToList())
            {
                if (c.Name != null && (c.Name.StartsWith("lblItem_") || c.Name == "lblMoreItems"))
                {
                    c.Dispose();
                }
            }

            string currency = Sestamk.Classes.SettingsService.CurrencyName;

            label1.Text = Invoice.HoldTime.ToString("hh:mm tt");

            label3.Text = Invoice.InvoiceNumber ?? "#0000";

            string typeIcon = Invoice.InvoiceType switch
            {
                0 => "🛍",
                1 => "🍽",
                2 => "🚗",
                _ => "📋"
            };
            label4.Text = $"{typeIcon}  {Invoice.DisplayTitle}";

            int yPos = 115;
            int maxItems = Math.Min(Invoice.Items.Count, 3);
            for (int i = 0; i < maxItems; i++)
            {
                var item = Invoice.Items[i];
                var lblItem = new Label
                {
                    Text = $"{item.Quantity}x {item.ItemName}",
                    Font = new Font("Alexandria", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(170, 175, 180),
                    Location = new Point(100, yPos),
                    Size = new Size(230, 20),
                    TextAlign = ContentAlignment.MiddleRight,
                    Name = $"lblItem_{i}"
                };
                guna2Panel2.Controls.Add(lblItem);
                yPos += 22;
            }

            if (Invoice.Items.Count > 3)
            {
                int remaining = Invoice.Items.Count - 3;
                var lblMore = new Label
                {
                    Text = $"+ {remaining} أصناف أخرى",
                    Font = new Font("Alexandria", 9F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(130, 135, 140),
                    Location = new Point(100, yPos),
                    Size = new Size(230, 20),
                    TextAlign = ContentAlignment.MiddleRight,
                    Name = "lblMoreItems"
                };
                guna2Panel2.Controls.Add(lblMore);
            }

            lblTotalAmount.Text = $"{Invoice.TotalAmount:N2} {currency}";

            this.ResumeLayout(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            OnDelete?.Invoke(this, EventArgs.Empty);
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            OnView?.Invoke(this, EventArgs.Empty);
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            OnRestore?.Invoke(this, EventArgs.Empty);
        }
    }
}
