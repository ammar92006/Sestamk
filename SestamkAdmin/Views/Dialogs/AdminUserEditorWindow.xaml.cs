using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class AdminUserEditorWindow : FluentWindow
{
    public AdminUserEditorWindow() { InitializeComponent(); Loaded += (_, _) => txtFullName.Focus(); }
    private string Role => (cmbRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "admin";
    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text)) { ShowErr("اسم المستخدم مطلوب."); return; }
        if (string.IsNullOrWhiteSpace(txtPassword.Password)) { ShowErr("كلمة المرور مطلوبة."); return; }
        if (txtPassword.Password.Length < 8) { ShowErr("كلمة المرور يجب أن تكون 8 أحرف على الأقل."); return; }
        btnSave.IsEnabled = false; lblError.Visibility = Visibility.Collapsed;
        var (ok, err) = await AdminUsersService.CreateAsync(
            txtUsername.Text.Trim(), txtPassword.Password,
            txtEmail.Text.Trim(), txtFullName.Text.Trim(), Role);
        if (ok) { DialogResult = true; Close(); }
        else { ShowErr(err ?? "فشل الإنشاء."); btnSave.IsEnabled = true; }
    }
    private void ShowErr(string m) { lblError.Text = "⚠ " + m; lblError.Visibility = Visibility.Visible; }
}
