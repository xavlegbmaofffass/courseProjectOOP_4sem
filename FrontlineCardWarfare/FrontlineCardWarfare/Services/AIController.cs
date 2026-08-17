using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Services.Strategies;
using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Контроллер искусственного интеллекта с поддержкой стратегий.
/// </summary>
public class AIController : IAIController
{
    private readonly IAIStrategy _strategy;
    private readonly string _difficulty;
    private readonly Random _random = new();
    private readonly ILogger? _logger;

    /// <summary>
    /// Инициализирует новый экземпляр AIController.
    /// </summary>
    public AIController(string difficulty, ILogger? logger = null)
    {
        _difficulty = difficulty.ToLower();
        _logger = logger;
        _strategy = CreateStrategy(_difficulty);
    }

    /// <summary>
    /// Инициализирует AIController с конкретной стратегией.
    /// </summary>
    public AIController(IAIStrategy strategy, string difficulty, ILogger? logger = null)
    {
        _strategy = strategy;
        _difficulty = difficulty;
        _logger = logger;
    }

    /// <summary>
    /// Создаёт стратегию на основе уровня сложности.
    /// </summary>
    private static IAIStrategy CreateStrategy(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "easy" => new EasyAIStrategy(),
            "medium" => new MediumAIStrategy(),
            "hard" => new HardAIStrategy(),
            _ => new MediumAIStrategy()
        };
    }

    /// <summary>
    /// Выполняет ход ИИ с анализом стратегии и выполнением нескольких действий.
    /// </summary>
    public async Task MakeTurnAsync(IBattleManager battleManager, CancellationToken cancellationToken = default)
    {
        var gameState = battleManager.GameState;
        var evaluation = _strategy.EvaluateBoard(battleManager);
        
        Log($"Начало хода ИИ ({_difficulty}). Оценка позиции: {evaluation.Score}");

        // 1. ФАЗА РОЗЫГРЫША КАРТ
        if (gameState.EnemyHand.Count > 0)
        {
            var availableCells = battleManager.GetAvailableMovesForPlacement();
            if (availableCells.Count > 0)
            {
                // Стратегия выбирает лучшую карту
                var cardToPlay = _strategy.SelectCardToPlay(gameState.EnemyHand.Cards.ToList(), availableCells);
                
                if (cardToPlay != null)
                {
                    // Ограничения по рядам (Ближники во второй ряд, Дальники в первый для ИИ)
                    bool isMelee = cardToPlay.Range <= 1;
                    var validForCard = availableCells.Where(c => isMelee ? c.Row == 1 : c.Row == 0).ToList();
                    if (validForCard.Count == 0) validForCard = availableCells;

                    var bestCell = _strategy.SelectPlacementCell(cardToPlay, validForCard, battleManager);
                    
                    Log($"Действие: Розыгрыш карты '{cardToPlay.Name}' в ({bestCell.Row}, {bestCell.Column})");
                    var playResult = await battleManager.PlayCardAsync(cardToPlay, bestCell.Row, bestCell.Column);
                    
                    if (playResult.Success)
                    {
                        await Task.Delay(800, cancellationToken);
                    }
                }
            }
        }

        // 3. ФАЗА АТАКИ
        var unitsToAttack = gameState.Board.GetAllAliveUnits()
            .Where(u => !u.IsPlayer && u.CanAttack && !u.HasAttacked)
            .ToList();

        foreach (var unit in unitsToAttack)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var availableTargets = battleManager.GetAvailableTargets(unit.Id);
            if (availableTargets.Count > 0)
            {
                var targetId = _strategy.SelectTarget(unit, availableTargets, battleManager);
                var targetUnit = battleManager.GetUnitById(targetId);

                if (targetUnit != null)
                {
                    Log($"Действие: Атака '{unit.Name}' -> '{targetUnit.Name}'");
                    var attackResult = battleManager.Attack(unit.Id, targetId);
                    if (attackResult.Success)
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
        }

        Log($"Ход ИИ ({_difficulty}) завершён");
    }

    /// <summary>
    /// Розыгрыш карт ИИ (устарело, заменено на логику одной карты в MakeTurnAsync).
    /// </summary>
    private async Task PlayCardsAsync(IBattleManager battleManager, List<Card> cards, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Логирование действий ИИ.
    /// </summary>
    private void Log(string message)
    {
        if (_logger != null)
        {
            _logger.LogInformation($"[AI({_difficulty})] {message}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[AI({_difficulty})] {message}");
        }
    }

    /// <summary>
    /// Получает описание текущей стратегии.
    /// </summary>
    public string GetStrategyDescription() => _strategy.GetDescription();

    /// <summary>
    /// Оценивает текущую позицию.
    /// </summary>
    public PositionEvaluation EvaluatePosition(IBattleManager battleManager) => _strategy.EvaluateBoard(battleManager);

    /// <summary>
    /// Оценивает состояние поля (для совместимости).
    /// </summary>
    public int EvaluateBoardState(GameState gameState, bool isPlayer)
    {
        // Простая оценка: разница сил
        var enemyUnits = gameState.Board.GetPlayerUnits(!isPlayer);
        var playerUnits = gameState.Board.GetPlayerUnits(isPlayer);
        return enemyUnits.Sum(u => u.Attack + u.CurrentHealth / 2) - playerUnits.Sum(u => u.Attack + u.CurrentHealth / 2);
    }
}

/// <summary>
/// Интерфейс для логирования.
/// </summary>
public interface ILogger
{
    void LogInformation(string message);
}
