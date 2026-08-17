using FrontlineCardWarfare.Models;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс контроллера ИИ.
/// </summary>
public interface IAIController
{
    /// <summary>
    /// Выполняет ход ИИ.
    /// </summary>
    Task MakeTurnAsync(IBattleManager battleManager, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оценивает состояние поля.
    /// </summary>
    int EvaluateBoardState(GameState gameState, bool isPlayer);

    /// <summary>
    /// Получает описание текущей стратегии.
    /// </summary>
    string GetStrategyDescription();

    /// <summary>
    /// Оценивает текущую позицию.
    /// </summary>
    PositionEvaluation EvaluatePosition(IBattleManager battleManager);
}