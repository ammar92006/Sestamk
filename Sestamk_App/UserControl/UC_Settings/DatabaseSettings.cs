using Guna.UI2.WinForms;
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
    public partial class DatabaseSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        public DatabaseSettings()
        {
            InitializeComponent();
            SetupEvents();
        }

        private void SetupEvents()
        {
            guna2Button2.Click += async (s, e) => await SaveSettingsAsync();
            guna2Button3.Click += (s, e) => LoadSettings();
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {


                await SettingsService.SetAsync("DB_DataSource", txtDB_DataSource.Text);
                await SettingsService.SetAsync("DB_UserID", txtDB_UserID.Text);
                await SettingsService.SetAsync("DB_Password", txtDB_Password.Text);

                // تطبيق التغييرات فوراً على محرك الاتصال الحالي
                DB_Server.Initialize(txtDB_DataSource.Text, txtDB_UserID.Text, txtDB_Password.Text);

                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات النظام بنجاح ✅\nتم تطبيق الإعدادات الجديدة فوراً.");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }
        private void LoadSettings()
        {
            try
            {
                // Language
                txtDB_DataSource.Text = SettingsService.DB_DataSource;
                txtDB_UserID.Text = SettingsService.DB_UserID;
                txtDB_Password.Text = SettingsService.DB_Password;


            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل تحميل الإعدادات: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
