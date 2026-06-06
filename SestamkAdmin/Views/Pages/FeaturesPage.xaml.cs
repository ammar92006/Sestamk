using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SestamkAdmin.Models;
using SestamkAdmin.Services;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace SestamkAdmin.Views.Pages;

public class FeatureGroup
{
    public string Category { get; set; } = "";
    public List<FeatureFlag> Features { get; set; } = new();
}

// FeatureFlag مع PlanBadges
public class FeatureFlagEx : FeatureFlag
{
    public string[] PlanBadges { get; set; } = Array.Empty<string>();
}

public partial class FeaturesPage : Page
{
    private readonly ObservableCollection<FeatureGroup> _groups = new();
    private readonly List<FeatureFlag> _flags = new();
    private string? _selectedCompanyId;

    public FeaturesPage()
    {
        InitializeComponent();
        groupsList.ItemsSource = _groups;
        Loaded += async (_, _) => await LoadCompaniesAsync();
    }

    private async Task LoadCompaniesAsync()
    {
        try
        {
            var companies = await CompanyService.GetAllAsync();
            cmbCompany.ItemsSource = companies;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل الشركات:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async void Company_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (cmbCompany.SelectedItem is not CompanyRow company) return;
        _selectedCompanyId = company.Id;
        loading.Visibility = Visibility.Visible;
        lblHint.Visibility = Visibility.Collapsed;
        _groups.Clear();
        _flags.Clear();

        // إظهار باقة الشركة
        ShowPlanBadge(company);

        try
        {
            var flags = await FeatureService.GetForCompanyAsync(company.Id);
            foreach (var f in flags) _flags.Add(f);

            // تجميع حسب الكاتيجوري
            var grouped = flags
                .GroupBy(f => f.Category ?? "أخرى")
                .OrderBy(g => g.Key)
                .Select(g => new FeatureGroup
                {
                    Category = g.Key,
                    Features = g.Select(f => (FeatureFlag)new FeatureFlagEx
                    {
                        Key         = f.Key,
                        DisplayName = f.DisplayName,
                        Description = f.Description,
                        Category    = f.Category,
                        IsEnabled   = f.IsEnabled,
                        RowId       = f.RowId,
                        PlanBadges  = f is FeatureFlagEx fx ? fx.PlanBadges : Array.Empty<string>()
                    }).ToList()
                });

            foreach (var g in grouped) _groups.Add(g);
            lblHint.Visibility = _groups.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل المميزات:\n{ex.Message}", "خطأ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally { loading.Visibility = Visibility.Collapsed; }
    }

    private void ShowPlanBadge(CompanyRow company)
    {
        // ابحث عن الترخيص النشط (ليس في الـ model حالياً، بس نعرض update_channel كـ indicator)
        planBadge.Visibility = Visibility.Visible;
        planText.Text = $"قناة: {company.UpdateChannel ?? "public"}";
        planBadge.Background = new SolidColorBrush(Color.FromArgb(0x28, 0x10, 0xB9, 0x81));
        planText.Foreground = (Brush)FindResource("BrandPrimaryBrush");
    }

    private async void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleSwitch { Tag: FeatureFlag flag } toggle) return;
        if (_selectedCompanyId == null) return;
        bool desired = toggle.IsChecked == true;
        try
        {
            await FeatureService.SetAsync(_selectedCompanyId, flag, desired);
            flag.IsEnabled = desired;
        }
        catch (Exception ex)
        {
            toggle.IsChecked = flag.IsEnabled;
            MessageBox.Show(ex is SupabaseException se ? $"{se.Message}\n{se.Details}" : ex.Message,
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
