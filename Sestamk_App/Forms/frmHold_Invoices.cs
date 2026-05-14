using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sestamk.Classes;

namespace Sestamk.Forms
{
    public partial class frmHold_Invoices : BaseForm
    {
        private List<HeldInvoiceDto> _allHeldInvoices;
        private string _currentFilter = "all";

        public HeldInvoiceDto SelectedInvoiceToRestore { get; private set; }

        public frmHold_Invoices()
        {
            InitializeComponent();
            this.Load += FrmHold_Invoices_Load;

            guna2TextBox1.TextChanged += SearchBox_TextChanged;
            btnFilterLocal.Click += BtnFilterLocal_Click;
            btnFilterSafari.Click += BtnFilterSafari_Click;
            btnFilterall.Click += BtnFilterAll_Click;
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            _currentFilter = "all";

            Main_Methods.Attach(pnlTitle, this);
            Main_Methods.Attach(guna2Panel1, this);
        }

        private async void FrmHold_Invoices_Load(object sender, EventArgs e)
        {
            UserSession.UpdateUserDisplay(null, null, picAvatar);
            lblSubtitle.Text = $"نظام نشاط البيع - {SettingsService.StoreName}";

            _allHeldInvoices = await HeldInvoiceService.GetActiveAsync();
            ApplyFilterAndRender();
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            ApplyFilterAndRender();
        }

        private void BtnFilterAll_Click(object sender, EventArgs e)
        {
            if (_currentFilter == "local")
            {
                _currentFilter = "all";
                btnFilterLocal.FillColor = Color.FromArgb(31, 41, 55);
                btnFilterLocal.BorderColor = Color.FromArgb(47, 57, 72);
                btnFilterLocal.BorderThickness = 2;
            }
            else
            {
                _currentFilter = "local";
                btnFilterLocal.FillColor = Color.FromArgb(32, 143, 252);
                btnFilterLocal.BorderThickness = 0;
                btnFilterSafari.FillColor = Color.FromArgb(31, 41, 55);
                btnFilterSafari.BorderColor = Color.FromArgb(47, 57, 72);
                btnFilterSafari.BorderThickness = 2;
            }
            ApplyFilterAndRender();
        }
        private void BtnFilterLocal_Click(object sender, EventArgs e)
        {
            if (_currentFilter == "local")
            {
                _currentFilter = "all";
                btnFilterLocal.FillColor = Color.FromArgb(31, 41, 55);
                btnFilterLocal.BorderColor = Color.FromArgb(47, 57, 72);
                btnFilterLocal.BorderThickness = 2;
            }
            else
            {
                _currentFilter = "local";
                btnFilterLocal.FillColor = Color.FromArgb(32, 143, 252);
                btnFilterLocal.BorderThickness = 0;
                btnFilterSafari.FillColor = Color.FromArgb(31, 41, 55);
                btnFilterSafari.BorderColor = Color.FromArgb(47, 57, 72);
                btnFilterSafari.BorderThickness = 2;
            }
            ApplyFilterAndRender();
        }

        private void BtnFilterSafari_Click(object sender, EventArgs e)
        {
            if (_currentFilter == "safari")
            {
                _currentFilter = "all";
                btnFilterSafari.FillColor = Color.FromArgb(31, 41, 55);
                btnFilterSafari.BorderColor = Color.FromArgb(47, 57, 72);
                btnFilterSafari.BorderThickness = 2;
            }
            else
            {
                _currentFilter = "safari";
                btnFilterSafari.FillColor = Color.FromArgb(32, 143, 252);
                btnFilterSafari.BorderThickness = 0;
                btnFilterLocal.FillColor = Color.FromArgb(31, 41, 55);
                btnFilterLocal.BorderColor = Color.FromArgb(47, 57, 72);
                btnFilterLocal.BorderThickness = 2;
            }
            ApplyFilterAndRender();
        }

        private void ApplyFilterAndRender()
        {
            if (_allHeldInvoices == null)
                _allHeldInvoices = new List<HeldInvoiceDto>();

            var filtered = _allHeldInvoices.AsEnumerable();

            if (_currentFilter == "local")
            {
                filtered = filtered.Where(inv => inv.InvoiceType == 1);
            }
            else if (_currentFilter == "safari")
            {
                filtered = filtered.Where(inv => inv.InvoiceType == 0 || inv.InvoiceType == 2);
            }

            string search = guna2TextBox1.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(inv =>
                    (inv.InvoiceNumber != null && inv.InvoiceNumber.ToLower().Contains(search)) ||
                    (inv.TableName != null && inv.TableName.ToLower().Contains(search)) ||
                    (inv.CustomerName != null && inv.CustomerName.ToLower().Contains(search)) ||
                    (inv.DriverName != null && inv.DriverName.ToLower().Contains(search)) ||
                    (inv.DisplayTitle != null && inv.DisplayTitle.ToLower().Contains(search))
                );
            }

