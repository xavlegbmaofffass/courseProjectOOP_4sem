using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services.Strategies;

/// <summary>
/// Средняя стратегия: приоритет слабых целей и сильных карт.
/// </summary>
public class MediumAIStrategy : IAIStrategy
{
    private readonly Random _random = new();

    public string GetDescription() => "Средний: приоритет слабых целей, атака минимального Health";

    public int SelectTarget(Unit attacker, List<int> availableTargets, IBattleManager battleManager)
    {
        var gameState = battleManager.GameState;
        var targets = availableTargets
            .Select(id => gameState.Board.GetUnitById(id))
            .Where(u => u != null)
            .ToList();

        // 1. Приоритет: юниты, которых можно убить с одного удара
        var killable = targets.Where(t => t!.CurrentHealth <= attacker.Attack).OrderByDescending(t => t!.Attack).FirstOrDefault();
        if (killable != null) return killable.Id;

        // 2. Иначе просто цель с наименьшим Health
        return targets.OrderBy(u => u!.CurrentHealth).FirstOrDefault()!.Id;
    }

    public (int Row, int Column) SelectMoveTarget(Unit unit, List<(int Row, int Column)> availableMoves, IBattleManager battleManager)
    {
        // Перемещение к центру и вперёд (если не в первом ряду)
        if (unit.Row == 0 && availableMoves.Any(m => m.Row == 1))
        {
            return availableMoves.OrderBy(m => Math.Abs(m.Column - 1.5)).ThenByDescending(m => m.Row).First();
        }

        return availableMoves[_random.Next(availableMoves.Count)];
    }

    public Card SelectCardToPlay(List<Card> hand, List<(int Row, int Column)> availableCells)
    {
        // Выбор самой сильной карты (Attack + Health)
        return hand.OrderByDescending(c => c.Attack + c.Health).FirstOrDefault()!;
    }

    public (int Row, int Column) SelectPlacementCell(Card card, List<(int Row, int Column)> availableCells, IBattleManager battleManager)
    {
        // Размещение ближе к врагу (ряды 0-1, ближе к центру)
        var validCells = availableCells.Where(c => c.Row < 2).ToList();
        if (validCells.Count == 0) validCells = availableCells;

        // Приоритет центру (ряд 0-1, колонка 1-2)
        return validCells
            .OrderBy(c => Math.Abs(c.Column - 1.5))
            .ThenBy(c => c.Row)
            .FirstOrDefault();
    }

    public PositionEvaluation EvaluateBoard(IBattleManager battleManager)
    {
        var gameState = battleManager.GameState;
        var enemyUnits = gameState.Board.GetPlayerUnits(false).ToList();
        var playerUnits = gameState.Board.GetPlayerUnits(true).ToList();

        // Сила = Attack + Health
        int enemyStrength = enemyUnits.Sum(u => u.Attack + u.CurrentHealth / 2);
        int playerStrength = playerUnits.Sum(u => u.Attack + u.CurrentHealth / 2);

        // Бонус за контроль центра
        int enemyCenterControl = enemyUnits.Count(u => u.Row < 2 && u.Column >= 1 && u.Column <= 2);
        int playerCenterControl = playerUnits.Count(u => u.Row >= 2 && u.Column >= 1 && u.Column <= 2);

        int score = (enemyStrength - playerStrength) + (enemyCenterControl - playerCenterControl) * 2;

        return new PositionEvaluation
        {
            Score = score,
            Reason = $"Сила: {enemyStrength} vs {playerStrength}, Центр: {enemyCenterControl} vs {playerCenterControl}"
        };
    }
}
