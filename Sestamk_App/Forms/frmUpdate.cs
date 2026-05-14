using Sestamk.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmUpdate : BaseForm
    {
        private UpdateManager updateManager;
        private UpdateInfo currentUpdate;
        private const string GITHUB_OWNER = "ammar92006";  // ضع اسم المستخدم بتاعك على GitHub
        private const string GITHUB_REPO = "Sestamk";      // ضع اسم الـ Repository

        public frmUpdate()
        {
            InitializeComponent();
            //string currentVersion = "1.0.0"; // الإصدار الحالي
            //updateManager = new UpdateManager(GITHUB_OWNER, GITHUB_REPO, currentVersion);
        }

        private async void frmUpdate_Load(object sender, EventArgs e)
        {
            await CheckForUpdates();
        }
        private async Task CheckForUpdates()
        {
            try
            {
                lblStatus.Text = "جاري البحث عن تحديثات...";
                progressBar1.Style = ProgressBarStyle.Marquee;
                btnDownload.Enabled = false;

                //currentUpdate = await updateManager.CheckForUpdatesAsync();

                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = 0;

                if (currentUpdate.UpdateAvailable)
                {
                    lblStatus.Text = $"✅ تحديث متاح: {currentUpdate.LatestVersion}";

                    txtChangeLog.Text = $"الإصدار الحالي: {currentUpdate.CurrentVersion}\r\n";
                    txtChangeLog.Text += $"الإصدار الجديد: {currentUpdate.LatestVersion}\r\n\r\n";
                    txtChangeLog.Text += "========== ما الجديد ==========\r\n\r\n";
                    txtChangeLog.Text += currentUpdate.ChangeLog;

                    //lblFileSize.Text = $"الحجم: {UpdateManager.FormatFileSize(currentUpdate.FileSize)}";

                    btnDownload.Enabled = true;
                }
                else
                {
                    lblStatus.Text = "✓ لديك أحدث إصدار";
                    txtChangeLog.Text = "أنت تستخدم أحدث إصدار متاح.";
                }
            }
            catch (Exception ex)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                lblStatus.Text = "❌ فشل التحقق";
                txtChangeLog.Text = $"خطأ: {ex.Message}";
            }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            try
            {
                btnDownload.Enabled = false;
                btnClose.Enabled = false;

                lblStatus.Text = "جاري التحميل...";

                var progress = new Progress<int>(percent =>
                {
                    progressBar1.Value = percent;
                    lblStatus.Text = $"جاري التحميل... {percent}%";
                });

                //string updateFile = await updateManager.DownloadUpdateAsync(currentUpdate.DownloadUrl, progress);

                lblStatus.Text = "✓ تم التحميل!";

                if (frmConfirm.Show("تطبيق التحديث", "تم التحميل بنجاح.\n\nسيتم إعادة تشغيل البرنامج.\n\nمتابعة؟"))
                {
                    //updateManager.ApplyUpdate(updateFile);
                }
                else
                {
                    btnDownload.Enabled = true;
                    btnClose.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", $"خطأ: {ex.Message}");
                btnDownload.Enabled = true;
                btnClose.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
