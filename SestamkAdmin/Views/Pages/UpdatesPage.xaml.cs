using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace SestamkAdmin.Views.Pages;

public partial class UpdatesPage : Page
{
    private readonly ObservableCollection<UpdateRow> _history = new();

    public UpdatesPage()
    {
        InitializeComponent();
        grid.ItemsSource = _history;
        lblBuildPath.Text = AdminConfig.BuildFolderPath;
        Loaded += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        var channels = await UpdateService.GetChannelsAsync();
        cmbChannel.ItemsSource = channels;
        cmbChannel.SelectedItem = channels.Contains("beta") ? "beta" : channels.FirstOrDefault();
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        loading.Visibility = Visibility.Visible;
        try
        {
            var list = await UpdateService.GetHistoryAsync();
            _history.Clear();
            foreach (var u in list) _history.Add(u);

            // اقتراح رقم الإصدار التالي والسابق
            if (list.Count > 0 && string.IsNullOrWhiteSpace(txtPrevVersion.Text))
                txtPrevVersion.Text = list[0].Version;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل السجل:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadHistoryAsync();

    private void Log(string line)
    {
        Dispatcher.Invoke(() =>
        {
            txtLog.Text += "\n" + line;
            logScroll.ScrollToBottom();
        });
    }

    private async void Publish_Click(object sender, RoutedEventArgs e)
    {
        string version = txtVersion.Text.Trim();
        string prev = txtPrevVersion.Text.Trim();
        string channel = cmbChannel.SelectedItem?.ToString() ?? "beta";

        if (string.IsNullOrWhiteSpace(version))
        {
            MessageBox.Show("أدخل رقم الإصدار الجديد.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var whatsNew = txtWhatsNew.Text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (MessageBox.Show(
                $"سيتم بناء ونشر الإصدار {version} على قناة «{channel}».\nمتابعة؟",
                "تأكيد النشر", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        btnPublish.IsEnabled = false;
        txtLog.Text = "🚀 بدء النشر...";
        try
        {
            await Task.Run(() => UpdateService.PublishAsync(
                version, prev, channel, swMandatory.IsChecked == true, whatsNew, Log));

            MessageBox.Show($"تم نشر الإصدار {version} بنجاح ✅", "تم",
                MessageBoxButton.OK, MessageBoxImage.Information);
            txtVersion.Clear();
            txtWhatsNew.Clear();
            await LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            Log("❌ خطأ: " + ex.Message);
            MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
                "فشل النشر", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { btnPublish.IsEnabled = true; }
    }

    private async void ToggleActive(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: UpdateRow row }) return;
        if (MessageBox.Show($"{(row.IsActive ? "إيقاف" : "تفعيل")} الإصدار {row.Version}؟",
                "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            await UpdateService.SetActiveAsync(row.Id, row.Version, !row.IsActive);
            await LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
