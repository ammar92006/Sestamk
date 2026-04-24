using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class BlurEffect
{
    #region ── DWM Acrylic ────────────────────────────────────────────
    [DllImport("user32.dll")]
    private static extern int SetWindowCompositionAttribute(
        IntPtr hwnd, ref WindowCompositionAttributeData data);

    private enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    public static void EnableAcrylic(Form form,
        int argbColor = unchecked((int)0x18000000))
    {
        var accent = new AccentPolicy
        {
            AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
            GradientColor = argbColor
        };
        int size = Marshal.SizeOf(accent);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(accent, ptr, false);
        var data = new WindowCompositionAttributeData
        { Attribute = 19, SizeOfData = size, Data = ptr };
        SetWindowCompositionAttribute(form.Handle, ref data);
        Marshal.FreeHGlobal(ptr);
    }
    #endregion

    #region ── State ──────────────────────────────────────────────────
    private static Panel _panel;
    private static Bitmap _bmp;

    // ── اضبط هنا حسب ذوقك ─────────────────────────────────────────
    // كلما قلت = blur أقوى + أسرع  (0.03 ~ 0.15)
    private const float SCALE = 0.05f;
    // تعتيم: 0.0 أسود كامل ← 1.0 بدون تعتيم
    private const float DARKNESS = 0.55f;
    // lون overlay (ARGB)
    private static readonly Color TINT = Color.FromArgb(50, 10, 15, 40);
    #endregion

    #region ── Public API ─────────────────────────────────────────────

    /// <summary>
    /// استدعيها من الفورم الأم قبل ShowDialog مباشرةً
    /// </summary>
    public static void BlurParent(Form parentForm)
    {
        if (_panel != null) return;

        // ① احسب الـ bounds الفعلية على الشاشة
        Rectangle screen = parentForm.Bounds;
        // ↑ Bounds = موقع + حجم الفورم على الشاشة كاملاً

        // ② التقط الشاشة الفعلية (الفورم الأم + اللي وراها)
        //    ده أهم خطوة - CopyFromScreen مش DrawToBitmap
        var fullShot = new Bitmap(screen.Width, screen.Height,
                                   PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(fullShot))
            g.CopyFromScreen(screen.X, screen.Y, 0, 0, screen.Size);

        // ③ Downscale بنسبة صغيرة = blur قوي جداً وسريع
        int sw = Math.Max(1, (int)(screen.Width * SCALE));
        int sh = Math.Max(1, (int)(screen.Height * SCALE));

        var small = new Bitmap(sw, sh, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(small))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.DrawImage(fullShot, 0, 0, sw, sh);
        }
        fullShot.Dispose();

        // ④ Upscale تاني = blur ناعم
        _bmp = new Bitmap(screen.Width, screen.Height,
                           PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(_bmp))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.DrawImage(small, 0, 0, screen.Width, screen.Height);
        }
        small.Dispose();

        // ⑤ تعتيم + tint مباشرة على الـ bitmap
        using (var g = Graphics.FromImage(_bmp))
        {
            // Darkening layer
            using (var darkBrush = new SolidBrush(
                Color.FromArgb((int)((1f - DARKNESS) * 255), 0, 0, 0)))
                g.FillRectangle(darkBrush, 0, 0, _bmp.Width, _bmp.Height);

            // Tint layer
            using (var tintBrush = new SolidBrush(TINT))
                g.FillRectangle(tintBrush, 0, 0, _bmp.Width, _bmp.Height);
        }

        // ⑥ أنشئ Panel بحجم الـ ClientArea
        //    (ClientSize = داخل الفورم بدون الـ title bar والـ borders)
        Point clientOrigin = parentForm.PointToClient(screen.Location);

        _panel = new Panel
        {
            Location = Point.Empty,
            Size = parentForm.ClientSize,
            BackgroundImage = _bmp,
            BackgroundImageLayout = ImageLayout.Zoom,
        };

        // ⑦ أضف الـ panel وارسمه قبل ShowDialog
        parentForm.Controls.Add(_panel);
        _panel.BringToFront();

        // ↓ ده السطر المهم - بيجبر الـ UI يرسم الـ panel
        //   قبل ما ShowDialog يبلك الـ thread
        _panel.Refresh();
        Application.DoEvents();
    }

    /// <summary>
    /// استدعيها بعد ما ShowDialog يرجع
    /// </summary>
    public static void RemoveBlur(Form parentForm)
    {
        if (_panel != null)
        {
            parentForm.Controls.Remove(_panel);
            _panel.Dispose();
            _panel = null;
        }
        _bmp?.Dispose();
        _bmp = null;
    }

    #endregion
}