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
    public partial class PrinterSettings : System.Windows.Forms.UserControl, ICloseRequest
    {

        public PrinterSettings()
        {
            InitializeComponent();
        }

        public event EventHandler CloseRequested;

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
