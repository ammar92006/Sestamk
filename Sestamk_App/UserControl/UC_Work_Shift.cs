using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Sestamk.UserControl
{
    public partial class UC_Work_Shift : System.Windows.Forms.UserControl
    {
        public event EventHandler OnShiftStarted;

        public UC_Work_Shift()
        {
            InitializeComponent();
            guna2Button1.Click += BtnStartShift_Click;
        }

        public async void LoadData()
        {
            try
            {
                // Set current user name
                label4.Text = $"الموظف: {Sestamk.Classes.UserSession.Full_Name ?? Sestamk.Classes.UserSession.UserName}";

                // Get last closed shift
                string query = "SELECT TOP 1 CloseDateTime FROM Shifts WHERE Status = 1 ORDER BY CloseDateTime DESC";
                object lastClose = await Sestamk.Classes.DB_Server.ScalarAsync(query, null);

                if (lastClose != null && lastClose != DBNull.Value)
                {
                    DateTime closeTime = Convert.ToDateTime(lastClose);
                    string displayTime = "";
                    if (closeTime.Date == DateTime.Today)
                        displayTime = $"اليوم {closeTime:hh:mm tt}";
                    else if (closeTime.Date == DateTime.Today.AddDays(-1))
                        displayTime = $"أمس {closeTime:hh:mm tt}";
                    else
                        displayTime = closeTime.ToString("yyyy/MM/dd hh:mm tt");
                    
                    label3.Text = $"آخر إغلاق: {displayTime}";
                }
                else
                {
                    label3.Text = "آخر إغلاق: غير متوفر";
                }
            }
            catch
            {
                // Ignore silent errors on init
            }
        }

        private async void BtnStartShift_Click(object sender, EventArgs e)
        {
            // Open a shift with 0 default cash (or could also ask for starting cash)
            guna2Button1.Enabled = false;
            var shift = await Sestamk.Classes.ShiftService.OpenShiftAsync(Sestamk.Classes.UserSession.UserId, 0, "فتح الوردية");
            guna2Button1.Enabled = true;

            if (shift != null)
            {
                // Notify parent
                OnShiftStarted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
