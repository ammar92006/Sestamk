using System.Windows;
using System.Windows.Threading;

namespace SestamkAdmin;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnUnhandledException;

        // امنع الإغلاق التلقائي عند غلق نافذة الدخول (قبل فتح الشاشة الرئيسية)
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var login = new Views.LoginWindow();
        var ok = login.ShowDialog();

        if (ok == true)
        {
            var main = new MainWindow();
            MainWindow = main;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            main.Show();
        }
        else
        {
            Shutdown();
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"حدث خطأ غير متوقع:\n\n{e.Exception.Message}",
            "خطأ",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }
}
