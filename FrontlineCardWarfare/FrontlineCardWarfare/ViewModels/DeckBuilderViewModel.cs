using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для конструктора колод.
/// </summary>
public class DeckBuilderViewModel : ViewModelBase
{
    private readonly ICardService _cardService;
    private readonly IDeckService _deckService;
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private ObservableCollection<Card> _availableCards = new();
    private ObservableCollection<DeckCard> _deckCards = new();
    private string _deckName = string.Empty;
    private Card? _selectedCard;
    private DeckCard? _selectedDeckCard;
    private string _validationMessage = string.Empty;
    private bool _isValid;
    private string _currentDeckName = "Новая колода";
    private int _selectedTypeIndex;
    private string _searchText = string.Empty;
    private bool _isSaveDialogVisible;
    private ObservableCollection<Deck> _userDecks = new();
    private Deck? _selectedUserDeck;
    private bool _isEditing;
    private bool _isEditModeVisible;

    /// <summary>
    /// Инициализирует новый экземпляр DeckBuilderViewModel.
    /// </summary>
    public DeckBuilderViewModel(
        ICardService cardService,
        IDeckService deckService,
        IUserService userService,
        INavigationService navigationService)
    {
        _cardService = cardService;
        _deckService = deckService;
        _userService = userService;
        _navigationService = navigationService;

        CardTypeFilters = new ObservableCollection<string>
        {
            "Все типы",
            "Ближний бой",
            "Дальний бой",
            "Осадная",
            "Поддержка",
            "Особенная"
        };

        LoadCardsCommand = new AsyncRelayCommand(LoadCardsAsync);
        AddCardToDeckCommand = new RelayCommand(AddCardToDeck, CanAddCardToDeck);
        RemoveCardFromDeckCommand = new RelayCommand(RemoveCardFromDeck, CanRemoveCardFromDeck);
        SaveDeckCommand = new RelayCommand(ShowSaveDialog, CanSaveDeck);
        ConfirmSaveDeckCommand = new AsyncRelayCommand(ConfirmSaveDeckAsync, CanConfirmSaveDeck);
        CancelSaveDialogCommand = new RelayCommand(CancelSaveDialog);
        ValidateDeckCommand = new RelayCommand(ValidateDeck);
        BackToMenuCommand = new RelayCommand(BackToMenu);
        ClearDeckCommand = new RelayCommand(ClearDeck, CanClearDeck);
        EditExistingDeckCommand = new RelayCommand(ToggleEditMode);
    }

    /// <summary>
    /// Список колод пользователя.
    /// </summary>
    public ObservableCollection<Deck> UserDecks
    {
        get => _userDecks;
        set => SetProperty(ref _userDecks, value);
    }

    /// <summary>
    /// Выбранная существующая колода для редактирования.
    /// </summary>
    public Deck? SelectedUserDeck
    {
        get => _selectedUserDeck;
        set
        {
            if (SetProperty(ref _selectedUserDeck, value))
            {
                LoadSelectedDeck();
            }
        }
    }

