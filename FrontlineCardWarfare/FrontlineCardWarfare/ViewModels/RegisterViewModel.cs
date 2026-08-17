using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для окна регистрации.
/// </summary>
public class RegisterViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isRegistering;
    private string _usernameValidation = string.Empty;
    private string _passwordValidation = string.Empty;
    private string _confirmPasswordValidation = string.Empty;

    public RegisterViewModel(IUserService userService, INavigationService navigationService)
    {
        _userService = userService;
        _navigationService = navigationService;
        BackToLoginCommand = new RelayCommand(BackToLogin);
    }

    public override string Title => "Регистрация";

    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value);
            ValidateUsername();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ValidatePassword();
            ValidateConfirmPassword();
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            SetProperty(ref _confirmPassword, value);
            ValidateConfirmPassword();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            SetProperty(ref _successMessage, value);
            OnPropertyChanged(nameof(HasSuccess));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

    public bool IsRegistering
    {
        get => _isRegistering;
        set => SetProperty(ref _isRegistering, value);
    }

    public ICommand BackToLoginCommand { get; }

    // Валидационные сообщения для отображения под полями
    public string UsernameValidation
    {
        get => _usernameValidation;
        set => SetProperty(ref _usernameValidation, value);
    }

    public string PasswordValidation
    {
        get => _passwordValidation;
        set => SetProperty(ref _passwordValidation, value);
    }

    public string ConfirmPasswordValidation
    {
        get => _confirmPasswordValidation;
        set => SetProperty(ref _confirmPasswordValidation, value);
    }

    public bool HasUsernameError => !string.IsNullOrWhiteSpace(UsernameValidation);
    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordValidation);
    public bool HasConfirmPasswordError => !string.IsNullOrWhiteSpace(ConfirmPasswordValidation);

    private void ValidateUsername()
    {
        if (string.IsNullOrWhiteSpace(Username))
            UsernameValidation = "Введите имя пользователя";
        else if (Username.Length < 3)
            UsernameValidation = "Минимум 3 символа";
        else if (Username.Length > 20)
            UsernameValidation = "Максимум 20 символов";
        else if (!System.Text.RegularExpressions.Regex.IsMatch(Username, @"^[a-zA-Z0-9_]+$"))
            UsernameValidation = "Только буквы, цифры и _";
        else
            UsernameValidation = string.Empty;

        OnPropertyChanged(nameof(HasUsernameError));
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(Password))
            PasswordValidation = "Введите пароль";
        else if (Password.Length < 6)
            PasswordValidation = "Минимум 6 символов";
        else
            PasswordValidation = string.Empty;

        OnPropertyChanged(nameof(HasPasswordError));
    }

    private void ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
            ConfirmPasswordValidation = "Подтвердите пароль";
        else if (ConfirmPassword != Password)
            ConfirmPasswordValidation = "Пароли не совпадают";
        else
            ConfirmPasswordValidation = string.Empty;

        OnPropertyChanged(nameof(HasConfirmPasswordError));
    }

    public bool CanRegister()
    {
        return Validator.ValidateUsername(Username) &&
               Validator.ValidatePassword(Password) &&
               Password == ConfirmPassword &&
               !IsRegistering;
    }

    public async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        // Очищаем старые ошибки перед валидацией
        UsernameValidation = string.Empty;
        PasswordValidation = string.Empty;
        ConfirmPasswordValidation = string.Empty;
        OnPropertyChanged(nameof(HasUsernameError));
        OnPropertyChanged(nameof(HasPasswordError));
        OnPropertyChanged(nameof(HasConfirmPasswordError));

        ValidateUsername();
        ValidatePassword();
        ValidateConfirmPassword();

        if (!CanRegister())
        {
            ErrorMessage = "Проверьте правильность заполнения всех полей";
            return;
        }

        IsRegistering = true;

        try
        {
            var result = await _userService.RegisterAsync(Username, Password);

            if (result.Success)
            {
                SuccessMessage = "Регистрация успешна! Теперь вы можете войти.";
            }
            else
            {
                ErrorMessage = result.Error ?? "Ошибка регистрации";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка регистрации: {ex.Message}";
        }
        finally
        {
            IsRegistering = false;
        }
    }

    public void ClearPasswords()
    {
        _password = string.Empty;
        _confirmPassword = string.Empty;
        _passwordValidation = string.Empty;
        _confirmPasswordValidation = string.Empty;
        OnPropertyChanged(nameof(Password));
        OnPropertyChanged(nameof(ConfirmPassword));
        OnPropertyChanged(nameof(PasswordValidation));
        OnPropertyChanged(nameof(ConfirmPasswordValidation));
        OnPropertyChanged(nameof(HasPasswordError));
        OnPropertyChanged(nameof(HasConfirmPasswordError));
    }

    private void BackToLogin(object? parameter)
    {
        _navigationService.NavigateTo<LoginViewModel>();
    }
}