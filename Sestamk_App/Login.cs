using Microsoft.Data.SqlClient;
using Sestamk.Classes;
using Sestamk.Forms;
using System.Net;
using System.Runtime.InteropServices;

namespace Sestamk
{
    public partial class Login : BaseForm
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        private const int MaxFailAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private const string GenericAuthFailureMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";

        private static string _cachedLocalIp;
        private bool _loginInProgress;

        public Login()
        {
            InitializeComponent();
            Main_Methods.Attach(pn_0, this);
            Main_Methods.Attach(pn_1, this);
            Main_Methods.Attach(lbl_login_subtitle, this);
            Main_Methods.Attach(lbl_login_title, this);
            Main_Methods.Attach(lblVersion, this);
            lblVersion.Text = $"Version {LicenseManager.Version}";
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void chk_show_password_CheckedChanged(object sender, EventArgs e)
        {
            txt_password.UseSystemPasswordChar = !chk_show_password.Checked;
            txt_password.PasswordChar = chk_show_password.Checked ? '\0' : '●';
        }

        private void pn_1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        private async void btn_login_Click(object sender, EventArgs e)
        {
            try
            {
                await DoLogin();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login click error: " + ex);
                ToastManager.ShowError("خطأ", "حدث خطأ غير متوقع. حاول مرة أخرى.");
            }
        }

        private async Task DoLogin()
        {
            if (_loginInProgress) return;

            if (string.IsNullOrEmpty(txt_username.Text))
            {
                ToastManager.ShowWarning("تنبيه", "برجاء ادخال اسم المستخدم");
                return;
            }
            if (string.IsNullOrEmpty(txt_password.Text))
            {
                ToastManager.ShowWarning("تنبيه", "برجاء ادخال كلمه السر");
                return;
            }

            _loginInProgress = true;
            btn_login.Enabled = false;

            try
            {
                string enteredUsername = txt_username.Text.Trim();
                // Do NOT trim password — preserves intentional leading/trailing whitespace.
                string enteredPassword = txt_password.Text;

                const string query = @"
                    SELECT TOP 1 ID, Username, Password, Full_Name, Email, User_Image, Role_Id,
                                 Is_blocked, block_reason, FailCount, LockoutUntil
                    FROM Users
                    WHERE Username = @user OR Email = @user";

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", enteredUsername);
                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            // Unknown user — generic failure (no enumeration).
                            await Log_Login_info_Async(
                                username: enteredUsername, full_name: "Unknown", note: "Nothing",
                                login_status: "failed", action_type: "login",
                                fail_reason: "اسم المستخدم غير موجود");
                            ToastManager.ShowError("خطأ", GenericAuthFailureMessage);
                            txt_password.Clear();
                            txt_password.Focus();
                            return;
                        }

                        int userId = Convert.ToInt32(reader["ID"]);
                        string canonicalUsername = reader["Username"]?.ToString() ?? enteredUsername;
                        string storedPassword = reader["Password"]?.ToString() ?? "";
                        bool isBlocked = reader["Is_blocked"] != DBNull.Value && Convert.ToBoolean(reader["Is_blocked"]);
                        string blockReason = reader["block_reason"]?.ToString() ?? "";
                        int failCount = reader["FailCount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["FailCount"]);
                        DateTime? lockoutUntil = reader["LockoutUntil"] == DBNull.Value
                            ? (DateTime?)null
                            : Convert.ToDateTime(reader["LockoutUntil"]);
                        string fullName = reader["Full_Name"]?.ToString() ?? string.Empty;
                        string email = reader["Email"]?.ToString() ?? string.Empty;
                        string userImage = reader["User_Image"]?.ToString() ?? string.Empty;
                        int roleId = reader["Role_Id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Role_Id"]);

                        reader.Close();

                        // Per-user lockout check
                        if (lockoutUntil.HasValue && DateTime.UtcNow < lockoutUntil.Value)
                        {
                            await Log_Login_info_Async(
                                username: canonicalUsername, full_name: fullName, note: "Nothing",
                                login_status: "failed", action_type: "login",
                                fail_reason: "الحساب مقفل مؤقتاً");
                            // Generic message — don't reveal that the username exists.
                            ToastManager.ShowError("خطأ", GenericAuthFailureMessage);
                            txt_password.Clear();
                            txt_password.Focus();
                            return;
                        }

                        bool passwordMatch = PasswordHelper.VerifyPassword(enteredPassword, storedPassword);

                        if (!passwordMatch)
                        {
                            await RecordFailureAsync(userId, failCount + 1);
                            await Log_Login_info_Async(
                                username: canonicalUsername, full_name: fullName, note: "Nothing",
                                login_status: "failed", action_type: "login",
                                fail_reason: "كلمة المرور غير صحيحة");
                            ToastManager.ShowError("خطأ", GenericAuthFailureMessage);
                            txt_password.Clear();
                            txt_password.Focus();
                            return;
                        }

                        if (isBlocked)
                        {
                            await Log_Login_info_Async(
                                username: canonicalUsername, full_name: fullName, note: "Nothing",
                                login_status: "failed", action_type: "login",
                                fail_reason: "حساب محظور: " + blockReason);
                            // Generic message — block status is internal, not user-facing.
                            ToastManager.ShowError("خطأ", GenericAuthFailureMessage);
                            txt_password.Clear();
                            txt_username.Clear();
                            txt_username.Focus();
                            return;
                        }

                        if (PasswordHelper.NeedsMigration(storedPassword))
                        {
                            string newHash = PasswordHelper.HashPassword(enteredPassword);
                            _ = DB_Server.ExecuteAsync(
                                "UPDATE Users SET Password = @pwd WHERE ID = @id",
                                new[] { new SqlParameter("@pwd", newHash), new SqlParameter("@id", userId) });
                        }

                        await ResetFailuresAsync(userId);

                        UserSession.UserId = userId;
                        UserSession.UserName = canonicalUsername;
                        UserSession.Full_Name = fullName;
                        UserSession.Email = email;
                        UserSession.UserImage = userImage;
                        UserSession.Role_Id = roleId;

                        await UserSession.LoadPermissionsAsync(roleId);
                        await UserSession.LoadRoleNameAsync(roleId);

                        await Log_Login_info_Async(
                            username: canonicalUsername, full_name: fullName, note: "Nothing",
                            login_status: "Success", action_type: "login");

                        await UpdateLastLoginAsync(userId);

                        Mainform mainform = new Mainform();
                        mainform.FormClosed += (s, args) => this.Close();
                        this.Hide();
                        mainform.Show();
                        ToastManager.ShowSuccess("نجاح", "تم تسجيل الدخول بنجاح ✅");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login Error: " + ex);
                ToastManager.ShowError("خطأ", "تعذّر الاتصال بالسيرفر. تأكد من الإعدادات وحاول مجدداً.");
            }
            finally
            {
                _loginInProgress = false;
                btn_login.Enabled = true;
            }
        }

