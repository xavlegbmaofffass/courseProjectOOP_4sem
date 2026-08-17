using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса для работы с картами.
/// </summary>
public interface ICardService
{
    /// <summary>
    /// Получает все карты.
    /// </summary>
    Task<List<Card>> GetAllCardsAsync();

    /// <summary>
    /// Получает карту по ID.
    /// </summary>
    Task<Card?> GetCardByIdAsync(int id);

    /// <summary>
    /// Добавляет новую карту (администратор).
    /// </summary>
    Task<(bool Success, string? Error)> AddCardAsync(Card card);

    /// <summary>
    /// Обновляет существующую карту (администратор).
    /// </summary>
    Task<(bool Success, string? Error)> UpdateCardAsync(Card card);

    /// <summary>
    /// Удаляет карту по ID (администратор).
    /// </summary>
    Task DeleteCardAsync(int id);

    /// <summary>
    /// Поиск карт по критериям.
    /// </summary>
    Task<List<Card>> SearchCardsAsync(
        string? name = null,
        CardType? type = null,
        int? minAttack = null,
        int? maxAttack = null);

    /// <summary>
    /// Поиск карт по названию и типу (упрощённая версия для AdminViewModel).
    /// </summary>
    Task<List<Card>> SearchCardsAsync(string searchText, CardType filterType);
}
