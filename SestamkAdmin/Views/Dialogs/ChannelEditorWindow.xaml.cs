using System.Windows;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class ChannelEditorWindow : FluentWindow
{
    private readonly ChannelRow? _row;

    public ChannelEditorWindow(ChannelRow? row = null)
    {
        InitializeComponent();
        _row = row;
        if (row != null)
        {
            Title = "تعديل قناة";
            titleBar.Title = "تعديل قناة";
            txtName.Text = row.ChannelName;
            txtDesc.Text = row.Description;
        }
        Loaded += (_, _) => txtName.Focus();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        string name = txtName.Text.Trim().ToLower();
        string desc = txtDesc.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) { ShowErr("اسم القناة مطلوب."); return; }
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-z0-9_]{2,30}$"))
        { ShowErr("الاسم: حروف إنجليزية صغيرة وأرقام وـ فقط (2-30 حرف)."); return; }

        btnSave.IsEnabled = false;
        lblError.Visibility = Visibility.Collapsed;
        try
        {
            if (_row == null) await ChannelService.CreateAsync(name, desc);
            else              await ChannelService.UpdateAsync(_row.Id, name, desc);
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
