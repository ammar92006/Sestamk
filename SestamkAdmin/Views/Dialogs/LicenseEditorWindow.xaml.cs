using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace SestamkAdmin.Views.Dialogs;

public partial class LicenseEditorWindow : FluentWindow
{
    private List<CompanyRow> _companies = new();

    public LicenseEditorWindow()
    {
        InitializeComponent();
        dpStart.SelectedDate = DateTime.Today;
        txtSerial.Text = LicenseService.GenerateSerial();
        Loaded += async (_, _) => await LoadCompaniesAsync();
        UpdateExpiry();
    }

    private async Task LoadCompaniesAsync()
    {
        try
        {
            _companies = await LicenseService.GetCompaniesAsync();
            cmbCompany.ItemsSource = _companies;
            if (_companies.Count > 0) cmbCompany.SelectedIndex = 0;
            lblNoCompany.Visibility = _companies.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            btnSave.IsEnabled = _companies.Count > 0;
        }
        catch (Exception ex)
        {
            ShowError(ex is SupabaseException se ? se.Message : ex.Message);
        }
    }

    private int SelectedMonths =>
        int.TryParse((cmbDuration.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out var m) ? m : 12;

    private string SelectedPlan =>
        (cmbPlan.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "basic";

    private DateTime StartDate => dpStart.SelectedDate ?? DateTime.Today;
    private DateTime ExpiryDate => StartDate.AddMonths(SelectedMonths);

    private void UpdateExpiry()
    {
        if (lblExpiry != null)
            lblExpiry.Text = $"ينتهي الاشتراك في: {ExpiryDate:yyyy/MM/dd}  ({SelectedMonths} شهر)";
    }

    private void Duration_Changed(object sender, SelectionChangedEventArgs e) => UpdateExpiry();

    private void Plan_Changed(object sender, SelectionChangedEventArgs e)
    {
        // قيم افتراضية حسب الباقة
        switch (SelectedPlan)
        {
            case "trial":
                Set(numUsers, 2); Set(numDevices, 1); Set(numBranches, 1); break;
            case "basic":
                Set(numUsers, 5); Set(numDevices, 3); Set(numBranches, 1); break;
            case "pro":
                Set(numUsers, 15); Set(numDevices, 8); Set(numBranches, 3); break;
            case "vip":
                Set(numUsers, 50); Set(numDevices, 25); Set(numBranches, 10); break;
        }
    }

    private static void Set(NumberBox? box, double v) { if (box != null) box.Value = v; }

    private void Regen_Click(object sender, RoutedEventArgs e) => txtSerial.Text = LicenseService.GenerateSerial();

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (cmbCompany.SelectedItem is not CompanyRow company)
        {
            ShowError("اختر الشركة أولاً.");
            return;
        }

        btnSave.IsEnabled = false;
        lblError.Visibility = Visibility.Collapsed;
        try
        {
            decimal? price = numPrice.Value is > 0 ? (decimal)numPrice.Value : null;
            string currency = (cmbCurrency.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "EGP";

            await LicenseService.CreateLicenseAsync(
                company.Id, txtSerial.Text, SelectedPlan,
                (int)(numUsers.Value ?? 0), (int)(numDevices.Value ?? 0), (int)(numBranches.Value ?? 0),
                StartDate, ExpiryDate,
                SelectedPlan == "trial", price, currency);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message);
            btnSave.IsEnabled = true;
        }
    }

    private void ShowError(string msg)
    {
        lblError.Text = "⚠ " + msg;
        lblError.Visibility = Visibility.Visible;
    }
}
