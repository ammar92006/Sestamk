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

public partial class DevicesPage : Page
{
    private readonly ObservableCollection<DeviceRow> _all = new();
    private readonly ObservableCollection<DeviceRow> _view = new();

    public DevicesPage()
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
            var list = await DeviceService.GetAllAsync();
            _all.Clear();
            foreach (var d in list) _all.Add(d);
            ApplyFilter();
            lblCount.Text = $"{_all.Count} جهاز";
            lblEmpty.Visibility = _all.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الأجهزة:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void ApplyFilter()
    {
        string q = txtSearch.Text?.Trim() ?? "";
        _view.Clear();
        foreach (var d in _all)
            if (q.Length == 0 || d.Hwid.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.CompanyName.Contains(q, StringComparison.OrdinalIgnoreCase))
                _view.Add(d);
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private void CopyHwid_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string hwid } || string.IsNullOrEmpty(hwid)) return;
        try
        {
            Clipboard.SetText(hwid);
            if (sender is Wpf.Ui.Controls.Button btn)
            {
                btn.Icon = new Wpf.Ui.Controls.SymbolIcon { Symbol = Wpf.Ui.Controls.SymbolRegular.Checkmark24 };
                btn.ToolTip = "تم النسخ ✓";
            }
        }
        catch { }
    }

    private async void Block_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DeviceRow row }) return;
        if (row.IsBlocked)
        {
            if (Confirm("فك حظر الجهاز", $"سيُسمح للجهاز ({row.ShortHwid}) بالعمل مرة أخرى. متابعة؟", "فك الحظر"))
                await Run(() => DeviceService.SetBlockedAsync(row.Id, row.Hwid, false, null));
            return;
        }
        var prompt = new TextPromptWindow("حظر الجهاز",
            $"سبب حظر الجهاز {row.ShortHwid} (اختياري):") { Owner = Window.GetWindow(this) };
        if (prompt.ShowDialog() == true)
            await Run(() => DeviceService.SetBlockedAsync(row.Id, row.Hwid, true, prompt.Value));
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: DeviceRow row }) return;
        if (!Confirm("فك ربط الجهاز",
                $"سيتم فك ربط الجهاز ({row.ShortHwid}) نهائياً من الشركة، وسيُسمح بتسجيل جهاز جديد بدلاً منه.\nلا يمكن التراجع عن هذا الإجراء.",
                "فك الربط", danger: true)) return;
        await Run(() => DeviceService.DeleteAsync(row.Id, row.Hwid));
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
