using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class NotificationsPage : Page
{
    private readonly ObservableCollection<NotificationRow> _list = new();

    public NotificationsPage()
    {
        InitializeComponent();
        list.ItemsSource = _list;
        Loaded += async (_, _) => await LoadAsync();
    }

    private bool UnreadOnly =>
        (cmbFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "unread";

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        emptyState.Visibility = Visibility.Collapsed;
        try
        {
            var data = await NotificationService.GetAllAsync(UnreadOnly);
            _list.Clear();
            foreach (var n in data) _list.Add(n);
            lblCount.Text = $"{_list.Count} إشعار";
            emptyState.Visibility = _list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الإشعارات:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) _ = LoadAsync();
    }
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void MarkRead_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: NotificationRow row }) return;
        try { await NotificationService.MarkReadAsync(row.Id); await LoadAsync(); } catch { }
    }

    private async void MarkAll_Click(object sender, RoutedEventArgs e)
    {
        try { await NotificationService.MarkAllReadAsync(); await LoadAsync(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ"); }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: NotificationRow row }) return;
        try { await NotificationService.DeleteAsync(row.Id); await LoadAsync(); } catch { }
    }
}
