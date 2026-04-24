using Sestamk.Classes;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sestamk.Forms
{
    public partial class frmToast : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ToastModel Model { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TargetY { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsClosing { get; set; }

        public frmToast(ToastModel model)
        {
            InitializeComponent();

            Model = model;

            Opacity = 0;
            DoubleBuffered = true;

            ApplyToastStyle(model);
        }

        /// <summary>
        /// تطبيق الشكل حسب نوع التوست
        /// </summary>
        private void ApplyToastStyle(ToastModel model)
        {
            lblTitle.Text = model.Title;
            lblMessage.Text = model.Message;
            lblIcon.Text = GetIcon(model.Type);

            Color accentColor = GetAccentColor(model.Type);
            pnlAccent.BackColor = accentColor;
            lblIcon.ForeColor = accentColor;
        }

        private string GetIcon(ToastType type)
        {
            return type switch
            {
                ToastType.Success => "✓",
                ToastType.Error => "✕",
                ToastType.Warning => "⚠",
                _ => "ℹ"
            };
        }

        private Color GetAccentColor(ToastType type)
        {
            return type switch
            {
                ToastType.Success => Color.FromArgb(34, 197, 94),    // أخضر
                ToastType.Error => Color.FromArgb(239, 68, 68),    // أحمر
                ToastType.Warning => Color.FromArgb(245, 158, 11),   // برتقالي
                _ => Color.FromArgb(59, 130, 246)    // أزرق
            };
        }

        public void StartClose()
        {
            IsClosing = true;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            StartClose();
        }
    }
}
