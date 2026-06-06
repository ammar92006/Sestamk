using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class ConfirmWindow : FluentWindow
{
    public ConfirmWindow(string title, string message, string confirmText = "تأكيد", bool danger = false)
    {
        InitializeComponent();
        titleBar.Title = title;
        lblTitle.Text = title;
        lblMessage.Text = message;
        btnConfirm.Content = confirmText;

        if (danger)
        {
            icon.Symbol = SymbolRegular.Warning24;
            icon.Foreground = (Brush)FindResource("BrandDangerBrush");
            iconCircle.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26EF4444"));
            btnConfirm.Appearance = ControlAppearance.Danger;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void Ok_Click(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }

    /// <summary>عرض نافذة تأكيد احترافية. تُرجع true لو ضغط المستخدم زر التأكيد.</summary>
    public static bool Show(Window? owner, string title, string message,
        string confirmText = "تأكيد", bool danger = false)
    {
        var dlg = new ConfirmWindow(title, message, confirmText, danger);
        if (owner != null) dlg.Owner = owner;
        return dlg.ShowDialog() == true;
    }
}
