using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Sestamk.Classes
{
    public enum AppTheme
    {
        Light,
        Dark
    }

    public class ThemePalette
    {
        // Surfaces
        public Color Background { get; set; }
        public Color Card { get; set; }
        public Color Sidebar { get; set; }
        public Color Header { get; set; }
        public Color Modal { get; set; }

        // Interactive
        public Color Primary { get; set; }
        public Color Secondary { get; set; }
        public Color Success { get; set; }
        public Color Danger { get; set; }
        public Color Warning { get; set; }

        // Text
        public Color MainText { get; set; }
        public Color SubText { get; set; }
        public Color Placeholder { get; set; }
        public Color InverseText { get; set; }

        // Borders
        public Color ThinBorder { get; set; }
        public Color FocusedBorder { get; set; }
        public Color Divider { get; set; }
    }

    public static class ThemeManager
    {
        public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;
        private static readonly string configPath = Path.Combine(Application.StartupPath, "theme_config.json");

        public static event EventHandler ThemeChanged;

        public static ThemePalette DarkPalette = new ThemePalette
        {
            Background = Color.FromArgb(18, 18, 18),
            Card = Color.FromArgb(30, 30, 47),
            Sidebar = Color.FromArgb(26, 26, 39),
            Header = Color.FromArgb(26, 26, 39),
            Modal = Color.FromArgb(42, 42, 61),

            Primary = Color.FromArgb(59, 130, 246), // Professional Blue
            Secondary = Color.FromArgb(107, 114, 128),
            Success = Color.FromArgb(16, 185, 129),
            Danger = Color.FromArgb(239, 68, 68),
            Warning = Color.FromArgb(245, 158, 11),

            MainText = Color.FromArgb(249, 250, 251),
            SubText = Color.FromArgb(156, 163, 175),
            Placeholder = Color.FromArgb(107, 114, 128),
            InverseText = Color.FromArgb(17, 24, 39),

            ThinBorder = Color.FromArgb(55, 65, 81),
            FocusedBorder = Color.FromArgb(59, 130, 246),
            Divider = Color.FromArgb(45, 55, 72)
        };

        public static ThemePalette LightPalette = new ThemePalette
        {
            Background = Color.FromArgb(243, 244, 246),
            Card = Color.FromArgb(255, 255, 255),
            Sidebar = Color.FromArgb(248, 250, 252),
            Header = Color.FromArgb(248, 250, 252),
            Modal = Color.FromArgb(255, 255, 255),

            Primary = Color.FromArgb(0, 191, 166), // Sestamk Teal
            Secondary = Color.FromArgb(148, 163, 184),
            Success = Color.FromArgb(5, 150, 105),
            Danger = Color.FromArgb(220, 38, 38),
            Warning = Color.FromArgb(217, 119, 6),

            MainText = Color.FromArgb(31, 41, 55),
            SubText = Color.FromArgb(107, 114, 128),
            Placeholder = Color.FromArgb(156, 163, 175),
            InverseText = Color.FromArgb(255, 255, 255),

            ThinBorder = Color.FromArgb(229, 231, 235),
            FocusedBorder = Color.FromArgb(0, 191, 166),
            Divider = Color.FromArgb(209, 213, 219)
        };

        public static void LoadTheme()
        {
            try
            {
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    if (Enum.TryParse(json, out AppTheme loadedTheme))
                    {
                        CurrentTheme = loadedTheme;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading theme: " + ex.Message);
            }
            // Default fallback
            CurrentTheme = AppTheme.Light;
        }

        public static void SaveTheme()
        {
            try
            {
                File.WriteAllText(configPath, CurrentTheme.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving theme: " + ex.Message);
            }
        }

        public static void ToggleTheme()
        {
            CurrentTheme = CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
            SaveTheme();
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void ApplyTheme(Control parentControl)
        {
            if (parentControl.Tag?.ToString() == "NoTheme") return;

            bool isTopLevel = false;
            if (parentControl is Form form)
            {
                isTopLevel = true;
                form.SuspendLayout();
            }

            ApplyThemeRecursive(parentControl);

            if (isTopLevel)
            {
                parentControl.ResumeLayout(true);
            }
        }

        private static void ApplyThemeRecursive(Control ctrl)
        {
            if (ctrl.Tag?.ToString() == "NoTheme") return;

            ThemePalette currentPalette = CurrentTheme == AppTheme.Dark ? DarkPalette : LightPalette;

            ApplyStylesToControl(ctrl, currentPalette);

            foreach (Control child in ctrl.Controls)
            {
                ApplyThemeRecursive(child);
            }
        }

        private static Color DarkenColor(Color color, float percentage)
        {
            int r = (int)(color.R * (1 - percentage));
            int g = (int)(color.G * (1 - percentage));
            int b = (int)(color.B * (1 - percentage));
            return Color.FromArgb(color.A, Math.Max(0, r), Math.Max(0, g), Math.Max(0, b));
        }

        private static Color LightenColor(Color color, float percentage)
        {
            int r = (int)(color.R + (255 - color.R) * percentage);
            int g = (int)(color.G + (255 - color.G) * percentage);
            int b = (int)(color.B + (255 - color.B) * percentage);
            return Color.FromArgb(color.A, Math.Min(255, r), Math.Min(255, g), Math.Min(255, b));
        }

        private static void EnableDoubleBuffered(Control control)
        {
            try
            {
                PropertyInfo pi = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                if (pi != null)
                {
                    pi.SetValue(control, true, null);
                }
            }
            catch { }
        }

        private static void ApplyStylesToControl(Control ctrl, ThemePalette p)
        {
            // 1. تفعيل الوميض المزدوج (DoubleBuffering) لجميع الحاويات والعناصر المعقدة
            if (ctrl is Panel || ctrl is DataGridView || ctrl is Guna2Panel || ctrl is Guna2GradientPanel || ctrl is System.Windows.Forms.UserControl || ctrl is FlowLayoutPanel)
            {
                EnableDoubleBuffered(ctrl);
            }

            // 2. حل مشكلة "الحواف البيضاء" (White Edges) بجعل الخلفية شفافة لجميع عناصر Guna2 التي تدعم BorderRadius
            // هذا يسمح لخصائص التنعيم (Antialiasing) بالعمل بشكل صحيح فوق خلفية الحاوية الأم
            if (ctrl.GetType().Namespace != null && ctrl.GetType().Namespace.Contains("Guna.UI2"))
            {
                if (!(ctrl is DataGridView)) // الجداول غالباً لا تدعم الشفافية وتسبب خطأ
                {
                    try { ctrl.BackColor = Color.Transparent; } catch { }
                }
            }

            // 3. الحصول على الـ Tag لتخصيص السلوك بناءً على الدور (Sidebar, Header, Modal, etc)
            string tag = ctrl.Tag?.ToString() ?? "";
            bool isDark = CurrentTheme == AppTheme.Dark;

            if (ctrl is Form frm)
            {
                frm.BackColor = p.Background;
            }
            else if (ctrl is Guna2GradientPanel gradPanel)
            {
                // تخصيص التدرج اللوني بناءً على مكان الحاوية
                if (tag == "Header")
                {
                    gradPanel.FillColor = p.Header;
                    gradPanel.FillColor2 = DarkenColor(p.Header, 0.1f);
                }
                else if (tag == "Sidebar")
                {
                    gradPanel.FillColor = p.Sidebar;
                    gradPanel.FillColor2 = DarkenColor(p.Sidebar, 0.05f);
                }
                else
                {
                    gradPanel.FillColor = isDark ? p.Background : p.Primary;
                    gradPanel.FillColor2 = isDark ? DarkenColor(p.Background, 0.15f) : DarkenColor(p.Primary, 0.15f);
                }
                gradPanel.BorderThickness = 0;
            }
            else if (ctrl is Guna2Panel panel)
            {
                // توجيه الألوان بناءً على الأسطح (Surfaces)
                if (tag == "Sidebar") panel.FillColor = p.Sidebar;
                else if (tag == "Header") panel.FillColor = p.Header;
                else if (tag == "Modal") panel.FillColor = p.Modal;
                else panel.FillColor = p.Card;

                // تطبيق قاعدة "سماكة الحدود 1"
                if (panel.BorderThickness > 0 || tag == "Bordered")
                {
                    panel.BorderThickness = 1;
                    panel.BorderColor = p.ThinBorder;
                }
            }
            else if (ctrl is Panel standardPanel)
            {
                if (tag == "Sidebar") standardPanel.BackColor = p.Sidebar;
                else if (tag == "Header") standardPanel.BackColor = p.Header;
                else standardPanel.BackColor = p.Card;
            }
            else if (ctrl is Guna2TextBox txt)
            {
                txt.FillColor = p.Card;
                txt.ForeColor = p.MainText;
                txt.BorderColor = p.ThinBorder;
                txt.BorderThickness = 1;
                txt.FocusedState.BorderColor = p.FocusedBorder;
                txt.HoverState.BorderColor = p.FocusedBorder;
                txt.PlaceholderForeColor = p.Placeholder;
            }
            else if (ctrl is Guna2Button btn)
            {
                // استثناء أزرار التحكم في النظام
                if (btn.Name == "btn_theme_toggle_base" || btn.Name == "btn_close" || btn.Name == "btnMin" || btn.Name == "btnMax" || tag == "NoTheme")
                {
                    if (btn.Name == "btn_theme_toggle_base") btn.ForeColor = p.SubText;
                    return;
                }

                // تحديد اللون التفاعلي بناءً على الحالة (Success, Danger, etc)
                Color btnPrimary = p.Primary;
                if (tag.Equals("Success", StringComparison.OrdinalIgnoreCase)) btnPrimary = p.Success;
                else if (tag.Equals("Danger", StringComparison.OrdinalIgnoreCase)) btnPrimary = p.Danger;
                else if (tag.Equals("Warning", StringComparison.OrdinalIgnoreCase)) btnPrimary = p.Warning;
                else if (tag.Equals("Secondary", StringComparison.OrdinalIgnoreCase)) btnPrimary = p.Secondary;

                bool isOutline = btn.ButtonMode == Guna.UI2.WinForms.Enums.ButtonMode.ToogleButton || 
                                 btn.ButtonMode == Guna.UI2.WinForms.Enums.ButtonMode.RadioButton || 
                                 btn.FillColor == Color.Transparent || btn.BorderThickness > 0;

                if (isOutline && tag != "Solid")
                {
                    btn.FillColor = Color.Transparent;
                    btn.BorderThickness = 1;
                    btn.BorderColor = p.ThinBorder;
                    btn.ForeColor = btnPrimary;
                    btn.HoverState.FillColor = Color.FromArgb(30, btnPrimary); 
                    btn.HoverState.BorderColor = btnPrimary;
                    btn.CheckedState.FillColor = Color.FromArgb(50, btnPrimary);
                }
                else
                {
                    btn.FillColor = btnPrimary;
                    btn.ForeColor = Color.White;
                    btn.BorderThickness = 0;
                    btn.HoverState.FillColor = isDark ? LightenColor(btnPrimary, 0.12f) : DarkenColor(btnPrimary, 0.12f);
                    btn.CheckedState.FillColor = DarkenColor(btnPrimary, 0.2f);
                }
            }
            else if (ctrl is Label lbl)
            {
                // معالجة نصوص الخلفيات المتباينة
                if (lbl.Parent is Guna2GradientPanel || tag == "Inverse")
                {
                    lbl.ForeColor = p.InverseText;
                }
                else if (tag == "Secondary" || tag == "SubText")
                {
                    lbl.ForeColor = p.SubText;
                }
                else
                {
                    lbl.ForeColor = p.MainText;
                }
                lbl.BackColor = Color.Transparent;
            }
            else if (ctrl is Guna2ToggleSwitch toggle)
            {
                toggle.CheckedState.FillColor = p.Primary;
                toggle.CheckedState.InnerColor = Color.White;
                toggle.UncheckedState.FillColor = p.Divider;
                toggle.UncheckedState.InnerColor = isDark ? Color.Gray : Color.White;
            }
            else if (ctrl is Guna2Separator sep)
            {
                sep.FillColor = p.Divider;
            }
            else if (ctrl is Guna2CheckBox chk)
            {
                chk.CheckedState.BorderColor = p.Primary;
                chk.CheckedState.FillColor = p.Primary;
                chk.UncheckedState.FillColor = p.Card;
                chk.UncheckedState.BorderColor = p.ThinBorder;
                chk.ForeColor = p.SubText;
            }
            else if (ctrl is Guna2ControlBox controlBox)
            {
                controlBox.FillColor = Color.Transparent;
                controlBox.IconColor = p.MainText;
                controlBox.HoverState.IconColor = Color.White;
                if (controlBox.ControlBoxType == Guna.UI2.WinForms.Enums.ControlBoxType.CloseBox)
                    controlBox.HoverState.FillColor = p.Danger;
            }
            else if (ctrl is Guna2DataGridView gunaDgv)
            {
                gunaDgv.BackgroundColor = p.Card;
                gunaDgv.GridColor = p.Divider;
                gunaDgv.ThemeStyle.BackColor = p.Card;
                gunaDgv.ThemeStyle.RowsStyle.BackColor = p.Card;
                gunaDgv.ThemeStyle.RowsStyle.ForeColor = p.MainText;
                gunaDgv.ThemeStyle.RowsStyle.SelectionBackColor = p.Primary;
                gunaDgv.ThemeStyle.RowsStyle.SelectionForeColor = Color.White;
                gunaDgv.ThemeStyle.AlternatingRowsStyle.BackColor = isDark ? DarkenColor(p.Card, 0.05f) : DarkenColor(p.Card, 0.02f);
                gunaDgv.ThemeStyle.HeaderStyle.BackColor = p.Background;
                gunaDgv.ThemeStyle.HeaderStyle.ForeColor = p.MainText;
            }
            else if (ctrl is DataGridView dgv)
            {
                dgv.BackgroundColor = p.Card;
                dgv.GridColor = p.Divider;
                dgv.DefaultCellStyle.BackColor = p.Card;
                dgv.DefaultCellStyle.ForeColor = p.MainText;
                dgv.DefaultCellStyle.SelectionBackColor = p.Primary;
                dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = p.Background;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = p.MainText;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = p.Background;
            }
            else if (ctrl is Guna2ProgressBar progress)
            {
                progress.FillColor = p.Divider;
                progress.ProgressColor = p.Primary;
                progress.ProgressColor2 = DarkenColor(p.Primary, 0.2f);
                progress.BackColor = Color.Transparent;
            }
            else if (ctrl is Guna2CircleButton cbtn)
            {
                cbtn.FillColor = p.Primary;
                cbtn.HoverState.FillColor = DarkenColor(p.Primary, 0.15f);
                cbtn.CheckedState.FillColor = DarkenColor(p.Primary, 0.25f);
            }
            else if (ctrl is Guna2PictureBox pic)
            {
                pic.BackColor = Color.Transparent;
            }
            else if (ctrl is LinkLabel link)
            {
                link.LinkColor = p.Primary;
            }
        }
    }
}

