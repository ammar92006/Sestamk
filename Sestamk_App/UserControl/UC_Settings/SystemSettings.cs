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
    public partial class SystemSettings : System.Windows.Forms.UserControl , ICloseRequest
    {
        public SystemSettings()
        {
            InitializeComponent();
            SetupEvents();
            LoadSettings();
            this.Load += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    guna2Panel3.AutoScrollPosition = new Point(0, 0);
                    guna2vScrollBar1.Refresh();
                }));
            };
        }

        private void SetupEvents()
        {
            guna2Button2.Click += async (s, e) => await SaveSettingsAsync();
            guna2Button3.Click += (s, e) => LoadSettings();
            guna2Button1.Click += (s, e) => BrowseBackupPath();
        }

        private void BrowseBackupPath()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "اختر مسار النسخ الاحتياطي";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    guna2TextBox1.Text = fbd.SelectedPath;
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                // Language
                guna2ComboBox1.Text = SettingsService.SystemLanguage;
                
                // Currency
                guna2ComboBox2.Text = SettingsService.CurrencyName; // Alternatively CurrencySymbol based on what's in combo

                // Auto Backup and Startup
                guna2ToggleSwitch1.Checked = SettingsService.SystemAutoBackup;
                guna2ToggleSwitch2.Checked = SettingsService.SystemRunAtStartup;

                // Max Login Attempts
                guna2TextBox2.Text = SettingsService.MaxLoginAttempts.ToString();

                // Backup Path
                guna2TextBox1.Text = SettingsService.SystemBackupPath;

                // Auto Logout
                guna2ToggleSwitch4.Checked = SettingsService.SystemAutoLogoutEnabled;
                guna2NumericUpDown1.Value = SettingsService.SystemAutoLogoutTimer;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل تحميل الإعدادات: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {
                if (!int.TryParse(guna2TextBox2.Text, out int attempts) || attempts <= 0)
                {
                    ToastManager.ShowWarning("تنبيه", "يرجى إدخال عدد محاولات صحيح (أكبر من 0)");
                    return;
                }

                await SettingsService.SetAsync("Login_MaxAttempts", attempts.ToString());
                await SettingsService.SetAsync("System_Language", guna2ComboBox1.Text);
                await SettingsService.SetAsync("CurrencyName", guna2ComboBox2.Text);
                await SettingsService.SetAsync("System_AutoBackup", guna2ToggleSwitch1.Checked.ToString());
                await SettingsService.SetAsync("System_RunAtStartup", guna2ToggleSwitch2.Checked.ToString());
                await SettingsService.SetAsync("System_BackupPath", guna2TextBox1.Text);
                await SettingsService.SetAsync("System_AutoLogoutEnabled", guna2ToggleSwitch4.Checked.ToString());
                await SettingsService.SetAsync("System_AutoLogoutTimer", guna2NumericUpDown1.Value.ToString());
                
                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات النظام بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }

        public event EventHandler CloseRequested;

        private void btnClose_Click(object sender, EventArgs e)
        {
            //ToastManager.ShowSuccess("Clicked", "تم الإغلاق بنجاح");
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }


    }
}
