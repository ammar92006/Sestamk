using Sestamk.Classes;
using Sestamk.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sestamk.UserControl.UC_Settings
{
    // ═══════════════════════════════════════════════════════════════════
    //  WhatsAppSettings — Full logic layer for the WhatsApp settings panel
    //  Maps to the existing designer controls (guna2Button2, label5, etc.)
    //  Uses the production-grade WhatsAppService + WorkerService
    // ═══════════════════════════════════════════════════════════════════
    public partial class WhatsAppSettings : System.Windows.Forms.UserControl, ICloseRequest
    {
        public event EventHandler CloseRequested;

        // ── State ──────────────────────────────────────────────────────────
        private CancellationTokenSource? _qrPollCts;
        private System.Windows.Forms.Timer _statusTimer;
        private System.Windows.Forms.Timer? _countdownTimer;
        private int _countdownSeconds = 0;
        private WhatsAppStatus _lastStatus = WhatsAppStatus.Unknown;
        private string? _cachedPhoneNumber;

        // ── Color constants ────────────────────────────────────────────────
        private static readonly Color ColorConnected    = Color.FromArgb(34, 197, 94);   // Green
        private static readonly Color ColorQrReady      = Color.FromArgb(234, 179, 8);   // Amber
        private static readonly Color ColorDisconnected = Color.FromArgb(251, 44, 54);   // Red
        private static readonly Color ColorUnknown      = Color.FromArgb(156, 163, 175); // Gray

        public WhatsAppSettings()
        {
            InitializeComponent();
            SetupUI();
        }

        private Guna.UI2.WinForms.Guna2CustomRadioButton rbFormatImage;
        private Label lblFormatImage;

        // ── Setup UI after InitializeComponent ────────────────────────────
        private void SetupUI()
        {
            InjectImageRadioButton();
            
            // Wire button events
            BtnConnect.Click += BtnConnect_Click;     // ربط الحساب
            BtnSave.Click += BtnSave_Click;         // حفظ التغييرات
            BtnReset.Click += BtnReset_Click;        // إعادة ضبط
            btnClose.Click     += btnClose_Click;
            btnSendTest.Click += BtnSendTest_Click;   // إرسال تجربة

            // Status polling timer (every 2 s)
            _statusTimer          = new System.Windows.Forms.Timer();
            _statusTimer.Interval = 2000;
            _statusTimer.Tick    += async (s, e) => await RefreshStatusAsync();

            // Hook into worker status events for real-time badge update
            WhatsAppWorkerService.StatusChanged    += OnWorkerStatusChanged;
            WhatsAppWorkerService.QueueDepthChanged += OnQueueDepthChanged;

            // Load saved settings into controls
            LoadSettings();

            // Always re-save the secret to overwrite any corrupted AES value in the DB
            _ = WhatsAppService.SaveSecretAsync("40ddff3e42dce8ecae15405ba523f572e526c673e0b40051a8d67aee1bdab190");

            // Auto-start: شغّل سيرفر Node.js تلقائياً لو مش شغال
            _ = AutoStartBridgeAsync();

            // Start timer
            _statusTimer.Start();
        }

        /// <summary>
        /// يشغّل سيرفر الواتساب تلقائياً ويحدّث الحالة
        /// </summary>
        private async Task AutoStartBridgeAsync()
        {
            UpdateStatusBadge(WhatsAppStatus.Connecting);

            bool bridgeOk = await WhatsAppService.EnsureBridgeRunningAsync();
            if (bridgeOk)
            {
                await RefreshStatusAsync();
            }
            else
            {
                UpdateStatusBadge(WhatsAppStatus.ServiceDown);
            }
        }

        // ── Load Settings into Controls ────────────────────────────────────
        private void LoadSettings()
        {
            // Server URL (guna2TextBox1 = welcome template, guna2TextBox2 = invoice template per designer)
            // We re-purpose them: guna2TextBox1 = URL, but since designer has them for templates,
            // we keep the designer's intent and just bind the content.
            guna2TextBox1.Text = SettingsService.WhatsAppWelcomeTemplate;
            guna2TextBox2.Text = SettingsService.WhatsAppInvoiceTemplate;
            txtServerUrl.Text  = SettingsService.WhatsAppServerUrl;
            txtBridgeInfo.Text = SettingsService.WhatsAppServerUrl;

            // Auto-send toggle
            guna2ToggleSwitch1.Checked = SettingsService.WhatsAppAutoSendInvoice;

            // Send type: radio buttons
            string format = SettingsService.WhatsAppInvoiceFormat;
            guna2CustomRadioButton1.Checked = (format == "Text");
            rbFormatImage.Checked = (format == "Image");
            guna2CustomRadioButton2.Checked = (format == "PDF");
        }

        private void InjectImageRadioButton()
        {
            rbFormatImage = new Guna.UI2.WinForms.Guna2CustomRadioButton();
            lblFormatImage = new Label();
            
            rbFormatImage.CheckedState.BorderColor = Color.FromArgb(94, 148, 255);
            rbFormatImage.CheckedState.BorderThickness = 0;
            rbFormatImage.CheckedState.FillColor = Color.FromArgb(94, 148, 255);
            rbFormatImage.CheckedState.InnerColor = Color.White;
            rbFormatImage.CheckedState.InnerOffset = 3;
            rbFormatImage.Size = new Size(30, 30);
            rbFormatImage.UncheckedState.BorderColor = Color.FromArgb(125, 137, 149);
            rbFormatImage.UncheckedState.BorderThickness = 5;
            rbFormatImage.UncheckedState.FillColor = Color.Transparent;
            rbFormatImage.UncheckedState.InnerColor = Color.Transparent;
            rbFormatImage.Name = "rbFormatImage";
            rbFormatImage.Cursor = Cursors.Hand;

            lblFormatImage.AutoSize = true;
            lblFormatImage.Font = new Font("Alexandria", 16F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblFormatImage.ForeColor = Color.FromArgb(229, 231, 235);
            lblFormatImage.Size = new Size(62, 38);
            lblFormatImage.Text = "صورة";
            lblFormatImage.TextAlign = ContentAlignment.MiddleRight;

            guna2Panel4.Controls.Add(rbFormatImage);
            guna2Panel4.Controls.Add(lblFormatImage);

            // Re-arrange labels and buttons slightly
            // PDF
            label12.Location = new Point(40, 226);
            guna2CustomRadioButton2.Location = new Point(100, 230);

            // Image
            lblFormatImage.Location = new Point(160, 226);
            rbFormatImage.Location = new Point(230, 230);

            // Text
            label13.Location = new Point(290, 226);
            guna2CustomRadioButton1.Location = new Point(350, 230);
        }

        // ── Refresh Status Badge ────────────────────────────────────────────
        private async Task RefreshStatusAsync()
        {
            try
            {
                var (ok, rawStatus, me) = await WhatsAppService.HealthCheckAsync();
                WhatsAppStatus status = WhatsAppStatus.Unknown;

                if (!ok) status = WhatsAppStatus.ServiceDown;
                else status = rawStatus switch
                {
                    "CONNECTED"    => WhatsAppStatus.Connected,
                    "QR_READY"     => WhatsAppStatus.QrReady,
                    "INITIALIZING" => WhatsAppStatus.Connecting,
                    "DISCONNECTED" => WhatsAppStatus.Disconnected,
                    "AUTH_FAILURE" => WhatsAppStatus.AuthFailure,
                    _              => WhatsAppStatus.Unknown
                };

                UpdateStatusBadge(status, me);

                // Auto-display QR if needed
                if (status == WhatsAppStatus.QrReady)
                    await LoadQrCodeAsync();

                // Success Notification
                if (status == WhatsAppStatus.Connected && _lastStatus != WhatsAppStatus.Connected)
                {
                    //ToastManager.ShowSuccess("واتساب", "تم الربط بنجاح ✅");
                    WhatsAppLogger.Info("SettingsUI", "WhatsApp connected successfully", Guid.Empty);
                    guna2PictureBox3.Image = null; 
                }

                _lastStatus = status;
            }
            catch { /* ignore UI refresh errors */ }
        }

        // ── Update Status Badge (label5 = status text in designer) ─────────
        private void UpdateStatusBadge(WhatsAppStatus status, string? me = null)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateStatusBadge(status, me));
                return;
            }

            (string text, Color color) = status switch
            {
                WhatsAppStatus.Connected    => ("متصل ✅",           ColorConnected),
                WhatsAppStatus.QrReady      => ("في انتظار المسح ⏳", ColorQrReady),
                WhatsAppStatus.Connecting   => ("جارٍ الاتصال…",     ColorQrReady),
                WhatsAppStatus.Disconnected => ("غير متصل ❌",        ColorDisconnected),
                WhatsAppStatus.ServiceDown  => ("الخدمة متوقفة 🔴",  ColorDisconnected),
                WhatsAppStatus.AuthFailure  => ("خطأ في المصادقة",   ColorDisconnected),
                _                           => ("غير معروف",          ColorUnknown)
            };

            label5.Text      = text;
            label5.ForeColor = color;

            // Update account number label
            if (status == WhatsAppStatus.Connected)
            {
                if (!string.IsNullOrEmpty(me))
                    _cachedPhoneNumber = me;

                label16.Text = !string.IsNullOrEmpty(_cachedPhoneNumber) ? _cachedPhoneNumber : "جارٍ التحميل...";
                label16.Visible = true;
                label15.Visible = true;
            }
            else
            {
                _cachedPhoneNumber = null;
                label16.Text = "---";
            }

            // Update connect button based on status
            if (status == WhatsAppStatus.Connected)
            {
                BtnConnect.Enabled   = true;
                BtnConnect.Text      = "إلغاء الربط 🔓";
                BtnConnect.FillColor = Color.FromArgb(239, 68, 68); // Red for disconnect
            }
            else
            {
                BtnConnect.Enabled   = status != WhatsAppStatus.Connecting;
                BtnConnect.Text      = status == WhatsAppStatus.Connecting ? "جارٍ الاتصال…" : "ربط الحساب";
                BtnConnect.FillColor = Color.FromArgb(34, 197, 94); // Green for connect
            }
        }

        // ── Load QR Code into PictureBox ────────────────────────────────────
        private async Task LoadQrCodeAsync()
        {
            try
            {
                WhatsAppLogger.Info("SettingsUI", "LoadQrCodeAsync: Fetching QR code...", Guid.Empty);
                string? qrBase64 = await WhatsAppService.GetQrBase64Async();

                if (string.IsNullOrEmpty(qrBase64))
                {
                    WhatsAppLogger.Warn("SettingsUI", "LoadQrCodeAsync: QR base64 is null or empty", Guid.Empty);
                    return;
                }

                WhatsAppLogger.Info("SettingsUI", $"LoadQrCodeAsync: QR received, length={qrBase64.Length}", Guid.Empty);

                // Strip data URL prefix
                string base64 = qrBase64.Contains(',')
                    ? qrBase64.Split(',')[1]
                    : qrBase64;

                byte[] bytes = Convert.FromBase64String(base64);
                WhatsAppLogger.Info("SettingsUI", $"LoadQrCodeAsync: Decoded {bytes.Length} bytes", Guid.Empty);

                // Important: Keep MemoryStream alive during image lifetime
                var ms = new MemoryStream(bytes);
                var img = Image.FromStream(ms);

                WhatsAppLogger.Info("SettingsUI", "LoadQrCodeAsync: Image created, displaying...", Guid.Empty);

                if (InvokeRequired)
                {
                    Invoke(() => {
                        guna2PictureBox3.Image = img;
                        guna2PictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                        WhatsAppLogger.Info("SettingsUI", "LoadQrCodeAsync: QR displayed on UI thread", Guid.Empty);
                    });
                }
                else
                {
                    guna2PictureBox3.Image    = img;
                    guna2PictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                    WhatsAppLogger.Info("SettingsUI", "LoadQrCodeAsync: QR displayed on current thread", Guid.Empty);
                }
            }
            catch (Exception ex)
            {
                WhatsAppLogger.Error("SettingsUI", $"LoadQrCodeAsync exception: {ex.Message}\n{ex.StackTrace}", Guid.Empty, ex);
            }
        }

        // ── Button: ربط الحساب (Connect) ────────────────────────────────────
        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            if (BtnConnect.Text.Contains("إلغاء الربط"))
            {
                BtnReset_Click(sender, e);
                return;
            }

            // Sync URL first
            txtBridgeInfo.Text = txtServerUrl.Text;

            BtnConnect.Enabled = false;
            BtnConnect.Text    = "جارٍ الاتصال…";
            UpdateStatusBadge(WhatsAppStatus.Connecting);

            // تأكد إن السيرفر شغال أولاً
            bool bridgeOk = await WhatsAppService.EnsureBridgeRunningAsync();
            if (!bridgeOk)
            {
                ToastManager.ShowError("خطأ", "فشل تشغيل خدمة الواتساب. تأكد من تثبيت Node.js");
                BtnConnect.Enabled = true;
                BtnConnect.Text    = "ربط الحساب";
                UpdateStatusBadge(WhatsAppStatus.ServiceDown);
                return;
            }

            // أعد ضبط جلسة الواتساب لإنشاء QR جديد (السيرفر قد يكون في حالة DISCONNECTED من جلسة قديمة)
            WhatsAppLogger.Info("SettingsUI", "Resetting WhatsApp session to force new QR code", Guid.Empty);
            bool resetOk = await WhatsAppService.ResetSessionAsync();
            WhatsAppLogger.Info("SettingsUI", $"Session reset result: {resetOk}", Guid.Empty);

            // انتظر ثانيتين حتى يعيد السيرفر التشغيل ويصبح في حالة INITIALIZING
            await Task.Delay(2000);

            // بدء العداد - انتظر 3 دقائق أقصى
            StartCountdownTimer(180); // 180 ثانية = 3 دقائق

            // Poll for QR until connected or timeout (3 دقائق)
            _qrPollCts?.Cancel();
            _qrPollCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!_qrPollCts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(1500, _qrPollCts.Token);
                        var (ok, rawStatus, me) = await WhatsAppService.HealthCheckAsync(_qrPollCts.Token);

                        WhatsAppLogger.Info("SettingsUI", $"Poll: ok={ok}, status={rawStatus}", Guid.Empty);

                        if (!ok)
                        {
                            if (InvokeRequired) Invoke(() => UpdateStatusBadge(WhatsAppStatus.ServiceDown));
                            continue;
                        }

                        if (rawStatus == "QR_READY")
                        {
                            await LoadQrCodeAsync();
                            StopCountdownTimer(); // QR is ready - stop the waiting countdown
                            if (InvokeRequired) Invoke(() => UpdateStatusBadge(WhatsAppStatus.QrReady));
                        }
                        else if (rawStatus == "INITIALIZING")
                        {
                            if (InvokeRequired) Invoke(() => UpdateStatusBadge(WhatsAppStatus.Connecting));
                        }
                        else if (rawStatus == "CONNECTED")
                        {
                            StopCountdownTimer();

                            // Mark WhatsApp as enabled and start the worker if not already running
                            await SettingsService.SetAsync("WhatsApp_Enabled", "true");
                            WhatsAppWorkerService.Start();

                            if (InvokeRequired) Invoke(() => UpdateStatusBadge(WhatsAppStatus.Connected, me));
                            else UpdateStatusBadge(WhatsAppStatus.Connected, me);

                            _qrPollCts.Cancel();
                            break;
                        }
                        else if (rawStatus == "DISCONNECTED" || rawStatus == "AUTH_FAILURE")
                        {
                            // Server is stuck - this shouldn't happen after reset, log it
                            WhatsAppLogger.Warn("SettingsUI", $"Unexpected status after reset: {rawStatus}", Guid.Empty);
                            if (InvokeRequired) Invoke(() => UpdateStatusBadge(WhatsAppStatus.Disconnected));
                        }
                    }
                }
                catch (OperationCanceledException) { }
            });

            await RefreshStatusAsync();
        }

        // ── Button: حفظ التغييرات ────────────────────────────────────────────
        private async void BtnSave_Click(object sender, EventArgs e)
        {
            BtnSave.Enabled = false;
            BtnSave.Text    = "جارٍ الحفظ…";

            try
            {
                await SettingsService.SetAsync("WhatsApp_WelcomeTemplate", guna2TextBox1.Text.Trim());
                await SettingsService.SetAsync("WhatsApp_InvoiceTemplate",  guna2TextBox2.Text.Trim());
                await SettingsService.SetAsync("WhatsApp_ServerUrl",       txtServerUrl.Text.Trim());
                await SettingsService.SetAsync("WhatsApp_AutoSendInvoice",  guna2ToggleSwitch1.Checked.ToString().ToLower());
                await SettingsService.SetAsync("WhatsApp_Enabled",         "true");
                
                string format = "Text";
                if (rbFormatImage.Checked) format = "Image";
                if (guna2CustomRadioButton2.Checked) format = "PDF";
                await SettingsService.SetAsync("WhatsApp_InvoiceFormat", format);

                ToastManager.ShowSuccess("واتساب", "تم حفظ الإعدادات بنجاح ✅");
                WhatsAppLogger.Info("SettingsUI", "WhatsApp settings saved by user", Guid.Empty);
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", "فشل حفظ الإعدادات: " + ex.Message);
            }
            finally
            {
                BtnSave.Enabled = true;
                BtnSave.Text    = "حفظ التغييرات";
            }
        }

        // ── Button: إعادة ضبط (Reset Session) ─────────────────────────────
        private async void BtnReset_Click(object sender, EventArgs e)
        {
            bool confirmed = frmConfirm.Show("تأكيد إعادة الضبط", "هل تريد إعادة ضبط جلسة واتساب؟\nسيتطلب ذلك مسح QR مرة أخرى.");
            if (!confirmed) return;

            BtnReset.Enabled = false;
            bool ok = await WhatsAppService.ResetSessionAsync();

            if (ok)
            {
                guna2PictureBox3.Image = Properties.Resources.qr_code__4_;
                ToastManager.ShowInfo("واتساب", "تم إعادة الضبط. امسح QR من جديد.");
            }
            else
            {
                ToastManager.ShowError("خطأ", "فشل إعادة الضبط. تأكد من تشغيل خدمة واتساب.");
            }

            BtnReset.Enabled = true;
            await RefreshStatusAsync();
        }

        // ── Button: إرسال تجربة ────────────────────────────────────────────
        private async void BtnSendTest_Click(object sender, EventArgs e)
        {
            string phone = txtTestPhone.Text.Trim();
            if (string.IsNullOrEmpty(phone))
            {
                ToastManager.ShowError("تنبيه", "يرجى إدخال رقم الهاتف أولاً");
                return;
            }

            if (_lastStatus != WhatsAppStatus.Connected)
            {
                ToastManager.ShowError("خطأ", "يجب ربط الواتساب أولاً قبل إرسال تجربة");
                return;
            }

            btnSendTest.Enabled = false;
            btnSendTest.Text = "جارٍ الإرسال…";

            try
            {
                string countryCode = SettingsService.GetString("WhatsApp_DefaultCountryCode", "+20");
                var msg = new WhatsAppMessage
                {
                    Phone = WhatsAppHelper.FormatPhoneNumber(phone, countryCode),
                    Text = "هذه رسالة تجريبية من برنامج Sestamk للتأكد من ربط الواتساب بنجاح ✅",
                    CorrelationId = Guid.NewGuid()
                };

                var (success, error) = await WhatsAppService.SendTextAsync(msg);

                if (success)
                    ToastManager.ShowSuccess("واتساب", "تم إرسال الرسالة التجريبية بنجاح ✅");
                else
                    ToastManager.ShowError("فشل الإرسال", error ?? "خطأ غير معروف");
            }
            catch (Exception ex)
            {
                ToastManager.ShowError("خطأ", ex.Message);
            }
            finally
            {
                btnSendTest.Enabled = true;
                btnSendTest.Text = "إرسال تجربة";
            }
        }

        // ── Worker Events (thread-safe) ─────────────────────────────────────
        private void OnWorkerStatusChanged(WhatsAppStatus status)
        {
            if (IsHandleCreated && !IsDisposed)
                Invoke(() => UpdateStatusBadge(status));
        }

        private void OnQueueDepthChanged(int depth)
        {
            if (!IsHandleCreated || IsDisposed) return;
            Invoke(() =>
            {
                // Show queue depth in label7 (the placeholder label in designer)
                label7.Text      = $"الرسائل المعلقة: {depth}";
                label7.ForeColor = depth > 50
                    ? ColorDisconnected
                    : depth > 10
                        ? ColorQrReady
                        : ColorUnknown;
            });
        }

        // ── Close ──────────────────────────────────────────────────────────
        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        // ── Countdown Timer: Display remaining wait time ────────────────────
        private void StartCountdownTimer(int totalSeconds)
        {
            StopCountdownTimer(); // Clear any existing timer first

            _countdownSeconds = totalSeconds;

            if (_countdownTimer == null)
            {
                _countdownTimer = new System.Windows.Forms.Timer();
                _countdownTimer.Interval = 1000; // Update every second
                _countdownTimer.Tick += (s, e) =>
                {
                    _countdownSeconds--;

                    // Update UI with countdown: "انتظر... M:SS (خدمة الواتساب تبدأ)"
                    if (InvokeRequired)
                    {
                        Invoke(() =>
                        {
                            int minutes = _countdownSeconds / 60;
                            int seconds = _countdownSeconds % 60;
                            label5.Text = $"انتظر... {minutes}:{seconds:D2} (خدمة الواتساب تبدأ)";
                        });
                    }
                    else
                    {
                        int minutes = _countdownSeconds / 60;
                        int seconds = _countdownSeconds % 60;
                        label5.Text = $"انتظر... {minutes}:{seconds:D2} (خدمة الواتساب تبدأ)";
                    }

                    // Stop when countdown reaches 0
                    if (_countdownSeconds <= 0)
                    {
                        StopCountdownTimer();
                    }
                };
            }

            _countdownTimer.Start();
        }

        private void StopCountdownTimer()
        {
            if (_countdownTimer != null)
            {
                _countdownTimer.Stop();
                _countdownTimer.Dispose();
                _countdownTimer = null;
            }
            _countdownSeconds = 0;
        }

        // ── Cleanup on dispose (called by Designer's Dispose method) ───────
        private void DisposeWhatsApp()
        {
            _statusTimer?.Stop();
            _statusTimer?.Dispose();
            _qrPollCts?.Cancel();
            _qrPollCts?.Dispose();
            StopCountdownTimer();

            WhatsAppWorkerService.StatusChanged     -= OnWorkerStatusChanged;
            WhatsAppWorkerService.QueueDepthChanged -= OnQueueDepthChanged;
        }
    }
}
