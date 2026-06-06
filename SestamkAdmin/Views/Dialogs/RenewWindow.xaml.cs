using System.Windows;
using System.Windows.Controls;
using SestamkAdmin.Models;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Dialogs;

public partial class RenewWindow : FluentWindow
{
    private readonly LicenseRow _row;
    public DateTime? NewExpiry { get; private set; }

    public RenewWindow(LicenseRow row)
    {
        InitializeComponent();
        _row = row;
        lblSerial.Text = row.SerialKey;
        lblCurrent.Text = row.DaysLeft < 0
            ? $"الترخيص منتهٍ منذ {-row.DaysLeft} يوم"
            : $"ينتهي حالياً في {row.ExpiresAt} ({row.DaysLeft} يوم متبقٍ)";
        Recalc();
    }

    private int Months =>
        int.TryParse((cmbDuration.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out var m) ? m : 12;

    private void Recalc()
    {
        if (_row == null) return; // يُستدعى أثناء التهيئة قبل تعيين _row

        // التجديد من تاريخ الانتهاء الحالي إن كان مستقبلاً، وإلا من اليوم
        DateTime baseDate = DateTime.TryParse(_row.ExpiresAt, out var cur) && cur.Date > DateTime.Today
            ? cur.Date
            : DateTime.Today;
        NewExpiry = baseDate.AddMonths(Months);
        if (lblNew != null) lblNew.Text = NewExpiry.Value.ToString("yyyy/MM/dd");
    }

    private void Changed(object sender, SelectionChangedEventArgs e) => Recalc();
    private void Cancel_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    private void Ok_Click(object sender, RoutedEventArgs e) { Recalc(); DialogResult = true; Close(); }
}
