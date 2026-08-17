using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса сохранения и загрузки игровых сессий.
/// </summary>
public interface IGameSaveService
{
    /// <summary>
    /// Сохраняет состояние игры в БД и файл.
    /// </summary>
    Task<bool> SaveGameAsync(GameSession session);

    /// <summary>
    /// Загружает состояние игры по ID сессии.
    /// </summary>
    Task<GameState?> LoadGameAsync(int sessionId);

    /// <summary>
    /// Загружает последнее сохранение для игрока.
    /// </summary>
    Task<GameState?> LoadLastGameAsync(int playerId);

    /// <summary>
    /// Удаляет сессию игры.
    /// </summary>
    Task<bool> DeleteGameAsync(int sessionId);

    /// <summary>
    /// Проверяет целостность данных сессии.
    /// </summary>
    bool VerifyChecksum(GameSession session);

    /// <summary>
    /// Проверяет наличие незавершённых игр у игрока.
    /// </summary>
    Task<bool> HasActiveGamesAsync(int playerId);

    /// <summary>
    /// Получает список активных игр игрока.
    /// </summary>
    Task<List<GameSession>> GetActiveGamesAsync(int playerId);

    /// <summary>
    /// Получает список всех сохранённых игр пользователя.
    /// </summary>
    Task<List<GameSession>> GetSavedGamesAsync(int userId);

    /// <summary>
    /// Автосохранение после розыгрыша карты.
    /// </summary>
    Task<bool> SaveAfterPlayCardAsync(GameState gameState, int sessionId);

    /// <summary>
    /// Автосохранение после перемещения юнита.
    /// </summary>
    Task<bool> SaveAfterMoveUnitAsync(GameState gameState, int sessionId);

    /// <summary>
    /// Автосохранение после атаки.
    /// </summary>
    Task<bool> SaveAfterAttackAsync(GameState gameState, int sessionId);

    /// <summary>
    /// Автосохранение после завершения хода.
    /// </summary>
    Task<bool> SaveAfterEndTurnAsync(GameState gameState, int sessionId);

    /// <summary>
    /// Инициализирует новую игровую сессию.
    /// </summary>
    Task<GameSession> InitializeGameAsync(int playerDeckId, string difficulty);

    /// <summary>
    /// Сохраняет результат завершённой игры с полной статистикой.
    /// </summary>
    Task<bool> SaveGameResultAsync(int userId, GameEndStatistics statistics, string difficulty);
}