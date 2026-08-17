using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel панели администратора.
/// </summary>
public class AdminViewModel : ViewModelBase
{
    private readonly ICardService _cardService;
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private int _selectedTabIndex;
    private bool _isLoading;
    private bool _isCardsTabVisible = true;
    private bool _isUsersTabVisible = false;

    // Карты
    private ObservableCollection<Card> _cards = new();
    private Card? _selectedCard;
    private string _cardSearchText = string.Empty;
    private CardType _cardFilterType = CardType.Melee;

    // Пользователи
    private ObservableCollection<User> _users = new();
    private User? _selectedUser;

    // Форма карты
    private string _newCardName = string.Empty;
    private int _newCardAttack;
    private int _newCardHealth;
    private int _newCardRange;
    private string _newCardAbility = string.Empty;
    private string _newCardDescription = string.Empty;
    private CardType _newCardType = CardType.Melee;
    private bool _isEditingCard;
    private int _editingCardId;

    /// <summary>
    /// Инициализирует новый экземпляр AdminViewModel.
    /// </summary>
    public AdminViewModel(
        ICardService cardService,
        IUserService userService,
        INavigationService navigationService)
    {
        _cardService = cardService;
        _userService = userService;
        _navigationService = navigationService;

        // Команды карт
        LoadCardsCommand = new AsyncRelayCommand(LoadCardsAsync);
        AddCardCommand = new AsyncRelayCommand(AddCardAsync, CanAddCard);
        EditCardCommand = new RelayCommand(StartEditCard, CanEditCard);
        SaveCardCommand = new AsyncRelayCommand(SaveCardAsync, CanSaveCard);
        CancelEditCommand = new RelayCommand(CancelEdit);
        DeleteCardCommand = new AsyncRelayCommand(DeleteCardAsync, CanDeleteCard);
        SearchCardsCommand = new AsyncRelayCommand(SearchCardsAsync);

        // Команды пользователей
        LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync);
        ToggleBlockUserCommand = new AsyncRelayCommand(ToggleBlockUserAsync, CanToggleBlockUser);

        // Команды вкладок
        SwitchToCardsTabCommand = new RelayCommand(SwitchToCardsTab);
        SwitchToUsersTabCommand = new RelayCommand(SwitchToUsersTab);

        // Общие команды
        BackToMenuCommand = new RelayCommand(BackToMenu);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Панель администратора";

    #region Navigation Properties

