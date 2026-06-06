using System.Windows;
using Newtonsoft.Json.Linq;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class ResellerEditorWindow : FluentWindow
{
    private readonly ResellerRow? _row;

    public ResellerEditorWindow(ResellerRow? row = null)
    {
        InitializeComponent();
        _row = row;
        if (row != null)
        {
            Title = "تعديل موزّع";
            titleBar.Title = "تعديل موزّع";
            txtName.Text = row.Name;
            txtPhone.Text = row.Phone;
            txtCity.Text = row.City;
            txtEmail.Text = row.Email;
            numCommission.Value = (double)row.CommissionRate;
            txtNotes.Text = row.Notes;
        }
        Loaded += (_, _) => txtName.Focus();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text)) { ShowErr("اسم الموزّع مطلوب."); return; }
        btnSave.IsEnabled = false; lblError.Visibility = Visibility.Collapsed;
        try
        {
            var payload = new JObject
            {
                ["name"]            = txtName.Text.Trim(),
                ["phone"]           = Empty(txtPhone.Text),
                ["city"]            = Empty(txtCity.Text),
                ["email"]           = Empty(txtEmail.Text),
                ["commission_rate"] = numCommission.Value ?? 0,
                ["notes"]           = Empty(txtNotes.Text),
            };
            if (_row == null) await ResellerService.CreateAsync(payload);
            else              await ResellerService.UpdateAsync(_row.Id, payload, txtName.Text.Trim());
            DialogResult = true; Close();
        }
        catch (Exception ex)
        {
            ShowErr(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message);
            btnSave.IsEnabled = true;
        }
    }

    private static JToken Empty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? JValue.CreateNull() : new JValue(s.Trim());

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void ShowErr(string m) { lblError.Text = "⚠ " + m; lblError.Visibility = Visibility.Visible; }
}
