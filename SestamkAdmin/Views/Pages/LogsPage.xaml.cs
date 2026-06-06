using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class LogsPage : Page
{
    public class SyncRow
    {
        public string When { get; set; } = "";
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public string Version { get; set; } = "";
        public string Message { get; set; } = "";
    }

    private readonly ObservableCollection<AuditRow> _audit = new();
    private readonly ObservableCollection<SyncRow> _sync = new();

    public LogsPage()
    {
        InitializeComponent();
        gridAudit.ItemsSource = _audit;
        gridSync.ItemsSource = _sync;
        Loaded += async (_, _) => await LoadAsync();
    }

    private bool IsAudit =>
        (cmbType.SelectedItem as ComboBoxItem)?.Tag?.ToString() != "sync";

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        gridAudit.Visibility = IsAudit ? Visibility.Visible : Visibility.Collapsed;
        gridSync.Visibility = IsAudit ? Visibility.Collapsed : Visibility.Visible;
        try
        {
            if (IsAudit)
            {
                var arr = await SupabaseService.Instance.SelectAsync("audit_logs",
                    "select=*&order=created_at.desc&limit=300");
                _audit.Clear();
                foreach (var a in arr) _audit.Add(AuditRow.From(a));
                lblEmpty.Visibility = _audit.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                var arr = await SupabaseService.Instance.SelectAsync("sync_logs",
                    "select=sync_type,status,app_version,message,created_at&order=created_at.desc&limit=300");
                _sync.Clear();
                foreach (var s in arr)
                    _sync.Add(new SyncRow
                    {
                        When = DateTime.TryParse(s["created_at"]?.ToString(), out var d)
                            ? d.ToLocalTime().ToString("yyyy/MM/dd HH:mm") : "",
                        Type = s["sync_type"]?.ToString() ?? "",
                        Status = s["status"]?.ToString() ?? "",
                        Version = s["app_version"]?.ToString() ?? "",
                        Message = s["message"]?.ToString() ?? ""
                    });
                lblEmpty.Visibility = _sync.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل السجلات:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Type_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) _ = LoadAsync();
    }
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();
}
