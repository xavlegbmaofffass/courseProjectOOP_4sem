using System.Windows.Controls;
using System.Windows;
using FrontlineCardWarfare.ViewModels;

namespace FrontlineCardWarfare.Views;

/// <summary>
/// Логика взаимодействия для ProfileView.xaml
/// </summary>
public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
        CurrentPasswordBox.PasswordChanged += CurrentPasswordBox_PasswordChanged;
        NewPasswordBox.PasswordChanged += NewPasswordBox_PasswordChanged;
        ConfirmPasswordBox.PasswordChanged += ConfirmPasswordBox_PasswordChanged;
    }

    private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
        {
            vm.GetType().GetProperty("CurrentPassword")?.SetValue(vm, CurrentPasswordBox.Password);
        }
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
        {
            vm.GetType().GetProperty("NewPassword")?.SetValue(vm, NewPasswordBox.Password);
        }
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProfileViewModel vm)
        {
            vm.GetType().GetProperty("ConfirmPassword")?.SetValue(vm, ConfirmPasswordBox.Password);
        }
    }
}
