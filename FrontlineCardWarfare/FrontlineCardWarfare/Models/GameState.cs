using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Models;

/// <summary>
/// Полное состояние игровой сессии.
/// </summary>
public class GameState
{
    /// <summary>
    /// Время начала игры.
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Игровое поле.
    /// </summary>
    public Board Board { get; set; } = new();

    /// <summary>
    /// Рука игрока.
    /// </summary>
    public Hand PlayerHand { get; set; } = new();

    /// <summary>
    /// Рука врага (скрыта от игрока).
    /// </summary>
    public Hand EnemyHand { get; set; } = new();

    /// <summary>
    /// Информация о ходе.
    /// </summary>
    public GameTurn Turn { get; set; } = new();

    /// <summary>
    /// ID колоды игрока.
    /// </summary>
    public int PlayerDeckId { get; set; }

    /// <summary>
    /// ID колоды врага.
    /// </summary>
    public int EnemyDeckId { get; set; }

    /// <summary>
    /// Оставшиеся карты в колоде врага (для добора).
    /// </summary>
    public List<Card> EnemyDeck { get; set; } = new();

    /// <summary>
    /// Оставшиеся карты в колоде игрока (для добора).
    /// </summary>
    public List<Card> PlayerDeck { get; set; } = new();

    /// <summary>
    /// Уровень сложности ИИ.
    /// </summary>
    public string Difficulty { get; set; } = "Medium";

    /// <summary>
    /// Результат игры (победа/поражение/ничья).
    /// </summary>
    public string? GameResult { get; set; }

    /// <summary>
    /// Игра завершена.
    /// </summary>
    public bool IsGameOver { get; set; }

    /// <summary>
    /// Количество убитых юнитов игроком.
    /// </summary>
    public int KilledEnemyUnitsCount { get; set; }

    /// <summary>
    /// Количество убитых юнитов противником.
    /// </summary>
    public int KilledPlayerUnitsCount { get; set; }

    /// <summary>
    /// Нанесённый урон игроком.
    /// </summary>
    public int PlayerTotalDamageDealt { get; set; }

    /// <summary>
    /// Нанесённый урон противником.
    /// </summary>
    public int EnemyTotalDamageDealt { get; set; }

    /// <summary>
    /// Создаёт копию состояния игры.
    /// </summary>
    public GameState Clone()
    {
        return new GameState
        {
            Board = Board.Clone(),
            PlayerHand = PlayerHand.Clone(),
            EnemyHand = EnemyHand.Clone(),
            Turn = Turn.Clone(),
            PlayerDeckId = PlayerDeckId,
            EnemyDeckId = EnemyDeckId,
            Difficulty = Difficulty,
            GameResult = GameResult
        };
    }
}
