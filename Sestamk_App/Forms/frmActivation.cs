using Sestamk.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using QRCoder; 
using Newtonsoft.Json.Linq;

namespace Sestamk.Forms
{
    public partial class frmActivation : Form
    {

        public frmActivation()
        {
            InitializeComponent();
            Main_Methods.Attach(guna2Panel1, this);
            Main_Methods.Attach(label1, this);
            Main_Methods.Attach(guna2PictureBox1, this);
            Main_Methods.Attach(label2, this);
        }

        private async void btnClipboard_Click(object sender, EventArgs e)
        {

            //if (!string.IsNullOrEmpty(lblHWID.Text))
            //{
            //    Clipboard.SetText(lblHWID.Text);
            //    ToastManager.ShowSuccess("نسخ الي الحافظة", "تم نسخ النص  بنجاح ✅");

            //}
            //else
            //{
            //    ToastManager.ShowError("خطأ", "لا يوجد نص لنسخه ❌");
            //}

            if (!string.IsNullOrEmpty(lblHWID.Text))
            {
                Main_Methods.SetClipboardTextSafe(lblHWID.Text);
                ToastManager.ShowSuccess("نسخ الي الحافظة", "تم النسخ ✅");
            }


        }
      

        private void btnSupport_Click(object sender, EventArgs e)
        {
            string number = "201281637066";
            string url = $"https://wa.me/{number}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void frmActivation_Load(object sender, EventArgs e)
        {
            // 1. جلب بصمة الجهاز الفريدة
            string hwid = LicenseManager.GetHWID();
            lblHWID.Text = hwid;

            // 2. تحويل البصمة إلى QR Code
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(hwid, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // توليد الصورة (رقم 5 يعني حجم المربع، يمكنك تكبيره أو تصغيره)
            Bitmap qrCodeImage = qrCode.GetGraphic(5, Color.White, Color.FromArgb(30, 30, 30), true);

            picQRCode.Image = qrCodeImage;
            picQRCode.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private async void btnActivate_Click(object sender, EventArgs e)
        {
            string serial = txtSerial.Text.Trim();


            if (string.IsNullOrEmpty(serial))
            {
                MessageBox.Show("يرجى إدخال كود التفعيل أولاً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnActivate.Enabled = false;
            btnActivate.Text = "جاري التحقق...";

            try
            {
                // 3. الاتصال بالسيرفر للتحقق من السيريال
                JObject result = await LicenseManager.CheckLicenseAsync(serial);

                string status = result["status"]?.ToString();

                if (status == "active")
                {
                    // 4. حفظ الترخيص محلياً (شرحناها مسبقاً في LocalLicenseManager)
                    // نقوم بإضافة السيريال للبيانات عشان نحفظه معاهم
                    result["saved_serial"] = serial;
                    LocalLicenseManager.SaveLicenseLocally(result);

                    //MessageBox.Show("تم التفعيل بنجاح! مرحباً بك في سيستمك.", "تفعيل النظام", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ToastManager.ShowSuccess("تم التفعيل", "مرحباً بك في سيستمك ✅");
                    await UpdateDeviceHardwareInfo();
                    // إغلاق شاشة التفعيل وفتح الشاشة الرئيسية أو شاشة الدخول
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    // frmLogin loginForm = new frmLogin();
                    // loginForm.Show();


                }
                else
                {
                    //MessageBox.Show("فشل التفعيل: " + result["message"], "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ToastManager.ShowError("فشل التفعيل", result["message"]?.ToString() ?? "حدث خطأ غير معروف ❌");
                    btnActivate.Enabled = true;
                    btnActivate.Text = "تفعيل البرنامج";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ في الاتصال بالسيرفر. تأكد من الإنترنت.\n" + ex.Message, "خطأ شبكة", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnActivate.Enabled = true;
                btnActivate.Text = "تفعيل البرنامج";
            }
        }


        public static async Task UpdateDeviceHardwareInfo()
        {
            try
            {
                string hwid = LicenseManager.GetHWID(); // السيريال بتاع الجهاز اللي اتعمله Insert

                var hardwareData = new
                {
                    device_name = HardwareInfo.GetDeviceName(),
                    os_info = HardwareInfo.GetOSInfo(),
                    processor = HardwareInfo.GetProcessorName(),
                    ram_gb = HardwareInfo.GetRamGB()
                };

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apikey", SecureConfig.SupabaseKey);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + SecureConfig.SupabaseKey);

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(hardwareData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    // بنعمل PATCH عشان نحدث السطر اللي الـ hwid بتاعه بيطابق الجهاز ده
                    string url = $"{SecureConfig.SupabaseUrl}/rest/v1/devices?hwid=eq.{hwid}";
                    await client.PatchAsync(url, content);
                }
            }
            catch { /* تجاهل الأخطاء لعدم تعطيل دخول المستخدم */ }
        }
    }
}
