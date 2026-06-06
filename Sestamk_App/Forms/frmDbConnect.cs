using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using Sestamk.Classes;

namespace Sestamk.Forms
{
    /// <summary>
    /// Standalone DB connection dialog used at startup when SecureConfig points
    /// to an unreachable SQL Server. Writes directly to SecureConfig (bypassing
    /// SettingsService, which itself depends on a working DB connection).
    /// </summary>
    public sealed class frmDbConnect : Form
    {
        private readonly TextBox _txtServer;
        private readonly TextBox _txtUser;
        private readonly TextBox _txtPassword;
        private readonly Button  _btnConnect;
        private readonly Button  _btnCancel;
        private readonly Label   _lblError;
        private readonly Label   _lblStatus;

        public frmDbConnect(string? initialError = null)
        {
            // ── Form chrome ─────────────────────────────────────────────────
            Text            = "إعدادات قاعدة البيانات";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterScreen;
            MaximizeBox     = false;
            MinimizeBox     = false;
            ShowInTaskbar   = true;
            Size            = new Size(500, 420);
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 10f);
            RightToLeft     = RightToLeft.Yes;
            RightToLeftLayout = true;

            // ── Header banner ──────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 122, 204)
            };
            header.Controls.Add(new Label
            {
                Text = "تعذّر الاتصال بقاعدة البيانات",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            });

            // ── Error banner (optional) ────────────────────────────────────
            _lblError = new Label
            {
                Dock = DockStyle.Top,
                Height = 0,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Color.FromArgb(255, 235, 235),
                ForeColor = Color.FromArgb(170, 30, 30),
                Font = new Font("Segoe UI", 9f),
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Visible = false
            };

