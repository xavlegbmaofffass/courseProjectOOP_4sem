using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;
using FrontlineCardWarfare.Repositories;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис для работы с картами.
/// </summary>
public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly IUserService _userService;

    /// <summary>
    /// Инициализирует новый экземпляр CardService.
    /// </summary>
    public CardService(ICardRepository cardRepository, IUserService userService)
    {
        _cardRepository = cardRepository;
        _userService = userService;
    }

    /// <summary>
    /// Получает все карты.
    /// </summary>
    public async Task<List<Card>> GetAllCardsAsync()
    {
        return await _cardRepository.GetAllAsync();
    }

    /// <summary>
    /// Получает карту по ID.
    /// </summary>
    public async Task<Card?> GetCardByIdAsync(int id)
    {
        return await _cardRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Добавляет новую карту (администратор).
    /// </summary>
    public async Task<(bool Success, string? Error)> AddCardAsync(Card card)
    {
        // Проверка прав администратора
        if (_userService.CurrentUser?.Role != UserRole.Admin)
        {
            return (false, "Требуется роль администратора");
        }

        // Валидация названия карты
        if (!Validator.ValidateCardName(card.Name))
        {
            return (false, "Название карты должно быть от 1 до 50 символов");
        }

        // Валидация числовых параметров
        if (card.Attack < 0 || card.Health < 0 || card.Range < 0)
        {
            return (false, "Все числовые параметры должны быть неотрицательными");
        }

        await _cardRepository.AddAsync(card);
        return (true, null);
    }

    /// <summary>
    /// Обновляет существующую карту (администратор).
    /// </summary>
    public async Task<(bool Success, string? Error)> UpdateCardAsync(Card card)
    {
        // Проверка прав администратора
        if (_userService.CurrentUser?.Role != UserRole.Admin)
        {
            return (false, "Требуется роль администратора");
        }

        // Валидация названия карты
        if (!Validator.ValidateCardName(card.Name))
        {
            return (false, "Название карты должно быть от 1 до 50 символов");
        }

        // Валидация числовых параметров
        if (card.Attack < 0 || card.Health < 0 || card.Range < 0)
        {
            return (false, "Все числовые параметры должны быть неотрицательными");
        }

        await _cardRepository.UpdateAsync(card);
        return (true, null);
    }

    /// <summary>
    /// Удаляет карту по ID (администратор).
    /// </summary>
    public async Task DeleteCardAsync(int id)
    {
        if (_userService.CurrentUser?.Role != UserRole.Admin)
        {
            return;
        }

        await _cardRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Поиск карт по критериям.
    /// </summary>
    public async Task<List<Card>> SearchCardsAsync(
        string? name = null,
        CardType? type = null,
        int? minAttack = null,
        int? maxAttack = null)
    {
        return await _cardRepository.SearchAsync(name, type, minAttack, maxAttack);
    }

    /// <summary>
    /// Поиск карт по названию и типу (упрощённая версия для AdminViewModel).
    /// </summary>
    public async Task<List<Card>> SearchCardsAsync(string searchText, CardType filterType)
    {
        return await _cardRepository.SearchAsync(
            string.IsNullOrWhiteSpace(searchText) ? null : searchText,
            filterType,
            null,
            null);
    }
}
