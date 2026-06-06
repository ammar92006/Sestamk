using Guna.UI2.WinForms;
using Sestamk.Classes;
using Sestamk.Classes.Data;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmSalesReports : BaseForm
    {
        protected override Size DesignClientSize => new Size(1300, 780);

        private Label[] cardValues = new Label[6];
        private Label[] cardLabels = new Label[6];

        public frmSalesReports()
        {
            InitializeComponent();

            var borderless = new Guna2BorderlessForm();
            borderless.ContainerControl = this;
            borderless.BorderRadius = 20;
            borderless.ResizeForm = true;
            borderless.AnimateWindow = true;

            Main_Methods.Attach(pnlHeader, this);
            Main_Methods.Attach(lblTitle, this);

            // Wire events
            btnSearch.Click += (s, e) => DoSearch();
            btnReset.Click += BtnReset_Click;
            btnPrint.Click += BtnPrint_Click;
            btnExport.Click += BtnExport_Click;
            btnToday.Click += (s, e) => { dtpFrom.Value = DateTime.Today; dtpTo.Value = DateTime.Today; DoSearch(); };
            btnYesterday.Click += (s, e) => { dtpFrom.Value = DateTime.Today.AddDays(-1); dtpTo.Value = DateTime.Today.AddDays(-1); DoSearch(); };
            btnWeek.Click += (s, e) => { dtpFrom.Value = DateTime.Today.AddDays(-7); dtpTo.Value = DateTime.Today; DoSearch(); };
            btnMonth.Click += (s, e) => { dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); dtpTo.Value = DateTime.Today; DoSearch(); };

            BuildSummaryCards();
            Main_Methods.StyleDataGridView(dgvOrders);
            Main_Methods.StyleDataGridView(dgvProducts);
            Main_Methods.StyleDataGridView(dgvShifts);
            Main_Methods.StyleDataGridView(dgvPayments);
            Main_Methods.StyleDataGridView(dgvCustomers);
            Main_Methods.StyleDataGridView(dgvReturns);

            this.Load += FrmSalesReports_Load;
        }

        private void BuildSummaryCards()
        {
            string[] names = { "إجمالي المبيعات", "عدد الفواتير", "متوسط الفاتورة", "الخصومات", "الضرائب", "صافي الربح" };
            string[] icons = { "💰", "📃", "📊", "🏷️", "🧾", "💎" };
            Color[] colors = {
                Color.FromArgb(16,185,129), Color.FromArgb(59,130,246),
                Color.FromArgb(167,106,255), Color.FromArgb(245,158,11),
                Color.FromArgb(6,182,212), Color.FromArgb(34,197,94)
            };

            for (int i = 0; i < 6; i++)
            {
                var card = new Guna2Panel { Size = new Size(185, 95), FillColor = Color.FromArgb(30, 30, 47), BorderRadius = 15, Margin = new Padding(5) };
                var ico = new Label { Text = icons[i], Font = new Font("Segoe UI Emoji", 16), Location = new Point(130, 8), AutoSize = true, BackColor = Color.Transparent };
                cardValues[i] = new Label { Text = "0", Font = new Font("Alexandria", 20, FontStyle.Bold), ForeColor = colors[i], Location = new Point(10, 10), AutoSize = true, BackColor = Color.Transparent };
                cardLabels[i] = new Label { Text = names[i], Font = new Font("Alexandria", 9), ForeColor = Color.FromArgb(156, 163, 175), Location = new Point(10, 62), AutoSize = true, BackColor = Color.Transparent };
                card.Controls.AddRange(new Control[] { ico, cardValues[i], cardLabels[i] });
                flowCards.Controls.Add(card);
            }
        }

        private async void FrmSalesReports_Load(object sender, EventArgs e)
        {
            try
            {
                var cashiers = await SalesReportService.GetCashiersListAsync();
                cboCashier.Items.Clear();
                cboCashier.Items.Add("الكل");
                foreach (DataRow row in cashiers.Rows)
                    cboCashier.Items.Add(row["DisplayName"].ToString());
                cboCashier.SelectedIndex = 0;
            }
            catch { }
            DoSearch();
        }

        private async void DoSearch()
        {
            try
            {
                lblStatus.Text = "جاري التحميل...";
                btnSearch.Enabled = false;
                this.Cursor = Cursors.WaitCursor;

                var filters = BuildFilters();
                var summary = await SalesReportService.GetSalesSummaryAsync(filters);

                cardValues[0].Text = $"{summary.TotalSales:N2}";
                cardValues[1].Text = $"{summary.TotalOrders}";
                cardValues[2].Text = $"{summary.AverageOrderValue:N2}";
                cardValues[3].Text = $"{summary.TotalDiscounts:N2}";
                cardValues[4].Text = $"{summary.TotalTax:N2}";
                cardValues[5].Text = $"{summary.NetSales:N2}";

                dgvOrders.DataSource = await SalesReportService.GetOrdersDetailAsync(filters);
                dgvProducts.DataSource = await SalesReportService.GetProductSalesAsync(filters);
                dgvShifts.DataSource = await SalesReportService.GetShiftReportAsync(filters);
                dgvPayments.DataSource = await SalesReportService.GetPaymentMethodsAsync(filters);
                dgvCustomers.DataSource = await SalesReportService.GetCustomerReportAsync(filters);
                dgvReturns.DataSource = await SalesReportService.GetReturnsDetailAsync(filters);

                FormatMoney(dgvOrders, "المجموع الفرعي", "الخصم", "الضريبة", "الإجمالي");
                FormatMoney(dgvProducts, "إجمالي المبيعات", "متوسط السعر");
                FormatMoney(dgvShifts, "كاش الفتح", "كاش الإغلاق", "المبيعات", "الفرق");
                FormatMoney(dgvPayments, "إجمالي المبلغ");
                FormatMoney(dgvCustomers, "إجمالي المشتريات", "متوسط الطلب", "الرصيد الحالي");
                FormatMoney(dgvReturns, "إجمالي المرتجع");

                lblStatus.Text = $"تم تحميل {dgvOrders.Rows.Count} فاتورة — من {filters.DateFrom:yyyy/MM/dd} إلى {filters.DateTo:yyyy/MM/dd}";
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "خطأ في تحميل التقارير: " + ex.Message);
                lblStatus.Text = "خطأ في التحميل";
            }
            finally { btnSearch.Enabled = true; this.Cursor = Cursors.Default; }
        }

        private ReportFilters BuildFilters()
        {
            var f = new ReportFilters { DateFrom = dtpFrom.Value.Date, DateTo = dtpTo.Value.Date, ExcludeVoided = true };
            if (cboOrderType.SelectedIndex > 0) f.OrderType = cboOrderType.SelectedIndex - 1;
            if (cboPaymentMethod.SelectedIndex > 0) f.PaymentMethod = cboPaymentMethod.SelectedIndex - 1;
            return f;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            var dgv = GetActiveDgv();
            if (dgv == null || dgv.Rows.Count == 0) { ToastManager.ShowWarning("تنبيه", "لا توجد بيانات للتصدير"); return; }
            using var sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"تقرير_{DateTime.Now:yyyyMMdd_HHmmss}.csv" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var sb = new StringBuilder("\uFEFF");
            for (int i = 0; i < dgv.Columns.Count; i++) { if (i > 0) sb.Append(","); sb.Append($"\"{dgv.Columns[i].HeaderText}\""); }
            sb.AppendLine();
            foreach (DataGridViewRow row in dgv.Rows)
            { for (int i = 0; i < dgv.Columns.Count; i++) { if (i > 0) sb.Append(","); sb.Append($"\"{row.Cells[i].Value}\""); } sb.AppendLine(); }
            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            ToastManager.ShowSuccess("تم", "تم حفظ الملف بنجاح");
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            var dgv = GetActiveDgv();
            if (dgv == null || dgv.Rows.Count == 0) { ToastManager.ShowWarning("تنبيه", "لا توجد بيانات"); return; }
            var doc = new System.Drawing.Printing.PrintDocument(); doc.DefaultPageSettings.Landscape = true;
            int ri = 0;
            doc.PrintPage += (s, ev) => {
                var g = ev.Graphics; var b = ev.MarginBounds; float y = b.Top;
                var ft = new Font("Alexandria", 14, FontStyle.Bold); var fh = new Font("Alexandria", 9, FontStyle.Bold); var fc = new Font("Alexandria", 8);
                g.DrawString($"تقرير المبيعات — {dtpFrom.Value:yyyy/MM/dd} إلى {dtpTo.Value:yyyy/MM/dd}", ft, Brushes.Black, b.Left, y); y += 35;
                int cols = dgv.Columns.Count; float cw = b.Width / (float)cols;
                for (int c = 0; c < cols; c++) g.DrawString(dgv.Columns[c].HeaderText, fh, Brushes.Black, b.Left + c * cw, y);
                y += 22; g.DrawLine(Pens.Black, b.Left, y, b.Right, y); y += 3;
                while (ri < dgv.Rows.Count) { if (y + 18 > b.Bottom) { ev.HasMorePages = true; return; }
                    for (int c = 0; c < cols; c++) g.DrawString(dgv.Rows[ri].Cells[c].Value?.ToString() ?? "", fc, Brushes.Black, b.Left + c * cw, y);
                    y += 18; ri++; }
                ev.HasMorePages = false;
            };
            new PrintPreviewDialog { Document = doc, Width = 900, Height = 600 }.ShowDialog();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            dtpFrom.Value = DateTime.Today; dtpTo.Value = DateTime.Today;
            cboOrderType.SelectedIndex = 0; cboPaymentMethod.SelectedIndex = 0; cboCashier.SelectedIndex = 0;
            DoSearch();
        }

        private Guna2DataGridView GetActiveDgv() => tabReports.SelectedIndex switch
        { 0 => dgvOrders, 1 => dgvProducts, 2 => dgvShifts, 3 => dgvPayments, 4 => dgvCustomers, 5 => dgvReturns, _ => dgvOrders };

        private void FormatMoney(DataGridView dgv, params string[] cols)
        { 
            foreach (var c in cols) if (dgv.Columns.Contains(c)) dgv.Columns[c].DefaultCellStyle.Format = "N2";
            foreach (DataGridViewColumn col in dgv.Columns) col.MinimumWidth = 120;
        }
    }
}
