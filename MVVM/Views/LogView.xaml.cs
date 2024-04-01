using System.Windows.Controls;

namespace Aniflex.MVVM.Views;

/// <summary>
/// Interaction logic for LogView.xaml
/// </summary>
public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
    }

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer || e.ExtentHeightChange <= 0)
            return;

        scrollViewer.ScrollToEnd();
    }
}
