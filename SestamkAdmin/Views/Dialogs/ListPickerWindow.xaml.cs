using System.Windows;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class ListPickerWindow : FluentWindow
{
    public int SelectedIndex => listBox.SelectedIndex;

    public ListPickerWindow(string title, string message, string[] items, int defaultIndex = 0)
    {
        InitializeComponent();
        titleBar.Title = title;
        lblMessage.Text = message;
        foreach (var item in items)
            listBox.Items.Add(new System.Windows.Controls.ListBoxItem { Content = item, Padding = new Thickness(8, 6, 8, 6) });
        if (defaultIndex >= 0 && defaultIndex < items.Length)
            listBox.SelectedIndex = defaultIndex;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (listBox.SelectedIndex < 0) return;
        DialogResult = true; Close();
    }
}
