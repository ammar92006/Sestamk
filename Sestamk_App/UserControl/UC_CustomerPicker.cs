using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    public partial class UC_CustomerPicker : System.Windows.Forms.UserControl
    {
        // ═══════════════════════════════════════════
        //  الأحداث
        // ═══════════════════════════════════════════
        public event EventHandler<Customer> OnCustomerSelected;
        public event EventHandler OnCancel;

        // ═══════════════════════════════════════════
        //  المتغيرات
        // ═══════════════════════════════════════════
        private List<Customer> _allCustomers = new List<Customer>();
        private Guna2TextBox txtSearch;
        private FlowLayoutPanel flowCustomers;
        private Guna2Button btnClose;
        private Label lblTitle;
        private Guna2Panel pnlHeader;
        private Guna2Panel pnlContainer;

        // ═══════════════════════════════════════════
        //  متغيرات السحب (Drag to scroll)
        // ═══════════════════════════════════════════
        private bool _isDragging = false;
        private Point _dragStartPoint;
        private Point _scrollStartOffset;

        public UC_CustomerPicker()
        {
            InitializeComponent();
            BuildUI();
            this.VisibleChanged += UC_CustomerPicker_VisibleChanged;
        }

        // ═══════════════════════════════════════════
        //  بناء الواجهة برمجياً
        // ═══════════════════════════════════════════
        private void BuildUI()
        {
            // ── الحاوية الرئيسية ──
            pnlContainer = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                FillColor = Color.FromArgb(16, 25, 34),
                BorderColor = Color.FromArgb(32, 143, 252),
                BorderThickness = 2,
                BorderRadius = 20,
                BackColor = Color.Transparent
            };
            this.Controls.Add(pnlContainer);

            // ── الهيدر ──
            pnlHeader = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                FillColor = Color.FromArgb(20, 30, 42),
                BackColor = Color.Transparent
            };
            pnlContainer.Controls.Add(pnlHeader);

            // عنوان
            lblTitle = new Label
            {
                Text = "تحديد العميل",
                Font = new Font("Alexandria", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlHeader.Controls.Add(lblTitle);

            // زر إغلاق
            btnClose = new Guna2Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68),
                FillColor = Color.FromArgb(30, 40, 55),
                BorderRadius = 12,
                Size = new Size(42, 42),
                Location = new Point(10, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            btnClose.Click += (s, e) => OnCancel?.Invoke(this, EventArgs.Empty);
            pnlHeader.Controls.Add(btnClose);
            btnClose.BringToFront();

            // خانة البحث
            txtSearch = new Guna2TextBox
            {
                PlaceholderText = "بحث بالاسم أو الكود أو رقم التلفون...",
                Font = new Font("Alexandria", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(24, 36, 48),
                BorderColor = Color.FromArgb(40, 55, 72),
                BorderRadius = 12,
                BorderThickness = 2,
                Size = new Size(560, 45),
                Location = new Point(20, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                TextAlign = HorizontalAlignment.Right
            };
            txtSearch.FocusedState.BorderColor = Color.FromArgb(32, 143, 252);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlHeader.Controls.Add(txtSearch);

            // ── قائمة العملاء ──
            flowCustomers = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10, 5, 10, 5)
            };
            pnlContainer.Controls.Add(flowCustomers);

            // تفعيل السحب والتمرير
            flowCustomers.MouseDown += FlowCustomers_MouseDown;
            flowCustomers.MouseMove += FlowCustomers_MouseMove;
            flowCustomers.MouseUp += FlowCustomers_MouseUp;

            // ترتيب Z-order
            flowCustomers.BringToFront();
        }

        // ═══════════════════════════════════════════
        //  عند ظهور الـ UC — تحميل العملاء
        // ═══════════════════════════════════════════
        private async void UC_CustomerPicker_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                txtSearch.Text = "";
                await LoadCustomersAsync();
                txtSearch.Focus();
            }
        }

        // ═══════════════════════════════════════════
        //  تحميل العملاء من الداتابيز
        // ═══════════════════════════════════════════
        private async Task LoadCustomersAsync()
        {
            try
            {
                string query = @"SELECT CustomerID, CustomerCode, CustomerName, Phone1, Phone2,
                                        Email, Address, CurrentBalance, AllowCredit, CreditLimit,
                                        DiscountPercent, IsActive, StopReason, Rating, Notes,
                                        CreatedDate, CreatedByUserID, LastTransactionDate
                                 FROM Customers
                                 WHERE IsActive = 1 AND IsDeleted = 0
                                 ORDER BY CustomerName";

                _allCustomers.Clear();

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _allCustomers.Add(new Customer
                            {
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                CustomerCode = reader.IsDBNull(reader.GetOrdinal("CustomerCode")) ? "" : reader.GetString(reader.GetOrdinal("CustomerCode")),
                                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? "" : reader.GetString(reader.GetOrdinal("CustomerName")),
                                Phone1 = reader.IsDBNull(reader.GetOrdinal("Phone1")) ? "" : reader.GetString(reader.GetOrdinal("Phone1")),
                                Phone2 = reader.IsDBNull(reader.GetOrdinal("Phone2")) ? "" : reader.GetString(reader.GetOrdinal("Phone2")),
                                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString(reader.GetOrdinal("Address")),
                                CurrentBalance = reader.IsDBNull(reader.GetOrdinal("CurrentBalance")) ? 0m : reader.GetDecimal(reader.GetOrdinal("CurrentBalance")),
                                AllowCredit = !reader.IsDBNull(reader.GetOrdinal("AllowCredit")) && reader.GetBoolean(reader.GetOrdinal("AllowCredit")),
                                CreditLimit = reader.IsDBNull(reader.GetOrdinal("CreditLimit")) ? 0m : reader.GetDecimal(reader.GetOrdinal("CreditLimit")),
                                DiscountPercent = reader.IsDBNull(reader.GetOrdinal("DiscountPercent")) ? 0f : Convert.ToSingle(reader.GetValue(reader.GetOrdinal("DiscountPercent"))),
                                IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            });
                        }
                    }
                }

                RenderCustomers(_allCustomers);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل العملاء: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  البحث الفوري
        // ═══════════════════════════════════════════
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(search))
            {
                RenderCustomers(_allCustomers);
                return;
            }

            var filtered = _allCustomers.Where(c =>
                (c.CustomerName != null && c.CustomerName.ToLower().Contains(search)) ||
                (c.CustomerCode != null && c.CustomerCode.ToLower().Contains(search)) ||
                (c.Phone1 != null && c.Phone1.Contains(search)) ||
                (c.Phone2 != null && c.Phone2.Contains(search))
            ).ToList();

            RenderCustomers(filtered);
        }

        // ═══════════════════════════════════════════
        //  رسم كروت العملاء
        // ═══════════════════════════════════════════
        private void RenderCustomers(List<Customer> customers)
        {
            flowCustomers.SuspendLayout();

            while (flowCustomers.Controls.Count > 0)
            {
                var ctrl = flowCustomers.Controls[0];
                flowCustomers.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            foreach (var customer in customers)
            {
                var card = CreateCustomerCard(customer);
                flowCustomers.Controls.Add(card);
            }

            flowCustomers.ResumeLayout(true);
        }

        // ═══════════════════════════════════════════
        //  أحداث السحب (Drag to scroll)
        // ═══════════════════════════════════════════
        private void FlowCustomers_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _dragStartPoint = e.Location;
                _scrollStartOffset = flowCustomers.AutoScrollPosition;
                flowCustomers.Cursor = Cursors.SizeAll;
            }
        }

        private void FlowCustomers_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            int deltaY = e.Y - _dragStartPoint.Y;
            flowCustomers.AutoScrollPosition = new Point(0, Math.Abs(_scrollStartOffset.Y) - deltaY);
        }

        private void FlowCustomers_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            flowCustomers.Cursor = Cursors.Default;
        }

        // ═══════════════════════════════════════════
        //  إنشاء كارت عميل واحد
        // ═══════════════════════════════════════════
        private Guna2Panel CreateCustomerCard(Customer customer)
        {
            int cardWidth = flowCustomers.ClientSize.Width - 30;

            var card = new Guna2Panel
            {
                Size = new Size(cardWidth, 110),
                FillColor = Color.FromArgb(24, 36, 48),
                BorderRadius = 14,
                Margin = new Padding(5, 4, 5, 4),
                Cursor = Cursors.Hand,
                Tag = customer,
                BackColor = Color.Transparent
            };

            // اسم العميل
            var lblName = new Label
            {
                Text = customer.CustomerName,
                Font = new Font("Alexandria", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 245, 249),
                Location = new Point(cardWidth - 280, 10),
                Size = new Size(270, 28),
                TextAlign = ContentAlignment.MiddleRight
            };

            // الكود
            var lblCode = new Label
            {
                Text = $"#{customer.CustomerCode}",
                Font = new Font("Alexandria", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(120, 135, 155),
                Location = new Point(cardWidth - 280, 40),
                Size = new Size(120, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            // التلفون
            var lblPhone = new Label
            {
                Text = $"📞 {customer.Phone1}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(120, 135, 155),
                Location = new Point(cardWidth - 160, 40),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            // الرصيد
            var lblBalance = new Label
            {
                Text = $"الرصيد: {customer.CurrentBalance:N2} ج",
                Font = new Font("Alexandria", 10F, FontStyle.Bold),
                ForeColor = customer.CurrentBalance > 0
                    ? Color.FromArgb(239, 68, 68)   // أحمر لو عليه رصيد
                    : Color.FromArgb(45, 204, 113),  // أخضر لو ملوش
                Location = new Point(cardWidth - 280, 68),
                Size = new Size(180, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            // حد الائتمان
            var lblCredit = new Label
            {
                Text = customer.AllowCredit
                    ? $"حد الائتمان: {customer.CreditLimit:N2} ج"
                    : "لا يسمح بالآجل",
                Font = new Font("Alexandria", 9F),
                ForeColor = customer.AllowCredit
                    ? Color.FromArgb(47, 143, 238)
                    : Color.FromArgb(100, 110, 125),
                Location = new Point(cardWidth - 280, 90),
                Size = new Size(200, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            // شريط جانبي ملون
            var sideBar = new Guna2Panel
            {
                Size = new Size(6, 94),
                Location = new Point(cardWidth - 8, 8),
                FillColor = customer.AllowCredit
                    ? Color.FromArgb(32, 143, 252)
                    : Color.FromArgb(100, 110, 125),
                BorderRadius = 3,
                BackColor = Color.Transparent
            };

            // أيقونة اختيار
            var lblSelectIcon = new Label
            {
                Text = "اختيار ←",
                Font = new Font("Alexandria", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 204, 113),
                Location = new Point(10, 42),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            // إضافة الكنترولز
            card.Controls.Add(lblName);
            card.Controls.Add(lblCode);
            card.Controls.Add(lblPhone);
            card.Controls.Add(lblBalance);
            card.Controls.Add(lblCredit);
            card.Controls.Add(sideBar);
            card.Controls.Add(lblSelectIcon);

            // حدث النقر — على كل الكنترولز
            EventHandler clickHandler = (s, ev) =>
            {
                OnCustomerSelected?.Invoke(this, customer);
            };

            card.Click += clickHandler;
            lblName.Click += clickHandler;
            lblCode.Click += clickHandler;
            lblPhone.Click += clickHandler;
            lblBalance.Click += clickHandler;
            lblCredit.Click += clickHandler;
            lblSelectIcon.Click += clickHandler;

            // تأثير Hover
            EventHandler enterHandler = (s, ev) => card.FillColor = Color.FromArgb(32, 48, 65);
            EventHandler leaveHandler = (s, ev) => card.FillColor = Color.FromArgb(24, 36, 48);

            card.MouseEnter += enterHandler;
            card.MouseLeave += leaveHandler;

            foreach (Control c in card.Controls)
            {
                c.MouseEnter += enterHandler;
                c.MouseLeave += leaveHandler;
                c.Cursor = Cursors.Hand;
                
                // تمرير أحداث الماوس لتفعيل السحب حتى لو المستخدم ضغط على الكنترول الداخلي
                c.MouseDown += (s, ev) => 
                {
                    if (ev.Button == MouseButtons.Left)
                    {
                        _isDragging = true;
                        _dragStartPoint = flowCustomers.PointToClient(c.PointToScreen(ev.Location));
                        _scrollStartOffset = flowCustomers.AutoScrollPosition;
                        flowCustomers.Cursor = Cursors.SizeAll;
                    }
                };
                c.MouseMove += (s, ev) => 
                {
                    if (!_isDragging) return;
                    var curPos = flowCustomers.PointToClient(c.PointToScreen(ev.Location));
                    int deltaY = curPos.Y - _dragStartPoint.Y;
                    flowCustomers.AutoScrollPosition = new Point(0, Math.Abs(_scrollStartOffset.Y) - deltaY);
                };
                c.MouseUp += (s, ev) => 
                {
                    _isDragging = false;
                    flowCustomers.Cursor = Cursors.Default;
                };
            }

            return card;
        }
    }
}
