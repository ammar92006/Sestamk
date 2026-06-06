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
    public partial class NotificationsSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        public NotificationsSettings()
        {
            InitializeComponent();
            SetupEvents();
            LoadSettings();
        }

        // ── ربط الأحداث ──────────────────────────────────────────────────
        private void SetupEvents()
        {
            guna2Button2.Click += (s, e) => SaveSettings();   // حفظ التغييرات
            guna2Button3.Click += (s, e) => ResetSettings();  // إعادة ضبط
        }

        // ── تحميل الإعدادات من قاعدة البيانات ────────────────────────────
        private void LoadSettings()
        {
            try
            {
                guna2ToggleSwitch1.Checked = SettingsService.NotificationNewOrderSound;
                guna2ToggleSwitch2.Checked = SettingsService.NotificationErrorSound;
                guna2ToggleSwitch4.Checked = SettingsService.NotificationLowStockAlert;
                guna2ToggleSwitch3.Checked = SettingsService.NotificationPrintFailAlert;
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل تحميل إعدادات الإشعارات: " + ex.Message);
            }
        }

        // ── حفظ التغييرات إلى قاعدة البيانات ─────────────────────────────
        private async void SaveSettings()
        {
            guna2Button2.Enabled = false;
            guna2Button2.Text = "جارٍ الحفظ…";

            try
            {
                await SettingsService.SetAsync("Notification_NewOrderSound", guna2ToggleSwitch1.Checked.ToString().ToLower());
                await SettingsService.SetAsync("Notification_ErrorSound", guna2ToggleSwitch2.Checked.ToString().ToLower());
                await SettingsService.SetAsync("Notification_LowStockAlert", guna2ToggleSwitch4.Checked.ToString().ToLower());
                await SettingsService.SetAsync("Notification_PrintFailAlert", guna2ToggleSwitch3.Checked.ToString().ToLower());

                ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات الإشعارات بنجاح ✅");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
            finally
            {
                guna2Button2.Enabled = true;
                guna2Button2.Text = "حفظ التغييرات";
            }
        }

        // ── إعادة ضبط إلى القيم الافتراضية ───────────────────────────────
        private void ResetSettings()
        {
            guna2ToggleSwitch1.Checked = true;  // صوت طلب جديد
            guna2ToggleSwitch2.Checked = true;  // صوت الخطأ
            guna2ToggleSwitch3.Checked = true;  // تنبيه فشل الطباعة
            guna2ToggleSwitch4.Checked = true;  // تنبيه نقص المخزون

            ToastManager.ShowInfo("إعادة ضبط", "تم إعادة ضبط الإعدادات إلى القيم الافتراضية.\nاضغط \"حفظ التغييرات\" للتطبيق.");
        }

        // ── إغلاق ────────────────────────────────────────────────────────
        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