    /// <summary>
    /// Выбранная вкладка (0 — карты, 1 — пользователи).
    /// </summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value))
            {
                if (value == 0 && _cards.Count == 0)
                {
                    _ = LoadCardsAsync(null);
                }
                else if (value == 1 && _users.Count == 0)
                {
                    _ = LoadUsersAsync(null);
                }
            }
        }
    }

    /// <summary>
    /// Индикатор загрузки.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    #endregion

    #region Cards Properties

    /// <summary>
    /// Список всех карт.
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
            if (SetProperty(ref _selectedCard, value))
            {
                (EditCardCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Текст поиска карт.
    /// </summary>
    public string CardSearchText
    {
        get => _cardSearchText;
        set => SetProperty(ref _cardSearchText, value);
    }

    /// <summary>
    /// Фильтр по типу карты.
    /// </summary>
    public CardType CardFilterType
    {
        get => _cardFilterType;
        set => SetProperty(ref _cardFilterType, value);
    }

    #endregion

    #region Users Properties

    /// <summary>
    /// Список всех пользователей.
    /// </summary>
    public ObservableCollection<User> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }

    /// <summary>
    /// Выбранный пользователь.
    /// </summary>
    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                (ToggleBlockUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    #region Card Form Properties

    /// <summary>
    /// Название новой/редактируемой карты.
    /// </summary>
    public string NewCardName
    {
        get => _newCardName;
        set
        {
            if (SetProperty(ref _newCardName, value))
            {
                (AddCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Атака карты.
    /// </summary>
    public int NewCardAttack
    {
        get => _newCardAttack;
        set
        {
            if (SetProperty(ref _newCardAttack, value))
            {
                (AddCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Здоровье карты.
    /// </summary>
    public int NewCardHealth
    {
        get => _newCardHealth;
        set
        {
            if (SetProperty(ref _newCardHealth, value))
            {
                (AddCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Дальность атаки.
    /// </summary>
    public int NewCardRange
    {
        get => _newCardRange;
        set
        {
            if (SetProperty(ref _newCardRange, value))
            {
                (AddCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Способность карты.
    /// </summary>
    public string NewCardAbility
    {
        get => _newCardAbility;
        set
        {
            if (SetProperty(ref _newCardAbility, value))
            {
                (AddCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Описание карты.
    /// </summary>
    public string NewCardDescription
    {
        get => _newCardDescription;
        set => SetProperty(ref _newCardDescription, value);
    }

    /// <summary>
    /// Тип карты.
    /// </summary>
    public CardType NewCardType
    {
        get => _newCardType;
        set
        {
            if (SetProperty(ref _newCardType, value))
            {
                (AddCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
                (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Режим редактирования существующей карты.
    /// </summary>
    public bool IsEditingCard
    {
        get => _isEditingCard;
        set => SetProperty(ref _isEditingCard, value);
    }

    /// <summary>
    /// Id редактируемой карты.
    /// </summary>
    public int EditingCardId
    {
        get => _editingCardId;
        set => SetProperty(ref _editingCardId, value);
    }

    #region Commands

    public ICommand LoadCardsCommand { get; }
    public ICommand AddCardCommand { get; }
    public ICommand EditCardCommand { get; }
    public ICommand SaveCardCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand DeleteCardCommand { get; }
    public ICommand SearchCardsCommand { get; }

    public ICommand LoadUsersCommand { get; }
    public ICommand ToggleBlockUserCommand { get; }

    public ICommand SwitchToCardsTabCommand { get; }
    public ICommand SwitchToUsersTabCommand { get; }

    public ICommand BackToMenuCommand { get; }

    #endregion

    #region Tab Properties

    /// <summary>
    /// Видима ли вкладка карт.
    /// </summary>
    public bool IsCardsTabVisible
    {
        get => _isCardsTabVisible;
        private set => SetProperty(ref _isCardsTabVisible, value);
    }

    /// <summary>
    /// Видима ли вкладка пользователей.
    /// </summary>
    public bool IsUsersTabVisible
    {
        get => _isUsersTabVisible;
        private set => SetProperty(ref _isUsersTabVisible, value);
    }

    /// <summary>
    /// Цвет вкладки карт.
    /// </summary>
    public System.Windows.Media.Brush CardsTabColor => _isCardsTabVisible
        ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan)
        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);

    /// <summary>
    /// Цвет вкладки пользователей.
    /// </summary>
    public System.Windows.Media.Brush UsersTabColor => _isUsersTabVisible
        ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Cyan)
        : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray);

    private static readonly ReadOnlyCollection<CardType> _allCardTypes = 
        new(new[] { CardType.Melee, CardType.Ranged, CardType.Siege, CardType.Support, CardType.Special });

    /// <summary>
    /// Список типов карт для фильтра.
    /// </summary>
    public IList<CardType> CardTypes => _allCardTypes;

    #endregion

    #region Activation

    /// <summary>
    /// Активация — загрузка карт.
    /// </summary>
    public override async void OnActivated()
    {
        base.OnActivated();
        await LoadCardsAsync(null);
    }

    #endregion

    #region Card Methods

    /// <summary>
    /// Загружает все карты.
    /// </summary>
    private async Task LoadCardsAsync(object? parameter)
    {
        IsLoading = true;
        try
        {
            var cards = await _cardService.GetAllCardsAsync();
            Cards.Clear();
            foreach (var card in cards)
            {
                Cards.Add(card);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка загрузки карт: {ex.Message}", "Ошибка");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Поиск карт по критериям.
    /// </summary>
    private async Task SearchCardsAsync(object? parameter)
    {
        IsLoading = true;
        try
        {
            var cards = await _cardService.SearchCardsAsync(CardSearchText, CardFilterType);
            Cards.Clear();
            foreach (var card in cards)
            {
                Cards.Add(card);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Начинает создание новой карты.
    /// </summary>
    private async Task AddCardAsync(object? parameter)
    {
        if (!ValidateCardForm())
            return;

        try
        {
            // Ограничиваем способность только для Support и Special
            bool canHaveAbilities = NewCardType == CardType.Support || NewCardType == CardType.Special;

            var newCard = new Card
            {
                Name = NewCardName,
                Attack = NewCardAttack,
                Health = NewCardHealth,
                Range = NewCardRange,
                Ability = canHaveAbilities ? NewCardAbility : null,
                Description = NewCardDescription,
                CardType = NewCardType,
                ImagePath = "Resources/Images/default.png"
            };

            await _cardService.AddCardAsync(newCard);
            ClearCardForm();
            await LoadCardsAsync(null);

            System.Windows.MessageBox.Show("Карта успешно добавлена!", "Успех",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка добавления карты: {ex.Message}", "Ошибка");
        }
    }

    private bool CanAddCard(object? parameter)
    {
        return true;
    }

    /// <summary>
    /// Начинает редактирование выбранной карты.
    /// </summary>
    private void StartEditCard(object? parameter)
    {
        if (SelectedCard == null)
            return;

        IsEditingCard = true;
        EditingCardId = SelectedCard.Id;
        NewCardName = SelectedCard.Name;
        NewCardAttack = SelectedCard.Attack;
        NewCardHealth = SelectedCard.Health;
        NewCardRange = SelectedCard.Range;
        NewCardAbility = SelectedCard.Ability ?? string.Empty;
        NewCardDescription = SelectedCard.Description ?? string.Empty;
        NewCardType = SelectedCard.CardType;

        (SaveCardCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        (CancelEditCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private bool CanEditCard(object? parameter)
    {
        return SelectedCard != null;
    }

    /// <summary>
    /// Сохраняет изменения карты.
    /// </summary>
    private async Task SaveCardAsync(object? parameter)
    {
        if (!ValidateCardForm())
            return;

        try
        {
            // Ограничиваем способность только для Support и Special
            bool canHaveAbilities = NewCardType == CardType.Support || NewCardType == CardType.Special;

            var card = new Card
            {
                Id = EditingCardId,
                Name = NewCardName,
                Attack = NewCardAttack,
                Health = NewCardHealth,
                Range = NewCardRange,
                Ability = canHaveAbilities ? NewCardAbility : null,
                Description = NewCardDescription,
                CardType = NewCardType,
                ImagePath = SelectedCard?.ImagePath ?? "Resources/Images/default.png"
            };

            await _cardService.UpdateCardAsync(card);
            ClearCardForm();
            await LoadCardsAsync(null);

            System.Windows.MessageBox.Show("Карта успешно обновлена!", "Успех",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка обновления карты: {ex.Message}", "Ошибка");
        }
    }

    private bool CanSaveCard(object? parameter)
    {
        return IsEditingCard;
    }

    /// <summary>
    /// Валидация формы карты.
    /// </summary>
    private bool ValidateCardForm()
    {
        if (string.IsNullOrWhiteSpace(NewCardName))
        {
            System.Windows.MessageBox.Show("Введите позывной (название) юнита.", "Валидация");
            return false;
        }

        if (NewCardAttack <= 0)
        {
            System.Windows.MessageBox.Show("Атака юнита должна быть больше 0.", "Валидация");
            return false;
        }

        if (NewCardHealth <= 0)
        {
            System.Windows.MessageBox.Show("Защита (ХП) юнита должна быть больше 0.", "Валидация");
            return false;
        }

        if (NewCardRange < 0)
        {
            System.Windows.MessageBox.Show("Дистанция атаки не может быть отрицательной.", "Валидация");
            return false;
        }

        bool canHaveAbilities = NewCardType == CardType.Support || NewCardType == CardType.Special;
        if (canHaveAbilities && string.IsNullOrWhiteSpace(NewCardAbility))
        {
            System.Windows.MessageBox.Show("Введите описание способности для карт поддержки или специальных карт.", "Валидация");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Отменяет редактирование.
    /// </summary>
    private void CancelEdit(object? parameter)
    {
        ClearCardForm();
    }

    /// <summary>
    /// Удаляет выбранную карту.
    /// </summary>
    private async Task DeleteCardAsync(object? parameter)
    {
        if (SelectedCard == null)
            return;

        var result = System.Windows.MessageBox.Show(
            $"Вы уверены, что хотите удалить карту \"{SelectedCard.Name}\"?",
            "Подтверждение удаления",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _cardService.DeleteCardAsync(SelectedCard.Id);
                await LoadCardsAsync(null);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка удаления карты: {ex.Message}", "Ошибка");
            }
        }
    }

    private bool CanDeleteCard(object? parameter)
    {
        return SelectedCard != null;
    }

    /// <summary>
    /// Очищает форму карты.
    /// </summary>
    private void ClearCardForm()
    {
        IsEditingCard = false;
        EditingCardId = 0;
        NewCardName = string.Empty;
        NewCardAttack = 0;
        NewCardHealth = 0;
        NewCardRange = 0;
        NewCardAbility = string.Empty;
        NewCardDescription = string.Empty;
        NewCardType = CardType.Melee;
    }

    #endregion

    #region User Methods

    /// <summary>
    /// Загружает всех пользователей.
    /// </summary>
    private async Task LoadUsersAsync(object? parameter)
    {
        IsLoading = true;
        try
        {
            var users = await _userService.GetAllUsersAsync();
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Переключает статус блокировки пользователя.
    /// </summary>
    private async Task ToggleBlockUserAsync(object? parameter)
    {
        if (SelectedUser == null)
            return;

        var user = SelectedUser;
        var username = user.Username;

        try
        {
            user.IsBlocked = !user.IsBlocked;
            await _userService.UpdateUserAsync(user);
            
            var action = user.IsBlocked ? "заблокирован" : "разблокирован";
            
            await LoadUsersAsync(null);

            System.Windows.MessageBox.Show(
                $"Пользователь \"{username}\" {action}.",
                "Успех",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}", "Ошибка");
        }
    }

    private bool CanToggleBlockUser(object? parameter)
    {
        return SelectedUser != null;
    }

    #endregion

    #region Navigation

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }

    /// <summary>
    /// Переключение на вкладку карт.
    /// </summary>
    private void SwitchToCardsTab(object? parameter)
    {
        IsCardsTabVisible = true;
        IsUsersTabVisible = false;
        if (Cards.Count == 0)
        {
            _ = LoadCardsAsync(null);
        }
    }

    /// <summary>
    /// Переключение на вкладку пользователей.
    /// </summary>
    private void SwitchToUsersTab(object? parameter)
    {
        IsCardsTabVisible = false;
        IsUsersTabVisible = true;
        if (Users.Count == 0)
        {
            _ = LoadUsersAsync(null);
        }
    }

    #endregion
}
