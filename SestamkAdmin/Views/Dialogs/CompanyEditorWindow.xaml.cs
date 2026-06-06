using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace SestamkAdmin.Views.Dialogs;

public partial class CompanyEditorWindow : FluentWindow
{
    private readonly string? _id;
    private bool IsEdit => _id != null;

    public CompanyEditorWindow(string? id = null)
    {
        InitializeComponent();
        _id = id;
        if (IsEdit)
        {
            Title = "تعديل شركة";
            titleBar.Title = "تعديل شركة";
            Loaded += async (_, _) => await LoadAsync();
        }
        else
        {
            dpStart.SelectedDate = DateTime.Today;
            dpEnd.SelectedDate = DateTime.Today.AddYears(1);
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            var c = await CompanyService.GetByIdAsync(_id!);
            txtName.Text = c["company_name"]?.ToString();
            txtPhone.Text = c["phone"]?.ToString();
            txtEmail.Text = c["email"]?.ToString();
            txtCity.Text = c["city"]?.ToString();
            txtContact.Text = c["contact_name"]?.ToString();
            txtNotes.Text = c["notes"]?.ToString();
            SelectCombo(cmbChannel, c["update_channel"]?.ToString());
            SelectCombo(cmbSupport, c["support_level"]?.ToString());
            if (DateTime.TryParse(c["contract_start"]?.ToString(), out var s)) dpStart.SelectedDate = s;
            if (DateTime.TryParse(c["contract_end"]?.ToString(), out var en)) dpEnd.SelectedDate = en;
        }
        catch (Exception ex)
        {
            ShowError(ex is SupabaseException se ? se.Message : ex.Message);
        }
    }

    private static void SelectCombo(ComboBox box, string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        foreach (ComboBoxItem item in box.Items)
            if (item.Content?.ToString() == value) { box.SelectedItem = item; return; }
    }

    private static string Combo(ComboBox box) =>
        (box.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            ShowError("اسم الشركة مطلوب.");
            return;
        }

        btnSave.IsEnabled = false;
        lblError.Visibility = Visibility.Collapsed;
        try
        {
            var payload = new JObject
            {
                ["company_name"] = txtName.Text.Trim(),
                ["phone"] = Empty(txtPhone.Text),
                ["email"] = Empty(txtEmail.Text),
                ["city"] = Empty(txtCity.Text),
                ["contact_name"] = Empty(txtContact.Text),
                ["notes"] = Empty(txtNotes.Text),
                ["update_channel"] = Combo(cmbChannel),
                ["support_level"] = Combo(cmbSupport),
                ["contract_start"] = dpStart.SelectedDate?.ToString("yyyy-MM-dd"),
                ["contract_end"] = dpEnd.SelectedDate?.ToString("yyyy-MM-dd"),
            };

            if (IsEdit)
            {
                await CompanyService.UpdateAsync(_id!, payload, txtName.Text.Trim());
            }
            else
            {
                payload["created_by"] = AppSession.AdminId;
                await CompanyService.CreateAsync(payload);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            ShowError(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message);
            btnSave.IsEnabled = true;
        }
    }

    private static JToken Empty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? JValue.CreateNull() : new JValue(s.Trim());

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }

    private void ShowError(string msg)
    {
        lblError.Text = "⚠ " + msg;
        lblError.Visibility = Visibility.Visible;
    }
}