            // ── Fields panel ───────────────────────────────────────────────
            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(16, 12, 16, 12),
                ColumnCount = 2,
                RowCount = 4
            };
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            _txtServer   = MakeTextBox(SecureConfig.DbDataSource);
            _txtUser     = MakeTextBox(SecureConfig.DbUserId);
            _txtPassword = MakeTextBox(SecureConfig.DbPassword);
            _txtPassword.UseSystemPasswordChar = true;

            fields.Controls.Add(MakeLabel("اسم الخادم (Server):"), 0, 0);
            fields.Controls.Add(_txtServer, 1, 0);
            fields.Controls.Add(MakeLabel("اسم المستخدم (اختياري):"), 0, 1);
            fields.Controls.Add(_txtUser, 1, 1);
            fields.Controls.Add(MakeLabel("كلمة المرور (اختياري):"), 0, 2);
            fields.Controls.Add(_txtPassword, 1, 2);

            var hint = new Label
            {
                Text = "إذا كانت قاعدة البيانات على نفس الجهاز، استخدم .\\SQLEXPRESS",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            fields.Controls.Add(new Label(), 0, 3); // spacer
            fields.Controls.Add(hint, 1, 3);

            // ── Status (busy / inline feedback) ────────────────────────────
            _lblStatus = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Padding = new Padding(16, 4, 16, 4),
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(80, 80, 80),
                TextAlign = ContentAlignment.MiddleRight
            };

            // ── Buttons ────────────────────────────────────────────────────
            var buttons = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(16, 10, 16, 10)
            };

            _btnConnect = new Button
            {
                Text = "اتصال وحفظ",
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnConnect.FlatAppearance.BorderSize = 0;
            _btnConnect.Click += async (s, e) => await OnConnectClickedAsync();

            _btnCancel = new Button
            {
                Text = "إلغاء وإغلاق التطبيق",
                Size = new Size(170, 40),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnCancel.FlatAppearance.BorderColor = Color.Silver;
            _btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // Right-align inside RTL container — first added control sits on the right
            buttons.Controls.Add(_btnConnect);
            buttons.Controls.Add(_btnCancel);
            _btnConnect.Location = new Point(buttons.Padding.Left, buttons.Padding.Top);
            _btnCancel.Location  = new Point(buttons.Padding.Left + _btnConnect.Width + 10, buttons.Padding.Top);

            // ── Compose ────────────────────────────────────────────────────
            Controls.Add(buttons);
            Controls.Add(_lblStatus);
            Controls.Add(fields);
            Controls.Add(_lblError);
            Controls.Add(header);

            AcceptButton = _btnConnect;
            CancelButton = _btnCancel;

            if (!string.IsNullOrEmpty(initialError))
                ShowError(initialError!);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────────────────────────────
        private static Label MakeLabel(string text) => new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9.5f)
        };

        private static TextBox MakeTextBox(string initial) => new TextBox
        {
            Text = initial ?? "",
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10.5f),
            Margin = new Padding(4)
        };

        private void ShowError(string message)
        {
            _lblError.Text = message;
            _lblError.Height = TextRenderer.MeasureText(message,
                _lblError.Font, new Size(_lblError.ClientSize.Width - 24, int.MaxValue),
                TextFormatFlags.WordBreak).Height + 20;
            _lblError.Visible = true;
        }

        private void SetBusy(bool busy, string status = "")
        {
            _btnConnect.Enabled = !busy;
            _btnCancel.Enabled = !busy;
            _txtServer.Enabled = !busy;
            _txtUser.Enabled = !busy;
            _txtPassword.Enabled = !busy;
            _lblStatus.Text = status;
            _lblStatus.ForeColor = busy ? Color.FromArgb(0, 122, 204) : Color.FromArgb(80, 80, 80);
            UseWaitCursor = busy;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Connect button
        // ─────────────────────────────────────────────────────────────────────
        private async Task OnConnectClickedAsync()
        {
            string server   = _txtServer.Text.Trim();
            string user     = _txtUser.Text.Trim();
            string password = _txtPassword.Text;

            if (string.IsNullOrWhiteSpace(server))
            {
                ShowError("من فضلك أدخل اسم الخادم.");
                return;
            }

            _lblError.Visible = false;
            SetBusy(true, "جاري اختبار الاتصال...");

            var (ok, error) = await TryConnectAsync(server);
            if (!ok)
            {
                SetBusy(false);
                ShowError("فشل الاتصال:\n" + error);
                return;
            }

            // Success → persist and signal completion
            SetBusy(true, "تم الاتصال بنجاح. جاري الحفظ...");
            try
            {
                SecureConfig.UpdateDbCredentials(server, user, password);
            }
            catch (Exception ex)
            {
                SetBusy(false);
                ShowError("تم الاتصال لكن فشل حفظ الإعدادات:\n" + ex.Message);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private static async Task<(bool ok, string? error)> TryConnectAsync(string dataSource)
        {
            const int timeoutSeconds = 10;
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = dataSource,
                InitialCatalog = "master",
                IntegratedSecurity = true,
                Encrypt = false,
                TrustServerCertificate = true,
                ConnectTimeout = timeoutSeconds
            };

            var openTask = Task.Run(() =>
            {
                try
                {
                    using var conn = new SqlConnection(csb.ConnectionString);
                    conn.Open();
                    return (true, (string?)null);
                }
                catch (Exception ex) { return (false, (string?)ex.Message); }
            });

            var finished = await Task.WhenAny(openTask, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds + 2)));
            if (finished != openTask)
                return (false, $"انتهت مهلة الاتصال بعد {timeoutSeconds + 2} ثانية.");

            return openTask.Result;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Static helper used by StartupOrchestrator
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Shows the connection dialog. Returns true if the user successfully
        /// configured and tested a connection (settings already saved); false if
        /// the user cancelled (caller should treat as fatal and exit).
        /// </summary>
        public static bool ShowAndConnect(IWin32Window? owner, string? initialError)
        {
            using var frm = new frmDbConnect(initialError);
            return frm.ShowDialog(owner) == DialogResult.OK;
        }
    }
}
