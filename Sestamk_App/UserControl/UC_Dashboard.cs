using Sestamk.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    public partial class UC_Dashboard : System.Windows.Forms.UserControl    
    {
        public UC_Dashboard()
        {
            InitializeComponent();

            // ── تحديث بيانات المستخدم من الجلسة ──
            UserSession.UpdateUserDisplay(label37, label38);
        }
    }
}
