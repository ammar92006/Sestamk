using System.Windows;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Pages;
using Wpf.Ui.Controls;

namespace SestamkAdmin;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        lblAdminName.Text = AppSession.AdminName ?? "المسؤول";
        lblAdminRole.Text = AppSession.AdminRole ?? "admin";
        var name = AppSession.AdminName;
        lblAvatar.Text = string.IsNullOrEmpty(name) ? "A" : name.Trim()[0].ToString().ToUpper();

        Loaded += (_, _) => RootNavigation.Navigate(typeof(DashboardPage));
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var result = System.Windows.MessageBox.Show(
            "هل تريد تسجيل الخروج؟",
            "تأكيد",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            AppSession.Clear();
            Application.Current.Shutdown();
        }
    }
}
