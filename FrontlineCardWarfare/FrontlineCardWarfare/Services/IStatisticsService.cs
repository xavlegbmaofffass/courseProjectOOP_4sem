using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса статистики.
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Получает статистику игрока.
    /// </summary>
    Task<GameStatistics?> GetPlayerStatisticsAsync(int userId);

    /// <summary>
    /// Получает историю игр игрока.
    /// </summary>
    Task<List<GameSession>> GetGameHistoryAsync(int userId, int count = 20);

    /// <summary>
    /// Обновляет статистику после игры.
    /// </summary>
    Task UpdateStatisticsAsync(int userId, string gameResult);

    /// <summary>
    /// Получает таблицу лидеров.
    /// </summary>
    Task<List<UserStatisticsDto>> GetLeaderboardAsync(int topCount = 10);
}

/// <summary>
/// DTO для таблицы лидеров.
/// </summary>
public class UserStatisticsDto
{
    public string Username { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames { get; set; }
    public double WinRate { get; set; }
}
