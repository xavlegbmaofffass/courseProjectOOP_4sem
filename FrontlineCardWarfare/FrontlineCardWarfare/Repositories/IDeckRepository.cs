using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с колодами.
/// </summary>
public interface IDeckRepository
{
    /// <summary>
    /// Получает колоду по ID.
    /// </summary>
    Task<Deck?> GetByIdAsync(int deckId);

    /// <summary>
    /// Получает все колоды пользователя.
    /// </summary>
    Task<List<Deck>> GetAllByUserAsync(int userId);

    /// <summary>
    /// Добавляет новую колоду.
    /// </summary>
    Task AddAsync(Deck deck);

    /// <summary>
    /// Обновляет колоду.
    /// </summary>
    Task UpdateAsync(Deck deck);

    /// <summary>
    /// Удаляет колоду.
    /// </summary>
    Task DeleteAsync(int deckId);

    /// <summary>
    /// Добавляет карту в колоду.
    /// </summary>
    Task AddCardToDeckAsync(DeckCard deckCard);

    /// <summary>
    /// Удаляет карту из колоды.
    /// </summary>
    Task RemoveCardFromDeckAsync(int deckId, int cardId);

    /// <summary>
    /// Получает DeckCard по ID.
    /// </summary>
    Task<DeckCard?> GetDeckCardAsync(int deckId, int cardId);
}
