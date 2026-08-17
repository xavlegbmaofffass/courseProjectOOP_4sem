using System.Collections.ObjectModel;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;
using FrontlineCardWarfare.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис для работы с пользователями.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private User? _currentUser;
    private readonly ObservableCollection<Deck> _guestDecks = new();

    /// <summary>
    /// Инициализирует новый экземпляр UserService.
    /// </summary>
    /// <param name="userRepository">Репозиторий пользователей.</param>
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Текущий авторизованный пользователь.
    /// </summary>
    public User? CurrentUser => _currentUser;

    /// <summary>
    /// Колоды гостевого пользователя (хранятся в памяти).
    /// </summary>
    public ObservableCollection<Deck> GuestDecks => _guestDecks;

    /// <summary>
    /// Является ли текущий пользователь гостем.
    /// </summary>
    public bool IsGuestMode => _currentUser?.Id == 0;

    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    public async Task<(bool Success, string? Error)> RegisterAsync(string username, string password)
    {
        // Валидация имени пользователя
        if (!Validator.ValidateUsername(username))
        {
            return (false, "Имя пользователя должно содержать от 3 до 20 символов (буквы, цифры, _)");
        }

        // Валидация пароля
        if (!Validator.ValidatePassword(password))
        {
            return (false, "Пароль должен содержать минимум 6 символов");
        }

        // Проверка существования пользователя
        var existingUser = await _userRepository.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            return (false, "Пользователь с таким именем уже существует");
        }

        // Создание нового пользователя
        var user = new User
        {
            Username = username,
            PasswordHash = PasswordHelper.HashPassword(password),
            Role = UserRole.Player,
            CreatedAt = DateTime.UtcNow,
            IsBlocked = false
        };

        await _userRepository.AddUserAsync(user);

        // Создаём пустую статистику для нового пользователя
        using var context = new GameDbContext();
        context.GameStatistics.Add(new GameStatistics { UserId = user.Id });
        await context.SaveChangesAsync();

        return (true, null);
    }

    /// <summary>
    /// Выполняет вход пользователя.
    /// </summary>
    public async Task<(User? User, string? Error)> LoginAsync(string username, string password)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user == null)
        {
            return (null, "Неверное имя пользователя или пароль");
        }

        if (user.IsBlocked)
        {
            return (null, "Ваш профиль заблокирован администратором");
        }

        if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            return (null, "Неверное имя пользователя или пароль");
        }

        _currentUser = user;
        return (user, null);
    }

    /// <summary>
    /// Возвращает гостевого пользователя.
    /// </summary>
    public User? GetGuestUser()
    {
        var guest = new User
        {
            Id = 0,
            Username = "Гость",
            PasswordHash = string.Empty,
            Role = UserRole.Player,
            CreatedAt = DateTime.MinValue,
            IsBlocked = false
        };
        _currentUser = guest;
        _guestDecks.Clear();
        return guest;
    }

    /// <summary>
    /// Получает пользователя по ID.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    /// <summary>
    /// Выполняет выход из системы.
    /// </summary>
    public void Logout()
    {
        _currentUser = null;
        _guestDecks.Clear();
    }

    /// <summary>
    /// Получает всех пользователей (для администратора).
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        if (_currentUser?.Role != UserRole.Admin)
        {
            return new List<User>();
        }

        return await _userRepository.GetAllUsersAsync();
    }

    /// <summary>
    /// Обновляет данные пользователя (для администратора).
    /// </summary>
    public async Task UpdateUserAsync(User user)
    {
        if (_currentUser?.Role != UserRole.Admin)
        {
            return;
        }

        await _userRepository.UpdateUserAsync(user);
    }

    /// <summary>
    /// Обновляет данные текущего пользователя (имя, пароль).
    /// </summary>
    public async Task<bool> UpdateUserProfileAsync(User user)
    {
        if (_currentUser == null || user.Id != _currentUser.Id)
        {
            return false;
        }

        try
        {
            using var context = new GameDbContext();
            var dbUser = await context.Users.FindAsync(user.Id);
            if (dbUser == null)
            {
                return false;
            }

            // Обновление имени
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == user.Username && u.Id != user.Id);
                if (existingUser != null)
                {
                    return false; // Имя занято
                }
                dbUser.Username = user.Username;
            }

            // Обновление пароля (если передан новый хэш)
            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                dbUser.PasswordHash = user.PasswordHash;
            }

            context.Users.Update(dbUser);
            await context.SaveChangesAsync();

            // Обновление текущего пользователя
            _currentUser.Username = dbUser.Username;
            _currentUser.PasswordHash = dbUser.PasswordHash;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
