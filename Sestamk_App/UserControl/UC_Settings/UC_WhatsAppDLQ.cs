using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sestamk.Classes;

namespace Sestamk.UserControl.UC_Settings
{
    public class UC_WhatsAppDLQ : System.Windows.Forms.UserControl
    {
        private DataGridView gridDLQ;
        private Button btnRefresh;
        private Button btnRetryAll;
        private Label lblTitle;
        private Label lblCount;

        public UC_WhatsAppDLQ()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(17, 24, 39); // Dark mode theme matching Sestamk
            this.Size = new Size(800, 600);
            this.Padding = new Padding(20);

            // Title
            lblTitle = new Label
            {
                Text = "إدارة الرسائل الفاشلة (Dead-Letter Queue)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20),
                RightToLeft = RightToLeft.Yes
            };
            this.Controls.Add(lblTitle);

            // Count Label
            lblCount = new Label
            {
                Text = "العدد: 0",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Location = new Point(20, 60),
                RightToLeft = RightToLeft.Yes
            };
            this.Controls.Add(lblCount);

            // Retry All Button
            btnRetryAll = new Button
            {
                Text = "إعادة محاولة للجميع",
                BackColor = Color.FromArgb(59, 130, 246), // Blue
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(160, 40),
                Location = new Point(this.Width - 180, 20),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes
            };
            btnRetryAll.FlatAppearance.BorderSize = 0;
            btnRetryAll.Click += async (s, e) => await RetryAllAsync();
            this.Controls.Add(btnRetryAll);

            // Refresh Button
            btnRefresh = new Button
            {
                Text = "تحديث",
                BackColor = Color.FromArgb(55, 65, 81), // Gray
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Size = new Size(100, 40),
                Location = new Point(this.Width - 290, 20),
                Cursor = Cursors.Hand,
                RightToLeft = RightToLeft.Yes
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await LoadDataAsync();
            this.Controls.Add(btnRefresh);

            // DataGridView
            gridDLQ = new DataGridView
            {
                Location = new Point(20, 90),
                Size = new Size(this.Width - 40, this.Height - 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackgroundColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RightToLeft = RightToLeft.Yes,
                RowTemplate = { Height = 40 }
            };

            // Basic styling for dark mode grid
            gridDLQ.EnableHeadersVisualStyles = false;
            gridDLQ.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(17, 24, 39);
            gridDLQ.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridDLQ.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gridDLQ.ColumnHeadersHeight = 40;
            gridDLQ.DefaultCellStyle.BackColor = Color.FromArgb(31, 41, 55);
            gridDLQ.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 65, 81);
            gridDLQ.CellFormatting += GridDLQ_CellFormatting;

            this.Controls.Add(gridDLQ);
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                btnRefresh.Enabled = false;
                DataTable dt = await WhatsAppQueueManager.GetDLQMessagesAsync();
                
                if (InvokeRequired)
                {
                    Invoke(new Action(() => {
                        gridDLQ.DataSource = dt;
                        lblCount.Text = $"العدد: {dt.Rows.Count}";
                    }));
                }
                else
                {
                    gridDLQ.DataSource = dt;
                    lblCount.Text = $"العدد: {dt.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل تحميل البيانات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (InvokeRequired) Invoke(new Action(() => btnRefresh.Enabled = true));
                else btnRefresh.Enabled = true;
            }
        }

        private async Task RetryAllAsync()
        {
            if (MessageBox.Show("هل أنت متأكد من إعادة جميع الرسائل الفاشلة إلى طابور الإرسال؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    btnRetryAll.Enabled = false;
                    int count = await WhatsAppQueueManager.RequeueDLQAsync();
                    MessageBox.Show($"تمت إعادة {count} رسالة إلى طابور الإرسال بنجاح.", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("فشل إعادة الإرسال: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnRetryAll.Enabled = true;
                }
            }
        }

        private void GridDLQ_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Optional: format specific columns like Date
            if (gridDLQ.Columns[e.ColumnIndex].Name == "FailedAt" && e.Value != null)
            {
                if (DateTime.TryParse(e.Value.ToString(), out DateTime date))
                {
                    e.Value = date.ToString("yyyy-MM-dd HH:mm");
                    e.FormattingApplied = true;
                }
            }
        }
    }
}
