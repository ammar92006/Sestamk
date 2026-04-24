using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using Sestamk.Classes;

namespace Sestamk.Forms
{
    public partial class BaseForm : Form
    {
        protected Guna2Button btn_theme_toggle_base;

        public BaseForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
            // InitializeThemeToggle(); // تعطيل مؤقت
        }

        private void InitializeThemeToggle()
        {
            // Do not create UI elements in design mode to prevent designer crashing
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase)) return;

            btn_theme_toggle_base = new Guna2Button();
            btn_theme_toggle_base.Name = "btn_theme_toggle_base";
            btn_theme_toggle_base.Size = new Size(40, 40);
            
            // Allow button to float top-left. 
            // In Arabic UI RTL, Top-Left is often opposite to the standard Top-Right Close buttons.
            btn_theme_toggle_base.Location = new Point(10, 10);
            
            btn_theme_toggle_base.Font = new Font("Segoe UI", 16F);
            btn_theme_toggle_base.BackColor = Color.Transparent;
            btn_theme_toggle_base.FillColor = Color.Transparent;
            btn_theme_toggle_base.ForeColor = Color.Gray;
            btn_theme_toggle_base.BorderRadius = 15;
            btn_theme_toggle_base.Cursor = Cursors.Hand;
            btn_theme_toggle_base.Click += Btn_theme_toggle_Click;

            this.Controls.Add(btn_theme_toggle_base);
            btn_theme_toggle_base.BringToFront();
        }

        private void Btn_theme_toggle_Click(object sender, EventArgs e)
        {
            ThemeManager.ToggleTheme();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase)) return;

            if (btn_theme_toggle_base != null)
            {
                // Ensure it stays in front of any newly added panels or custom headers
                btn_theme_toggle_base.BringToFront();
            }

            // ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            // UpdateThemeToggleUI();
            // ThemeManager.ApplyTheme(this);
            
            if (btn_theme_toggle_base != null)
            {
                btn_theme_toggle_base.BringToFront();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime && !System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase))
            {
                // ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.OnFormClosed(e);
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            UpdateThemeToggleUI();
            ThemeManager.ApplyTheme(this);
            
            if (btn_theme_toggle_base != null)
            {
                btn_theme_toggle_base.BringToFront();
            }
            
            this.Invalidate(true);
        }

        private void UpdateThemeToggleUI()
        {
            if (btn_theme_toggle_base != null)
            {
                btn_theme_toggle_base.Text = ThemeManager.CurrentTheme == AppTheme.Light ? "🌙" : "☀️";
            }
        }
    }
}
