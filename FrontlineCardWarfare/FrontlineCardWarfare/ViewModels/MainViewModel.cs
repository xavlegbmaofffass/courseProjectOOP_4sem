using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel главного меню.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private readonly IDeckService _deckService;
    private User? _currentUser;
    private string _welcomeMessage = string.Empty;
    private bool _isAdmin;
    private ObservableCollection<Deck> _playerDecks = new();
    private Deck? _selectedDeck;
    private string _deckCountText = "Нет колод";

    /// <summary>
    /// Инициализирует новый экземпляр MainViewModel.
    /// </summary>
    public MainViewModel(
        IUserService userService,
        INavigationService navigationService,
        IDeckService deckService)
    {
        _userService = userService;
        _navigationService = navigationService;
        _deckService = deckService;

        StartBattleCommand = new RelayCommand(StartBattle, CanStartBattle);
        OpenCollectionCommand = new RelayCommand(OpenCollection);
        OpenDeckBuilderCommand = new RelayCommand(OpenDeckBuilder);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        OpenAdminPanelCommand = new RelayCommand(OpenAdminPanel, CanOpenAdminPanel);
        OpenProfileCommand = new RelayCommand(OpenProfile, CanOpenProfile);
        OpenRulesCommand = new RelayCommand(OpenRules);
        LogoutCommand = new RelayCommand(Logout);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Главное меню";

    /// <summary>
    /// Приветственное сообщение.
    /// </summary>
    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    /// <summary>
    /// Является ли текущий пользователь администратором.
    /// </summary>
    public bool IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    /// <summary>
    /// Колоды игрока.
    /// </summary>
    public ObservableCollection<Deck> PlayerDecks
    {
        get => _playerDecks;
        set => SetProperty(ref _playerDecks, value);
    }

    /// <summary>
    /// Выбранная колода.
    /// </summary>
    public Deck? SelectedDeck
    {
        get => _selectedDeck;
        set
        {
            SetProperty(ref _selectedDeck, value);
            (StartBattleCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Текст с количеством колод.
    /// </summary>
    public string DeckCountText
    {
        get => _deckCountText;
        set => SetProperty(ref _deckCountText, value);
    }

    /// <summary>
    /// Команда начала боя.
    /// </summary>
    public ICommand StartBattleCommand { get; }

    /// <summary>
    /// Команда открытия коллекции.
    /// </summary>
    public ICommand OpenCollectionCommand { get; }

    /// <summary>
    /// Команда открытия конструктора колод.
    /// </summary>
    public ICommand OpenDeckBuilderCommand { get; }

    /// <summary>
    /// Команда открытия настроек.
    /// </summary>
    public ICommand OpenSettingsCommand { get; }

    /// <summary>
    /// Команда открытия панели администратора.
    /// </summary>
    public ICommand OpenAdminPanelCommand { get; }

    /// <summary>
    /// Команда выхода.
    /// </summary>
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// Команда открытия профиля.
    /// </summary>
    public ICommand OpenProfileCommand { get; }

    /// <summary>
    /// Команда открытия правил игры.
    /// </summary>
    public ICommand OpenRulesCommand { get; }

    /// <summary>
    /// Активация ViewModel — загрузка данных пользователя.
    /// </summary>
    public override async void OnActivated()
    {
        base.OnActivated();
        _currentUser = _userService.CurrentUser;

        if (_currentUser != null)
        {
            WelcomeMessage = $"Добро пожаловать, {_currentUser.Username}!";
            IsAdmin = _currentUser.Role == UserRole.Admin;
        }

        await LoadUserDataAsync();
    }

    /// <summary>
    /// Загружает данные текущего пользователя.
    /// </summary>
    private async Task LoadUserDataAsync()
    {
        if (_currentUser == null) return;

        try
        {
            // Для гостя загружаем колоды из памяти
            if (_userService.IsGuestMode)
            {
                PlayerDecks.Clear();
                foreach (var deck in _userService.GuestDecks)
                {
                    PlayerDecks.Add(deck);
                }
                SelectedDeck = PlayerDecks.FirstOrDefault();
                DeckCountText = $"Колод: {PlayerDecks.Count}";
                (StartBattleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                return;
            }

            // Загружаем колоды пользователя из БД
            var decks = await _deckService.GetAllDecksByUserAsync(_currentUser.Id);
            PlayerDecks.Clear();
            foreach (var deck in decks)
            {
                PlayerDecks.Add(deck);
            }

            // Устанавливаем активную колоду (первую или null)
            SelectedDeck = PlayerDecks.FirstOrDefault();
            DeckCountText = $"Колод: {PlayerDecks.Count}";
            (StartBattleCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных пользователя: {ex.Message}");
        }
    }

    /// <summary>
    /// Начинает бой — открывает экран выбора сложности.
    /// </summary>
    private void StartBattle(object? parameter)
    {
        if (SelectedDeck == null)
        {
            System.Windows.MessageBox.Show(
                "Выберите колоду для начала боя.",
                "Внимание",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Открытие экрана выбора сложности и колоды
        var battleSetupVm = (App.Current as App)!.GetService<BattleSetupViewModel>();
        battleSetupVm.Initialize(SelectedDeck, _currentUser!);
        _navigationService.NavigateTo(battleSetupVm);
    }

    /// <summary>
    /// Проверка возможности начала боя.
    /// </summary>
    private bool CanStartBattle(object? parameter)
    {
        return SelectedDeck != null && _currentUser != null;
    }

    /// <summary>
    /// Открывает коллекцию карт.
    /// </summary>
    private void OpenCollection(object? parameter)
    {
        _navigationService.NavigateTo<CollectionViewModel>();
    }

    /// <summary>
    /// Открывает конструктор колод.
    /// </summary>
    private void OpenDeckBuilder(object? parameter)
    {
        _navigationService.NavigateTo<DeckBuilderViewModel>();
    }

    /// <summary>
    /// Открывает настройки.
    /// </summary>
    private void OpenSettings(object? parameter)
    {
        _navigationService.NavigateTo<SettingsViewModel>();
    }

    /// <summary>
    /// Открывает панель администратора.
    /// </summary>
    private void OpenAdminPanel(object? parameter)
    {
        _navigationService.NavigateTo<AdminViewModel>();
    }

    /// <summary>
    /// Проверка возможности открытия панели администратора.
    /// </summary>
    private bool CanOpenAdminPanel(object? parameter)
    {
        return IsAdmin;
    }

    /// <summary>
    /// Открывает профиль пользователя.
    /// </summary>
    private void OpenProfile(object? parameter)
    {
        _navigationService.NavigateTo<ProfileViewModel>();
    }

    /// <summary>
    /// Проверка возможности открытия профиля.
    /// </summary>
    private bool CanOpenProfile(object? parameter)
    {
        return _currentUser != null && !_userService.IsGuestMode;
    }

    /// <summary>
    /// Открывает правила игры.
    /// </summary>
    private void OpenRules(object? parameter)
    {
        _navigationService.NavigateTo<RulesViewModel>();
    }

    /// <summary>
    /// Выход из аккаунта.
    /// </summary>
    private void Logout(object? parameter)
    {
        _userService.Logout();
        _navigationService.ClearHistory();
        _navigationService.NavigateTo<LoginViewModel>();
    }
}
