using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    public partial class UC_DriverPicker : System.Windows.Forms.UserControl
    {
        // ═══════════════════════════════════════════
        //  الأحداث
        // ═══════════════════════════════════════════
        public event EventHandler<DeliveryStaffModel> OnDriverSelected;
        public event EventHandler OnCancel;

        // ═══════════════════════════════════════════
        //  المتغيرات
        // ═══════════════════════════════════════════
        private List<DeliveryStaffModel> _allDrivers = new List<DeliveryStaffModel>();

        // ─── متغيرات التحريك الناعم ───
        private bool _isDragging = false;
        private Point _mouseOffset;
        private Point _targetLocation;
        private System.Windows.Forms.Timer _smoothTimer;
        private const float LERP_SPEED = 0.2f;

        public UC_DriverPicker()
        {
            InitializeComponent();
            SetupEvents();
            SetupDragAnimation();
        }

        // ═══════════════════════════════════════════
        //  ربط الأحداث
        // ═══════════════════════════════════════════
        private void SetupEvents()
        {
            btnClose.Click += (s, e) => OnCancel?.Invoke(this, EventArgs.Empty);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            this.VisibleChanged += UC_DriverPicker_VisibleChanged;
        }

        // ═══════════════════════════════════════════
        //  إعداد التحريك الناعم (سحب بالماوس)
        // ═══════════════════════════════════════════
        private void SetupDragAnimation()
        {
            _smoothTimer = new System.Windows.Forms.Timer();
            _smoothTimer.Interval = 10;
            _smoothTimer.Tick += SmoothTimer_Tick;

            // ربط أحداث السحب على العنوان والهيدر
            lblTitle.MouseDown += DragHandle_MouseDown;
            lblTitle.MouseMove += DragHandle_MouseMove;
            lblTitle.MouseUp += DragHandle_MouseUp;
            lblTitle.MouseEnter += (s, e) => { lblTitle.Cursor = Cursors.SizeAll; };
            lblTitle.MouseLeave += (s, e) => { lblTitle.Cursor = Cursors.Default; };
        }

        private void DragHandle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && this.Parent != null)
            {
                _isDragging = true;
                _targetLocation = this.Location;
                this.Cursor = Cursors.SizeAll;

                Point cursorInParent = this.Parent.PointToClient(Cursor.Position);
                _mouseOffset = new Point(
                    cursorInParent.X - this.Left,
                    cursorInParent.Y - this.Top
                );

                ((Control)sender).Capture = true;
                _smoothTimer.Start();
            }
        }

        private void DragHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || this.Parent == null) return;

            Point cursorInParent = this.Parent.PointToClient(Cursor.Position);
            int newX = Math.Max(0, Math.Min(cursorInParent.X - _mouseOffset.X, this.Parent.ClientSize.Width - this.Width));
            int newY = Math.Max(0, Math.Min(cursorInParent.Y - _mouseOffset.Y, this.Parent.ClientSize.Height - this.Height));
            _targetLocation = new Point(newX, newY);
        }

        private void DragHandle_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            ((Control)sender).Capture = false;
            this.Cursor = Cursors.Default;
        }

        private void SmoothTimer_Tick(object sender, EventArgs e)
        {
            float newX = this.Left + (_targetLocation.X - this.Left) * LERP_SPEED;
            float newY = this.Top + (_targetLocation.Y - this.Top) * LERP_SPEED;
            this.Location = new Point((int)newX, (int)newY);

            if (!_isDragging &&
                Math.Abs(this.Left - _targetLocation.X) < 1 &&
                Math.Abs(this.Top - _targetLocation.Y) < 1)
            {
                this.Location = _targetLocation;
                _smoothTimer.Stop();
            }
        }

        // ═══════════════════════════════════════════
        //  عند ظهور الـ UC — تحميل الطيارين
        // ═══════════════════════════════════════════
        private async void UC_DriverPicker_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                txtSearch.Text = "";
                await LoadDriversAsync();
                txtSearch.Focus();
            }
        }

        // ═══════════════════════════════════════════
        //  تحميل الطيارين من الداتابيز
        // ═══════════════════════════════════════════
        private async Task LoadDriversAsync()
        {
            try
            {
                string query = @"SELECT delivery_id, full_name, phone, vehicle_type, 
                                        license_number, delivery_fee, salary_type,
                                        shift_start, shift_end, is_active, hire_date, notes
                                 FROM delivery_staff
                                 WHERE is_active = 1
                                 ORDER BY full_name";

                _allDrivers.Clear();

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _allDrivers.Add(new DeliveryStaffModel
                            {
                                DeliveryId = reader.GetInt32(reader.GetOrdinal("delivery_id")),
                                FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) ? "" : reader.GetString(reader.GetOrdinal("full_name")),
                                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? "" : reader.GetString(reader.GetOrdinal("phone")),
                                VehicleType = reader.IsDBNull(reader.GetOrdinal("vehicle_type")) ? "" : reader.GetString(reader.GetOrdinal("vehicle_type")),
                                LicenseNumber = reader.IsDBNull(reader.GetOrdinal("license_number")) ? "" : reader.GetString(reader.GetOrdinal("license_number")),
                                DeliveryFee = reader.IsDBNull(reader.GetOrdinal("delivery_fee")) ? 0m : reader.GetDecimal(reader.GetOrdinal("delivery_fee")),
                                SalaryType = reader.IsDBNull(reader.GetOrdinal("salary_type")) ? "" : reader.GetString(reader.GetOrdinal("salary_type")),
                                IsActive = !reader.IsDBNull(reader.GetOrdinal("is_active")) && reader.GetBoolean(reader.GetOrdinal("is_active")),
                            });
                        }
                    }
                }

                RenderDrivers(_allDrivers);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الطيارين: " + ex.Message);
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
                RenderDrivers(_allDrivers);
                return;
            }

            var filtered = _allDrivers.Where(d =>
                (d.FullName != null && d.FullName.ToLower().Contains(search)) ||
                (d.Phone != null && d.Phone.Contains(search))
            ).ToList();

            RenderDrivers(filtered);
        }

        // ═══════════════════════════════════════════
        //  رسم كروت الطيارين
        // ═══════════════════════════════════════════
        private void RenderDrivers(List<DeliveryStaffModel> drivers)
        {
            flowDrivers.SuspendLayout();

            while (flowDrivers.Controls.Count > 0)
            {
                var ctrl = flowDrivers.Controls[0];
                flowDrivers.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            foreach (var driver in drivers)
            {
                var card = CreateDriverCard(driver);
                flowDrivers.Controls.Add(card);
            }

            flowDrivers.ResumeLayout(true);
        }

        // ═══════════════════════════════════════════
        //  إنشاء كارت طيار واحد
        // ═══════════════════════════════════════════
        private Guna2Panel CreateDriverCard(DeliveryStaffModel driver)
        {
            int cardWidth = flowDrivers.ClientSize.Width - 30;

            var card = new Guna2Panel
            {
                Size = new Size(cardWidth, 100),
                FillColor = Color.FromArgb(24, 36, 48),
                BorderRadius = 14,
                Margin = new Padding(5, 4, 5, 4),
                Cursor = Cursors.Hand,
                Tag = driver,
                BackColor = Color.Transparent
            };

            // اسم الطيار
            var lblName = new Label
            {
                Text = driver.FullName,
                Font = new Font("Alexandria", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 245, 249),
                Location = new Point(cardWidth - 280, 10),
                Size = new Size(270, 28),
                TextAlign = ContentAlignment.MiddleRight
            };

            // التلفون
            var lblPhone = new Label
            {
                Text = $"📞 {driver.Phone}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(120, 135, 155),
                Location = new Point(cardWidth - 280, 40),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            // نوع المركبة
            var lblVehicle = new Label
            {
                Text = driver.VehicleTypeText,
                Font = new Font("Alexandria", 10F),
                ForeColor = Color.FromArgb(245, 158, 11),
                Location = new Point(cardWidth - 130, 40),
                Size = new Size(120, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            // سعر التوصيلة
            var lblFee = new Label
            {
                Text = $"التوصيلة: {driver.DeliveryFee:N2} {SettingsService.CurrencyName}",
                Font = new Font("Alexandria", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 204, 113),
                Location = new Point(cardWidth - 280, 66),
                Size = new Size(270, 25),
                TextAlign = ContentAlignment.MiddleRight
            };

            // شريط جانبي
            var sideBar = new Guna2Panel
            {
                Size = new Size(6, 84),
                Location = new Point(cardWidth - 8, 8),
                FillColor = Color.FromArgb(245, 158, 11),
                BorderRadius = 3,
                BackColor = Color.Transparent
            };

            // أيقونة اختيار
            var lblSelect = new Label
            {
                Text = "اختيار ←",
                Font = new Font("Alexandria", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 204, 113),
                Location = new Point(10, 38),
                Size = new Size(80, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            // إضافة الكنترولز
            card.Controls.Add(lblName);
            card.Controls.Add(lblPhone);
            card.Controls.Add(lblVehicle);
            card.Controls.Add(lblFee);
            card.Controls.Add(sideBar);
            card.Controls.Add(lblSelect);

            // حدث النقر
            EventHandler clickHandler = (s, ev) =>
            {
                OnDriverSelected?.Invoke(this, driver);
            };

            card.Click += clickHandler;
            lblName.Click += clickHandler;
            lblPhone.Click += clickHandler;
            lblVehicle.Click += clickHandler;
            lblFee.Click += clickHandler;
            lblSelect.Click += clickHandler;

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
            }

            return card;
        }
    }
}
