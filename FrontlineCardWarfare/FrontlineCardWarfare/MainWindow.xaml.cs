using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FrontlineCardWarfare.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FrontlineCardWarfare;

public partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(INavigationService navigationService)
    {
        InitializeComponent();
        _navigationService = navigationService;
        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        // Если уже есть текущая ViewModel, отображаем её
        if (_navigationService.CurrentViewModel != null)
        {
            MainContent.Content = _navigationService.CurrentViewModel;
        }
    }

    private void OnCurrentViewModelChanged(object? sender, ViewModels.ViewModelBase viewModel)
    {
        MainContent.Content = viewModel;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    private void MinimizeButton_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
    private void MaximizeButton_Click(object sender, RoutedEventArgs e) { WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; }
    private void CloseButton_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
    private void WindowControlButton_MouseEnter(object sender, MouseEventArgs e) { if (sender is System.Windows.Controls.Button btn) btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E2E4A")); }
    private void WindowControlButton_MouseLeave(object sender, MouseEventArgs e) { if (sender is System.Windows.Controls.Button btn) btn.Background = Brushes.Transparent; }
    private void CloseButton_MouseEnter(object sender, MouseEventArgs e) { if (sender is System.Windows.Controls.Button btn) btn.Background = Brushes.Red; }
    private void CloseButton_MouseLeave(object sender, MouseEventArgs e) { if (sender is System.Windows.Controls.Button btn) btn.Background = Brushes.Transparent; }
}
