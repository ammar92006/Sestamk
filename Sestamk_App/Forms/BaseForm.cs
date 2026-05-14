using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Sestamk.Classes;

namespace Sestamk.Forms
{
    public partial class BaseForm : Form
    {
        protected Guna2Button btn_theme_toggle_base;
        private readonly ScaleHelper _scaleHelper = new ScaleHelper();

        /// <summary>
        /// Override in each form and return the ClientSize written in its Designer.cs.
        /// When Size.Empty the form will not auto-scale.
        /// </summary>
        protected virtual Size DesignClientSize => Size.Empty;

        public BaseForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);
            this.UpdateStyles();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btn_theme_toggle_base?.BringToFront();

            var ds = DesignClientSize;
            if (ds == Size.Empty) return;

            // Snapshot controls at their design-time positions
            _scaleHelper.Snapshot(new SizeF(ds.Width, ds.Height), this);

            // If the form is larger than the working area, shrink it to fit.
            // This handles small screens without forcing Maximized on every form.
            var screen = Screen.FromControl(this).WorkingArea;
            if (ds.Width > screen.Width || ds.Height > screen.Height)
            {
                int newW = Math.Min(ds.Width,  screen.Width);
                int newH = Math.Min(ds.Height, screen.Height);
                this.ClientSize = new Size(newW, newH);
                this.Location = new Point(
                    screen.Left + (screen.Width  - newW) / 2,
                    screen.Top  + (screen.Height - newH) / 2);
                // Scale controls to new size
                _scaleHelper.ScaleTo(new SizeF(newW, newH), this);
            }
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            var ds = DesignClientSize;
            if (ds != Size.Empty)
                _scaleHelper.ScaleTo(new SizeF(ClientSize.Width, ClientSize.Height), this);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }
    }
}