        private static async Task RecordFailureAsync(int userId, int newFailCount)
        {
            if (newFailCount >= MaxFailAttempts)
            {
                DateTime until = DateTime.UtcNow.Add(LockoutDuration);
                await DB_Server.ExecuteAsync(
                    "UPDATE Users SET FailCount = @c, LockoutUntil = @u WHERE ID = @id",
                    new[]
                    {
                        new SqlParameter("@c", newFailCount),
                        new SqlParameter("@u", until),
                        new SqlParameter("@id", userId)
                    });
            }
            else
            {
                await DB_Server.ExecuteAsync(
                    "UPDATE Users SET FailCount = @c WHERE ID = @id",
                    new[]
                    {
                        new SqlParameter("@c", newFailCount),
                        new SqlParameter("@id", userId)
                    });
            }
        }

        private static async Task ResetFailuresAsync(int userId)
        {
            await DB_Server.ExecuteAsync(
                "UPDATE Users SET FailCount = 0, LockoutUntil = NULL WHERE ID = @id",
                new[] { new SqlParameter("@id", userId) });
        }

        private async Task UpdateLastLoginAsync(int userId)
        {
            using (SqlConnection conn = DB_Server.GetConnection())
            using (SqlCommand cmd = new SqlCommand("UPDATE Users SET Last_Login = GETDATE() WHERE ID = @ID", conn))
            {
                cmd.Parameters.AddWithValue("@ID", userId);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async void Login_Load(object sender, EventArgs e)
        {
            try
            {
                await BackupManager.InitializeDatabaseAsync();
                ThemeManager.LoadTheme();
                txt_username.Focus();
                await SettingsService.LoadAllAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login_Load error: " + ex);
                ToastManager.ShowError("خطأ", "تعذّر تهيئة التطبيق. تحقق من اتصال قاعدة البيانات.");
            }
        }

        private void txt_username_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                txt_password.Focus();
                e.SuppressKeyPress = true;
            }
        }

