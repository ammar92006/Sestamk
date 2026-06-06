using System.Windows;
using System.Windows.Input;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views;

public partial class LoginWindow : FluentWindow
{
    public LoginWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => txtUsername.Focus();
        txtPassword.KeyDown += OnPasswordKeyDown;
    }

    private async void OnPasswordKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await TryLogin();
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        => await TryLogin();

    private async Task TryLogin()
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Password;

        lblError.Visibility = Visibility.Collapsed;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("يرجى إدخال اسم المستخدم وكلمة المرور.");
            return;
        }

        SetBusy(true);
        var (ok, error) = await AuthService.LoginAsync(username, password);
        SetBusy(false);

        if (ok)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            ShowError(error ?? "فشل تسجيل الدخول.");
        }
    }

    private void ShowError(string message)
    {
        lblError.Text = "⚠ " + message;
        lblError.Visibility = Visibility.Visible;
    }

    private void SetBusy(bool busy)
    {
        btnLogin.IsEnabled = !busy;
        txtUsername.IsEnabled = !busy;
        txtPassword.IsEnabled = !busy;
        loading.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
    }
}
