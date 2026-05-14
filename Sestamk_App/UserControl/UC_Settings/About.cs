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
    public partial class About : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        public About()
        {
            InitializeComponent();
            SetupDynamicInfo();
            SetupClickableLinks();
        }

        // ── عرض المعلومات الديناميكية ─────────────────────────────────────
        private void SetupDynamicInfo()
        {
            try
            {
                // عرض الإصدار الفعلي من AssemblyInfo
                string version = Application.ProductVersion ?? "1.0.0";
                label6.Text = $"الإصدار {version}";

                // عرض اسم المتجر من الإعدادات
                string storeName = SettingsService.StoreName;
                if (!string.IsNullOrEmpty(storeName))
                    label4.Text = storeName;
            }
            catch
            {
                // fallback — الاحتفاظ بالقيم الافتراضية من Designer
            }
        }

        // ── جعل الروابط تفاعلية (قابلة للضغط) ────────────────────────────
        private void SetupClickableLinks()
        {
            // البريد الإلكتروني
            MakeClickable(label11, () => OpenUrl("mailto:engammarahmed@gmail.com"));

            // الهاتف
            MakeClickable(label14, () => OpenUrl("tel:01281637066"));

            // الموقع الإلكتروني
            MakeClickable(label16, () => OpenUrl(label16.Text));
        }

        /// <summary>
        /// تحويل Label إلى رابط قابل للضغط مع تأثيرات Hover
        /// </summary>
        private void MakeClickable(Label label, Action onClick)
        {
            Color originalColor = label.ForeColor;
            Color hoverColor = Color.FromArgb(58, 141, 252); // أزرق مميز

            label.Cursor = Cursors.Hand;

            label.Click += (s, e) => onClick();

            label.MouseEnter += (s, e) =>
            {
                label.ForeColor = hoverColor;
                label.Font = new Font(label.Font, label.Font.Style | FontStyle.Underline);
            };

            label.MouseLeave += (s, e) =>
            {
                label.ForeColor = originalColor;
                label.Font = new Font(label.Font, label.Font.Style & ~FontStyle.Underline);
            };
        }

        /// <summary>
        /// فتح رابط في المتصفح أو التطبيق المناسب
        /// </summary>
        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "تعذر فتح الرابط: " + ex.Message);
            }
        }

        // ── زر Facebook المطور ───────────────────────────────────────────
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            OpenUrl("https://www.facebook.com/ammar.92006");
        }

        // ── إغلاق ────────────────────────────────────────────────────────
        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
