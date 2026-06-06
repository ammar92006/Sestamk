using Sestamk.Classes;
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
    public partial class ActivationSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        public ActivationSettings()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadLicenseData()
        {
            var license = LocalLicenseManager.ReadLocalLicense();
            if (license != null)
            {
                lblStatus.Text = license["status"]?.ToString() == "active" ? "مفعل" : "غير مفعل";

                // 🔴 قراءة تاريخ بداية التفعيل (اللي لسه ضايفينه)
                if (license["created_at"] != null)
                {
                    DateTime startDate = DateTime.Parse(license["created_at"].ToString());
                    lblStartDate.Text = startDate.ToShortDateString();
                }
                else
                {
                    lblStartDate.Text = "غير متوفر";
                }

                //lblExpiryDate.Text = DateTime.Parse(license["expires_at"].ToString()).ToShortDateString();
                // قراءة تاريخ الانتهاء
                DateTime expiry = DateTime.Parse(license["expires_at"].ToString());
                lblExpiryDate.Text = expiry.ToShortDateString();


                lblPlanName.Text = license["plan"]?.ToString().ToUpper();
                lblMaxUsers.Text = license["max_users"]?.ToString();
                lblMaxDevices.Text = license["max_devices"]?.ToString();
                lblcurrencyprice.Text = license["currency"]?.ToString() ?? "USD";
                // أو لو الداتابيز بتبعت تاريخ الإنشاء:
                //lblStartDate.Text = DateTime.Parse(license["company_created_at"].ToString()).ToShortDateString();
                // جلب السعر من البيانات المخزنة
                if (license["plan"]?.ToString().ToLower() == "trial")
                {
                    lblPrice.Text = "نسخة تجريبية مجانية";
                }
                else
                {
                    lblPrice.Text = license["price"]?.ToString() ?? "0.00";
                }

            }
        }

        private async void ActivationSettings_Load(object sender, EventArgs e)
        {
            // عرض HWID الحالي ليسهل على المستخدم
            lblHWID.Text = LicenseManager.GetHWID();

            LoadLicenseData();
            //await LoadDevicesList();
        }

        private void btnChangeSerial_Click(object sender, EventArgs e)
        {
            using (frmActivation activationForm = new frmActivation())
            {
                if (activationForm.ShowDialog() == DialogResult.OK)
                {
                    LoadLicenseData(); // إعادة تحميل البيانات بعد التفعيل الجديد
                }
            }
        }

        private void btnClipboard_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lblHWID.Text))
            {
                Main_Methods.SetClipboardTextSafe(lblHWID.Text);
                ToastManager.ShowSuccess("نسخ الي الحافظة", "تم النسخ ✅");
            }
        }
    }
}
