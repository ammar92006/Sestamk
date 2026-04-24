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
        }

        public event EventHandler CloseRequested;

        private void btnClose_Click(object sender, EventArgs e)
        {
            //ToastManager.ShowSuccess("Clicked", "تم الإغلاق بنجاح");
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }


    }
}
