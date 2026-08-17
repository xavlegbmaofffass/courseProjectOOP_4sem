using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с пользователями.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Получает пользователя по имени.
    /// </summary>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Получает пользователя по ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Добавляет нового пользователя.
    /// </summary>
    Task AddUserAsync(User user);

    /// <summary>
    /// Обновляет существующего пользователя.
    /// </summary>
    Task UpdateUserAsync(User user);

    /// <summary>
    /// Получает всех пользователей.
    /// </summary>
    Task<List<User>> GetAllUsersAsync();

    /// <summary>
    /// Получает всех заблокированных пользователей.
    /// </summary>
    Task<List<User>> GetBlockedUsersAsync();
}
