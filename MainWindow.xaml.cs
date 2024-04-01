using Aniflex.Core;

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Aniflex;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool IsMaximizedAndDragStarted;

    public MainWindow()
    {
        InitializeComponent();

        Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        OnExit.Invoke();
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

            System.Windows.Point position = PointToScreen(e.MouseDevice.GetPosition(this));
            Top = position.Y;
            Left = position.X - RestoreBounds.Width / 2;

            WindowState = WindowState.Normal;

            DragMove();
        }
    }
}
