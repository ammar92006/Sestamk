using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class ChannelsPage : Page
{
    private readonly ObservableCollection<ChannelRow> _list = new();

    public ChannelsPage()
    {
        InitializeComponent();
        channelsList.ItemsSource = _list;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        try
        {
            var data = await ChannelService.GetAllAsync();
            _list.Clear();
            foreach (var c in data) _list.Add(c);
            lblCount.Text = $"{_list.Count} قناة";
            lblEmpty.Visibility = _list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل القنوات:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ChannelEditorWindow { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ChannelRow row }) return;
        var dlg = new ChannelEditorWindow(row) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ChannelRow row }) return;
        bool desired = !row.IsActive;
        string verb = desired ? "تفعيل" : "إيقاف";
        if (!ConfirmWindow.Show(Window.GetWindow(this), $"{verb} القناة",
                $"هل تريد {verb} قناة «{row.ChannelName}»؟", verb)) return;
        try { await ChannelService.SetActiveAsync(row.Id, row.ChannelName, desired); await LoadAsync(); }
        catch (Exception ex) { ShowErr(ex); }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ChannelRow row }) return;
        if (!ConfirmWindow.Show(Window.GetWindow(this), "حذف القناة",
                $"سيتم حذف قناة «{row.ChannelName}» نهائياً.\nتأكد أن لا توجد شركات مرتبطة بها.",
                "حذف", danger: true)) return;
        try { await ChannelService.DeleteAsync(row.Id, row.ChannelName); await LoadAsync(); }
        catch (Exception ex) { ShowErr(ex); }
    }

    private static void ShowErr(Exception ex) =>
        MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
            "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
}
