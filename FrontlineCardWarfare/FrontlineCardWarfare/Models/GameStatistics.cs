namespace FrontlineCardWarfare.Data;

/// <summary>
/// Сущность игровой статистики пользователя.
/// </summary>
public class GameStatistics
{
    /// <summary>
    /// Уникальный идентификатор статистики.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Количество побед.
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Количество поражений.
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Общее количество игр.
    /// </summary>
    public int TotalGames { get; set; }

    /// <summary>
    /// Дата последней игры.
    /// </summary>
    public DateTime? LastPlayedAt { get; set; }

    /// <summary>
    /// Процент побед (вычисляемое свойство).
    /// </summary>
    public double WinRate => TotalGames > 0 ? (double)Wins / TotalGames * 100 : 0;

    /// <summary>
    /// Навигационное свойство — пользователь.
    /// </summary>
    public User? User { get; set; }
}
