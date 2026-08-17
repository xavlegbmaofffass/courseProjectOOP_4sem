using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с картами.
/// </summary>
public interface ICardRepository
{
    /// <summary>
    /// Получает все карты.
    /// </summary>
    Task<List<Card>> GetAllAsync();

    /// <summary>
    /// Получает карту по ID.
    /// </summary>
    Task<Card?> GetByIdAsync(int id);

    /// <summary>
    /// Добавляет новую карту.
    /// </summary>
    Task AddAsync(Card card);

    /// <summary>
    /// Обновляет существующую карту.
    /// </summary>
    Task UpdateAsync(Card card);

    /// <summary>
    /// Удаляет карту по ID.
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Поиск карт по критериям.
    /// </summary>
    Task<List<Card>> SearchAsync(
        string? name = null,
        CardType? type = null,
        int? minAttack = null,
        int? maxAttack = null);
}
