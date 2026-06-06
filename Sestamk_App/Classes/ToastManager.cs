using Sestamk.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Sestamk.Classes
{
    //public class ToastModel
    //{
    //    public Guid Id { get; set; }
    //    public ToastType Type { get; set; }
    //    public string Title { get; set; }
    //    public string message { get; set; }
    //    public int Duration { get; set; }
    //    public ToastPriority Priority { get; set; } = ToastPriority.Normal;
    //    public bool autoclose { get; set; } = true;

    //}
    public class ToastModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ToastType Type { get; set; }
        public ToastPriority Priority { get; set; } = ToastPriority.Normal;
        public string Title { get; set; }
        public string Message { get; set; }
        public int Duration { get; set; } = 4000;
        public bool AutoClose { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public enum ToastPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    //public class ToastOptions
    //{
    //    public int MaxVisibleToasts { get; set; } = 5;
    //    public int Spacing { get; set; } = 10;
    //    public int MarginTop { get; set; } = 20;
    //    public int MarginRight { get; set; } = 20;
    //    public int AnimationSpeed { get; set; } = 20;
    //    public int AnimationInterval { get; set; } = 10;
    //}
    public class ToastOptions
    {
        public int MaxVisibleToasts { get; set; } = 5;
        public int Spacing { get; set; } = 10;
        public int MarginTop { get; set; } = 20;
        public int MarginRight { get; set; } = 20;
        public int AnimationInterval { get; set; } = 15;
        public int AnimationSpeed { get; set; } = 20;
        public double FadeSpeed { get; set; } = 0.08;
    }

    public static class ToastManager
    {
        //private static readonly List<frmToast> _visiableToast = new();
        //private static readonly Queue<ToastModel> _queue = new();
        //private static readonly object _lock = new();

        private static readonly List<frmToast> _visibleToasts = new();
        private static readonly List<ToastModel> _queue = new();
        private static readonly object _lock = new();
        private static ToastOptions _options = new();
        private static System.Windows.Forms.Timer _animationTimer;

        static ToastManager()
        {
            _animationTimer = new System.Windows.Forms.Timer();
            _animationTimer.Interval = _options.AnimationInterval;
            _animationTimer.Tick += AnimationTick;
            _animationTimer.Start();
        }

        public static void Configure(ToastOptions options)
        {
            _options = options;
        }

        public static void Show(ToastModel model)
        {
            if (Application.OpenForms.Count == 0)
                return;

            Application.OpenForms[0].Invoke((MethodInvoker)(() =>
            {
                lock (_lock)
                {
                    if (_visibleToasts.Count >= _options.MaxVisibleToasts)
                    {
                        _queue.Add(model);
                        _queue.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                        return;
                    }

                    CreateToast(model);
                }
            }));
        }

        private static void CreateToast(ToastModel model)
        {
            var toast = new frmToast(model);

            Screen screen = Screen.FromControl(Application.OpenForms[0]);
            int x = screen.WorkingArea.Right - toast.Width - _options.MarginRight;

            toast.Left = x;
            toast.Top = screen.WorkingArea.Bottom + toast.Height; // Start below screen

            _visibleToasts.Add(toast);
            UpdateLayout();

            toast.Show();
        }

        private static void UpdateLayout()
        {
            Screen screen = Screen.FromControl(Application.OpenForms[0]);
            int y = screen.WorkingArea.Bottom - _options.MarginTop;

            foreach (var toast in _visibleToasts)
            {
                y -= toast.Height;
                toast.TargetY = y;
                y -= _options.Spacing;
            }
        }

        private static void AnimationTick(object sender, EventArgs e)
        {
            lock (_lock)
            {
                if (_visibleToasts.Count == 0)
                    return;

                for (int i = _visibleToasts.Count - 1; i >= 0; i--)
                {
                    var toast = _visibleToasts[i];

                    // التأكد من أن الكنترول لا يزال صالحاً قبل العمل عليه
                    if (toast == null || toast.IsDisposed || !toast.IsHandleCreated)
                    {
                        _visibleToasts.RemoveAt(i);
                        continue;
                    }

                    // Slide animation
                    if (toast.Top < toast.TargetY)
                    {
                        toast.Top = Math.Min(toast.Top + _options.AnimationSpeed, toast.TargetY);
                    }
                    else if (toast.Top > toast.TargetY)
                    {
                        toast.Top = Math.Max(toast.Top - _options.AnimationSpeed, toast.TargetY);
                    }

                    // Fade In
                    if (!toast.IsClosing && toast.Opacity < 1)
                    {
                        toast.Opacity = Math.Min(toast.Opacity + _options.FadeSpeed, 1.0);
                    }

                    // Auto Close check
                    if (!toast.IsClosing &&
                        toast.Model.AutoClose &&
                        (DateTime.Now - toast.Model.CreatedAt).TotalMilliseconds >= toast.Model.Duration)
                    {
                        toast.IsClosing = true;
                    }

                    // Fade Out
                    if (toast.IsClosing)
                    {
                        toast.Opacity -= _options.FadeSpeed;

                        if (toast.Opacity <= 0)
                        {
                            toast.Close();
                            toast.Dispose();
                            _visibleToasts.RemoveAt(i);
                            UpdateLayout();

                            if (_queue.Count > 0)
                            {
                                var next = _queue[0];
                                _queue.RemoveAt(0);
                                CreateToast(next);
                            }
                        }
                    }
                }
            }
        }

        public static void CloseAll()
        {
            lock (_lock)
            {
                foreach (var toast in _visibleToasts.ToList())
                {
                    toast.Close();
                    toast.Dispose();
                }

                _visibleToasts.Clear();
                _queue.Clear();
            }
        }

        // Convenience Methods
        public static void ShowInfo(string title, string message)
        {
            Show(new ToastModel { Type = ToastType.Info, Title = title, Message = message });
        }

        public static void ShowSuccess(string title, string message)
        {
            Show(new ToastModel { Type = ToastType.Success, Title = title, Message = message });
        }

        public static void ShowWarning(string title, string message)
        {
            Show(new ToastModel { Type = ToastType.Warning, Title = title, Message = message });
        }

        public static void ShowError(string title, string message)
        {
            Show(new ToastModel { Type = ToastType.Error, Title = title, Message = message });
        }











        //    private static List<frmToast> activeToasts = new List<frmToast>();
        //    private static int verticalSpacing = 10;
        //    private static int topMargin = 20;

        //    /// <summary>
        //    /// Show a toast notification
        //    /// </summary>
        //    public static void Show(ToastType type, string title, string message)
        //    {
        //        // Clean up closed toasts
        //        activeToasts.RemoveAll(t => t.IsDisposed);

        //        // Create new toast
        //        frmToast toast = new frmToast(type, title, message);

        //        // Position toast
        //        PositionToast(toast);

        //        // Add to active list
        //        activeToasts.Add(toast);

        //        // Handle close event to reposition remaining toasts
        //        toast.FormClosed += (s, e) =>
        //        {
        //            activeToasts.Remove(toast);
        //            RepositionToasts();
        //        };

        //        // Show toast
        //        toast.Show();
        //    }

        //    /// <summary>
        //    /// Show Info toast
        //    /// </summary>
        //    public static void ShowInfo(string title, string message)
        //    {
        //        Show(ToastType.Info, title, message);
        //    }

        //    /// <summary>
        //    /// Show Success toast
        //    /// </summary>
        //    public static void ShowSuccess(string title, string message)
        //    {
        //        Show(ToastType.Success, title, message);
        //    }

        //    /// <summary>
        //    /// Show Warning toast
        //    /// </summary>
        //    public static void ShowWarning(string title, string message)
        //    {
        //        Show(ToastType.Warning, title, message);
        //    }

        //    /// <summary>
        //    /// Show Error toast
        //    /// </summary>
        //    public static void ShowError(string title, string message)
        //    {
        //        Show(ToastType.Error, title, message);
        //    }

        //    /// <summary>
        //    /// Position a new toast based on existing toasts
        //    /// </summary>
        //    private static void PositionToast(frmToast toast)
        //    {
        //        Screen screen = Screen.PrimaryScreen;
        //        int x = screen.WorkingArea.Right - toast.Width - 20;
        //        int y = topMargin;

        //        // Calculate Y position based on existing toasts
        //        foreach (var existingToast in activeToasts.Where(t => !t.IsDisposed))
        //        {
        //            y += existingToast.Height + verticalSpacing;
        //        }

        //        toast.Location = new System.Drawing.Point(x, -toast.Height);
        //        toast.Tag = y; // Store target Y position
        //    }

        //    /// <summary>
        //    /// Reposition all toasts after one is closed
        //    /// </summary>
        //    private static void RepositionToasts()
        //    {
        //        int y = topMargin;

        //        foreach (var toast in activeToasts.Where(t => !t.IsDisposed))
        //        {
        //            // Animate to new position
        //            System.Windows.Forms.Timer repositionTimer = new System.Windows.Forms.Timer();
        //            repositionTimer.Interval = 10;
        //            int targetY = y;

        //            repositionTimer.Tick += (s, e) =>
        //            {
        //                if (toast.Top < targetY)
        //                {
        //                    toast.Top += 15;
        //                    if (toast.Top >= targetY)
        //                    {
        //                        toast.Top = targetY;
        //                        repositionTimer.Stop();
        //                    }
        //                }
        //                else if (toast.Top > targetY)
        //                {
        //                    toast.Top -= 15;
        //                    if (toast.Top <= targetY)
        //                    {
        //                        toast.Top = targetY;
        //                        repositionTimer.Stop();
        //                    }
        //                }
        //            };

        //            repositionTimer.Start();
        //            y += toast.Height + verticalSpacing;
        //        }
        //    }

        //    /// <summary>
        //    /// Close all active toasts
        //    /// </summary>
        //    public static void CloseAll()
        //    {
        //        foreach (var toast in activeToasts.ToList())
        //        {
        //            if (!toast.IsDisposed)
        //            {
        //                toast.Close();
        //            }
        //        }
        //        activeToasts.Clear();
        //    }
    }
}

