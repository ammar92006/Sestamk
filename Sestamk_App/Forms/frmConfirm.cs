using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmConfirm : Form
    {
        private System.Windows.Forms.Timer _fadeTimer;
        private bool _isClosing = false;
        private double _targetOpacity = 0.95;

        public frmConfirm(string title, string message)
        {
            InitializeComponent();

            lblTitle.Text = title;
            lblMessage.Text = message;

            // ── Events ──
            btnConfirm.Click += (s, e) => { this.DialogResult = DialogResult.Yes; _isClosing = true; };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.No; _isClosing = true; };
            pnlOverlay.Click += (s, e) => { this.DialogResult = DialogResult.No; _isClosing = true; };

            // ── Keyboard shortcuts ──
            this.KeyPreview = true;
            this.KeyDown += FrmConfirm_KeyDown;

            // ── Fade in animation ──
            this.Opacity = 0;
            _fadeTimer = new System.Windows.Forms.Timer();
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += FadeTimer_Tick;

            // ── Overlay styling ──
            this.Load += FrmConfirm_Load;
            this.Resize += FrmConfirm_Resize;
        }

        private void FrmConfirm_Load(object sender, EventArgs e)
        {
            CenterCard();
            _fadeTimer.Start();
        }

        private void FrmConfirm_Resize(object sender, EventArgs e)
        {
            CenterCard();
        }

        private void CenterCard()
        {
            pnlCard.Left = (this.ClientSize.Width - pnlCard.Width) / 2;
            pnlCard.Top = (this.ClientSize.Height - pnlCard.Height) / 2;
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (!_isClosing)
            {
                // Fade In
                if (this.Opacity < _targetOpacity)
                {
                    this.Opacity += 0.08;
                    if (this.Opacity >= _targetOpacity)
                    {
                        this.Opacity = _targetOpacity;
                        _fadeTimer.Stop();
                    }
                }
            }
            else
            {
                // Fade Out
                this.Opacity -= 0.1;
                if (this.Opacity <= 0)
                {
                    _fadeTimer.Stop();
                    this.Close();
                }
            }
        }

        private void FrmConfirm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.No;
                _isClosing = true;
                _fadeTimer.Start();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.Yes;
                _isClosing = true;
                _fadeTimer.Start();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Static helper — استدعاء سهل من أي مكان في المشروع.
        /// يرجع true لو المستخدم أكد، false لو ألغى.
        /// </summary>
        public static bool Show(string title, string message)
        {
            using (var frm = new frmConfirm(title, message))
            {
                // ── عرض الفورم CenterScreen مع overlay ──
                Form owner = null;
                if (Application.OpenForms.Count > 0)
                    owner = Application.OpenForms[0];

                if (owner != null)
                {
                    // Full screen overlay
                    Screen screen = Screen.FromControl(owner);
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Bounds = screen.WorkingArea;
                }

                return frm.ShowDialog(owner) == DialogResult.Yes;
            }
        }

        /// <summary>
        /// Static overload — يقبل owner form مباشرة
        /// </summary>
        public static bool Show(IWin32Window owner, string title, string message)
        {
            using (var frm = new frmConfirm(title, message))
            {
                if (owner is Form ownerForm)
                {
                    Screen screen = Screen.FromControl(ownerForm);
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Bounds = screen.WorkingArea;
                }

                return frm.ShowDialog(owner) == DialogResult.Yes;
            }
        }
    }
}
