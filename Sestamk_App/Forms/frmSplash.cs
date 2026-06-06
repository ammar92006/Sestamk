using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sestamk.Classes;

namespace Sestamk.Forms
{
    /// <summary>
    /// Startup splash. Owns the orchestrator and surfaces step-by-step status
    /// to the user. Closes itself when startup finishes and exposes the
    /// requested <see cref="NextAction"/> for Program.cs to act on.
    /// </summary>
    public sealed class frmSplash : Form
    {
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int we, int he);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr h);

        private readonly Label _lblStatus;
        private readonly ProgressBar _progress;

        public StartupAction NextAction { get; private set; } = StartupAction.Exit;

        public frmSplash()
        {
            // ── Window chrome ──────────────────────────────────────────────
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;
            Size            = new Size(520, 320);
            BackColor       = Color.FromArgb(25, 25, 28);
            ForeColor       = Color.White;
            ShowInTaskbar   = true;
            DoubleBuffered  = true;
            Font            = new Font("Segoe UI", 9.5f);
            RightToLeft     = RightToLeft.Yes;
            RightToLeftLayout = true;

            // ── Logo ───────────────────────────────────────────────────────
            var logo = new PictureBox
            {
                Image    = Properties.Resources.Logo,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size     = new Size(120, 120),
                Location = new Point((Width - 120) / 2, 30),
                BackColor = Color.Transparent
            };

            // ── Title ──────────────────────────────────────────────────────
            var title = new Label
            {
                Text      = "Sestamk POS",
                Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = false,
                Size      = new Size(Width, 30),
                Location  = new Point(0, 160),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // ── Status label ───────────────────────────────────────────────
            _lblStatus = new Label
            {
                Text      = "جاري بدء التشغيل...",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(200, 200, 205),
                AutoSize  = false,
                Size      = new Size(Width - 40, 22),
                Location  = new Point(20, 210),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // ── Progress bar (marquee) ─────────────────────────────────────
            _progress = new ProgressBar
            {
                Style    = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Size     = new Size(Width - 80, 6),
                Location = new Point(40, 245)
            };

            // ── Version ────────────────────────────────────────────────────
            var version = new Label
            {
                Text      = $"الإصدار {GetVersionString()}",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(120, 120, 125),
                AutoSize  = false,
                Size      = new Size(Width - 40, 18),
                Location  = new Point(20, 280),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Controls.AddRange(new Control[] { logo, title, _lblStatus, _progress, version });

            // ── Rounded corners ────────────────────────────────────────────
            Load += (_, __) =>
            {
                var hRgn = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, 12, 12);
                Region = Region.FromHrgn(hRgn);
                DeleteObject(hRgn);
            };

            // ── Subtle border ──────────────────────────────────────────────
            Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(60, 60, 65), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };

            Shown += async (_, __) => await RunStartupAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Public API for orchestrator
        // ─────────────────────────────────────────────────────────────────────
        public void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => _lblStatus.Text = text));
                return;
            }
            _lblStatus.Text = text;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Orchestration
        // ─────────────────────────────────────────────────────────────────────
        private async Task RunStartupAsync()
        {
            try
            {
                var orchestrator = new StartupOrchestrator(SetStatus, this);
                NextAction = await orchestrator.RunAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"خطأ غير متوقع أثناء بدء التشغيل:\n{ex.Message}",
                    "Sestamk",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                NextAction = StartupAction.Exit;
            }
            finally
            {
                Close();
            }
        }

        private static string GetVersionString()
        {
            try
            {
                var v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return v == null ? "1.0" : $"{v.Major}.{v.Minor}.{v.Build}";
            }
            catch { return "1.0"; }
        }
    }
}
