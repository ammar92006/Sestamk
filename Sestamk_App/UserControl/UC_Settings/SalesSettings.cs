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
    public partial class SalesSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        private Guna.UI2.WinForms.Guna2Button btnSelectPilot;
        private Label lblDefaultPilot;
        private UC_DriverPicker _driverPicker;

        public SalesSettings()
        {
            InitializeComponent();
            SetupDefaultPilotUI();
            SetupEvents();
            LoadSettings();

            this.Load += (s, e) =>
            {
                BeginInvoke(new Action(() =>
                {
                    guna2Panel2.AutoScrollPosition = new Point(0, 0);
                    guna2vScrollBar1.Refresh();
                }));
            };
        }

        private void SetupDefaultPilotUI()
        {
            btnSelectPilot = new Guna.UI2.WinForms.Guna2Button();
            btnSelectPilot.Text = "تحديد الطيار الافتراضي";
            btnSelectPilot.Font = new Font("Alexandria", 10F, FontStyle.Regular, GraphicsUnit.Point);
            btnSelectPilot.Location = new Point(140, 306);
            btnSelectPilot.Size = new Size(180, 40);
            btnSelectPilot.BorderRadius = 8;
            btnSelectPilot.FillColor = Color.FromArgb(62, 144, 255);
            guna2Panel1.Controls.Add(btnSelectPilot);

            lblDefaultPilot = new Label();
            lblDefaultPilot.Text = "لم يتم التحديد";
            lblDefaultPilot.ForeColor = Color.White;
            lblDefaultPilot.Font = new Font("Alexandria", 11F, FontStyle.Bold, GraphicsUnit.Point);
            lblDefaultPilot.Location = new Point(340, 313);
            lblDefaultPilot.AutoSize = true;
            guna2Panel1.Controls.Add(lblDefaultPilot);

            _driverPicker = new UC_DriverPicker();
            _driverPicker.Size = new Size(600, 750);
            _driverPicker.Visible = false;
            _driverPicker.Anchor = AnchorStyles.None;
            this.Controls.Add(_driverPicker);
            _driverPicker.BringToFront();

            _driverPicker.OnDriverSelected += async (s, driver) => {
                await Sestamk.Classes.SettingsService.SetAsync("DefaultPilotId", driver.DeliveryId.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("DefaultPilotName", driver.FullName);
                lblDefaultPilot.Text = driver.FullName;
                _driverPicker.Visible = false;
                Sestamk.Classes.ToastManager.ShowSuccess("نجاح", "تم تعيين الطيار الافتراضي");
            };
            _driverPicker.OnCancel += (s, ev) => _driverPicker.Visible = false;

            btnSelectPilot.Click += (s, e) => {
                _driverPicker.Left = (this.Width - _driverPicker.Width) / 2;
                _driverPicker.Top = (this.Height - _driverPicker.Height) / 2;
                _driverPicker.Visible = true;
                _driverPicker.BringToFront();
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
                // Order Types
                guna2ToggleSwitch1.Checked = Sestamk.Classes.SettingsService.SalesEnableDineIn;
                guna2ToggleSwitch2.Checked = Sestamk.Classes.SettingsService.SalesEnableTakeaway;
                guna2ToggleSwitch3.Checked = Sestamk.Classes.SettingsService.SalesEnableDelivery;

                // Taxes
                guna2ToggleSwitch4.Checked = Sestamk.Classes.SettingsService.EnableTax;
                guna2TextBox1.Text = Sestamk.Classes.SettingsService.TaxPercent.ToString();

                // Discounts
                guna2ToggleSwitch5.Checked = Sestamk.Classes.SettingsService.EnableDiscount;
                guna2TextBox2.Text = Sestamk.Classes.SettingsService.DefaultDiscountPercent.ToString();

                // Payment Methods
                guna2CustomCheckBox1.Checked = Sestamk.Classes.SettingsService.PaymentCash;
                guna2CustomCheckBox2.Checked = Sestamk.Classes.SettingsService.PaymentVisa;
                guna2CustomCheckBox3.Checked = Sestamk.Classes.SettingsService.PaymentMaster;
                guna2CustomCheckBox4.Checked = Sestamk.Classes.SettingsService.PaymentMada;
                // استرجاع اسم الطيار
                if (Sestamk.Classes.SettingsService.DefaultPilotId > 0 && !string.IsNullOrEmpty(Sestamk.Classes.SettingsService.DefaultPilotName))
                {
                    lblDefaultPilot.Text = Sestamk.Classes.SettingsService.DefaultPilotName;
                }
                else
                {
                    lblDefaultPilot.Text = "لم يتم التحديد";
                }

                // Default Order Type
                string defaultType = Sestamk.Classes.SettingsService.DefaultOrderType;
                guna2CustomRadioButton1.Checked = (defaultType == "DineIn");
                guna2CustomRadioButton2.Checked = (defaultType == "Takeaway");
                guna2CustomRadioButton3.Checked = (defaultType == "Delivery");
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل الإعدادات: " + ex.Message);
            }
        }

        private async System.Threading.Tasks.Task SaveSettingsAsync()
        {
            try
            {
                // Order Types
                await Sestamk.Classes.SettingsService.SetAsync("Sales_EnableDineIn", guna2ToggleSwitch1.Checked.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("Sales_EnableTakeaway", guna2ToggleSwitch2.Checked.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("Sales_EnableDelivery", guna2ToggleSwitch3.Checked.ToString());

                // Taxes
                await Sestamk.Classes.SettingsService.SetAsync("EnableTax", guna2ToggleSwitch4.Checked.ToString());
                if (decimal.TryParse(guna2TextBox1.Text, out decimal tax))
                    await Sestamk.Classes.SettingsService.SetAsync("TaxPercent", tax.ToString());

                // Discounts
                await Sestamk.Classes.SettingsService.SetAsync("EnableDiscount", guna2ToggleSwitch5.Checked.ToString());
                if (decimal.TryParse(guna2TextBox2.Text, out decimal discount))
                    await Sestamk.Classes.SettingsService.SetAsync("DefaultDiscountPercent", discount.ToString());

                // Payment Methods
                await Sestamk.Classes.SettingsService.SetAsync("Payment_Cash", guna2CustomCheckBox1.Checked.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("Payment_Visa", guna2CustomCheckBox2.Checked.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("Payment_Master", guna2CustomCheckBox3.Checked.ToString());
                await Sestamk.Classes.SettingsService.SetAsync("Payment_Mada", guna2CustomCheckBox4.Checked.ToString());
                
                // Default Order Type
                string defaultOrderType = "Takeaway";
                if (guna2CustomRadioButton1.Checked) defaultOrderType = "DineIn";
                else if (guna2CustomRadioButton2.Checked) defaultOrderType = "Takeaway";
                else if (guna2CustomRadioButton3.Checked) defaultOrderType = "Delivery";
                await Sestamk.Classes.SettingsService.SetAsync("DefaultOrderType", defaultOrderType);

                Sestamk.Classes.ToastManager.ShowSuccess("تم الحفظ", "تم حفظ إعدادات المبيعات بنجاح ✅");
            }
            catch (Exception ex)
            {
                Sestamk.Classes.ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
        }

        public event EventHandler CloseRequested;

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}