        private async void txt_password_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                try
                {
                    await DoLogin();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Login keydown error: " + ex);
                    ToastManager.ShowError("خطأ", "حدث خطأ غير متوقع. حاول مرة أخرى.");
                }
            }
        }

        private static string GetLocalIp()
        {
            if (_cachedLocalIp != null) return _cachedLocalIp;
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        _cachedLocalIp = ip.ToString();
                        return _cachedLocalIp;
                    }
                }
            }
            catch
            {
                // fall through
            }
            _cachedLocalIp = "0.0.0.0";
            return _cachedLocalIp;
        }

        private async Task Log_Login_info_Async(string username, string full_name, string note,
                                                string login_status, string action_type,
                                                string fail_reason = "nothing")
        {
            // Code race fix: TABLOCKX + HOLDLOCK serializes concurrent inserts so MAX+1 stays unique.
            const string insert_query = @"
                INSERT INTO Login_Log
                    (Code, full_name, username, mac_address, devicename, Is_Onlne, Note, IP_Address, ExternalIp, HWID, login_status, fail_reason, action_type)
                SELECT
                    ISNULL(MAX(Code), 0) + 1,
                    @full_name, @username, @mac_address, @devicename, @Is_Onlne, @Note, @IP_Address, @ExternalIp, @HWID, @login_status, @fail_reason, @action_type
                FROM Login_Log WITH (TABLOCKX, HOLDLOCK)";

            try
            {
                string deviceName = Environment.MachineName;
                string localIp = await Task.Run(() => GetLocalIp());
                string externalIp = await Task.Run(() => Main_Methods.GetExternalIp());
                bool isOnline = await Task.Run(() => Main_Methods.IsInternetAvailable());
                string mac = Main_Methods.GetMacAddress() ?? "N/A";

                using (SqlConnection conn = DB_Server.GetConnection())
                using (SqlCommand cmd = new SqlCommand(insert_query, conn))
                {
                    cmd.Parameters.AddWithValue("@full_name", full_name ?? "Unknown");
                    cmd.Parameters.AddWithValue("@username", username ?? "Unknown");
                    cmd.Parameters.AddWithValue("@mac_address", mac);
                    cmd.Parameters.AddWithValue("@devicename", deviceName);
                    cmd.Parameters.AddWithValue("@Is_Onlne", isOnline);
                    cmd.Parameters.AddWithValue("@Note", note ?? "");
                    cmd.Parameters.AddWithValue("@IP_Address", localIp);
                    cmd.Parameters.AddWithValue("@ExternalIp", externalIp ?? "N/A");
                    cmd.Parameters.AddWithValue("@HWID", UserSession.hwid ?? "N/A");
                    cmd.Parameters.AddWithValue("@login_status", login_status);
                    cmd.Parameters.AddWithValue("@fail_reason", fail_reason);
                    cmd.Parameters.AddWithValue("@action_type", action_type);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Log Error: " + ex.Message);
            }
        }

        private async Task CheckLicenseAsync()
        {
            string mySerial = "SSTM-PRO-2025-TEST-0001";

            try
            {
                var result = await LicenseManager.CheckLicenseAsync(mySerial);
                string status = result["status"]?.ToString();

                if (status == "active")
                {
                    LocalLicenseManager.SaveLicenseLocally(result);
                }
                else
                {
                    string licensePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sestamk", "license.dat");
                    if (File.Exists(licensePath)) File.Delete(licensePath);

                    MessageBox.Show("عذراً، البرنامج لن يعمل: " + result["message"]);
                    frmActivation formAct = new frmActivation();
                    formAct.Show();
                }
            }
            catch (HttpRequestException)
            {
                var localLicense = LocalLicenseManager.ReadLocalLicense();
                bool isAllowed = LocalLicenseManager.IsOfflinePlayAllowed(localLicense, out string msg);

                if (!isAllowed)
                {
                    MessageBox.Show(msg, "مطلوب إنترنت", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ غير متوقع: " + ex.Message);
            }
        }

        private void buttonTestLicense_Click(object sender, EventArgs e)
        {
        }

        private void Login_Shown(object sender, EventArgs e)
        {
        }
    }
}
