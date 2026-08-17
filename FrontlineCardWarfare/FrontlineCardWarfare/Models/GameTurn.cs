namespace FrontlineCardWarfare.Models;

/// <summary>
/// Информация о текущем ходе.
/// </summary>
public class GameTurn
{
    /// <summary>
    /// Номер текущего хода.
    /// </summary>
    public int TurnNumber { get; set; }

    /// <summary>
    /// Сейчас ход игрока.
    /// </summary>
    public bool IsPlayerTurn { get; set; }

    /// <summary>
    /// Начинает новый ход.
    /// </summary>
    public void StartNewTurn(bool isPlayerTurn)
    {
        IsPlayerTurn = isPlayerTurn;
        TurnNumber++;
    }

    /// <summary>
    /// Создаёт копию информации о ходе.
    /// </summary>
    public GameTurn Clone()
    {
        return new GameTurn
        {
            TurnNumber = TurnNumber,
            IsPlayerTurn = IsPlayerTurn
        };
    }
}
