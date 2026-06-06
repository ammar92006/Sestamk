using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class AnnouncementEditorWindow : FluentWindow
{
    public AnnouncementEditorWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => txtTitle.Focus();
    }

    private string Channel => (cmbChannel.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTitle.Text)) { ShowErr("العنوان مطلوب."); return; }
        if (string.IsNullOrWhiteSpace(txtBody.Text)) { ShowErr("النص مطلوب."); return; }
        btnSave.IsEnabled = false; lblError.Visibility = Visibility.Collapsed;
        try
        {
            var payload = new JObject
            {
                ["title"]          = txtTitle.Text.Trim(),
                ["body"]           = txtBody.Text.Trim(),
                ["target_channel"] = string.IsNullOrEmpty(Channel) ? JValue.CreateNull() : new JValue(Channel),
                ["show_until"]     = dpUntil.SelectedDate.HasValue
                    ? new JValue(dpUntil.SelectedDate.Value.ToString("yyyy-MM-dd")) : JValue.CreateNull(),
                ["is_active"]      = true,
                ["created_by"]     = AppSession.AdminId,
            };
            await AnnouncementService.CreateAsync(payload);
            DialogResult = true; Close();
        }
        catch (Exception ex)
        {
            ShowErr(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message);
            btnSave.IsEnabled = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void ShowErr(string m) { lblError.Text = "⚠ " + m; lblError.Visibility = Visibility.Visible; }
}
