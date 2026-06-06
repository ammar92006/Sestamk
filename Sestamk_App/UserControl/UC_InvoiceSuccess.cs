using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Sestamk.Classes;
using Sestamk.Classes.Data;

namespace Sestamk.UserControl
{
    public class UC_InvoiceSuccess : System.Windows.Forms.UserControl
    {
        private Guna2Panel mainPanel;
        private Guna2CirclePictureBox iconSuccess;
        private Label lblTitle;
        private Label lblInvoiceNumber;
        private Label lblTotalAmount;
        private Guna2Button btnPrintInvoice;
        private Guna2Button btnPrintKitchen;
        private Guna2Button btnWhatsApp;
        private Guna2Button btnNewOrder;

        public event EventHandler OnNewOrderRequested;
        public event EventHandler OnPrintInvoiceRequested;
        public event EventHandler OnPrintKitchenRequested;
        public event EventHandler OnWhatsAppRequested;

        public UC_InvoiceSuccess(OrderModel order)
        {
            InitializeComponents();
            
            lblInvoiceNumber.Text = $"رقم الفاتورة: {order.OrderNumber}";
            lblTotalAmount.Text = $"الإجمالي: {order.TotalAmount:N2} {SettingsService.CurrencySymbol}";
        }

        private void InitializeComponents()
        {
            this.Size = new Size(500, 520);
            this.BackColor = Color.Transparent;

            mainPanel = new Guna2Panel
            {
                Size = new Size(500, 520),
                Location = new Point(0, 0),
                FillColor = Color.FromArgb(26, 31, 43), // لون متناسق مع الدارك مود
                BorderRadius = 20,
                BorderColor = Color.FromArgb(45, 204, 113),
                BorderThickness = 2,
                ShadowDecoration = { Enabled = true, Shadow = new Padding(5, 5, 5, 5) }
            };
            this.Controls.Add(mainPanel);

            // أيقونة النجاح
            iconSuccess = new Guna2CirclePictureBox
            {
                Size = new Size(80, 80),
                Location = new Point((this.Width - 80) / 2, 30),
                FillColor = Color.FromArgb(45, 204, 113), // أخضر
                SizeMode = PictureBoxSizeMode.Zoom
            };
            // رسم علامة صح برمجياً
            iconSuccess.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen pen = new Pen(Color.White, 5);
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                e.Graphics.DrawLines(pen, new Point[] { 
                    new Point(20, 40), 
                    new Point(35, 55), 
                    new Point(60, 25) 
                });
            };
            mainPanel.Controls.Add(iconSuccess);

            // العنوان
            lblTitle = new Label
            {
                Text = "تم حفظ الفاتورة بنجاح",
                Font = new Font("Alexandria", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 40),
                Location = new Point(50, 130)
            };
            mainPanel.Controls.Add(lblTitle);

            // رقم الفاتورة
            lblInvoiceNumber = new Label
            {
                Text = "رقم الفاتورة: ---",
                Font = new Font("Alexandria", 14F),
                ForeColor = Color.FromArgb(170, 170, 170),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 30),
                Location = new Point(50, 180)
            };
            mainPanel.Controls.Add(lblInvoiceNumber);

            // الإجمالي
            lblTotalAmount = new Label
            {
                Text = "الإجمالي: 0.00",
                Font = new Font("Alexandria", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 204, 113),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(400, 35),
                Location = new Point(50, 220)
            };
            mainPanel.Controls.Add(lblTotalAmount);

            // زر طباعة الفاتورة
            btnPrintInvoice = new Guna2Button
            {
                Text = "طباعة الفاتورة",
                Font = new Font("Alexandria", 12F, FontStyle.Bold),
                Size = new Size(180, 45),
                Location = new Point(260, 280),
                BorderRadius = 10,
                FillColor = Color.FromArgb(59, 130, 246), // أزرق
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnPrintInvoice.Click += (s, e) => OnPrintInvoiceRequested?.Invoke(this, EventArgs.Empty);
            mainPanel.Controls.Add(btnPrintInvoice);

            // زر طباعة للمطبخ
            btnPrintKitchen = new Guna2Button
            {
                Text = "طباعة للمطبخ",
                Font = new Font("Alexandria", 12F, FontStyle.Bold),
                Size = new Size(180, 45),
                Location = new Point(60, 280),
                BorderRadius = 10,
                FillColor = Color.FromArgb(245, 158, 11), // برتقالي
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnPrintKitchen.Click += (s, e) => OnPrintKitchenRequested?.Invoke(this, EventArgs.Empty);
            mainPanel.Controls.Add(btnPrintKitchen);

            // زر إرسال واتساب
            btnWhatsApp = new Guna2Button
            {
                Text = "إرسال واتساب 💬",
                Font = new Font("Alexandria", 12F, FontStyle.Bold),
                Size = new Size(380, 45),
                Location = new Point(60, 340),
                BorderRadius = 10,
                FillColor = Color.FromArgb(37, 211, 102), // لون واتساب الرسمي
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnWhatsApp.Click += (s, e) => OnWhatsAppRequested?.Invoke(this, EventArgs.Empty);
            mainPanel.Controls.Add(btnWhatsApp);

            // زر فاتورة جديدة (متابعة)
            btnNewOrder = new Guna2Button
            {
                Text = "فاتورة جديدة / متابعة",
                Font = new Font("Alexandria", 14F, FontStyle.Bold),
                Size = new Size(380, 50),
                Location = new Point(60, 410),
                BorderRadius = 10,
                FillColor = Color.FromArgb(45, 204, 113), // أخضر
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnNewOrder.Click += (s, e) => OnNewOrderRequested?.Invoke(this, EventArgs.Empty);
            mainPanel.Controls.Add(btnNewOrder);
        }
    }
}
