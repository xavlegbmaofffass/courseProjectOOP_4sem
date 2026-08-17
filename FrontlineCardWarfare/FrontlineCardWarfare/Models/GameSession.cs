namespace FrontlineCardWarfare.Data;

/// <summary>
/// Сущность игровой сессии (сохранённая игра).
/// </summary>
public class GameSession
{
    /// <summary>
    /// Уникальный идентификатор сессии.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор игрока.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Идентификатор колоды игрока.
    /// </summary>
    public int DeckId { get; set; }

    /// <summary>
    /// Состояние игрового поля (JSON).
    /// </summary>
    public string BoardStateJson { get; set; } = string.Empty;

    /// <summary>
    /// Рука игрока (JSON).
    /// </summary>
    public string PlayerHandJson { get; set; } = string.Empty;

    /// <summary>
    /// Рука противника (JSON).
    /// </summary>
    public string EnemyHandJson { get; set; } = string.Empty;

    /// <summary>
    /// Сейчас ход игрока.
    /// </summary>
    public bool IsPlayerTurn { get; set; }

    /// <summary>
    /// Номер текущего хода (для сохранений) или общее число ходов (для логов).
    /// </summary>
    public int TurnNumber { get; set; }

    /// <summary>
    /// Результат игры (победа/поражение/ничья).
    /// </summary>
    public string? GameResult { get; set; }

    /// <summary>
    /// Псевдоним для GameResult для совместимости с привязками в XAML.
    /// </summary>
    public string Result => GameResult ?? "Неизвестно";

    /// <summary>
    /// Имя противника.
    /// </summary>
    public string OpponentName { get; set; } = "ИИ: Противник";

    /// <summary>
    /// Длительность игры.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Дата игры.
    /// </summary>
    public DateTime DatePlayed => LastSavedAt;

    /// <summary>
    /// Нанесенный игроком урон.
    /// </summary>
    public int PlayerDamageDealt { get; set; }

    /// <summary>
    /// Нанесенный противником урон.
    /// </summary>
    public int EnemyDamageDealt { get; set; }

    /// <summary>
    /// Дата последнего сохранения.
    /// </summary>
    public DateTime LastSavedAt { get; set; }

    /// <summary>
    /// Контрольная сумма для проверки целостности.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Уровень сложности ИИ.
    /// </summary>
    public string Difficulty { get; set; } = "Medium";
}
