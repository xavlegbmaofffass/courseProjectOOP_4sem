using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Repositories;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис для работы с колодами.
/// </summary>
public class DeckService : IDeckService
{
    private readonly IDeckRepository _deckRepository;
    private readonly IUserService _userService;
    private readonly int _minDeckSize = 20;
    private readonly int _maxDeckSize = 30;

    /// <summary>
    /// Инициализирует новый экземпляр DeckService.
    /// </summary>
    public DeckService(IDeckRepository deckRepository, IUserService userService)
    {
        _deckRepository = deckRepository;
        _userService = userService;
    }

    /// <summary>
    /// Создаёт новую колоду.
    /// </summary>
    public async Task<(bool Success, string? Error, Deck? Deck)> CreateDeckAsync(int userId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Название колоды не может быть пустым", null);
        }

        if (name.Length > 50)
        {
            return (false, "Название колоды не может превышать 50 символов", null);
        }

        var deck = new Deck
        {
            UserId = userId,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            DeckCards = new List<DeckCard>()
        };

        await _deckRepository.AddAsync(deck);

        // Получаем полную информацию о колоде
        var createdDeck = await _deckRepository.GetByIdAsync(deck.Id);

        return (true, null, createdDeck);
    }

    /// <summary>
    /// Добавляет карту в колоду.
    /// </summary>
    public async Task<(bool Success, string? Error)> AddCardToDeckAsync(int deckId, int cardId, int quantity)
    {
        var deck = await _deckRepository.GetByIdAsync(deckId);
        if (deck == null)
        {
            return (false, "Колода не найдена");
        }

        // Проверка владельца колоды
        if (deck.UserId != _userService.CurrentUser?.Id && _userService.CurrentUser?.Role != UserRole.Admin)
        {
            return (false, "Вы не владеете этой колодой");
        }

        // Проверка количества карт в колоде
        var currentCardCount = deck.DeckCards.Sum(dc => dc.Quantity);
        if (currentCardCount + quantity > _maxDeckSize)
        {
            return (false, $"Превышен максимальный размер колоды ({_maxDeckSize} карт)");
        }

        await _deckRepository.AddCardToDeckAsync(new DeckCard
        {
            DeckId = deckId,
            CardId = cardId,
            Quantity = quantity
        });

        return (true, null);
    }

    /// <summary>
    /// Удаляет карту из колоды.
    /// </summary>
    public async Task RemoveCardFromDeckAsync(int deckId, int cardId)
    {
        var deck = await _deckRepository.GetByIdAsync(deckId);
        if (deck == null)
        {
            return;
        }

        // Проверка владельца колоды
        if (deck.UserId != _userService.CurrentUser?.Id && _userService.CurrentUser?.Role != UserRole.Admin)
        {
            return;
        }

        await _deckRepository.RemoveCardFromDeckAsync(deckId, cardId);
    }

    /// <summary>
    /// Получает колоду по ID с картами.
    /// </summary>
    public async Task<Deck?> GetDeckByIdAsync(int deckId)
    {
        return await _deckRepository.GetByIdAsync(deckId);
    }

    /// <summary>
    /// Получает все колоды пользователя.
    /// </summary>
    public async Task<List<Deck>> GetAllDecksByUserAsync(int userId)
    {
        return await _deckRepository.GetAllByUserAsync(userId);
    }

    /// <summary>
    /// Проверяет валидность колоды (20-30 карт).
    /// </summary>
    public (bool IsValid, string Error) ValidateDeck(Deck deck)
    {
        var cardCount = deck.DeckCards.Sum(dc => dc.Quantity);

        if (cardCount < _minDeckSize)
        {
            return (false, $"В колоде недостаточно карт. Минимум: {_minDeckSize}, сейчас: {cardCount}");
        }

        if (cardCount > _maxDeckSize)
        {
            return (false, $"В колоде слишком много карт. Максимум: {_maxDeckSize}, сейчас: {cardCount}");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Удаляет колоду.
    /// </summary>
    public async Task DeleteDeckAsync(int deckId)
    {
        var deck = await _deckRepository.GetByIdAsync(deckId);
        if (deck == null)
        {
            return;
        }

        // Проверка владельца колоды
        if (deck.UserId != _userService.CurrentUser?.Id && _userService.CurrentUser?.Role != UserRole.Admin)
        {
            return;
        }

        await _deckRepository.DeleteAsync(deckId);
    }

    /// <summary>
    /// Обновляет существующую колоду.
    /// </summary>
    public async Task<(bool Success, string? Error)> UpdateDeckAsync(int deckId, string name, List<DeckCard> cards)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Название колоды не может быть пустым");
        }

        var deck = await _deckRepository.GetByIdAsync(deckId);
        if (deck == null)
        {
            return (false, "Колода не найдена");
        }

        // Проверка владельца колоды
        if (deck.UserId != _userService.CurrentUser?.Id && _userService.CurrentUser?.Role != UserRole.Admin)
        {
            return (false, "Вы не владеете этой колодой");
        }

        deck.Name = name;
        
        // Очищаем старые карты и добавляем новые
        // Это можно сделать более эффективно, но для простоты — так
        foreach (var oldCard in deck.DeckCards.ToList())
        {
            await _deckRepository.RemoveCardFromDeckAsync(deckId, oldCard.CardId);
        }

        foreach (var newCard in cards)
        {
            await _deckRepository.AddCardToDeckAsync(new DeckCard
            {
                DeckId = deckId,
                CardId = newCard.CardId,
                Quantity = newCard.Quantity
            });
        }

        await _deckRepository.UpdateAsync(deck);
        return (true, null);
    }
}
