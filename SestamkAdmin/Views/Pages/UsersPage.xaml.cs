using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace SestamkAdmin.Views.Pages;

public partial class UsersPage : Page
{
    private readonly ObservableCollection<UserRow> _all = new();
    private readonly ObservableCollection<UserRow> _view = new();

    public UsersPage()
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
            var arr = await SupabaseService.Instance.SelectAsync("users",
                "select=*,companies(company_name)&order=created_at.desc");
            _all.Clear();
            foreach (var u in arr) _all.Add(UserRow.From(u));
            ApplyFilter();
            lblCount.Text = $"{_all.Count} مستخدم";
            lblEmpty.Visibility = _all.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل المستخدمين:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void ApplyFilter()
    {
        string q = txtSearch.Text?.Trim() ?? "";
        _view.Clear();
        foreach (var u in _all)
            if (q.Length == 0 || u.FullName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.Username.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.CompanyName.Contains(q, StringComparison.OrdinalIgnoreCase))
                _view.Add(u);
    }

    private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();
    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: UserRow row }) return;
        bool newState = !row.IsActive;
        if (MessageBox.Show($"{(newState ? "تفعيل" : "إيقاف")} المستخدم {row.FullName}؟", "تأكيد",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            await SupabaseService.Instance.UpdateAsync("users", $"id=eq.{row.Id}",
                new { is_active = newState, updated_at = DateTime.UtcNow });
            await AuditLog.WriteAsync("user", row.Id, newState ? "enable" : "disable",
                $"{(newState ? "تفعيل" : "إيقاف")} مستخدم {row.Username}");
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
