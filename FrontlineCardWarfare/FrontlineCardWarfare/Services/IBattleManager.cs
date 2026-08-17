using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс менеджера боя.
/// </summary>
public interface IBattleManager
{
    /// <summary>
    /// Текущее состояние игры.
    /// </summary>
    GameState GameState { get; }

    /// <summary>
    /// Инициализирует новую игру.
    /// </summary>
    Task InitializeAsync(Deck playerDeck, Deck enemyDeck, string difficulty);

    /// <summary>
    /// Разыгрывает карту на поле (асинхронно).
    /// </summary>
    Task<(bool Success, string Error)> PlayCardAsync(Card card, int row, int column);

    /// <summary>
    /// Атакует цель юнитом.
    /// </summary>
    (bool Success, string Error, int Damage, bool TargetDestroyed) Attack(int unitId, int targetUnitId);

    /// <summary>
    /// Завершает ход.
    /// </summary>
    Task EndTurnAsync();

    /// <summary>
    /// Проверяет условие победы.
    /// </summary>
    string? CheckWinCondition();

    /// <summary>
    /// Получает доступные цели для атаки юнита.
    /// </summary>
    List<int> GetAvailableTargets(int unitId);

    /// <summary>
    /// Получает юнита по ID.
    /// </summary>
    Unit? GetUnitById(int unitId);

    /// <summary>
    /// Получает доступные клетки для размещения карт.
    /// </summary>
    List<(int Row, int Column)> GetAvailableMovesForPlacement();

    /// <summary>
    /// Завершает ход ИИ и передаёт управление игроку.
    /// </summary>
    Task CompleteEnemyTurnAsync();

    /// <summary>
    /// Сдаётся.
    /// </summary>
    void Surrender();

    /// <summary>
    /// Получает детальную статистику завершённой игры.
    /// </summary>
    GameEndStatistics GetGameEndStatistics(string difficulty);

    /// <summary>
    /// Добор карты игроком.
    /// </summary>
    Task PlayerDrawCardAsync();

    /// <summary>
    /// Добор карты врагом.
    /// </summary>
    Task EnemyDrawCardAsync();

    /// <summary>
    /// Разрешает автоматические сражения на линиях.
    /// </summary>
    Task ResolveLaneCombatAsync();
}
