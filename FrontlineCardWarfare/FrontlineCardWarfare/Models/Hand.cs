using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Models;

/// <summary>
/// Представляет руку игрока с картами.
/// </summary>
public class Hand
{
    /// <summary>
    /// Карты в руке.
    /// </summary>
    public List<Card> Cards { get; set; } = new();

    /// <summary>
    /// Максимальный размер руки.
    /// </summary>
    public const int MaxHandSize = 10;

    /// <summary>
    /// Количество карт в руке.
    /// </summary>
    public int Count => Cards.Count;

    /// <summary>
    /// Добавляет карту в руку.
    /// </summary>
    public void AddCard(Card card)
    {
        if (Cards.Count < MaxHandSize)
        {
            Cards.Add(card);
        }
    }

    /// <summary>
    /// Удаляет карту из руки.
    /// </summary>
    public void RemoveCard(Card card)
    {
        Cards.Remove(card);
    }

    /// <summary>
    /// Удаляет карту по индексу.
    /// </summary>
    public void RemoveCardAt(int index)
    {
        if (index >= 0 && index < Cards.Count)
        {
            Cards.RemoveAt(index);
        }
    }

    /// <summary>
    /// Очищает руку.
    /// </summary>
    public void Clear()
    {
        Cards.Clear();
    }

    /// <summary>
    /// Создаёт копию руки.
    /// </summary>
    public Hand Clone()
    {
        return new Hand
        {
            Cards = new List<Card>(Cards)
        };
    }
}
