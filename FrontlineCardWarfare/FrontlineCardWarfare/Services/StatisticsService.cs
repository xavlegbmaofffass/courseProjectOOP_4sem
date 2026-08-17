using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис статистики.
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly GameDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр StatisticsService.
    /// </summary>
    public StatisticsService(GameDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает статистику игрока.
    /// </summary>
    public async Task<GameStatistics?> GetPlayerStatisticsAsync(int userId)
    {
        return await _context.GameStatistics
            .FirstOrDefaultAsync(gs => gs.UserId == userId);
    }

    /// <summary>
    /// Получает историю игр игрока.
    /// </summary>
    public async Task<List<GameSession>> GetGameHistoryAsync(int userId, int count = 20)
    {
        return await _context.GameSessions
            .Where(gs => gs.UserId == userId && gs.GameResult != null)
            .OrderByDescending(gs => gs.LastSavedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Обновляет статистику после игры.
    /// </summary>
    public async Task UpdateStatisticsAsync(int userId, string gameResult)
    {
        var stats = await _context.GameStatistics
            .FirstOrDefaultAsync(gs => gs.UserId == userId);

        if (stats == null)
        {
            stats = new GameStatistics
            {
                UserId = userId,
                Wins = 0,
                Losses = 0,
                TotalGames = 0
            };
            _context.GameStatistics.Add(stats);
        }

        stats.TotalGames++;

        if (gameResult.Contains("Победа", StringComparison.OrdinalIgnoreCase))
        {
            stats.Wins++;
        }
        else if (gameResult.Contains("Поражение", StringComparison.OrdinalIgnoreCase))
        {
            stats.Losses++;
        }

        stats.LastPlayedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Получает таблицу лидеров.
    /// </summary>
    public async Task<List<UserStatisticsDto>> GetLeaderboardAsync(int topCount = 10)
    {
        var statsList = await _context.GameStatistics
            .Include(gs => gs.User)
            .Where(gs => gs.TotalGames > 0)
            .OrderByDescending(gs => gs.Wins)
            .ThenByDescending(gs => (double)gs.Wins / gs.TotalGames * 100)
            .Take(topCount)
            .ToListAsync();

        return statsList.Select(gs => new UserStatisticsDto
        {
            Username = gs.User?.Username ?? "Неизвестно",
            Wins = gs.Wins,
            Losses = gs.Losses,
            TotalGames = gs.TotalGames,
            WinRate = gs.WinRate
        }).ToList();
    }
}
