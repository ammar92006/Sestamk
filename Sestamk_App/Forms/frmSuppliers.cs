using Sestamk.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmSuppliers : BaseForm
    {
        public frmSuppliers()
        {
            InitializeComponent();

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(guna2Button4, guna2Button5);
        }
    }
}
