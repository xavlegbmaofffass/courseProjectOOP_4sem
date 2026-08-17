using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services.Strategies;

/// <summary>
/// Сложная стратегия: оценка позиции, сохранение юнитов, контроль центра.
/// </summary>
public class HardAIStrategy : IAIStrategy
{
    private readonly Random _random = new();

    public string GetDescription() => "Сложный: стратегическое преимущество, оценка позиции, блокирование путей";

    public int SelectTarget(Unit attacker, List<int> availableTargets, IBattleManager battleManager)
    {
        var gameState = battleManager.GameState;
        var targets = availableTargets
            .Select(id => gameState.Board.GetUnitById(id))
            .Where(u => u != null)
            .ToList();

        if (!targets.Any()) return availableTargets[0];

        // 1. Приоритет: юниты, которых можно убить (Focus Fire)
        var killable = targets.Where(t => t!.CurrentHealth <= attacker.Attack).OrderByDescending(t => t!.Attack).FirstOrDefault();
        if (killable != null) return killable.Id;

        // 2. Приоритет: самые опасные враги (высокий Attack)
        return targets.OrderByDescending(u => u!.Attack).ThenBy(u => u!.CurrentHealth).FirstOrDefault()!.Id;
    }

    public (int Row, int Column) SelectMoveTarget(Unit unit, List<(int Row, int Column)> availableMoves, IBattleManager battleManager)
    {
        var gameState = battleManager.GameState;
        
        // 1. Если юнит ранен (Health < 3) и он впереди (Row 1), отводим назад (Row 0)
        if (unit.CurrentHealth < 3 && unit.Row == 1)
        {
            var backMoves = availableMoves.Where(m => m.Row == 0).ToList();
            if (backMoves.Any()) return backMoves.OrderBy(m => Math.Abs(m.Column - 1.5)).First();
        }

        // 2. Если юнит - танк (Health >= 5) и он сзади (Row 0), выводим вперед (Row 1)
        if (unit.MaxHealth >= 5 && unit.Row == 0)
        {
            var forwardMoves = availableMoves.Where(m => m.Row == 1).ToList();
            if (forwardMoves.Any()) return forwardMoves.OrderBy(m => Math.Abs(m.Column - 1.5)).First();
        }

        // 3. Дальники (Range > 1) должны стремиться назад (Row 0)
        if (unit.Range > 1 && unit.Row == 1)
        {
             var backMoves = availableMoves.Where(m => m.Row == 0).ToList();
             if (backMoves.Any()) return backMoves.First();
        }

        // 4. По умолчанию - к ближайшему врагу
        return availableMoves.OrderBy(m => 
        {
             var enemies = gameState.Board.GetPlayerUnits(true);
             return enemies.Any() ? enemies.Min(e => Math.Abs(m.Row - e.Row) + Math.Abs(m.Column - e.Column)) : 0;
        }).First();
    }

    public Card SelectCardToPlay(List<Card> hand, List<(int Row, int Column)> availableCells)
    {
        // Выбор карты, которая может убить кого-то на поле сразу или имеет лучшие статы
        return hand.OrderByDescending(c => c.Attack * 2 + c.Health).FirstOrDefault()!;
    }

    public (int Row, int Column) SelectPlacementCell(Card card, List<(int Row, int Column)> availableCells, IBattleManager battleManager)
    {
        // Танки (Health >= 5) - в ряд 1, остальные (дальники или хлипкие) - в ряд 0
        bool isTank = card.Health >= 5;
        var preferredRow = isTank ? 1 : 0;
        
        var preferredCells = availableCells.Where(c => c.Row == preferredRow).ToList();
        if (!preferredCells.Any()) preferredCells = availableCells;

        return preferredCells.OrderBy(c => Math.Abs(c.Column - 1.5)).First();
    }

    public PositionEvaluation EvaluateBoard(IBattleManager battleManager)
    {
        var gameState = battleManager.GameState;
        var enemyUnits = gameState.Board.GetPlayerUnits(false).ToList();
        var playerUnits = gameState.Board.GetPlayerUnits(true).ToList();

        // Базовая сила
        int enemyStrength = enemyUnits.Sum(u => u.Attack + u.CurrentHealth / 2);
        int playerStrength = playerUnits.Sum(u => u.Attack + u.CurrentHealth / 2);

        // Контроль центра (ряды 0-1 для ИИ, 2-3 для игрока)
        int enemyCenterControl = enemyUnits.Count(u => u.Row <= 1 && u.Column >= 1 && u.Column <= 2) * 3;
        int playerCenterControl = playerUnits.Count(u => u.Row >= 2 && u.Column >= 1 && u.Column <= 2) * 3;

        // Бонус за танков вперёд (Health >= 5 в рядах 0-1)
        int enemyFrontlineTanks = enemyUnits.Count(u => u.CurrentHealth >= 5 && u.Row <= 1) * 2;
        int playerFrontlineTanks = playerUnits.Count(u => u.CurrentHealth >= 5 && u.Row >= 2) * 2;

        // Бонус за уязвимых юнитов в тылу (Health <= 2 в рядах 0-1)
        int enemyBacklineSupport = enemyUnits.Count(u => u.CurrentHealth <= 2 && u.Row == 0) * 1;
        int playerBacklineSupport = playerUnits.Count(u => u.CurrentHealth <= 2 && u.Row == 3) * 1;

        // Бонус за контроль углов (защита)
        int enemyCorners = enemyUnits.Count(u => (u.Row == 0 || u.Row == 1) && (u.Column == 0 || u.Column == 2)) * 1;
        int playerCorners = playerUnits.Count(u => (u.Row == 2 || u.Row == 3) && (u.Column == 0 || u.Column == 2)) * 1;

        int score = (enemyStrength - playerStrength) +
                    (enemyCenterControl - playerCenterControl) +
                    (enemyFrontlineTanks - playerFrontlineTanks) +
                    (enemyBacklineSupport - playerBacklineSupport) +
                    (enemyCorners - playerCorners);

        return new PositionEvaluation
        {
            Score = score,
            Reason = $"Сила: {enemyStrength} vs {playerStrength}, Центр: {enemyCenterControl} vs {playerCenterControl}, Танки: {enemyFrontlineTanks} vs {playerFrontlineTanks}"
        };
    }

    /// <summary>
    /// Подсчитывает, сколько путей игрока блокирует клетка.
    /// </summary>
    private int CountBlockedPaths((int Row, int Column) cell, List<Unit> playerUnits, List<Unit> enemyUnits)
    {
        int blockedPaths = 0;
        
        // Простая эвристика: клетки в центре блокируют больше путей
        if (cell.Column >= 1 && cell.Column <= 2)
            blockedPaths += 2;

        // Клетки в рядах 0-1 блокируют продвижение игрока
        if (cell.Row <= 1)
            blockedPaths += 1;

        return blockedPaths;
    }
}
