using Sestamk.Classes;
using Sestamk.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sestamk.UserControl.UC_Settings
{
    public partial class ReceiptSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        public ReceiptSettings()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                txtRestaurantName.Text = SettingsService.StoreName;
                txtPhone.Text = SettingsService.StorePhone;
                txtPhone2.Text = SettingsService.StorePhone2;
                txtAddress.Text = SettingsService.StoreAddress;

                // Paper Size
                string paperSize = SettingsService.PrinterPaperSize;
                if (paperSize == "58mm") cmbPaperSize.SelectedIndex = 0;
                else cmbPaperSize.SelectedIndex = 1;

                // Font Size Mapping
                float fs = SettingsService.ReceiptFontSize;
                if (fs <= 7.5f) cmbFontSize.SelectedIndex = 0; // صغير
                else if (fs <= 9.5f) cmbFontSize.SelectedIndex = 1; // متوسط
                else cmbFontSize.SelectedIndex = 2; // كبير

                toggleShowTax.Checked = SettingsService.ShowTax;
                toggleShowDiscount.Checked = SettingsService.ShowDiscount;
                toggleShowCashier.Checked = SettingsService.ShowCashier;
                toggleShowLogo.Checked = SettingsService.ShowLogo;

                cmbReceiptStyle.SelectedIndex = SettingsService.ReceiptStyle == "Table" ? 1 : 0;

                txtFooterMessage.Text = SettingsService.ReceiptFooter;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in LoadSettings: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //this.Visible = false;
            CloseRequested?.Invoke(this, EventArgs.Empty);

        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // UI to Logic Mapping
                await SettingsService.SetAsync("StoreName", txtRestaurantName.Text.Trim());
                await SettingsService.SetAsync("StorePhone", txtPhone.Text.Trim());
                await SettingsService.SetAsync("StorePhone2", txtPhone2.Text.Trim());
                await SettingsService.SetAsync("StoreAddress", txtAddress.Text.Trim());
                
                string selectedPaper = cmbPaperSize.SelectedIndex == 0 ? "58mm" : "80mm";
                await SettingsService.SetAsync("Printer_PaperSize", selectedPaper);

                float fs = 8.5f;
                if (cmbFontSize.SelectedIndex == 0) fs = 7.0f;
                else if (cmbFontSize.SelectedIndex == 1) fs = 8.5f;
                else if (cmbFontSize.SelectedIndex == 2) fs = 11.0f;
                await SettingsService.SetAsync("Receipt_FontSize", fs.ToString());

                await SettingsService.SetAsync("ShowTax", toggleShowTax.Checked.ToString().ToLower());
                await SettingsService.SetAsync("ShowDiscount", toggleShowDiscount.Checked.ToString().ToLower());
                await SettingsService.SetAsync("ShowCashier", toggleShowCashier.Checked.ToString().ToLower());
                await SettingsService.SetAsync("ShowLogo", toggleShowLogo.Checked.ToString().ToLower());
                await SettingsService.SetAsync("Receipt_Footer", txtFooterMessage.Text.Trim());

                string receiptStyle = cmbReceiptStyle.SelectedIndex == 1 ? "Table" : "Classic";
                await SettingsService.SetAsync("Receipt_Style", receiptStyle);

                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات الفاتورة بنجاح وتحديث نظام الطباعة.");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل أنت متأكد من إعادة تعيين الإعدادات الافتراضية؟", "تأكيد الإعادة", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                txtRestaurantName.Text = "سيستمك";
                txtPhone.Text = "";
                txtPhone2.Text = "";
                txtAddress.Text = "";
                cmbPaperSize.SelectedIndex = 1; // 80mm
                cmbFontSize.SelectedIndex = 1; // متوسط
                toggleShowTax.Checked = true;
                toggleShowDiscount.Checked = true;
                toggleShowCashier.Checked = true;
                toggleShowLogo.Checked = true;
                txtFooterMessage.Text = "شكراً لزيارتكم\nنتمنى لكم تجربة سعيدة";
                cmbReceiptStyle.SelectedIndex = 0; // الكلاسيكي
                
                ToastManager.ShowInfo("إعادة ضبط", "تمت استعادة القيم الافتراضية. اضغط حفظ لتأكيد التغيير.");
            }
        }

        private async void btnUploadLogo_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "اختر شعار المطعم";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // In a real scenario, you might want to copy the file to a local app folder
                        await SettingsService.SetAsync("Receipt_LogoPath", ofd.FileName);
                        ToastManager.ShowSuccess("تم الرفع", "تم تحديث مسار الشعار بنجاح.");
                    }
                    catch (Exception ex)
                    {
                        ToastManager.ShowError("خطأ", "فشل في رفع الشعار: " + ex.Message);
                    }
                }
            }
        }
    }
}
