using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = System.Windows.MessageBoxResult;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class LicensesPage : Page
{
    private readonly ObservableCollection<LicenseRow> _all = new();
    private readonly ObservableCollection<LicenseRow> _view = new();

    public LicensesPage()
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
            var list = await LicenseService.GetLicensesAsync();
            _all.Clear();
            foreach (var l in list) _all.Add(l);
            ApplyFilter();
            lblCount.Text = $"{_all.Count} ترخيص";
            lblEmpty.Visibility = _all.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل التراخيص:\n{ex.Message}\n\n{ex.Details}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            loading.Visibility = Visibility.Collapsed;
        }
    }

    private void ApplyFilter()
    {
        string q = txtSearch.Text?.Trim() ?? "";
        _view.Clear();
        foreach (var l in _all)
        {
            if (q.Length == 0 ||
                l.SerialKey.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                l.CompanyName.Contains(q, StringComparison.OrdinalIgnoreCase))
                _view.Add(l);
        }
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string serial)
        {
            try { Clipboard.SetText(serial); } catch { }
        }
    }

    private async void NewLicense_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new LicenseEditorWindow { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true)
            await LoadAsync();
    }

    private void History_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LicenseRow row }) return;
        new LicenseHistoryWindow(row) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private async void Renew_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LicenseRow row }) return;
        var dlg = new RenewWindow(row) { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true && dlg.NewExpiry.HasValue)
        {
            try
            {
                await LicenseService.RenewAsync(row.Id, row.SerialKey, dlg.NewExpiry.Value);
                await LoadAsync();
            }
            catch (Exception ex) { ShowErr(ex); }
        }
    }

    private async void Suspend_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LicenseRow row }) return;

        if (row.IsSuspended)
        {
            if (Confirm("إعادة تفعيل الترخيص", $"سيعود الترخيص ({row.SerialKey}) للعمل. متابعة؟", "إعادة التفعيل"))
            {
                try { await LicenseService.SetSuspendedAsync(row.Id, row.SerialKey, false, null); await LoadAsync(); }
                catch (Exception ex) { ShowErr(ex); }
            }
            return;
        }

        var prompt = new TextPromptWindow("إيقاف الترخيص",
            $"سبب إيقاف الترخيص {row.SerialKey} (اختياري):") { Owner = Window.GetWindow(this) };
        if (prompt.ShowDialog() == true)
        {
            try { await LicenseService.SetSuspendedAsync(row.Id, row.SerialKey, true, prompt.Value); await LoadAsync(); }
            catch (Exception ex) { ShowErr(ex); }
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LicenseRow row }) return;
        if (!Confirm("حذف الترخيص", $"سيتم حذف الترخيص ({row.SerialKey}) نهائياً.\nلا يمكن التراجع عن هذا الإجراء.", "حذف", danger: true)) return;
        try { await LicenseService.DeleteAsync(row.Id, row.SerialKey); await LoadAsync(); }
        catch (Exception ex) { ShowErr(ex); }
    }

    private bool Confirm(string title, string message, string confirmText = "تأكيد", bool danger = false) =>
        Dialogs.ConfirmWindow.Show(Window.GetWindow(this), title, message, confirmText, danger);

    private static void ShowErr(Exception ex) =>
        MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
            "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
}
