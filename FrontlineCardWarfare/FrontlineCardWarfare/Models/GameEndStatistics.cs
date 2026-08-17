namespace FrontlineCardWarfare.Models;

/// <summary>
/// Детальная статистика завершённой игры.
/// </summary>
public class GameEndStatistics
{
    /// <summary>
    /// Результат игры (Победа/Поражение/Ничья).
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Количество ходов.
    /// </summary>
    public int TurnCount { get; set; }

    /// <summary>
    /// Нанесённый урон игроком.
    /// </summary>
    public int PlayerDamageDealt { get; set; }

    /// <summary>
    /// Нанесённый урон противником.
    /// </summary>
    public int EnemyDamageDealt { get; set; }

    /// <summary>
    /// Убито юнитов игроком.
    /// </summary>
    public int PlayerUnitsKilled { get; set; }

    /// <summary>
    /// Убито юнитов противником.
    /// </summary>
    public int EnemyUnitsKilled { get; set; }

    /// <summary>
    /// Оставшиеся карты в руке игрока.
    /// </summary>
    public int PlayerCardsRemaining { get; set; }

    /// <summary>
    /// Оставшиеся карты в руке противника.
    /// </summary>
    public int EnemyCardsRemaining { get; set; }

    /// <summary>
    /// Уровень сложности.
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>
    /// Время начала игры.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Время завершения игры.
    /// </summary>
    public DateTime EndedAt { get; set; } = DateTime.Now;
}
