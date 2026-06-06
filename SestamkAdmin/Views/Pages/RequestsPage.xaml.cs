using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class RequestsPage : Page
{
    private readonly ObservableCollection<RequestRow> _view = new();

    public RequestsPage()
    {
        InitializeComponent();
        grid.ItemsSource = _view;
        Loaded += async (_, _) => await LoadAsync();
    }

    private string Filter =>
        (cmbFilter.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "pending";

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        try
        {
            string q = "select=*,companies(company_name)&order=created_at.desc&limit=200";
            if (Filter != "all") q += $"&status=eq.{Filter}";
            var arr = await SupabaseService.Instance.SelectAsync("requests", q);
            _view.Clear();
            foreach (var r in arr) _view.Add(RequestRow.From(r));
            lblCount.Text = $"{_view.Count} طلب";
            lblEmpty.Visibility = _view.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الطلبات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Filter_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) _ = LoadAsync();
    }
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void Approve_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: RequestRow row }) return;
        await Handle(row, "approved", "قبول");
    }

    private async void Reject_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: RequestRow row }) return;
        await Handle(row, "rejected", "رفض");
    }

    private async Task Handle(RequestRow row, string status, string verb)
    {
        var prompt = new TextPromptWindow($"{verb} الطلب",
            $"ملاحظات الرد على طلب «{row.TypeDisplay}» من {row.CompanyName} (اختياري):")
        { Owner = Window.GetWindow(this) };
        if (prompt.ShowDialog() != true) return;

        try
        {
            await SupabaseService.Instance.UpdateAsync("requests", $"id=eq.{row.Id}", new
            {
                status,
                response_notes = string.IsNullOrWhiteSpace(prompt.Value) ? null : prompt.Value,
                handled_by = AppSession.AdminId,
                handled_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow
            });
            await AuditLog.WriteAsync("request", row.Id, status, $"{verb} طلب {row.TypeDisplay} من {row.CompanyName}");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
