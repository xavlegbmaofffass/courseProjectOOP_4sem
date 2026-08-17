using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для просмотра коллекции карт.
/// </summary>
public class CollectionViewModel : ViewModelBase
{
    private readonly ICardService _cardService;
    private readonly INavigationService _navigationService;
    private ObservableCollection<Card> _cards = new();
    private Card? _selectedCard;
    private string _searchText = string.Empty;
    private CardType? _filterType;
    private bool _isLoading;
    private int _selectedTypeIndex;

    /// <summary>
    /// Инициализирует новый экземпляр CollectionViewModel.
    /// </summary>
    public CollectionViewModel(ICardService cardService, INavigationService navigationService)
    {
        _cardService = cardService;
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

        FilterCommand = new AsyncRelayCommand(LoadCardsAsync);
        ViewCardDetailsCommand = new RelayCommand(ViewCardDetails, CanViewCardDetails);
        BackToMenuCommand = new RelayCommand(BackToMenu);
    }

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
                FilterType = value switch
                {
                    1 => CardType.Melee,
                    2 => CardType.Ranged,
                    3 => CardType.Siege,
                    4 => CardType.Support,
                    5 => CardType.Special,
                    _ => null
                };
                _ = LoadCardsAsync(null);
            }
        }
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Коллекция карт";

    /// <summary>
    /// Коллекция всех карт.
    /// </summary>
    public ObservableCollection<Card> Cards
    {
        get => _cards;
        set => SetProperty(ref _cards, value);
    }

    /// <summary>
    /// Выбранная карта.
    /// </summary>
    public Card? SelectedCard
    {
        get => _selectedCard;
        set
        {
            SetProperty(ref _selectedCard, value);
            OnPropertyChanged(nameof(CanViewCardDetails));
        }
    }

    /// <summary>
    /// Текст поиска.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    /// <summary>
    /// Фильтр по типу карты.
    /// </summary>
    public CardType? FilterType
    {
        get => _filterType;
        set => SetProperty(ref _filterType, value);
    }

    /// <summary>
    /// Индикатор загрузки.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Команда фильтрации/загрузки карт.
    /// </summary>
    public ICommand FilterCommand { get; }

    /// <summary>
    /// Команда просмотра деталей карты.
    /// </summary>
    public ICommand ViewCardDetailsCommand { get; }

    /// <summary>
    /// Команда возврата в меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Доступность просмотра деталей.
    /// </summary>
    private bool CanViewCardDetails(object? parameter) => SelectedCard != null;

    /// <summary>
    /// Загружает карты с фильтрацией.
    /// </summary>
    private async Task LoadCardsAsync(object? parameter)
    {
        IsLoading = true;

        try
        {
            var cards = await _cardService.SearchCardsAsync(
                name: string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
                type: FilterType,
                minAttack: null,
                maxAttack: null);

            Cards.Clear();
            foreach (var card in cards)
            {
                Cards.Add(card);
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибки
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки карт: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Просмотр деталей карты.
    /// </summary>
    private void ViewCardDetails(object? parameter)
    {
        if (SelectedCard != null)
        {
            // Показываем способность только для Support и Special
            bool canHaveAbilities = SelectedCard.CardType == CardType.Support || SelectedCard.CardType == CardType.Special;
            string abilityText = (canHaveAbilities && !string.IsNullOrWhiteSpace(SelectedCard.Ability))
                ? SelectedCard.Ability
                : "Нет";

            // В будущем: открытие окна с деталями карты
            System.Windows.MessageBox.Show(
                $"{SelectedCard.Name}\n\n" +
                $"Тип: {SelectedCard.CardType}\n" +
                $"Атака: {SelectedCard.Attack}\n" +
                $"Здоровье: {SelectedCard.Health}\n" +
                $"Дальность: {SelectedCard.Range}\n" +
                $"Способность: {abilityText}",
                "Информация о карте",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Вызывается при активации ViewModel.
    /// </summary>
    public override async void OnActivated()
    {
        base.OnActivated();
        await LoadCardsAsync(null);
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
