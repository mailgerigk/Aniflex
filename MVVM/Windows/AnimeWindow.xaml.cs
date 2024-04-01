using System.Windows;
using System.Windows.Input;

namespace Aniflex.MVVM.Windows;

/// <summary>
/// Interaction logic for AnimeWindow.xaml
/// </summary>
public partial class AnimeWindow : Window
{
    private bool IsMaximizedAndDragStarted;

    public AnimeWindow()
    {
        InitializeComponent();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            IsMaximizedAndDragStarted = true;
        }
        DragMove();
    }

    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        IsMaximizedAndDragStarted = false;
    }

    private void Border_MouseMove(object sender, MouseEventArgs e)
    {
        if (IsMaximizedAndDragStarted)
        {
            IsMaximizedAndDragStarted = false;

            Point position = PointToScreen(e.MouseDevice.GetPosition(this));
            Top = position.Y;
            Left = position.X - RestoreBounds.Width / 2;

            WindowState = WindowState.Normal;

            DragMove();
        }
    }
}
