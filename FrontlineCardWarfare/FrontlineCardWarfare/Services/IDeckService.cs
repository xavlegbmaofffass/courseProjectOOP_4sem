using FrontlineCardWarfare.Data;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса для работы с колодами.
/// </summary>
public interface IDeckService
{
    /// <summary>
    /// Создаёт новую колоду.
    /// </summary>
    Task<(bool Success, string? Error, Deck? Deck)> CreateDeckAsync(int userId, string name);

    /// <summary>
    /// Добавляет карту в колоду.
    /// </summary>
    Task<(bool Success, string? Error)> AddCardToDeckAsync(int deckId, int cardId, int quantity);

    /// <summary>
    /// Удаляет карту из колоды.
    /// </summary>
    Task RemoveCardFromDeckAsync(int deckId, int cardId);

    /// <summary>
    /// Получает колоду по ID с картами.
    /// </summary>
    Task<Deck?> GetDeckByIdAsync(int deckId);

    /// <summary>
    /// Получает все колоды пользователя.
    /// </summary>
    Task<List<Deck>> GetAllDecksByUserAsync(int userId);

    /// <summary>
    /// Проверяет валидность колоды (20-30 карт).
    /// </summary>
    (bool IsValid, string Error) ValidateDeck(Deck deck);

    /// <summary>
    /// Удаляет колоду.
    /// </summary>
    Task DeleteDeckAsync(int deckId);

    /// <summary>
    /// Обновляет существующую колоду.
    /// </summary>
    Task<(bool Success, string? Error)> UpdateDeckAsync(int deckId, string name, List<DeckCard> cards);
}
