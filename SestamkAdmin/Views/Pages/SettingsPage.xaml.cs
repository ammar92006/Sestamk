using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Services;

namespace SestamkAdmin.Views.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        var c = AdminConfig.Load();
        txtSupabaseUrl.Text = c.SupabaseUrl;
        txtAnonKey.Text = c.SupabaseAnonKey;
        txtGhOwner.Text = c.GitHubOwner;
        txtGhRepo.Text = c.GitHubRepo;
        txtGhToken.Password = c.GitHubToken;
        txtBuildPath.Text = c.BuildFolderPath;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        AdminConfig.Save(new ConfigData
        {
            SupabaseUrl = txtSupabaseUrl.Text.Trim(),
            SupabaseAnonKey = txtAnonKey.Text.Trim(),
            GitHubOwner = txtGhOwner.Text.Trim(),
            GitHubRepo = txtGhRepo.Text.Trim(),
            GitHubToken = txtGhToken.Password,
            BuildFolderPath = txtBuildPath.Text.Trim()
        });
        lblSaved.Text = "✓ تم الحفظ";
    }
}
