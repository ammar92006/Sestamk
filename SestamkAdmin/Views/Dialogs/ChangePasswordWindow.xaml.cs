using System.Windows;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class ChangePasswordWindow : FluentWindow
{
    private readonly AdminUserRow _row;
    public ChangePasswordWindow(AdminUserRow row)
    {
        InitializeComponent();
        _row = row;
        lblFor.Text = $"تغيير كلمة مرور: {row.FullName} ({row.Username})";
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtOld.Password)) { ShowErr("أدخل كلمة المرور الحالية."); return; }
        if (txtNew.Password.Length < 8) { ShowErr("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل."); return; }
        btnSave.IsEnabled = false; lblError.Visibility = Visibility.Collapsed;
        var (ok, err) = await AdminUsersService.ChangePasswordAsync(_row.Id, txtOld.Password, txtNew.Password);
        if (ok) { lblSuccess.Visibility = Visibility.Visible; txtOld.Password = ""; txtNew.Password = ""; }
        else { ShowErr(err ?? "فشل تغيير كلمة المرور."); }
        btnSave.IsEnabled = true;
    }
    private void ShowErr(string m) { lblError.Text = "⚠ " + m; lblError.Visibility = Visibility.Visible; }
}
