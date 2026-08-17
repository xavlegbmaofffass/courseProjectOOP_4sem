using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Repositories;

/// <summary>
/// Репозиторий для работы с картами.
/// </summary>
public class CardRepository : ICardRepository
{
    private readonly GameDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр CardRepository.
    /// </summary>
    public CardRepository(GameDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает все карты.
    /// </summary>
    public async Task<List<Card>> GetAllAsync()
    {
        return await _context.Cards.ToListAsync();
    }

    /// <summary>
    /// Получает карту по ID.
    /// </summary>
    public async Task<Card?> GetByIdAsync(int id)
    {
        return await _context.Cards.FindAsync(id);
    }

    /// <summary>
    /// Добавляет новую карту.
    /// </summary>
    public async Task AddAsync(Card card)
    {
        await _context.Cards.AddAsync(card);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет существующую карту.
    /// </summary>
    public async Task UpdateAsync(Card card)
    {
        // Используя Attach и Setting State, избегаем конфликта отслеживания
        var existingCard = await _context.Cards.FindAsync(card.Id);
        if (existingCard != null)
        {
            _context.Entry(existingCard).CurrentValues.SetValues(card);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Удаляет карту по ID.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var card = await _context.Cards.FindAsync(id);
        if (card != null)
        {
            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Поиск карт по критериям.
    /// </summary>
    public async Task<List<Card>> SearchAsync(
        string? name = null,
        CardType? type = null,
        int? minAttack = null,
        int? maxAttack = null)
    {
        var query = _context.Cards.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.CardType == type.Value);
        }

        if (minAttack.HasValue)
        {
            query = query.Where(c => c.Attack >= minAttack.Value);
        }

        if (maxAttack.HasValue)
        {
            query = query.Where(c => c.Attack <= maxAttack.Value);
        }

        return await query.ToListAsync();
    }
}
