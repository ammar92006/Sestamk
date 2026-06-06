using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using SestamkAdmin.Services;

namespace SestamkAdmin.Views.Pages;

public partial class DashboardPage : Page
{
    public class ActivityItem
    {
        public string Message { get; set; } = "";
        public string When { get; set; } = "";
    }

    private readonly ObservableCollection<ActivityItem> _activity = new();

    public DashboardPage()
    {
        InitializeComponent();
        activityList.ItemsSource = _activity;
        lblWelcome.Text = $"أهلاً، {AppSession.AdminName ?? "بك"} 👋";
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loadingActivity.Visibility = Visibility.Visible;
        lblNoActivity.Visibility = Visibility.Collapsed;
        try
        {
            var db = SupabaseService.Instance;

            // إحصائيات شاملة من دالة واحدة (أسرع)
            var s = await DashboardService.GetStatsAsync();

            statCompanies.Text = s.CompaniesTotal.ToString();
            statCompaniesSub.Text = $"{s.CompaniesActive} نشطة";

            statLicenses.Text = s.LicensesActive.ToString();
            statLicensesSub.Text = s.LicensesExpiring30 > 0
                ? $"⚠ {s.LicensesExpiring30} ينتهي خلال شهر"
                : $"من إجمالي {s.LicensesTotal}";

            statDevices.Text = s.DevicesTotal.ToString();
            statDevicesSub.Text = s.DevicesBlocked > 0 ? $"{s.DevicesBlocked} محظور" : "لا حظر";

            statRequests.Text = s.RequestsPending.ToString();
            statRequestsSub.Text = "بانتظار الرد";

            // الإيرادات + التنبيهات
            statRevenue.Text = s.RevenueTotal.ToString("#,0.##");
            statRevenueSub.Text = "إجمالي قيمة التراخيص";
            statExpiring.Text = s.LicensesExpiring7.ToString();
            statExpiringSub.Text = s.LicensesExpired > 0 ? $"{s.LicensesExpired} منتهٍ بالفعل" : "خلال 7 أيام";

            // آخر نشاط من sync_logs
            _activity.Clear();
            try
            {
                var logs = await db.SelectAsync("sync_logs",
                    "select=sync_type,status,app_version,created_at&order=created_at.desc&limit=8");
                foreach (var log in logs)
                {
                    _activity.Add(new ActivityItem
                    {
                        Message = $"{TranslateSync(log["sync_type"]?.ToString())} — إصدار {log["app_version"]}",
                        When = FormatTime(log["created_at"]?.ToString())
                    });
                }
            }
            catch { /* sync_logs قد يكون فارغاً */ }

            lblNoActivity.Visibility = _activity.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (SupabaseException ex)
        {
            MessageBox.Show($"تعذّر تحميل البيانات:\n{ex.Message}\n\n{ex.Details}",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            loadingActivity.Visibility = Visibility.Collapsed;
        }
    }

    private static string TranslateSync(string? type) => type switch
    {
        "startup_check" => "فحص بدء التشغيل",
        "update_check" => "فحص تحديث",
        "update_download" => "تنزيل تحديث",
        "update_applied" => "تطبيق تحديث",
        "login" => "تسجيل دخول",
        "heartbeat" => "نبضة اتصال",
        _ => type ?? "نشاط"
    };

    private static string FormatTime(string? iso)
    {
        if (DateTime.TryParse(iso, out var dt))
            return dt.ToLocalTime().ToString("yyyy/MM/dd HH:mm");
        return "";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => _ = LoadAsync();

    private void Navigate(Type page)
    {
        if (Application.Current.MainWindow is MainWindow mw)
            mw.RootNavigation.Navigate(page);
    }

    private void QuickNewLicense_Click(object sender, RoutedEventArgs e) => Navigate(typeof(LicensesPage));
    private void QuickNewCompany_Click(object sender, RoutedEventArgs e) => Navigate(typeof(CompaniesPage));
    private void QuickPublish_Click(object sender, RoutedEventArgs e) => Navigate(typeof(UpdatesPage));
    private void QuickRequests_Click(object sender, RoutedEventArgs e) => Navigate(typeof(RequestsPage));
}
