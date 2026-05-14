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
    public partial class RestaurantSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        private Guna.UI2.WinForms.Guna2TextBox txtName;
        private Guna.UI2.WinForms.Guna2TextBox txtPhone;
        private Guna.UI2.WinForms.Guna2TextBox txtAddress;
        private Guna.UI2.WinForms.Guna2Button btnSave;

        public RestaurantSettings()
        {
            InitializeComponent();
            CreateDynamicControls();
            SetupEvents();
            LoadSettings();
        }

        private void CreateDynamicControls()
        {
            // إنشاء حقول إدخال المطعم برمجياً للحفاظ على التصميم العام
            // TextBox Name
            txtName = CreateTextBox("txtName", new Point(400, 115));
            guna2Panel2.Controls.Add(txtName);

            // Label Phone
            Label lblPhone = CreateLabel("رقم الهاتف", new Point(951, 206));
            guna2Panel2.Controls.Add(lblPhone);

            // TextBox Phone
            txtPhone = CreateTextBox("txtPhone", new Point(400, 195));
            guna2Panel2.Controls.Add(txtPhone);

            // Label Address
            Label lblAddress = CreateLabel("العنوان", new Point(951, 286));
            guna2Panel2.Controls.Add(lblAddress);

            // TextBox Address
            txtAddress = CreateTextBox("txtAddress", new Point(400, 275));
            guna2Panel2.Controls.Add(txtAddress);

            // Button Save
            btnSave = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "حفظ التغييرات",
                Font = new Font("Alexandria", 16F, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(58, 141, 252),
                BorderRadius = 12,
                Size = new Size(254, 66),
                Location = new Point(46, 680)
            };
            guna2Panel1.Controls.Add(btnSave);
        }

        private Guna.UI2.WinForms.Guna2TextBox CreateTextBox(string name, Point location)
        {
            return new Guna.UI2.WinForms.Guna2TextBox
            {
                Name = name,
                Location = location,
                Size = new Size(500, 54),
                BorderRadius = 12,
                BorderThickness = 3,
                BorderColor = Color.FromArgb(42, 47, 59),
                FillColor = Color.FromArgb(26, 31, 43),
                ForeColor = Color.FromArgb(229, 231, 235),
                Font = new Font("Segoe UI", 14F),
                TextAlign = HorizontalAlignment.Right
            };
        }

        private Label CreateLabel(string text, Point location)
        {
            return new Label
            {
                Text = text,
                Location = location,
                AutoSize = true,
                Font = new Font("Alexandria", 18F, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(229, 231, 235),
                TextAlign = ContentAlignment.MiddleRight
            };
        }

        private void SetupEvents()
        {
            btnSave.Click += async (s, e) => await SaveSettingsAsync();
            btnClose.Click += (s, e) => CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void LoadSettings()
        {
            txtName.Text = Sestamk.Classes.SettingsService.StoreName;
            txtPhone.Text = Sestamk.Classes.SettingsService.StorePhone;
            txtAddress.Text = Sestamk.Classes.SettingsService.StoreAddress;
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {
                await Sestamk.Classes.SettingsService.SetAsync("StoreName", txtName.Text);
                await Sestamk.Classes.SettingsService.SetAsync("StorePhone", txtPhone.Text);
                await Sestamk.Classes.SettingsService.SetAsync("StoreAddress", txtAddress.Text);

                Sestamk.Classes.ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات المطعم بنجاح ✅");
            }
            catch (Exception ex)
            {
                Sestamk.Classes.ToastManager.ShowError("خطأ", "فشل الحفظ: " + ex.Message);
            }
        }
    }
}
