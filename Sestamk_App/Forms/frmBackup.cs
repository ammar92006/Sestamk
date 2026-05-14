using Sestamk.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmBackup : BaseForm
    {
        protected override Size DesignClientSize => new Size(1300, 900);

        public frmBackup()
        {
            InitializeComponent();
            
            // Link Events
            this.Load += FrmBackup_Load;
            this.btn_BrowseManual.Click += (s, e) => SelectFolder(txt_BackupPath);
            this.btn_BrowseAuto.Click += (s, e) => SelectFolder(txt_AutoPath);
            this.btn_CreateBackup.Click += Btn_CreateBackup_Click;
            this.btn_SaveSettings.Click += Btn_SaveSettings_Click;
        }

        private async void FrmBackup_Load(object sender, EventArgs e)
        {
            await RefreshDataAsync();
        }

        private async Task RefreshDataAsync()
        {
            try
            {
                // Load Settings
                var settings = await BackupManager.GetSettingsAsync();
                toggle_AutoBackup.Checked = settings.AutoBackupEnabled;
                num_Interval.Value = settings.BackupIntervalHours;
                txt_AutoPath.Text = settings.BackupPath;
                
                // Load History
                DataTable dt = await BackupManager.GetBackupHistoryAsync();
                dgv_History.DataSource = dt;
                dgv_Restore.DataSource = dt; // Simplified for now
                
                FormatGrids();
                UpdateStatCards(dt, settings);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "حدث خطأ أثناء تحميل البيانات: " + ex.Message);
            }
        }

        private void FormatGrids()
        {
            if (dgv_History.Columns.Count > 0)
            {
                dgv_History.Columns["Id"].Visible = false;
                dgv_History.Columns["FilePath"].Visible = false;
                dgv_History.Columns["FileName"].HeaderText = "اسم الملف";
                dgv_History.Columns["FileSizeMB"].HeaderText = "الحجم (MB)";
                dgv_History.Columns["BackupType"].HeaderText = "النوع";
                dgv_History.Columns["CreatedBy"].HeaderText = "بواسطة";
                dgv_History.Columns["CreatedDate"].HeaderText = "التاريخ";
                dgv_History.Columns["Status"].HeaderText = "الحالة";
            }
        }

        private void UpdateStatCards(DataTable history, BackupSettingsModel settings)
        {
            // Implementation of stat card updates based on history data
            // This would normally find the last success record, sum of records, etc.
        }

        private void SelectFolder(Guna.UI2.WinForms.Guna2TextBox target)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    target.Text = fbd.SelectedPath;
                }
            }
        }

        private async void Btn_CreateBackup_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_BackupPath.Text))
            {
                ToastManager.ShowWarning("تنبيه", "يرجى اختيار مسار الحفظ أولاً.");
                return;
            }

            try
            {
                btn_CreateBackup.Enabled = false;
                progress_Backup.Visible = true;
                progress_Backup.Value = 30;

                bool success = await BackupManager.CreateBackupAsync(txt_BackupPath.Text, txt_BackupName.Text);

                progress_Backup.Value = 100;
                await Task.Delay(500);

                if (success)
                {
                    ToastManager.ShowSuccess("تم بنجاح", "تم إنشاء النسخة الاحتياطية بنجاح.");
                    await RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("فشل العملية", "خطأ في إنشاء النسخة الاحتياطية: " + ex.Message);
            }
            finally
            {
                btn_CreateBackup.Enabled = true;
                progress_Backup.Visible = false;
            }
        }

        private async void Btn_SaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = new BackupSettingsModel
                {
                    AutoBackupEnabled = toggle_AutoBackup.Checked,
                    BackupIntervalHours = (int)num_Interval.Value,
                    BackupPath = txt_AutoPath.Text,
                    BackupTime = new TimeSpan(2, 0, 0) // Default for now
                };

                await BackupManager.SaveSettingsAsync(settings);
                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات النسخ التلقائي بنجاح.");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }
    }
}
