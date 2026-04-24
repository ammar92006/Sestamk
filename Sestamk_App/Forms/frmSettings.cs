using Sestamk.Classes;
using Sestamk.UserControl.UC_Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.Forms
{     
    public interface ICloseRequest
    {
            event EventHandler CloseRequested;
    }
    public partial class frmSettings : Form
    {
        public frmSettings()
        {
            InitializeComponent();
            Main_Methods.Attach(guna2Panel4, this);
            Main_Methods.Attach(label2, this);
            Main_Methods.Attach(label1, this);

            LoadUserControl("system", () => new SystemSettings());
            btnSystemSettings.Checked = true;
        }



        //private void LoadUserControl(System.Windows.Forms.UserControl control)
        //{
        //    panelMain.Controls.Clear();
        //    control.Dock = DockStyle.Fill;
        //    panelMain.Controls.Add(control);
        //}

        Dictionary<string, System.Windows.Forms.UserControl> controlsCache = new Dictionary<string, System.Windows.Forms.UserControl>();

        private void LoadUserControl(string key, Func<System.Windows.Forms.UserControl> createControl)
        {

            if (!controlsCache.ContainsKey(key))
            {
                controlsCache[key] = createControl();
            }

            var control = controlsCache[key];

            // لو الكنترول بيدعم القفل
            if (control is ICloseRequest closeControl)
            {
                closeControl.CloseRequested += (s, e) =>
                {
                    //ToastManager.ShowSuccess("Clicked", "تم الإغلاق بنجاح");
                    this.Close();
                };
            }


            //if (panelMain.Controls.Count > 0 && panelMain.Controls[0] == control)
            //    return;

            panelMain.Controls.Clear();
            control.Dock = DockStyle.Fill;

            panelMain.Controls.Add(control);

        }

        private void btnSystemSettings_Click(object sender, EventArgs e)
        {
            //LoadUserControl(new SystemSettings());

            LoadUserControl("system", () => new SystemSettings());
            setActiveButton(btnSystemSettings);

        }

        private void btnSalesSettings_Click(object sender, EventArgs e)
        {

            LoadUserControl("sales", () => new SalesSettings());
            setActiveButton(btnSalesSettings);
        }


        private void setActiveButton(Guna.UI2.WinForms.Guna2Button activeButton)
        {
            foreach (var control in guna2Panel5.Controls)
            {
                if (control is Guna.UI2.WinForms.Guna2Button button)
                {
                    button.Checked = button == activeButton;
                }
            }
        }

        private void btnPrinterSettings_Click(object sender, EventArgs e)
        {

            LoadUserControl("printer", () => new PrinterSettings());
            setActiveButton(btnPrinterSettings);
        }

        private void btnScannerSettings_Click(object sender, EventArgs e)
        {

            LoadUserControl("scanner", () => new ScannerSettings());
            setActiveButton(btnScannerSettings);
        }
    }
}
