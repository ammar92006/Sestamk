using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class ResellersPage : Page
{
    private readonly ObservableCollection<ResellerRow> _list = new();

    public ResellersPage()
    {
        InitializeComponent();
        grid.ItemsSource = _list;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        try
        {
            var data = await ResellerService.GetAllAsync();
            _list.Clear();
            foreach (var r in data) _list.Add(r);
            lblCount.Text = $"{_list.Count} موزّع";
            lblEmpty.Visibility = _list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الموزّعين:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new ResellerEditorWindow { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ResellerRow row }) return;
        var dlg = new ResellerEditorWindow(row) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: ResellerRow row }) return;
        if (!ConfirmWindow.Show(Window.GetWindow(this), "حذف الموزّع",
                $"سيتم حذف الموزّع ({row.Name}) نهائياً.", "حذف", danger: true)) return;
        try { await ResellerService.DeleteAsync(row.Id, row.Name); await LoadAsync(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
