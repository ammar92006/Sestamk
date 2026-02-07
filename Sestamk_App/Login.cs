using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;
using Sestamk.Classes;
using Sestamk.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Windows.Input;
using System.Net.NetworkInformation;

namespace Sestamk
{
    public partial class Login : Form
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        public Login()
        {
            InitializeComponent();
        }
        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void pn_1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        private void btn_login_Click(object sender, EventArgs e)
        {
            DoLogin();
        }
        private async Task DoLogin() // 1. التغيير من void إلى Task لجعلها قابلة للانتظار (Awaitable)
        {
            // التحقق من المدخلات (UI Validation) يبقى كما هو لأنه سريع ومحلي
            if (String.IsNullOrEmpty(txt_username.Text))
            {
                MessageBox.Show("برجاء ادخال اسم المستخدم");
                return;
            }
            if (String.IsNullOrEmpty(txt_password.Text))
            {
                MessageBox.Show("برجاء ادخال كلمه السر");
                return;
            }

            string enteredusername = txt_username.Text.Trim();
            string enteredpassword = txt_password.Text.Trim();

            // حساب الأدمن العام (بدون قاعدة بيانات)
            if (enteredusername == Admin_Info.usernameadmin && enteredpassword == Admin_Info.passwordadmin)
            {
                UserSession.UserName = enteredusername;
                UserSession.Password = enteredpassword;
                this.Hide();
                Mainform mainform = new Mainform();
                mainform.Show();
                return;
            }

            string query = "SELECT * FROM Users WHERE (Username = @enteredusername OR Email = @enteredusername) AND Password = @enteredpassword";

            try
            {
                // 2. استخدام GetConnection و using يضمن فتح وقفل الاتصال بشكل سليم وفوري (Connection Pooling)
                using (SqlConnection conn = DB_Server.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@enteredusername", enteredusername);
                        cmd.Parameters.AddWithValue("@enteredpassword", enteredpassword);

                        // 3. فتح الاتصال بشكل غير متزامن (OpenAsync) لمنع تهنيج الشاشة أثناء انتظار السيرفر
                        await conn.OpenAsync();

                        // 4. تنفيذ القارئ بشكل غير متزامن (ExecuteReaderAsync)
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            // 5. قراءة السطر الأول بشكل غير متزامن (ReadAsync)
                            if (await reader.ReadAsync())
                            {
                                // فحص حالة الحظر
                                bool userStats = reader["Is_blocked"] != DBNull.Value && Convert.ToBoolean(reader["Is_blocked"]);

                                if (userStats)
                                {
                                    string msg_block = $"هذا المستخدم محظور من النظام بسبب : \n {reader["block_reason"]}";

                                    // 6. يفضل جعل ميثود اللوج async برضه واستدعاؤها بـ await
                                    Log_Login_info_Async(username: enteredusername, password: enteredpassword, full_name: "Nothing", note: "Nothing", login_status: "failed", action_type: "login", fail_reason: msg_block);

                                    MessageBox.Show(msg_block);
                                    txt_password.Text = String.Empty;
                                    txt_username.Text = String.Empty;
                                    txt_username.Focus();
                                    return;
                                }

                                // تعبئة بيانات الجلسة من الـ Reader مباشرة (أسرع من الـ DataTable)
                                UserSession.UserId = Convert.ToInt32(reader["ID"]);
                                UserSession.UserName = enteredusername;
                                UserSession.Password = enteredpassword;
                                UserSession.Full_Name = reader["Full_Name"]?.ToString() ?? "string.Empty";
                                UserSession.Email = reader["Email"]?.ToString() ?? "string.Empty";
                                int roleid = reader["Role_Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Role_Id"]);
                                UserSession.Role_Id = roleid;

                                // 7. تحميل الصلاحيات "Async" لضمان عدم انتقال الشاشة إلا بعد اكتمال التحميل
                                await UserSession.LoadPermissionsAsync(roleid);

                                Log_Login_info_Async(username: enteredusername, password: enteredpassword, full_name: UserSession.Full_Name, note: "Nothing", login_status: "Success", action_type: "login");

                                // 8. تحديث آخر وقت دخول (Update) في عملية منفصلة وسريعة
                                await UpdateLastLoginAsync(UserSession.UserId);

                                this.Hide();
                                Mainform mainform = new Mainform();
                                mainform.Show();
                                MessageBox.Show("تم تسجيل الدخول بنجاح ✅", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                Log_Login_info_Async(username: enteredusername, password: enteredpassword, full_name: "Nothing", note: "Nothing", login_status: "failed", action_type: "login", fail_reason: "اسم المستخدم أو كلمة المرور غير صحيحة");
                                MessageBox.Show("❌ اسم المستخدم أو كلمة المرور غير صحيحة");
                                txt_username.Clear();
                                txt_password.Clear();
                                txt_username.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الاتصال بالسيرفر: " + ex.Message);
            }
        }

        private async Task UpdateLastLoginAsync(int userId)
        {
            using (SqlConnection conn = DB_Server.GetConnection())
            {
                string query = "UPDATE Users SET Last_Login = GETDATE() WHERE ID = @ID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", userId);
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
        private void Login_Load(object sender, EventArgs e)
        {
            //DB_Server.Connect();
            txt_username.Focus();
            try
            {
                var license = HardwareIdGenerator.LoadLicense();

                if (license != null)
                {
                    if (Main_Methods.IsInternetAvailable())
                    {
                        // فيه نت → نحدّث من السيرفر
                        LicenseModel.RefreshLicenseFromServer(license);
                    }

                    // فحص Offline
                    HardwareIdGenerator.ValidateOfflineUsage(license);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "License Error");
                Application.Exit();
            }
        }

        private void txt_username_KeyDown(object sender, KeyEventArgs e)
        {

            if (txt_username.Text == "" )
            {

            }
            else
            {
                if (txt_password.Text == "")
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        txt_password.Focus();
                        e.SuppressKeyPress = true;
                    }
                }
                else
                {
                    DoLogin();
                }
            }

      
        }

        private void txt_password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) 
            {
                DoLogin();
                e.SuppressKeyPress = true;
            }
        }
    
 
            private async Task Log_Login_info_Async(string username, string password, string full_name, string note, string login_status, string action_type, string fail_reason = "nothing")
        {
            string insert_query = @"
    INSERT INTO Login_Log 
    (Code, full_name, username, password, mac_address, devicename, Is_Onlne, Note, IP_Address, ExternalIp, HWID, login_status, fail_reason, action_type)
    SELECT 
        ISNULL(MAX(Code), 0) + 1, 
        @full_name, @username, @password, @mac_address, @devicename, @Is_Onlne, @Note, @IP_Address, @ExternalIp, @HWID, @login_status, @fail_reason, @action_type 
    FROM Login_Log";

            try
            {
                // تنفيذ العمليات التي قد تستغرق وقتاً في Task منفصل لمنع تهنيج الواجهة
                string device_name = Environment.MachineName;
                string localIP = "0.0.0.0";

                // الحصول على الـ IP المحلي بدون تعطيل الشاشة
                await Task.Run(() => {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            localIP = ip.ToString();
                            break;
                        }
                    }
                });

                // فتح اتصال جديد واستخدام Async
                using (SqlConnection conn = DB_Server.GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(insert_query, conn))
                    {
                        // إضافة الباراميترز بشكل صحيح
                        cmd.Parameters.AddWithValue("@full_name", full_name ?? "Unknown");
                        cmd.Parameters.AddWithValue("@username", username ?? "Unknown");
                        cmd.Parameters.AddWithValue("@password", password ?? "Unknown");
                        cmd.Parameters.AddWithValue("@mac_address", Main_Methods.GetMacAddress() ?? "N/A");
                        cmd.Parameters.AddWithValue("@devicename", device_name);
                        cmd.Parameters.AddWithValue("@Is_Onlne", Main_Methods.IsInternetAvailable());
                        cmd.Parameters.AddWithValue("@Note", note ?? "");
                        cmd.Parameters.AddWithValue("@IP_Address", localIP);
                        cmd.Parameters.AddWithValue("@ExternalIp", Main_Methods.GetExternalIp() ?? "N/A");
                        cmd.Parameters.AddWithValue("@HWID", UserSession.hwid ?? "N/A");
                        cmd.Parameters.AddWithValue("@login_status", login_status);
                        cmd.Parameters.AddWithValue("@fail_reason", fail_reason);
                        cmd.Parameters.AddWithValue("@action_type", action_type);

                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync(); // تنفيذ الإدخال بدون تهنيج
                    }
                }
            }
            catch (Exception ex)
            {
                // في اللوج يفضل عدم إظهار رسائل خطأ للمستخدم لعدم إزعاجه، نكتفي بكتابتها في الـ Console
                Console.WriteLine("Log Error: " + ex.Message);
            }
        }
    
    }
}