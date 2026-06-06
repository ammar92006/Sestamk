using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class AnnouncementsPage : Page
{
    private readonly ObservableCollection<AnnouncementRow> _list = new();

    public AnnouncementsPage()
    {
        InitializeComponent();
        list.ItemsSource = _list;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        try
        {
            var data = await AnnouncementService.GetAllAsync();
            _list.Clear();
            foreach (var a in data) _list.Add(a);
            lblCount.Text = $"{_list.Count} إعلان";
            lblEmpty.Visibility = _list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الإعلانات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AnnouncementEditorWindow { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: AnnouncementRow row }) return;
        try { await AnnouncementService.SetActiveAsync(row.Id, row.Title, !row.IsActive); await LoadAsync(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: AnnouncementRow row }) return;
        if (!ConfirmWindow.Show(Window.GetWindow(this), "حذف الإعلان",
                $"سيتم حذف الإعلان ({row.Title}) نهائياً.", "حذف", danger: true)) return;
        try { await AnnouncementService.DeleteAsync(row.Id, row.Title); await LoadAsync(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
