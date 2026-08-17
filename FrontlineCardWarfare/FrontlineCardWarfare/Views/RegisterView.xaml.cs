using System.Windows;
using System.Windows.Controls;
using FrontlineCardWarfare.ViewModels;

namespace FrontlineCardWarfare.Views;

/// <summary>
/// Interaction logic for RegisterView.xaml
/// </summary>
public partial class RegisterView : UserControl
{
    public RegisterView()
    {
        InitializeComponent();
        Loaded += RegisterView_Loaded;
    }

    private void RegisterView_Loaded(object sender, RoutedEventArgs e)
    {
        PasswordBox.PasswordChanged += OnPasswordChanged;
        ConfirmPasswordBox.PasswordChanged += OnPasswordChanged;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
        {
            if (sender == PasswordBox)
                vm.Password = PasswordBox.Password;
            else if (sender == ConfirmPasswordBox)
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
        {
            vm.Password = PasswordBox.Password;
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
            await vm.RegisterAsync();

            if (vm.HasSuccess)
            {
                // Отписываемся от событий, чтобы очистка не вызвала валидацию
                PasswordBox.PasswordChanged -= OnPasswordChanged;
                ConfirmPasswordBox.PasswordChanged -= OnPasswordChanged;
                
                PasswordBox.Password = string.Empty;
                ConfirmPasswordBox.Password = string.Empty;
                vm.ClearPasswords();
                
                PasswordBox.PasswordChanged += OnPasswordChanged;
                ConfirmPasswordBox.PasswordChanged += OnPasswordChanged;
            }
        }
    }
}
