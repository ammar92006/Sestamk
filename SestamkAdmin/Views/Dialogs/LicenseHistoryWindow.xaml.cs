using System.Collections.ObjectModel;
using System.Windows;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class LicenseHistoryWindow : FluentWindow
{
    private readonly ObservableCollection<HistoryRow> _list = new();
    private readonly LicenseRow _row;

    public LicenseHistoryWindow(LicenseRow row)
    {
        InitializeComponent();
        _row = row;
        lblSerial.Text = row.SerialKey;
        list.ItemsSource = _list;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        lblEmpty.Visibility = Visibility.Collapsed;
        try
        {
            var data = await HistoryService.GetForLicenseAsync(_row.Id);
            _list.Clear();
            foreach (var h in data) _list.Add(h);
            lblEmpty.Visibility = _list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch { lblEmpty.Visibility = Visibility.Visible; }
        finally { loading.Visibility = Visibility.Collapsed; }
    }
}
