using Sestamk.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.UserControl.UC_Settings
{
    public partial class PrinterSettings : System.Windows.Forms.UserControl, ICloseRequest
    {

        public PrinterSettings()
        {
            InitializeComponent();
            SetupEvents();
            LoadSettings();

            this.Load += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    // حساب أقصى ارتفاع للمحتوى داخل الباينل
                    int maxBottom = 0;
                    foreach (Control c in guna2Panel1.Controls)
                    {
                        int bottom = c.Top + c.Height;
                        if (bottom > maxBottom)
                            maxBottom = bottom;
                    }
                    maxBottom += 40; // padding

                    guna2vScrollBar1.Maximum = maxBottom;
                    guna2vScrollBar1.LargeChange = guna2Panel1.Height;

                    guna2Panel1.AutoScrollPosition = new Point(0, 0);
                    guna2vScrollBar1.Refresh();
                }));
            };
        }

        private void SetupEvents()
        {
            guna2Button2.Click += async (s, e) => await SaveSettingsAsync();
            guna2Button3.Click += (s, e) => LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Load Printers
                guna2ComboBox1.Items.Clear();
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    guna2ComboBox1.Items.Add(printer);
                }

                string defaultPrinter = Sestamk.Classes.SettingsService.DefaultPrinterName;
                if (!string.IsNullOrEmpty(defaultPrinter) && guna2ComboBox1.Items.Contains(defaultPrinter))
                    guna2ComboBox1.SelectedItem = defaultPrinter;
                else if (guna2ComboBox1.Items.Count > 0)
                    guna2ComboBox1.SelectedIndex = 0;

                // Paper Size
                string paperSize = Sestamk.Classes.SettingsService.PrinterPaperSize;
                guna2CustomRadioButton1.Checked = paperSize == "80mm";
                guna2CustomRadioButton2.Checked = paperSize == "58mm";
                guna2CustomRadioButton3.Checked = paperSize == "A4";
                if (!guna2CustomRadioButton1.Checked && !guna2CustomRadioButton2.Checked && !guna2CustomRadioButton3.Checked)
                    guna2CustomRadioButton1.Checked = true;

                // Cash Drawer Target
                string drawerPort = Sestamk.Classes.SettingsService.PrinterCashDrawerPort;
                guna2ComboBox2.Text = drawerPort; // Or populate with specific items if it's a fixed list

                // Toggles & Copies
                guna2ToggleSwitch1.Checked = Sestamk.Classes.SettingsService.PrintReceiptOnPayment;
                guna2ToggleSwitch4.Checked = Sestamk.Classes.SettingsService.OpenDrawerOnPayment;
                guna2TextBox1.Text = Sestamk.Classes.SettingsService.PrinterInvoiceCopies.ToString();
            }
            catch (Exception ex)
            {
                Sestamk.Classes.ToastManager.ShowError("خطأ", "فشل تحميل الإعدادات: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {
                if (guna2ComboBox1.SelectedItem != null)
                    await Sestamk.Classes.SettingsService.SetAsync("DefaultPrinterName", guna2ComboBox1.SelectedItem.ToString());

                string paperSize = guna2CustomRadioButton1.Checked ? "80mm" : (guna2CustomRadioButton2.Checked ? "58mm" : "A4");
                await Sestamk.Classes.SettingsService.SetAsync("Printer_PaperSize", paperSize);

                await Sestamk.Classes.SettingsService.SetAsync("Printer_CashDrawerPort", guna2ComboBox2.Text);
                await Sestamk.Classes.SettingsService.SetAsync("PrintReceiptOnPayment", guna2ToggleSwitch1.Checked.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("OpenDrawerOnPayment", guna2ToggleSwitch4.Checked.ToString());
                
                if (int.TryParse(guna2TextBox1.Text, out int copies) && copies > 0)
                    await Sestamk.Classes.SettingsService.SetAsync("Printer_InvoiceCopies", copies.ToString());
                else
                    await Sestamk.Classes.SettingsService.SetAsync("Printer_InvoiceCopies", "1");

                Sestamk.Classes.ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات الطابعة بنجاح ✅");
            }
            catch (Exception ex)
            {
                Sestamk.Classes.ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }

        public event EventHandler CloseRequested;

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
