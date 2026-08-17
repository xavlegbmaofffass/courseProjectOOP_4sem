using System.Collections.ObjectModel;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для экрана выбора настроек боя.
/// </summary>
public class BattleSetupViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IBattleManager _battleManager;
    private readonly IGameSaveService _gameSaveService;
    private Deck? _playerDeck;
    private User? _currentUser;
    private string _selectedDifficulty = "medium";
    private ObservableCollection<string> _difficulties = new() { "easy", "medium", "hard" };
    private ObservableCollection<Deck> _enemyDecks = new();
    private Deck? _selectedEnemyDeck;
    private bool _useRandomEnemyDeck = true;

    /// <summary>
    /// Инициализирует новый экземпляр BattleSetupViewModel.
    /// </summary>
    public BattleSetupViewModel(
        INavigationService navigationService,
        IBattleManager battleManager,
        IGameSaveService gameSaveService)
    {
        _navigationService = navigationService;
        _battleManager = battleManager;
        _gameSaveService = gameSaveService;

        StartBattleCommand = new RelayCommand(StartBattle);
        BackToMenuCommand = new RelayCommand(BackToMenu);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Настройки боя";

    /// <summary>
    /// Доступные уровни сложности.
    /// </summary>
    public ObservableCollection<string> Difficulties
    {
        get => _difficulties;
        set => SetProperty(ref _difficulties, value);
    }

    /// <summary>
    /// Выбранный уровень сложности.
    /// </summary>
    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set => SetProperty(ref _selectedDifficulty, value);
    }

    /// <summary>
    /// Использовать случайную колоду врага.
    /// </summary>
    public bool UseRandomEnemyDeck
    {
        get => _useRandomEnemyDeck;
        set => SetProperty(ref _useRandomEnemyDeck, value);
    }

    /// <summary>
    /// Колоды врага (если выбран ручной режим).
    /// </summary>
    public ObservableCollection<Deck> EnemyDecks
    {
        get => _enemyDecks;
        set => SetProperty(ref _enemyDecks, value);
    }

    /// <summary>
    /// Выбранная колода врага.
    /// </summary>
    public Deck? SelectedEnemyDeck
    {
        get => _selectedEnemyDeck;
        set => SetProperty(ref _selectedEnemyDeck, value);
    }

    /// <summary>
    /// Отображаемое имя колоды игрока.
    /// </summary>
    public string PlayerDeckName => _playerDeck?.Name ?? "Не выбрана";

    /// <summary>
    /// Отображаемое имя колоды врага.
    /// </summary>
    public string EnemyDeckName => _useRandomEnemyDeck ? "Случайная" : (_selectedEnemyDeck?.Name ?? "Не выбрана");

    /// <summary>
    /// Отображаемый уровень сложности.
    /// </summary>
    public string DifficultyDisplayName => SelectedDifficulty switch
    {
        "easy" => "Лёгкий",
        "medium" => "Средний",
        "hard" => "Сложный",
        _ => "Средний"
    };

    /// <summary>
    /// Команда начала боя.
    /// </summary>
    public ICommand StartBattleCommand { get; }

    /// <summary>
    /// Команда возврата в меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Инициализация с выбранной колодой игрока.
    /// </summary>
    public void Initialize(Deck playerDeck, User currentUser)
    {
        _playerDeck = playerDeck;
        _currentUser = currentUser;
        OnPropertyChanged(nameof(PlayerDeckName));
    }

    /// <summary>
    /// Начинает бой с выбранными настройками.
    /// </summary>
    private async void StartBattle(object? parameter)
    {
        if (_playerDeck == null || _currentUser == null)
        {
            System.Windows.MessageBox.Show(
                "Колода не выбрана. Вернитесь в главное меню и выберите колоду.",
                "Ошибка",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            return;
        }

        try
        {
            // Преобразуем сложность в правильный регистр: easy -> Easy
            string difficulty = char.ToUpper(SelectedDifficulty[0]) + SelectedDifficulty.Substring(1);
            
            // Создаём случайную колоду врага
            var enemyDeck = await CreateRandomEnemyDeckAsync();

            // Создаём AIController с выбранной сложностью
            var aiController = new AIController(difficulty);

            // Получаем BattleViewModel через DI (он будет создан с правильными зависимостями)
            var battleVm = (App.Current as App)!.GetService<BattleViewModel>();
            
            // Инициализируем бой
            await battleVm.InitializeGameAsync(_playerDeck, enemyDeck, difficulty);
            battleVm.SetUserAndAI(_currentUser, aiController);
            battleVm.SetDifficulty(difficulty);

            // Навигация через NavigationService (без передачи battleVm напрямую)
            _navigationService.NavigateTo(battleVm);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Ошибка при инициализации боя: {ex.Message}\n{ex.StackTrace}",
                "Ошибка",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }

    /// <summary>
    /// Создаёт случайную колоду врага из 20 карт.
    /// </summary>
    private async Task<Deck> CreateRandomEnemyDeckAsync()
    {
        var cardService = (App.Current as App)!.GetService<ICardService>();
        var allCards = await cardService.GetAllCardsAsync();
        
        var random = new Random();
        var shuffled = allCards.OrderBy(_ => random.Next()).Take(20).ToList();

        var enemyDeck = new Deck
        {
            Name = "Колода противника",
            UserId = _currentUser!.Id,
            CreatedAt = DateTime.Now,
            DeckCards = new List<DeckCard>()
        };

        // Группируем по CardId для Quantity
        var grouped = shuffled.GroupBy(c => c.Id).ToList();
        foreach (var g in grouped)
        {
            enemyDeck.DeckCards.Add(new DeckCard
            {
                DeckId = 0, // Временный
                CardId = g.Key,
                Quantity = g.Count(),
                Card = g.First()
            });
        }

        return enemyDeck;
    }
}
