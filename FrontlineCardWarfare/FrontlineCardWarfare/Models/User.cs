namespace FrontlineCardWarfare.Data;

/// <summary>
/// Роли пользователей в системе.
/// </summary>
public enum UserRole
{
    Player = 0,
    Admin = 1
}

/// <summary>
/// Сущность пользователя.
/// </summary>
public class User
{
    /// <summary>
    /// Уникальный идентификатор пользователя.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя пользователя (уникальное).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Хэш пароля.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Роль пользователя (Player/Admin).
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Дата создания аккаунта.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Заблокирован ли пользователь.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Навигационное свойство для колод.
    /// </summary>
    public ICollection<Deck> Decks { get; set; } = new List<Deck>();

    /// <summary>
    /// Навигационное свойство для игровых сессий.
    /// </summary>
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();

    /// <summary>
    /// Навигационное свойство для статистики.
    /// </summary>
    public GameStatistics? Statistics { get; set; }
}
