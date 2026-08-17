using FrontlineCardWarfare.Data;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Repositories;

/// <summary>
/// Репозиторий для работы с колодами.
/// </summary>
public class DeckRepository : IDeckRepository
{
    private readonly GameDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр DeckRepository.
    /// </summary>
    public DeckRepository(GameDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает колоду по ID.
    /// </summary>
    public async Task<Deck?> GetByIdAsync(int deckId)
    {
        return await _context.Decks
            .Include(d => d.DeckCards)
                .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == deckId);
    }

    /// <summary>
    /// Получает все колоды пользователя.
    /// </summary>
    public async Task<List<Deck>> GetAllByUserAsync(int userId)
    {
        return await _context.Decks
            .Include(d => d.DeckCards)
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Добавляет новую колоду.
    /// </summary>
    public async Task AddAsync(Deck deck)
    {
        await _context.Decks.AddAsync(deck);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Обновляет колоду.
    /// </summary>
    public async Task UpdateAsync(Deck deck)
    {
        _context.Decks.Update(deck);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Удаляет колоду.
    /// </summary>
    public async Task DeleteAsync(int deckId)
    {
        var deck = await _context.Decks.FindAsync(deckId);
        if (deck != null)
        {
            _context.Decks.Remove(deck);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Добавляет карту в колоду.
    /// </summary>
    public async Task AddCardToDeckAsync(DeckCard deckCard)
    {
        // Проверяем, есть ли уже такая карта в колоде
        var existing = await _context.DeckCards
            .FirstOrDefaultAsync(dc => dc.DeckId == deckCard.DeckId && dc.CardId == deckCard.CardId);

        if (existing != null)
        {
            existing.Quantity += deckCard.Quantity;
        }
        else
        {
            await _context.DeckCards.AddAsync(deckCard);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Удаляет карту из колоды.
    /// </summary>
    public async Task RemoveCardFromDeckAsync(int deckId, int cardId)
    {
        var deckCard = await _context.DeckCards
            .FirstOrDefaultAsync(dc => dc.DeckId == deckId && dc.CardId == cardId);

        if (deckCard != null)
        {
            _context.DeckCards.Remove(deckCard);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Получает DeckCard по ID.
    /// </summary>
    public async Task<DeckCard?> GetDeckCardAsync(int deckId, int cardId)
    {
        return await _context.DeckCards
            .FirstOrDefaultAsync(dc => dc.DeckId == deckId && dc.CardId == cardId);
    }
}