            RenderInvoices(filtered.ToList());
        }

        private void RenderInvoices(List<HeldInvoiceDto> invoices)
        {
            flowInvoices.SuspendLayout();

            while (flowInvoices.Controls.Count > 0)
            {
                var ctrl = flowInvoices.Controls[0];
                flowInvoices.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            decimal totalAmounts = 0;
            string currency = SettingsService.CurrencyName;

            foreach (var inv in invoices)
            {
                totalAmounts += inv.TotalAmount;

                var card = new Sestamk.UserControl.UC_Hold_Invoices(inv);
                card.Margin = new Padding(12);
                card.Cursor = Cursors.Hand;

                card.OnRestore += (s, e) =>
                {
                    SelectedInvoiceToRestore = inv;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                card.OnDelete += async (s, e) =>
                {
                    if (frmConfirm.Show("حذف الفاتورة", $"هل أنت متأكد من حذف الفاتورة {inv.InvoiceNumber}؟\nلن يمكن استرجاعها."))
                    {
                        await HeldInvoiceService.DeleteAsync(inv.HeldInvoiceID);
                        _allHeldInvoices = await HeldInvoiceService.GetActiveAsync();
                        ApplyFilterAndRender();
                        ToastManager.ShowSuccess("حذف", $"تم حذف الفاتورة {inv.InvoiceNumber} بنجاح");
                    }
                };

                card.OnView += (s, e) =>
                {
                    ShowInvoiceDetails(inv);
                };

                flowInvoices.Controls.Add(card);
            }

            var addCard = CreateNewHoldCard();
            flowInvoices.Controls.Add(addCard);

            lblInvoiceCount.Text = $"عدد الفواتير المعلقة: {invoices.Count}";
            lblTotalAmount.Text = $"{totalAmounts:N2} {currency}";
            lblPageInfo.Text = $"< 1 / 1 >";

            flowInvoices.ResumeLayout(true);
        }

        private Guna.UI2.WinForms.Guna2Panel CreateNewHoldCard()
        {
            var card = new Guna.UI2.WinForms.Guna2Panel
            {
                Size = new Size(354, 290),
                FillColor = Color.Transparent,
                BorderColor = Color.FromArgb(70, 80, 95),
                BorderThickness = 2,
                BorderRadius = 15,
                Margin = new Padding(12),
                Cursor = Cursors.Hand,
                Name = "pnlAddNew_NoTheme"
            };

            var lblPlus = new Label
            {
                Text = "+",
                Font = new Font("Segoe UI", 36F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 110, 125),
                Size = new Size(60, 60),
                Location = new Point((354 - 60) / 2, 90),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblText = new Label
            {
                Text = "تعليق فاتورة جديدة",
                Font = new Font("Alexandria", 12F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 110, 125),
                Size = new Size(250, 30),
                Location = new Point((354 - 250) / 2, 155),
                TextAlign = ContentAlignment.MiddleCenter
            };

            EventHandler closeHandler = (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            card.Click += closeHandler;
            lblPlus.Click += closeHandler;
            lblText.Click += closeHandler;

            card.Controls.Add(lblPlus);
            card.Controls.Add(lblText);

            return card;
        }

        private void ShowInvoiceDetails(HeldInvoiceDto inv)
        {
            string currency = SettingsService.CurrencyName;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"رقم الفاتورة: {inv.InvoiceNumber}");
            sb.AppendLine($"النوع: {inv.InvoiceTypeText}");
            sb.AppendLine($"التوقيت: {inv.HoldTime:hh:mm tt}");
            sb.AppendLine($"العميل: {inv.CustomerName}");

            if (!string.IsNullOrEmpty(inv.TableName))
                sb.AppendLine($"الطاولة: {inv.TableName}");
            if (!string.IsNullOrEmpty(inv.DriverName))
                sb.AppendLine($"السائق: {inv.DriverName}");

            sb.AppendLine();
            sb.AppendLine("══ الأصناف ══");
            foreach (var item in inv.Items)
            {
                sb.AppendLine($"  {item.Quantity}x {item.ItemName} — {item.UnitPrice * item.Quantity:N2} {currency}");
            }
            sb.AppendLine();
            sb.AppendLine($"الإجمالي: {inv.TotalAmount:N2} {currency}");

            MessageBox.Show(
                sb.ToString(),
                $"تفاصيل الفاتورة {inv.InvoiceNumber}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
            );
        }
    }
}
