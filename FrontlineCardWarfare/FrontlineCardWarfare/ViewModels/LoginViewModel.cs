using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для окна авторизации.
/// </summary>
public class LoginViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoggingIn;
    private string _usernameValidation = string.Empty;
    private string _passwordValidation = string.Empty;

    /// <summary>
    /// Инициализирует новый экземпляр LoginViewModel.
    /// </summary>
    public LoginViewModel(IUserService userService, INavigationService navigationService)
    {
        _userService = userService;
        _navigationService = navigationService;

        LoginCommand = new AsyncRelayCommand(LoginAsync);
        RegisterCommand = new RelayCommand(NavigateToRegister);
        GuestLoginCommand = new RelayCommand(GuestLogin);
        BackToMenuCommand = new RelayCommand(BackToMenu);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Авторизация";

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value);
            ValidateUsername();
        }
    }

    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            ValidatePassword();
        }
    }

    /// <summary>
    /// Сообщение об ошибке.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    /// Выполняется ли вход.
    /// </summary>
    public bool IsLoggingIn
    {
        get => _isLoggingIn;
        set => SetProperty(ref _isLoggingIn, value);
    }

    /// <summary>
    /// Есть ли ошибка.
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Команда входа.
    /// </summary>
    public ICommand LoginCommand { get; }

    /// <summary>
    /// Команда регистрации.
    /// </summary>
    public ICommand RegisterCommand { get; }

    /// <summary>
    /// Команда гостевого входа.
    /// </summary>
    public ICommand GuestLoginCommand { get; }

    /// <summary>
    /// Команда возврата в меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    // Валидационные сообщения
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

    public bool HasUsernameError => !string.IsNullOrWhiteSpace(UsernameValidation);
    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordValidation);

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

    private bool CanLogin()
    {
        return Validator.ValidateUsername(Username) &&
               Validator.ValidatePassword(Password) &&
               !IsLoggingIn;
    }

    /// <summary>
    /// Выполняет вход пользователя.
    /// </summary>
    private async Task LoginAsync(object? parameter)
    {
        ErrorMessage = string.Empty;

        ValidateUsername();
        ValidatePassword();

        if (!CanLogin())
        {
            ErrorMessage = "Проверьте правильность заполнения всех полей";
            return;
        }

        IsLoggingIn = true;

        try
        {
            var result = await _userService.LoginAsync(Username, Password);

            if (result.User != null)
            {
                OnLoginSuccess(result.User);
            }
            else
            {
                ErrorMessage = result.Error ?? "Неверное имя пользователя или пароль";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка входа: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    /// <summary>
    /// Переход к окну регистрации.
    /// </summary>
    private void NavigateToRegister(object? parameter)
    {
        _navigationService.NavigateTo<RegisterViewModel>();
    }

    /// <summary>
    /// Вход в режиме гостя.
    /// </summary>
    private void GuestLogin(object? parameter)
    {
        var guest = _userService.GetGuestUser();
        OnLoginSuccess(guest!);
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }

    /// <summary>
    /// Обработка успешного входа.
    /// </summary>
    private void OnLoginSuccess(User user)
    {
        // Переход к главному меню
        _navigationService.ClearHistory();
        _navigationService.NavigateTo<MainViewModel>();
    }
}
