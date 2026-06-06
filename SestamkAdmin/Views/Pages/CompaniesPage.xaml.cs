using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace SestamkAdmin.Views.Pages;

public partial class CompaniesPage : Page
{
    private readonly ObservableCollection<CompanyRow> _all = new();
    private readonly ObservableCollection<CompanyRow> _view = new();

    public CompaniesPage()
    {
        InitializeComponent();
        grid.ItemsSource = _view;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        try
        {
            var list = await CompanyService.GetAllAsync();
            _all.Clear();
            foreach (var c in list) _all.Add(c);
            ApplyFilter();
            lblCount.Text = $"{_all.Count} شركة";
            lblEmpty.Visibility = _all.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الشركات:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void ApplyFilter()
    {
        string q = txtSearch.Text?.Trim() ?? "";
        _view.Clear();
        foreach (var c in _all)
            if (q.Length == 0 ||
                c.CompanyName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (c.Phone?.Contains(q) ?? false))
                _view.Add(c);
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new CompanyEditorWindow { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: CompanyRow row }) return;
        var dlg = new CompanyEditorWindow(row.Id) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Suspend_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: CompanyRow row }) return;
        if (row.IsSuspended)
        {
            if (Confirm("إعادة تفعيل الشركة", $"ستعود شركة ({row.CompanyName}) للعمل. متابعة؟", "إعادة التفعيل"))
                await Run(() => CompanyService.SetSuspendedAsync(row.Id, row.CompanyName, false, null));
            return;
        }
        var prompt = new TextPromptWindow("إيقاف الشركة",
            $"سبب إيقاف شركة {row.CompanyName} (اختياري):") { Owner = Window.GetWindow(this) };
        if (prompt.ShowDialog() == true)
            await Run(() => CompanyService.SetSuspendedAsync(row.Id, row.CompanyName, true, prompt.Value));
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: CompanyRow row }) return;
        if (!Confirm("حذف الشركة",
                $"سيتم حذف شركة ({row.CompanyName}) نهائياً مع كل ما يرتبط بها من تراخيص وأجهزة ومستخدمين.\nلا يمكن التراجع.",
                "حذف", danger: true)) return;
        await Run(() => CompanyService.DeleteAsync(row.Id, row.CompanyName));
    }

    private async Task Run(Func<Task> action)
    {
        try { await action(); await LoadAsync(); }
        catch (Exception ex)
        {
            MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool Confirm(string title, string message, string confirmText = "تأكيد", bool danger = false) =>
        Dialogs.ConfirmWindow.Show(Window.GetWindow(this), title, message, confirmText, danger);
}
