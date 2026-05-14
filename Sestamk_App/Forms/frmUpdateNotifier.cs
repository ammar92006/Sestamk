using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sestamk.Classes;

namespace Sestamk.Forms
{
    public partial class frmUpdateNotifier : Form
    {
        private UpdateManifest _manifest;

        public frmUpdateNotifier(UpdateManifest manifest)
        {
            InitializeComponent();
            _manifest = manifest;

            // تحريك الفورم من الـ Panel (تأكد من وجود دوال Attach في Main_Methods)
            Main_Methods.Attach(lblVersion, this);
            Main_Methods.Attach(guna2Panel1, this);
        }

        private void frmUpdateNotifier_Load(object sender, EventArgs e)
        {
            //lblVersion.Text = $"تحديث جديد متاح: V{_manifest.Version} Channel {_manifest.Channel}";
            lblDate.Text = $"تاريخ الإصدار: {_manifest.ReleaseDate}";
            //lblUpdateText.Text = "تحديث جديد متاح:";
            lblVersion.Text = $"V{_manifest.Version}";
            lblChannel.Text = _manifest.Channel;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("مميزات هذا التحديث:");
            foreach (var item in _manifest.WhatsNew)
            {
                sb.AppendLine($"• {item}");
            }
            txtWhatsNew.Text = sb.ToString();

            // إخفاء زر "لاحقاً" لو التحديث إجباري
            if (_manifest.IsMandatory)
            {
                btnLater.Visible = false;
            }

            // تصفير وإخفاء شريط التقدم في البداية
            guna2ProgressBar1.Value = 0;
            guna2ProgressBar1.Visible = false;
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;
            btnLater.Enabled = false;
            btnUpdate.Text = "جاري التحميل...";

            // إظهار شريط التقدم
            guna2ProgressBar1.Visible = true;

            // 🔴 إنشاء كائن Progress لاستقبال التحديثات من UpdateManager
            var progressIndicator = new Progress<int>(ReportProgress);

            // بدء التحميل مع تمرير كائن الـ Progress
            bool success = await UpdateManager.DownloadUpdateFilesAsync(_manifest, progressIndicator);

            if (success)
            {
                guna2ProgressBar1.Value = 100;
                ToastManager.ShowSuccess("تم تحميل التحديثات بنجاح", "سيتم إعادة تشغيل النظام لتطبيقها ✅");

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "Updater.exe";
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);

                Environment.Exit(0);
            }
            else
            {
                ToastManager.ShowError("فشل تحميل التحديثات", "حدث خطأ أثناء تحميل الملفات. يرجى التأكد من الإنترنت ❌");
                btnUpdate.Enabled = true;
                btnUpdate.Text = "تحديث الآن";
                guna2ProgressBar1.Visible = false;

                if (!_manifest.IsMandatory) btnLater.Enabled = true;
            }
        }

        // دالة لتحديث شريط التقدم
        private void ReportProgress(int value)
        {
            // التأكد من أن القيمة لا تتجاوز 100
            if (value >= 0 && value <= 100)
            {
                guna2ProgressBar1.Value = value;
            }
        }

        private void btnLater_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}