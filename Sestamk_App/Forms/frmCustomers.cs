using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Guna.UI2.WinForms;

namespace Sestamk.Forms
{
    public partial class frmCustomers : Form
    {
        Color p_color = ColorTranslator.FromHtml("#0D8AFA");
        Color act_color = Color.FromArgb(34, 197, 94);
        Color inact_color = Color.DarkRed;
        public frmCustomers()
        {
            InitializeComponent();
            setupformUI();

        }
        private void setupformUI()
        {
            cmb_Cust_Rating.Items.Clear();
            cmb_Cust_Rating.Items.Add("★ - ضعيف");
            cmb_Cust_Rating.Items.Add("★★ - متوسط");
            cmb_Cust_Rating.Items.Add("★★★ - كويس");
            cmb_Cust_Rating.Items.Add("★★★★ - جيد جدًا");
            cmb_Cust_Rating.Items.Add("★★★★★ - ممتاز");

            // اختيار القيمة الافتراضية
            cmb_Cust_Rating.SelectedIndex = 0;

            pnlContent.HorizontalScroll.Enabled = false;
            pnlContent.HorizontalScroll.Visible = false;

            pnlContent.VerticalScroll.Enabled = false;
            pnlContent.VerticalScroll.Visible = false;

            pnlContent.MouseWheel += (s, e) => HideDefaultScrollBars();
            pnlContent.Scroll += (s, e) => HideDefaultScrollBars();


        }
        private void HideDefaultScrollBars()
        {
            pnlContent.HorizontalScroll.Enabled = false;
            pnlContent.HorizontalScroll.Visible = false;
            pnlContent.VerticalScroll.Enabled = false;
            pnlContent.VerticalScroll.Visible = false;
        }


        private void txt_Cust_Email_TextChanged(object sender, EventArgs e)
        {
            string email = txt_Cust_Email.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                txt_Cust_Email.IconLeft = null;
            }
            else if (IsValidEmail(email))
            {
                txt_Cust_Email.IconLeft = Properties.Resources.tick_mark_2;
            }
            else
            {
                txt_Cust_Email.IconLeft = Properties.Resources.cross_2;
            }
        }
        // دالة التحقق من صحة الإيميل
        private bool IsValidEmail(string email)
        {
            try
            {
                // Regex بسيط للتحقق من الإيميل
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMax_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;

        }
        private void SetButtonState(bool isActive)
        {
            Guna2Button btn = this.btnActive;
            Guna2Button btn2 = this.btnInactive;
            if (isActive)
            {
                btn.FillColor = act_color;
                //btn.Enabled = true;
                btn2.FillColor = Color.FromArgb(77, 89, 111);
                btn.ForeColor = Color.WhiteSmoke;
                btn2.ForeColor = Color.WhiteSmoke;
                //btn2.Enabled = false;
                lbl_Cust_Reason.Visible = false;
                txt_Cust_Reason.Visible = false;
            }
            else
            {
                btn.FillColor = Color.FromArgb(77, 89, 111);
                //btn.Enabled = false;
                btn2.FillColor = inact_color;
                btn.ForeColor = Color.WhiteSmoke;
                btn2.ForeColor = Color.WhiteSmoke;
                //btn2.Enabled = true;
                lbl_Cust_Reason.Visible = true;
                txt_Cust_Reason.Visible = true;
                lbl_Cust_Reason.Height = 30;
                txt_Cust_Reason.Height = 50;
            }
        }

        private void btnInactive_Click(object sender, EventArgs e)
        {
            SetButtonState(false);
        }

        private void btnActive_Click(object sender, EventArgs e)
        {
            SetButtonState(true);
        }
        private void dgvCustomers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colBalance" && e.Value != null)
            {
                decimal balance = Convert.ToDecimal(e.Value);
                if (balance < 0)
                    e.CellStyle.ForeColor = Color.FromArgb(239, 68, 68); 
                else if (balance > 0)
                    e.CellStyle.ForeColor = Color.FromArgb(34, 197, 94); 
                else
                    e.CellStyle.ForeColor = Color.FromArgb(148, 163, 184); 
            }

            // تلوين الـ Status
            if (dgvCustomers.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
            {
                string status = e.Value.ToString();

                if (status == "ACTIVE")
                {
                    e.CellStyle.BackColor = Color.FromArgb(22, 163, 74); // أخضر
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                }
                else if (status == "INACTIVE")
                {
                    e.CellStyle.BackColor = Color.FromArgb(71, 85, 105); // رمادي
                    e.CellStyle.ForeColor = Color.White;
                    e.CellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                }
            }
        }
    }
}
