using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Результат оценки позиции.
/// </summary>
public class PositionEvaluation
{
    public int Score { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Результат выбора действия.
/// </summary>
public class MoveDecision
{
    public int UnitId { get; set; }
    public int TargetId { get; set; }
    public (int Row, int Column) TargetCell { get; set; }
    public string Action { get; set; } = string.Empty; // "Attack", "Move", "PlayCard"
    public int Score { get; set; }
}

/// <summary>
/// Интерфейс стратегии ИИ.
/// </summary>
public interface IAIStrategy
{
    /// <summary>
    /// Выбор цели для атаки.
    /// </summary>
    int SelectTarget(Unit attacker, List<int> availableTargets, IBattleManager battleManager);

    /// <summary>
    /// Выбор клетки для перемещения.
    /// </summary>
    (int Row, int Column) SelectMoveTarget(Unit unit, List<(int Row, int Column)> availableMoves, IBattleManager battleManager);

    /// <summary>
    /// Выбор карты для розыгрыша.
    /// </summary>
    Card SelectCardToPlay(List<Card> hand, List<(int Row, int Column)> availableCells);

    /// <summary>
    /// Выбор клетки для размещения карты.
    /// </summary>
    (int Row, int Column) SelectPlacementCell(Card card, List<(int Row, int Column)> availableCells, IBattleManager battleManager);

    /// <summary>
    /// Оценка позиции (положительное — advantage для ИИ, отрицательное — для игрока).
    /// </summary>
    PositionEvaluation EvaluateBoard(IBattleManager battleManager);

    /// <summary>
    /// Получение описания стратегии.
    /// </summary>
    string GetDescription();
}
