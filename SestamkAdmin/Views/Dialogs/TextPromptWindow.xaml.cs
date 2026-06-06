using System.Windows;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class TextPromptWindow : FluentWindow
{
    public string Value => txtInput.Text.Trim();

    public TextPromptWindow(string title, string message, string initial = "")
    {
        InitializeComponent();
        Title = title;
        titleBar.Title = title;
        lblMessage.Text = message;
        txtInput.Text = initial;
        Loaded += (_, _) => txtInput.Focus();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void Ok_Click(object sender, RoutedEventArgs e) { DialogResult = true; Close(); }
}
