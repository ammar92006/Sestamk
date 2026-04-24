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
    public partial class UC_TablePicker : System.Windows.Forms.UserControl
    {
        // ═══════════════════════════════════════════
        //  الأحداث
        // ═══════════════════════════════════════════
        public event EventHandler<TableModel> OnTableSelected;
        public event EventHandler OnCancel;

        // ═══════════════════════════════════════════
        //  المتغيرات
        // ═══════════════════════════════════════════
        private List<SectionModel> _sections = new List<SectionModel>();
        private List<TableModel> _allTables = new List<TableModel>();
        private long _selectedSectionId = -1; // -1 = الكل
        private Guna2Button _selectedSectionBtn = null;

        // ─── متغيرات التحريك الناعم ───
        private bool _isDragging = false;
        private Point _mouseOffset;
        private Point _targetLocation;
        private System.Windows.Forms.Timer _smoothTimer;
        private const float LERP_SPEED = 0.2f;

        public UC_TablePicker()
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
            this.VisibleChanged += UC_TablePicker_VisibleChanged;
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
        //  عند ظهور الـ UC — تحميل البيانات
        // ═══════════════════════════════════════════
        private async void UC_TablePicker_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                await LoadSectionsAsync();
                await LoadTablesAsync();
                RenderSections();
                RenderTables();
            }
        }

        // ═══════════════════════════════════════════
        //  تحميل الأقسام
        // ═══════════════════════════════════════════
        private async Task LoadSectionsAsync()
        {
            try
            {
                string query = @"SELECT Id, SectionName, Description, SortOrder, IsActive
                                 FROM Sections
                                 WHERE IsActive = 1 AND DeletedAt IS NULL
                                 ORDER BY SortOrder, SectionName";

                _sections.Clear();

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _sections.Add(new SectionModel
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                SectionName = reader.IsDBNull(reader.GetOrdinal("SectionName")) ? "" : reader.GetString(reader.GetOrdinal("SectionName")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                                SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الأقسام: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  تحميل الطاولات
        // ═══════════════════════════════════════════
        private async Task LoadTablesAsync()
        {
            try
            {
                string query = @"SELECT t.Id, t.SectionId, t.TableNumber, t.TableName, 
                                        t.Capacity, t.Status, t.IsActive, t.Notes,
                                        t.WaiterId, t.OpenedAt,
                                        s.SectionName
                                 FROM Tables t
                                 LEFT JOIN Sections s ON t.SectionId = s.Id
                                 WHERE t.IsActive = 1 AND t.DeletedAt IS NULL
                                 ORDER BY s.SortOrder, t.TableNumber";

                _allTables.Clear();

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            _allTables.Add(new TableModel
                            {
                                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                                SectionId = reader.GetInt64(reader.GetOrdinal("SectionId")),
                                TableNumber = reader.IsDBNull(reader.GetOrdinal("TableNumber")) ? "" : reader.GetString(reader.GetOrdinal("TableNumber")),
                                TableName = reader.IsDBNull(reader.GetOrdinal("TableName")) ? "" : reader.GetString(reader.GetOrdinal("TableName")),
                                Capacity = reader.GetInt32(reader.GetOrdinal("Capacity")),
                                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? "available" : reader.GetString(reader.GetOrdinal("Status")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? "" : reader.GetString(reader.GetOrdinal("Notes")),
                                WaiterId = reader.IsDBNull(reader.GetOrdinal("WaiterId")) ? null : reader.GetInt32(reader.GetOrdinal("WaiterId")),
                                OpenedAt = reader.IsDBNull(reader.GetOrdinal("OpenedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("OpenedAt")),
                                SectionName = reader.IsDBNull(reader.GetOrdinal("SectionName")) ? "" : reader.GetString(reader.GetOrdinal("SectionName")),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل الطاولات: " + ex.Message);
            }
        }

        // ═══════════════════════════════════════════
        //  رسم أزرار الأقسام
        // ═══════════════════════════════════════════
        private void RenderSections()
        {
            flowSections.SuspendLayout();
            flowSections.Controls.Clear();

            // زر "الكل"
            var allBtn = new Guna2Button
            {
                Text = "الكل",
                Font = new Font("Alexandria", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(32, 143, 252),
                BorderRadius = 12,
                Size = new Size(120, 38),
                Margin = new Padding(3),
                Tag = (long)-1
            };
            allBtn.Click += SectionBtn_Click;
            flowSections.Controls.Add(allBtn);
            _selectedSectionBtn = allBtn;

            foreach (var section in _sections)
            {
                var btn = new Guna2Button
                {
                    Text = section.SectionName,
                    Font = new Font("Alexandria", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    FillColor = Color.FromArgb(45, 55, 72),
                    BorderRadius = 12,
                    Size = new Size(150, 38),
                    Margin = new Padding(3),
                    Tag = section.Id
                };
                btn.Click += SectionBtn_Click;
                flowSections.Controls.Add(btn);
            }

            flowSections.ResumeLayout(true);
        }

        private void SectionBtn_Click(object sender, EventArgs e)
        {
            if (sender is Guna2Button btn && btn.Tag is long sectionId)
            {
                // إعادة لون الزر السابق
                if (_selectedSectionBtn != null)
                    _selectedSectionBtn.FillColor = Color.FromArgb(45, 55, 72);

                // تمييز الزر الجديد
                btn.FillColor = Color.FromArgb(32, 143, 252);
                _selectedSectionBtn = btn;
                _selectedSectionId = sectionId;

                RenderTables();
            }
        }

        // ═══════════════════════════════════════════
        //  رسم بطاقات الطاولات
        // ═══════════════════════════════════════════
        private void RenderTables()
        {
            flowTables.SuspendLayout();

            while (flowTables.Controls.Count > 0)
            {
                var ctrl = flowTables.Controls[0];
                flowTables.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            var filtered = _selectedSectionId == -1
                ? _allTables
                : _allTables.Where(t => t.SectionId == _selectedSectionId).ToList();

            foreach (var table in filtered)
            {
                var card = CreateTableCard(table);
                flowTables.Controls.Add(card);
            }

            flowTables.ResumeLayout(true);
        }

        // ═══════════════════════════════════════════
        //  إنشاء بطاقة طاولة
        // ═══════════════════════════════════════════
        private Guna2Panel CreateTableCard(TableModel table)
        {
            // لون حسب الحالة
            Color statusColor = table.Status?.ToLower() switch
            {
                "available" => Color.FromArgb(45, 204, 113),    // أخضر
                "occupied" => Color.FromArgb(239, 68, 68),       // أحمر
                "reserved" => Color.FromArgb(245, 158, 11),      // أصفر
                "cleaning" => Color.FromArgb(107, 114, 128),     // رمادي
                "maintenance" => Color.FromArgb(107, 114, 128),  // رمادي
                _ => Color.FromArgb(45, 204, 113)
            };

            Color cardBg = table.Status == "available"
                ? Color.FromArgb(24, 36, 48)
                : Color.FromArgb(30, 30, 40);

            bool isAvailable = table.Status?.ToLower() == "available";

            var card = new Guna2Panel
            {
                Size = new Size(145, 130),
                FillColor = cardBg,
                BorderColor = statusColor,
                BorderThickness = isAvailable ? 2 : 1,
                BorderRadius = 16,
                Margin = new Padding(6),
                Cursor = isAvailable ? Cursors.Hand : Cursors.No,
                Tag = table,
                BackColor = Color.Transparent
            };

            // رقم الطاولة
            var lblNumber = new Label
            {
                Text = table.TableNumber,
                Font = new Font("Alexandria", 20F, FontStyle.Bold),
                ForeColor = statusColor,
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // اسم الطاولة
            var lblName = new Label
            {
                Text = table.TableName,
                Font = new Font("Alexandria", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 210, 220),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // السعة
            var lblCapacity = new Label
            {
                Text = $"👥 {table.Capacity}",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(120, 135, 155),
                Dock = DockStyle.Top,
                Height = 22,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // الحالة
            var lblStatus = new Label
            {
                Text = table.StatusText,
                Font = new Font("Alexandria", 9F, FontStyle.Bold),
                ForeColor = statusColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            card.Controls.Add(lblStatus);
            card.Controls.Add(lblCapacity);
            card.Controls.Add(lblName);
            card.Controls.Add(lblNumber);

            // حدث النقر — فقط للمتاحة
            if (isAvailable)
            {
                EventHandler clickHandler = (s, ev) =>
                {
                    OnTableSelected?.Invoke(this, table);
                };

                card.Click += clickHandler;
                lblNumber.Click += clickHandler;
                lblName.Click += clickHandler;
                lblCapacity.Click += clickHandler;
                lblStatus.Click += clickHandler;

                // تأثير Hover
                EventHandler enterHandler = (s, ev) =>
                {
                    card.FillColor = Color.FromArgb(32, 48, 65);
                    card.BorderThickness = 3;
                };
                EventHandler leaveHandler = (s, ev) =>
                {
                    card.FillColor = cardBg;
                    card.BorderThickness = 2;
                };

                card.MouseEnter += enterHandler;
                card.MouseLeave += leaveHandler;
                foreach (Control c in card.Controls)
                {
                    c.MouseEnter += enterHandler;
                    c.MouseLeave += leaveHandler;
                    c.Cursor = Cursors.Hand;
                }
            }

            return card;
        }
    }
}
