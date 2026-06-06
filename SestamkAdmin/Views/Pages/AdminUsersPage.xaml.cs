using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;
using SestamkAdmin.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public partial class AdminUsersPage : Page
{
    private readonly ObservableCollection<AdminUserRow> _list = new();

    public AdminUsersPage()
    {
        InitializeComponent();
        grid.ItemsSource = _list;
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loading.Visibility = Visibility.Visible;
        try
        {
            var data = await AdminUsersService.GetAllAsync();
            _list.Clear();
            foreach (var u in data) _list.Add(u);
            lblCount.Text = $"{_list.Count} مشرف";
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل المشرفين:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AdminUserEditorWindow { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true) await LoadAsync();
    }

    private async void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: AdminUserRow row }) return;
        if (row.Id == AppSession.AdminId) { MessageBox.Show("لا يمكن تعطيل حسابك الحالي.", "تنبيه"); return; }
        bool desired = !row.IsActive;
        if (!ConfirmWindow.Show(Window.GetWindow(this),
                $"{(desired ? "تفعيل" : "إيقاف")} المشرف",
                $"هل تريد {(desired ? "تفعيل" : "إيقاف")} المشرف {row.FullName}؟",
                desired ? "تفعيل" : "إيقاف")) return;
        try { await AdminUsersService.SetActiveAsync(row.Id, row.Username, desired); await LoadAsync(); }
        catch (Exception ex) { ShowErr(ex); }
    }

    private async void ChangeRole_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: AdminUserRow row }) return;
        var roles = new[] { "super_admin", "admin", "support" };
        var labels = new[] { "مالك (super_admin)", "مشرف (admin)", "دعم فني (support)" };

        var dlg = new ListPickerWindow("تغيير الدور",
            $"اختر الدور الجديد للمشرف {row.FullName}:",
            labels, Array.IndexOf(roles, row.Role))
        { Owner = Window.GetWindow(this) };
        if (dlg.ShowDialog() == true && dlg.SelectedIndex >= 0)
        {
            try { await AdminUsersService.SetRoleAsync(row.Id, row.Username, roles[dlg.SelectedIndex]); await LoadAsync(); }
            catch (Exception ex) { ShowErr(ex); }
        }
    }

    private async void ChangePass_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: AdminUserRow row }) return;
        var dlg = new ChangePasswordWindow(row) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
    }

    private static void ShowErr(Exception ex) =>
        MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
            "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
}
