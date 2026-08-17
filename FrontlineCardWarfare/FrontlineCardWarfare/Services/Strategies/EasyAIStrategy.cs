using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services.Strategies;

/// <summary>
/// Легкая стратегия: случайные действия без анализа позиции.
/// </summary>
public class EasyAIStrategy : IAIStrategy
{
    private readonly Random _random = new();

    public string GetDescription() => "Лёгкий: случайные действия, отсутствие стратегии";

    public int SelectTarget(Unit attacker, List<int> availableTargets, IBattleManager battleManager)
    {
        // Случайная цель
        return availableTargets[_random.Next(availableTargets.Count)];
    }

    public (int Row, int Column) SelectMoveTarget(Unit unit, List<(int Row, int Column)> availableMoves, IBattleManager battleManager)
    {
        // Случайное перемещение
        return availableMoves[_random.Next(availableMoves.Count)];
    }

    public Card SelectCardToPlay(List<Card> hand, List<(int Row, int Column)> availableCells)
    {
        // Случайная карта
        return hand[_random.Next(hand.Count)];
    }

    public (int Row, int Column) SelectPlacementCell(Card card, List<(int Row, int Column)> availableCells, IBattleManager battleManager)
    {
        // Случайная клетка на стороне врага (ряды 0-1)
        var validCells = availableCells.Where(c => c.Row < 2).ToList();
        if (validCells.Count == 0) validCells = availableCells;
        return validCells[_random.Next(validCells.Count)];
    }

    public PositionEvaluation EvaluateBoard(IBattleManager battleManager)
    {
        // Простая подсчёт силы: атака - защита
        var gameState = battleManager.GameState;
        var enemyUnits = gameState.Board.GetPlayerUnits(false);
        var playerUnits = gameState.Board.GetPlayerUnits(true);

        int enemyStrength = enemyUnits.Sum(u => u.Attack);
        int playerStrength = playerUnits.Sum(u => u.Attack);

        return new PositionEvaluation
        {
            Score = enemyStrength - playerStrength,
            Reason = $"Сила ИИ: {enemyStrength}, Сила игрока: {playerStrength}"
        };
    }
}