    /// <summary>
    /// Режим редактирования.
    /// </summary>
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            if (SetProperty(ref _isEditing, value))
            {
                OnPropertyChanged(nameof(SaveButtonText));
                CurrentDeckName = _isEditing ? "Редактирование колоды" : "Новая колода";
            }
        }
    }

    /// <summary>
    /// Видимость панели выбора колоды.
    /// </summary>
    public bool IsEditModeVisible
    {
        get => _isEditModeVisible;
        set => SetProperty(ref _isEditModeVisible, value);
    }

    /// <summary>
    /// Текст кнопки сохранения.
    /// </summary>
    public string SaveButtonText => IsEditing ? "Сохранить изменения" : "Сохранить колоду";

    /// <summary>
    /// Доступные фильтры по типу карты.
    /// </summary>
    public ObservableCollection<string> CardTypeFilters { get; }

    /// <summary>
    /// Выбранный индекс фильтра по типу карты.
    /// </summary>
    public int SelectedTypeIndex
    {
        get => _selectedTypeIndex;
        set
        {
            if (SetProperty(ref _selectedTypeIndex, value))
            {
                _ = LoadCardsAsync(null);
            }
        }
    }

    /// <summary>
    /// Текст поиска.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                _ = LoadCardsAsync(null);
            }
        }
    }

    /// <summary>
    /// Видимость диалога сохранения колоды.
    /// </summary>
    public bool IsSaveDialogVisible
    {
        get => _isSaveDialogVisible;
        set => SetProperty(ref _isSaveDialogVisible, value);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Конструктор колод";

    /// <summary>
    /// Доступные карты для добавления.
    /// </summary>
    public ObservableCollection<Card> AvailableCards
    {
        get => _availableCards;
        set => SetProperty(ref _availableCards, value);
    }

    /// <summary>
    /// Карты в текущей колоде.
    /// </summary>
    public ObservableCollection<DeckCard> DeckCards
    {
        get => _deckCards;
        set => SetProperty(ref _deckCards, value);
    }

    /// <summary>
    /// Название колоды.
    /// </summary>
    public string DeckName
    {
        get => _deckName;
        set
        {
            if (SetProperty(ref _deckName, value))
            {
                (ConfirmSaveDeckCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Текущее имя колоды для отображения.
    /// </summary>
    public string CurrentDeckName
    {
        get => _currentDeckName;
        private set => SetProperty(ref _currentDeckName, value);
    }

    /// <summary>
    /// Выбранная карта из доступных.
    /// </summary>
    public Card? SelectedCard
    {
        get => _selectedCard;
        set
        {
            if (SetProperty(ref _selectedCard, value))
            {
                (AddCardToDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Выбранная карта в колоде.
    /// </summary>
    public DeckCard? SelectedDeckCard
    {
        get => _selectedDeckCard;
        set
        {
            if (SetProperty(ref _selectedDeckCard, value))
            {
                (RemoveCardFromDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Сообщение валидации.
    /// </summary>
    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    /// <summary>
    /// Колода валидна.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }

    /// <summary>
    /// Общее количество карт в колоде.
    /// </summary>
    public int CardCount => DeckCards.Sum(dc => dc.Quantity);

    /// <summary>
    /// Команда загрузки карт.
    /// </summary>
    public ICommand LoadCardsCommand { get; }

    /// <summary>
    /// Команда добавления карты в колоду.
    /// </summary>
    public ICommand AddCardToDeckCommand { get; }

    /// <summary>
    /// Команда удаления карты из колоды.
    /// </summary>
    public ICommand RemoveCardFromDeckCommand { get; }

    /// <summary>
    /// Команда сохранения колоды (показ диалога).
    /// </summary>
    public ICommand SaveDeckCommand { get; }

    /// <summary>
    /// Команда подтверждения сохранения колоды.
    /// </summary>
    public ICommand ConfirmSaveDeckCommand { get; }

    /// <summary>
    /// Команда отмены диалога сохранения.
    /// </summary>
    public ICommand CancelSaveDialogCommand { get; }

    /// <summary>
    /// Команда проверки колоды.
    /// </summary>
    public ICommand ValidateDeckCommand { get; }

    /// <summary>
    /// Команда возврата в главное меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Команда очистки колоды.
    /// </summary>
    public ICommand ClearDeckCommand { get; }

    /// <summary>
    /// Команда перехода в режим редактирования существующей колоды.
    /// </summary>
    public ICommand EditExistingDeckCommand { get; }

    /// <summary>
    /// Доступность добавления карты.
    /// </summary>
    private bool CanAddCardToDeck(object? parameter)
    {
        return parameter is Card && CardCount < 30;
    }

    /// <summary>
    /// Доступность удаления карты.
    /// </summary>
    private bool CanRemoveCardFromDeck(object? parameter)
    {
        return parameter is DeckCard;
    }

    /// <summary>
    /// Доступность очистки колоды.
    /// </summary>
    private bool CanClearDeck(object? parameter)
    {
        return CardCount > 0;
    }

    /// <summary>
    /// Доступность сохранения колоды.
    /// </summary>
    private bool CanSaveDeck(object? parameter)
    {
        return IsValid && CardCount >= 20;
    }

    /// <summary>
    /// Доступность подтверждения сохранения.
    /// </summary>
    private bool CanConfirmSaveDeck(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(DeckName);
    }

    /// <summary>
    /// Показывает диалог сохранения колоды.
    /// </summary>
    private void ShowSaveDialog(object? parameter)
    {
        if (IsEditing && SelectedUserDeck != null)
        {
            DeckName = SelectedUserDeck.Name;
        }
        IsSaveDialogVisible = true;
    }

    /// <summary>
    /// Переключает режим редактирования.
    /// </summary>
    private async void ToggleEditMode(object? parameter)
    {
        IsEditModeVisible = !IsEditModeVisible;
        if (IsEditModeVisible)
        {
            await LoadUserDecksAsync();
        }
        else
        {
            IsEditing = false;
            SelectedUserDeck = null;
            DeckCards.Clear();
            DeckName = string.Empty;
            OnPropertyChanged(nameof(CardCount));
            ValidateDeck(null);
        }
    }

    /// <summary>
    /// Загружает колоды пользователя.
    /// </summary>
    private async Task LoadUserDecksAsync()
    {
        if (_userService.CurrentUser == null && !_userService.IsGuestMode) return;

        List<Deck> decks;
        if (_userService.IsGuestMode)
        {
            decks = _userService.GuestDecks.ToList();
        }
        else if (_userService.CurrentUser != null)
        {
            decks = await _deckService.GetAllDecksByUserAsync(_userService.CurrentUser.Id);
        }
        else
        {
            decks = new List<Deck>();
        }

        UserDecks.Clear();
        foreach (var deck in decks)
        {
            UserDecks.Add(deck);
        }
    }

    /// <summary>
    /// Загружает выбранную колоду в конструктор.
    /// </summary>
    private void LoadSelectedDeck()
    {
        if (SelectedUserDeck == null) return;

        IsEditing = true;
        DeckName = SelectedUserDeck.Name;
        DeckCards.Clear();
        
        // Клонируем карты, чтобы не менять оригинал до сохранения
        foreach (var deckCard in SelectedUserDeck.DeckCards)
        {
            DeckCards.Add(new DeckCard
            {
                CardId = deckCard.CardId,
                Card = deckCard.Card,
                Quantity = deckCard.Quantity,
                DeckId = SelectedUserDeck.Id
            });
        }

        OnPropertyChanged(nameof(CardCount));
        ValidateDeck(null);
    }

    /// <summary>
    /// Отменяет диалог сохранения.
    /// </summary>
    private void CancelSaveDialog(object? parameter)
    {
        IsSaveDialogVisible = false;
        DeckName = string.Empty;
    }

    /// <summary>
    /// Загружает доступные карты.
    /// </summary>
    private async Task LoadCardsAsync(object? parameter)
    {
        CardType? filterType = SelectedTypeIndex switch
        {
            1 => CardType.Melee,
            2 => CardType.Ranged,
            3 => CardType.Siege,
            4 => CardType.Support,
            5 => CardType.Special,
            _ => null
        };

        var cards = await _cardService.SearchCardsAsync(
            name: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
            type: filterType,
            minAttack: null,
            maxAttack: null);

        AvailableCards.Clear();
        foreach (var card in cards)
        {
            AvailableCards.Add(card);
        }
    }

    /// <summary>
    /// Добавляет карту в колоду.
    /// </summary>
    private void AddCardToDeck(object? parameter)
    {
        if (parameter is not Card card) return;

        var existingCard = DeckCards.FirstOrDefault(dc => dc.CardId == card.Id);

        if (existingCard != null)
        {
            if (existingCard.Quantity < 4) // Максимум 4 копии одной карты
            {
                existingCard.Quantity++;
            }
        }
        else
        {
            DeckCards.Add(new DeckCard
            {
                CardId = card.Id,
                Card = card,
                Quantity = 1
            });
        }

        OnPropertyChanged(nameof(CardCount));
        ValidateDeck(null);
        (SaveDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Удаляет карту из колоды.
    /// </summary>
    private void RemoveCardFromDeck(object? parameter)
    {
        if (parameter is not DeckCard deckCard) return;

        if (deckCard.Quantity > 1)
        {
            deckCard.Quantity--;
        }
        else
        {
            DeckCards.Remove(deckCard);
        }

        OnPropertyChanged(nameof(CardCount));
        ValidateDeck(null);
        (SaveDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Проверяет валидность колоды.
    /// </summary>
    private void ValidateDeck(object? parameter)
    {
        var tempDeck = new Deck
        {
            Name = DeckName,
            DeckCards = DeckCards.ToList()
        };

        var result = _deckService.ValidateDeck(tempDeck);
        IsValid = result.IsValid;
        ValidationMessage = result.Error;

        if (IsValid)
        {
            ValidationMessage = $"Колода готова! Карт: {CardCount} (20-30)";
        }

        (SaveDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AddCardToDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RemoveCardFromDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Сохраняет колоду.
    /// </summary>
    private async Task ConfirmSaveDeckAsync(object? parameter)
    {
        if (_userService.CurrentUser == null && !_userService.IsGuestMode)
        {
            ValidationMessage = "Требуется авторизация";
            IsSaveDialogVisible = false;
            return;
        }

        // Гостевой режим — сохраняем в память
        if (_userService.IsGuestMode)
        {
            if (IsEditing && SelectedUserDeck != null)
            {
                // Обновляем существующую гостевую колоду
                SelectedUserDeck.Name = DeckName;
                SelectedUserDeck.DeckCards = DeckCards.Select(dc => new DeckCard
                {
                    CardId = dc.CardId,
                    Quantity = dc.Quantity,
                    Card = dc.Card,
                    DeckId = SelectedUserDeck.Id
                }).ToList();

                ValidationMessage = "Изменения успешно сохранены (гостевой режим)!";
            }
            else
            {
                var guestDeck = new Deck
                {
                    Id = -(_userService.GuestDecks.Count + 1), // Отрицательные ID для гостевых колод
                    UserId = 0,
                    Name = DeckName,
                    CreatedAt = DateTime.UtcNow,
                    DeckCards = DeckCards.Select(dc => new DeckCard
                    {
                        CardId = dc.CardId,
                        Quantity = dc.Quantity,
                        Card = dc.Card
                    }).ToList()
                };

                foreach (var dc in guestDeck.DeckCards) dc.DeckId = guestDeck.Id;

                _userService.GuestDecks.Add(guestDeck);
                ValidationMessage = "Колода успешно сохранена (гостевой режим)!";
            }

            IsSaveDialogVisible = false;
            DeckName = string.Empty;
            DeckCards.Clear();
            IsEditing = false;
            IsEditModeVisible = false;
            SelectedUserDeck = null;
            OnPropertyChanged(nameof(CardCount));
            ValidateDeck(null);

            System.Windows.MessageBox.Show(
                $"Колода успешно сохранена!\n(Колода будет удалена при выходе из гостевого режима)",
                "Успех",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            return;
        }

        if (_userService.CurrentUser == null) return;

        bool success;
        string? error;

        if (IsEditing && SelectedUserDeck != null)
        {
            var result = await _deckService.UpdateDeckAsync(SelectedUserDeck.Id, DeckName, DeckCards.ToList());
            success = result.Success;
            error = result.Error;
        }
        else
        {
            var result = await _deckService.CreateDeckAsync(_userService.CurrentUser.Id, DeckName);
            success = result.Success;
            error = result.Error;

            if (success && result.Deck != null)
            {
                foreach (var deckCard in DeckCards)
                {
                    await _deckService.AddCardToDeckAsync(result.Deck.Id, deckCard.CardId, deckCard.Quantity);
                }
            }
        }

        if (success)
        {
            ValidationMessage = IsEditing ? "Изменения успешно сохранены!" : "Колода успешно сохранена!";
            IsSaveDialogVisible = false;
            DeckName = string.Empty;
            DeckCards.Clear();
            IsEditing = false;
            IsEditModeVisible = false;
            SelectedUserDeck = null;
            OnPropertyChanged(nameof(CardCount));
            ValidateDeck(null);

            System.Windows.MessageBox.Show(
                "Колода успешно сохранена!",
                "Успех",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        else
        {
            ValidationMessage = error ?? "Ошибка сохранения колоды";
        }
    }

    /// <summary>
    /// Вызывается при активации ViewModel.
    /// </summary>
    public override async void OnActivated()
    {
        base.OnActivated();
        await LoadCardsAsync(null);
        ValidateDeck(null);
    }

    /// <summary>
    /// Очищает колоду.
    /// </summary>
    private void ClearDeck(object? parameter)
    {
        DeckCards.Clear();
        OnPropertyChanged(nameof(CardCount));
        ValidateDeck(null);
        (SaveDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ClearDeckCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.GoBack();
    }
}