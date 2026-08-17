using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;
using FrontlineCardWarfare.Services;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для экрана профиля пользователя со статистикой.
/// </summary>
public class ProfileViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private readonly ILoggingService _loggingService;
    private readonly IStatisticsService _statisticsService;
    private string _currentUsername = string.Empty;
    private string _newUsername = string.Empty;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isUpdating;
    private string _usernameValidation = string.Empty;
    private string _passwordValidation = string.Empty;
    private string _confirmPasswordValidation = string.Empty;
    
    // Статистика
    private int _wins;
    private int _losses;
    private int _totalGames;
    private double _winRate;
    private ObservableCollection<GameSession> _gameHistory = new();

    /// <summary>
    /// Инициализирует новый экземпляр ProfileViewModel.
    /// </summary>
    public ProfileViewModel(
        IUserService userService,
        INavigationService navigationService,
        ILoggingService loggingService,
        IStatisticsService statisticsService)
    {
        _userService = userService;
        _navigationService = navigationService;
        _loggingService = loggingService;
        _statisticsService = statisticsService;

        UpdateProfileCommand = new AsyncRelayCommand(UpdateProfileAsync, CanUpdateProfile);
        ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanChangePassword);
        BackToMenuCommand = new RelayCommand(BackToMenu);
        LogoutCommand = new RelayCommand(Logout);
        ClearMessagesCommand = new RelayCommand(ClearMessages);

        LoadUserProfile();
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Профиль пользователя";

    /// <summary>
    /// Текущее имя пользователя.
    /// </summary>
    public string CurrentUsername
    {
        get => _currentUsername;
        set => SetProperty(ref _currentUsername, value);
    }

    /// <summary>
    /// Новое имя пользователя.
    /// </summary>
    public string NewUsername
    {
        get => _newUsername;
        set
        {
            if (SetProperty(ref _newUsername, value))
            {
                ValidateUsername();
                (UpdateProfileCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Текущий пароль (для проверки).
    /// </summary>
    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            SetProperty(ref _currentPassword, value);
            (ChangePasswordCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Новый пароль.
    /// </summary>
    public string NewPassword
    {
        get => _newPassword;
        set
        {
            if (SetProperty(ref _newPassword, value))
            {
                ValidatePassword();
                ValidateConfirmPassword();
                (ChangePasswordCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Подтверждение нового пароля.
    /// </summary>
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
            {
                ValidateConfirmPassword();
                (ChangePasswordCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
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
    /// Сообщение об успехе.
    /// </summary>
    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            SetProperty(ref _successMessage, value);
            OnPropertyChanged(nameof(HasSuccess));
        }
    }

    /// <summary>
    /// Есть ли ошибка.
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    /// <summary>
    /// Есть ли успех.
    /// </summary>
    public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

    /// <summary>
    /// Выполняется ли обновление.
    /// </summary>
    public bool IsUpdating
    {
        get => _isUpdating;
        set => SetProperty(ref _isUpdating, value);
    }

    #region Валидация

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
        if (string.IsNullOrWhiteSpace(NewUsername))
        {
            UsernameValidation = "Введите новое имя пользователя";
        }
        else if (NewUsername == CurrentUsername)
        {
            UsernameValidation = "Новое имя должно отличаться от текущего";
        }
        else if (NewUsername.Length < 3)
        {
            UsernameValidation = "Минимум 3 символа";
        }
        else if (NewUsername.Length > 20)
        {
            UsernameValidation = "Максимум 20 символов";
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(NewUsername, @"^[a-zA-Z0-9_]+$"))
        {
            UsernameValidation = "Только буквы, цифры и _";
        }
        else
        {
            UsernameValidation = string.Empty;
        }

        OnPropertyChanged(nameof(HasUsernameError));
    }

    private void ValidatePassword()
    {
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            PasswordValidation = "Введите новый пароль";
        }
        else if (NewPassword.Length < 6)
        {
            PasswordValidation = "Минимум 6 символов";
        }
        else
        {
            PasswordValidation = string.Empty;
        }

        OnPropertyChanged(nameof(HasPasswordError));
    }

    private void ValidateConfirmPassword()
    {
        if (string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ConfirmPasswordValidation = "Подтвердите новый пароль";
        }
        else if (ConfirmPassword != NewPassword)
        {
            ConfirmPasswordValidation = "Пароли не совпадают";
        }
        else
        {
            ConfirmPasswordValidation = string.Empty;
        }

        OnPropertyChanged(nameof(HasConfirmPasswordError));
    }

    #endregion

    #region Команды

    /// <summary>
    /// Команда обновления профиля.
    /// </summary>
    public ICommand UpdateProfileCommand { get; }

    /// <summary>
    /// Команда смены пароля.
    /// </summary>
    public ICommand ChangePasswordCommand { get; }

    /// <summary>
    /// Команда возврата в меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Команда выхода из аккаунта.
    /// </summary>
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// Команда очистки сообщений.
    /// </summary>
    public ICommand ClearMessagesCommand { get; }

    #endregion

    #region Свойства статистики

    /// <summary>
    /// Количество побед.
    /// </summary>
    public int Wins
    {
        get => _wins;
        set => SetProperty(ref _wins, value);
    }

    /// <summary>
    /// Количество поражений.
    /// </summary>
    public int Losses
    {
        get => _losses;
        set => SetProperty(ref _losses, value);
    }

    /// <summary>
    /// Общее количество игр.
    /// </summary>
    public int TotalGames
    {
        get => _totalGames;
        set => SetProperty(ref _totalGames, value);
    }

    /// <summary>
    /// Процент побед.
    /// </summary>
    public double WinRate
    {
        get => _winRate;
        set => SetProperty(ref _winRate, value);
    }

    /// <summary>
    /// История игр.
    /// </summary>
    public ObservableCollection<GameSession> GameHistory
    {
        get => _gameHistory;
        set => SetProperty(ref _gameHistory, value);
    }

    #endregion

    #region Методы

    /// <summary>
    /// Загружает данные текущего пользователя.
    /// </summary>
    private void LoadUserProfile()
    {
        var user = _userService.CurrentUser;
        if (user != null)
        {
            CurrentUsername = user.Username;
            NewUsername = user.Username;
        }
        
        // Загрузка статистики при инициализации
        _ = LoadStatisticsAsync(null);
    }

    /// <summary>
    /// Загружает статистику пользователя.
    /// </summary>
    private async Task LoadStatisticsAsync(object? parameter)
    {
        var currentUser = _userService.CurrentUser;
        if (currentUser == null)
            return;

        try
        {
            // Загрузка статистики
            var stats = await _statisticsService.GetPlayerStatisticsAsync(currentUser.Id);
            if (stats != null)
            {
                Wins = stats.Wins;
                Losses = stats.Losses;
                TotalGames = stats.TotalGames;
                WinRate = stats.TotalGames > 0 ? Math.Round((double)stats.Wins / stats.TotalGames * 100, 1) : 0;
            }

            // Загрузка истории игр (последние 10)
            var history = await _statisticsService.GetGameHistoryAsync(currentUser.Id, 10);
            GameHistory.Clear();
            foreach (var game in history)
            {
                GameHistory.Add(game);
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("Ошибка загрузки статистики", ex);
            ErrorMessage = $"Ошибка загрузки статистики: {ex.Message}";
        }
    }

    private bool CanUpdateProfile(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(NewUsername) &&
               !HasUsernameError &&
               NewUsername != CurrentUsername &&
               !IsUpdating;
    }

    private bool CanChangePassword(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(CurrentPassword) &&
               !string.IsNullOrWhiteSpace(NewPassword) &&
               !string.IsNullOrWhiteSpace(ConfirmPassword) &&
               !HasPasswordError &&
               !HasConfirmPasswordError &&
               !IsUpdating;
    }

    /// <summary>
    /// Обновляет имя пользователя.
    /// </summary>
    private async Task UpdateProfileAsync(object? parameter)
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsUpdating = true;

        try
        {
            var user = _userService.CurrentUser;
            if (user == null)
            {
                ErrorMessage = "Пользователь не авторизован";
                IsUpdating = false;
                return;
            }

            // Проверка, не занято ли новое имя
            using var context = new GameDbContext();
            var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == NewUsername && u.Id != user.Id);
            if (existingUser != null)
            {
                UsernameValidation = "Это имя уже занято";
                OnPropertyChanged(nameof(HasUsernameError));
                ErrorMessage = "Это имя пользователя уже занято";
                IsUpdating = false;
                return;
            }

            // Обновление имени пользователя
            user.Username = NewUsername;
            await _userService.UpdateUserProfileAsync(user);

            SuccessMessage = $"Профиль успешно обновлён! Новое имя: {NewUsername}";
            _loggingService.LogInfo($"Пользователь {user.Id} обновил имя на {NewUsername}");

            ClearMessages(null);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка обновления профиля: {ex.Message}";
            _loggingService.LogError("Ошибка обновления профиля", ex);
        }
        finally
        {
            IsUpdating = false;
        }
    }

    /// <summary>
    /// Меняет пароль пользователя.
    /// </summary>
    private async Task ChangePasswordAsync(object? parameter)
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsUpdating = true;

        try
        {
            var user = _userService.CurrentUser;
            if (user == null)
            {
                ErrorMessage = "Пользователь не авторизован";
                IsUpdating = false;
                return;
            }

            // Проверка текущего пароля
            using var context = new GameDbContext();
            var dbUser = await context.Users.FindAsync(user.Id);
            if (dbUser == null)
            {
                ErrorMessage = "Пользователь не найден";
                IsUpdating = false;
                return;
            }

            if (!PasswordHelper.VerifyPassword(CurrentPassword, dbUser.PasswordHash))
            {
                PasswordValidation = "Неверный текущий пароль";
                OnPropertyChanged(nameof(HasPasswordError));
                ErrorMessage = "Неверный текущий пароль";
                IsUpdating = false;
                return;
            }

            // Обновление пароля
            dbUser.PasswordHash = PasswordHelper.HashPassword(NewPassword);
            context.Users.Update(dbUser);
            await context.SaveChangesAsync();

            // Обновление текущего пользователя
            user.PasswordHash = dbUser.PasswordHash;

            SuccessMessage = "Пароль успешно изменён!";
            _loggingService.LogInfo($"Пользователь {user.Id} изменил пароль");

            // Очистка полей пароля
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            ClearMessages(null);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка смены пароля: {ex.Message}";
            _loggingService.LogError("Ошибка смены пароля", ex);
        }
        finally
        {
            IsUpdating = false;
        }
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }

    /// <summary>
    /// Выход из аккаунта и переход к окну входа.
    /// </summary>
    private void Logout(object? parameter)
    {
        _userService.Logout();
        _navigationService.NavigateTo<LoginViewModel>();
    }

    /// <summary>
    /// Очищает сообщения.
    /// </summary>
    private void ClearMessages(object? parameter)
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    #endregion
}
