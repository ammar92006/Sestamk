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
    public partial class ScannerSettings : System.Windows.Forms.UserControl , ICloseRequest
    {
        public event EventHandler CloseRequested;

        private Guna.UI2.WinForms.Guna2ComboBox cmbConnectionType;

        public ScannerSettings()
        {
            InitializeComponent();
            CreateConnectionTypeControls();
            SetupEvents();
            LoadSettings();
        }

        private void CreateConnectionTypeControls()
        {
            // إزاحة العناصر الحالية للأسفل لتوفير مساحة
            guna2ComboBox1.Top += 116;
            label4.Top += 116;
            guna2ComboBox2.Top += 116;
            label5.Top += 116;

            // إنشاء عنوان نوع الاتصال
            Label lblConnectionType = new Label
            {
                Text = "نوع الاتصال",
                Location = new Point(971, 112),
                AutoSize = true,
                Font = new Font("Alexandria", 18F, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(229, 231, 235),
                TextAlign = ContentAlignment.MiddleRight
            };
            guna2Panel2.Controls.Add(lblConnectionType);

            // إنشاء القائمة المنسدلة لنوع الاتصال
            cmbConnectionType = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Name = "cmbConnectionType",
                Location = new Point(55, 103),
                Size = new Size(500, 36),
                BorderRadius = 12,
                BorderThickness = 3,
                BorderColor = Color.FromArgb(42, 47, 59),
                FillColor = Color.FromArgb(26, 31, 43),
                ForeColor = Color.FromArgb(229, 231, 235),
                Font = new Font("Segoe UI", 14F),
                TextAlign = HorizontalAlignment.Right
            };
            cmbConnectionType.Items.Add("USB");
            cmbConnectionType.Items.Add("COM");
            guna2Panel2.Controls.Add(cmbConnectionType);

            cmbConnectionType.SelectedIndexChanged += (s, e) =>
            {
                bool isCom = cmbConnectionType.SelectedItem?.ToString() == "COM";
                guna2ComboBox1.Enabled = isCom;
                guna2ComboBox2.Enabled = isCom;
            };
        }

        private void SetupEvents()
        {
            guna2Button2.Click += async (s, e) => await SaveSettingsAsync();
            guna2Button3.Click += (s, e) => LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Connection Type
                string connType = Sestamk.Classes.SettingsService.ScannerConnectionType;
                if (cmbConnectionType.Items.Contains(connType))
                    cmbConnectionType.SelectedItem = connType;
                else
                    cmbConnectionType.SelectedItem = "USB";

                // Load COM Ports
                guna2ComboBox1.Items.Clear();
                foreach (string port in System.IO.Ports.SerialPort.GetPortNames())
                {
                    guna2ComboBox1.Items.Add(port);
                }

                string savedPort = Sestamk.Classes.SettingsService.ScannerCOMPort;
                if (!string.IsNullOrEmpty(savedPort) && guna2ComboBox1.Items.Contains(savedPort))
                    guna2ComboBox1.SelectedItem = savedPort;
                else if (guna2ComboBox1.Items.Count > 0)
                    guna2ComboBox1.SelectedIndex = 0;

                guna2ComboBox2.Text = Sestamk.Classes.SettingsService.ScannerBaudRate;
                guna2TextBox1.Text = Sestamk.Classes.SettingsService.ScannerTimeout.ToString();
                guna2ToggleSwitch1.Checked = Sestamk.Classes.SettingsService.ScannerEnabled;
            }
            catch (Exception ex)
            {
                Sestamk.Classes.ToastManager.ShowError("خطأ", "فشل تحميل الإعدادات: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {
                if (cmbConnectionType.SelectedItem != null)
                    await Sestamk.Classes.SettingsService.SetAsync("Scanner_ConnectionType", cmbConnectionType.SelectedItem.ToString());

                if (guna2ComboBox1.SelectedItem != null)
                    await Sestamk.Classes.SettingsService.SetAsync("Scanner_COMPort", guna2ComboBox1.SelectedItem.ToString());

                await Sestamk.Classes.SettingsService.SetAsync("Scanner_BaudRate", guna2ComboBox2.Text);
                await Sestamk.Classes.SettingsService.SetAsync("Scanner_Enabled", guna2ToggleSwitch1.Checked.ToString());

                if (int.TryParse(guna2TextBox1.Text, out int timeout))
                    await Sestamk.Classes.SettingsService.SetAsync("Scanner_Timeout", timeout.ToString());

                Sestamk.Classes.ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات الباركود بنجاح ✅");
            }
            catch (Exception ex)
            {
                Sestamk.Classes.ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
