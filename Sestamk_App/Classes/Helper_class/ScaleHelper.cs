using System.Drawing;
using System.Windows.Forms;

namespace Sestamk.Classes
{
    /// <summary>
    /// Proportional-resize engine for WinForms.
    ///
    /// Usage (handled automatically by BaseForm):
    ///   1. In OnLoad  → Snapshot(designSize, rootControl)
    ///   2. In SizeChanged → ScaleTo(newClientSize, rootControl)
    ///
    /// Per-panel scaling: ratios are re-computed at each container level
    /// so that Dock=Fill panels (whose size tracks the window) produce
    /// accurate ratios for their absolute-positioned children.
    /// </summary>
    public sealed class ScaleHelper
    {
        private record ControlState(float X, float Y, float W, float H, float FontSize);

        private readonly Dictionary<Control, ControlState> _snap = new();
        private SizeF _rootDesignSize;
        private bool _ready;

        /// <summary>
        /// Call once from BaseForm.OnLoad.
        /// designSize = the ClientSize value written in the form's Designer.
        /// </summary>
        public void Snapshot(SizeF designSize, Control root)
        {
            if (designSize.Width <= 0 || designSize.Height <= 0) return;
            _rootDesignSize = designSize;
            _snap.Clear();

            // Snapshot the root itself so per-parent ratios work below
            _snap[root] = new ControlState(0, 0, designSize.Width, designSize.Height, root.Font.Size);
            Capture(root);
            _ready = true;
        }

        private void Capture(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                _snap[c] = new ControlState(c.Left, c.Top, c.Width, c.Height, c.Font.Size);
                Capture(c);
            }
        }

        /// <summary>
        /// Call from BaseForm.OnClientSizeChanged.
        /// newSize = current ClientSize of the root form.
        /// </summary>
        public void ScaleTo(SizeF newSize, Control root)
        {
            if (!_ready || _rootDesignSize.Width == 0 || _rootDesignSize.Height == 0) return;

            root.SuspendLayout();
            try
            {
                // Root-level scale ratios
                float rx = newSize.Width  / _rootDesignSize.Width;
                float ry = newSize.Height / _rootDesignSize.Height;
                Apply(root, rx, ry);
            }
            finally
            {
                root.ResumeLayout(true);
            }
        }

        private void Apply(Control parent, float parentRx, float parentRy)
        {
            // For each child, figure out what ratio to use.
            // If the child itself is Dock'd (Fill/Left/Right/Top/Bottom),
            // its bounds are managed by WinForms layout — compute child's
            // OWN ratio from its snapshotted vs current size and recurse.
            foreach (Control c in parent.Controls)
            {
                if (!_snap.TryGetValue(c, out var s)) continue;

                if (c.Dock != DockStyle.None)
                {
                    // Docked: let WinForms manage bounds, but compute per-child rx/ry
                    // for its children based on how much it actually changed
                    float childRx = s.W > 0 ? (float)c.Width  / s.W : parentRx;
                    float childRy = s.H > 0 ? (float)c.Height / s.H : parentRy;
                    Apply(c, childRx, childRy);
                    continue;
                }

                // Non-docked: scale position and size proportionally using PARENT ratios
                c.SetBounds(
                    (int)Math.Round(s.X * parentRx),
                    (int)Math.Round(s.Y * parentRy),
                    (int)Math.Round(s.W * parentRx),
                    (int)Math.Round(s.H * parentRy),
                    BoundsSpecified.All);

                // Scale font (use the smaller ratio to avoid text clipping)
                float fontScale = Math.Min(parentRx, parentRy);
                float newFontSize = Math.Max(6f, s.FontSize * fontScale);
                if (Math.Abs(c.Font.Size - newFontSize) > 0.3f)
                    c.Font = new Font(c.Font.FontFamily, newFontSize, c.Font.Style, c.Font.Unit);

                // Now compute ratios for THIS child's children based on the child's
                // actual new size vs snapshotted size
                float cRx = s.W > 0 ? (float)c.Width  / s.W : parentRx;
                float cRy = s.H > 0 ? (float)c.Height / s.H : parentRy;
                Apply(c, cRx, cRy);
            }
        }
    }
}
