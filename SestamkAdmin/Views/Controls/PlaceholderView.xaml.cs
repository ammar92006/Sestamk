using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace SestamkAdmin.Views.Controls;

public partial class PlaceholderView : UserControl
{
    public PlaceholderView()
    {
        InitializeComponent();
    }

    public void Configure(string title, string description, SymbolRegular icon, string? body = null)
    {
        HeaderTitle.Text = title;
        HeaderDesc.Text = description;
        HeaderIcon.Symbol = icon;
        BigIcon.Symbol = icon;
        if (body != null)
            BodyText.Text = body;
    }
}
