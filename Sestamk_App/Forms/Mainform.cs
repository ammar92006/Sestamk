using Google.GenAI;
using Google.GenAI.Types;
using Guna.UI2.WinForms;
using Sestamk.Classes;
using Sestamk.UserControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Sestamk.Forms
{
    public partial class Mainform : Form
    {
        public Mainform()
        {
            InitializeComponent();

            pnl_Main.Controls.Clear();
            UC_Dashboard dashboard = new UC_Dashboard();
            dashboard.Dock = DockStyle.Fill;
            pnl_Main.Controls.Add(dashboard);
            user_permissions();
        }

        private void Mainform_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
        private void btn_logout_Click(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
            {
                Login login = new Login();
                if (form != login)
                {
                    form.Close();
                }
                login.Show();
            }
        }
        private void btn_Customers_Click(object sender, EventArgs e)
        {
            Main_Methods.OpenForm(typeof(frmCustomers));
        }

        private void btn_Suppliers_Click(object sender, EventArgs e)
        {
            Main_Methods.OpenForm(typeof(frmSuppliers));
        }
        private void user_permissions()
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // تعطيل الزر لمنع النقرات المتعددة
            btn_Menu3.Enabled = false;

            // يمكنك تغيير نص الزر للإشارة إلى التحميل
            string originalText = btn_Menu3.Text;
            btn_Menu3.Text = "جاري المعالجة...";

            // يمكنك إضافة مؤشر تحميل (اختياري)
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // استخدام الدالة الأساسية
                string result = await GeminiAI.GenerateText("متي فرضت الصلاه في الاسلام والادله الكامله وكل التفاصيل؟");

                // عرض النتيجة
                MessageBox.Show(result, "رد Gemini AI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    $"خطأ في البيانات المدخلة:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (TimeoutException)
            {
                MessageBox.Show(
                    "انتهت مهلة الاتصال بالخادم. يرجى المحاولة مرة أخرى.",
                    "انتهت المهلة",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"خطأ في الاتصال بالإنترنت:\n{ex.Message}",
                    "خطأ في الاتصال",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    $"خطأ في معالجة البيانات:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ غير متوقع:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                // إعادة تفعيل الزر واستعادة المؤشر
                btn_Menu3.Enabled = true;
                btn_Menu3.Text = originalText;
                Cursor.Current = Cursors.Default;
            }
        }

        private void btn_Menu4_Click(object sender, EventArgs e)
        {
            frmUpdate updateForm = new frmUpdate();
            updateForm.ShowDialog();
        }
    }
}
