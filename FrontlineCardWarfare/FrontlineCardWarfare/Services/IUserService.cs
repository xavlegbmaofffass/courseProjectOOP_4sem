using System.Collections.ObjectModel;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса для работы с пользователями.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Регистрирует нового пользователя.
    /// </summary>
    /// <param name="username">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <returns>True, если регистрация успешна.</returns>
    Task<(bool Success, string? Error)> RegisterAsync(string username, string password);

    /// <summary>
    /// Выполняет вход пользователя.
    /// </summary>
    /// <param name="username">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <returns>Данные пользователя и сообщение об ошибке (если есть).</returns>
    Task<(User? User, string? Error)> LoginAsync(string username, string password);

    /// <summary>
    /// Возвращает гостевого пользователя.
    /// </summary>
    User? GetGuestUser();

    /// <summary>
    /// Получает пользователя по ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Получает текущего пользователя.
    /// </summary>
    User? CurrentUser { get; }

    /// <summary>
    /// Выполняет выход из системы.
    /// </summary>
    void Logout();

    /// <summary>
    /// Получает всех пользователей (для администратора).
    /// </summary>
    Task<List<User>> GetAllUsersAsync();

    /// <summary>
    /// Обновляет данные пользователя (для администратора).
    /// </summary>
    Task UpdateUserAsync(User user);

    /// <summary>
    /// Обновляет данные текущего пользователя (имя, пароль).
    /// </summary>
    Task<bool> UpdateUserProfileAsync(User user);

    /// <summary>
    /// Колоды гостевого пользователя (хранятся в памяти).
    /// </summary>
    ObservableCollection<Deck> GuestDecks { get; }

    /// <summary>
    /// Является ли текущий пользователь гостем.
    /// </summary>
    bool IsGuestMode { get; }
}
