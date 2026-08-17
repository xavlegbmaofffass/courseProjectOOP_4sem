using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Services;
using FrontlineCardWarfare.Views;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// События для анимаций в BattleViewModel.
/// </summary>
public class BattleAnimationEvents
{
    public event Action<Unit>? OnUnitPlaced;
    public event Action<Card, int, int>? OnCardPlayed;
    public event Action<Card>? OnCardAddedToHand;
    public event Action<Unit, Unit, int>? OnAttackOccurred;
    public event Action<Unit, int>? OnUnitTookDamage;
    public event Action<Unit>? OnUnitDestroyed;
    public event Action? OnTurnChanged;

    /// <summary>
    /// Вызывает событие размещения юнита.
    /// </summary>
    public void InvokeOnUnitPlaced(Unit unit) => OnUnitPlaced?.Invoke(unit);

    /// <summary>
    /// Вызывает событие розыгрыша карты.
    /// </summary>
    public void InvokeOnCardPlayed(Card card, int row, int col) => OnCardPlayed?.Invoke(card, row, col);

    /// <summary>
    /// Вызывает событие добавления карты в руку.
    /// </summary>
    public void InvokeOnCardAddedToHand(Card card) => OnCardAddedToHand?.Invoke(card);

    /// <summary>
    /// Вызывает событие атаки.
    /// </summary>
    public void InvokeOnAttackOccurred(Unit attacker, Unit target, int damage) => OnAttackOccurred?.Invoke(attacker, target, damage);

    /// <summary>
    /// Вызывает событие получения урона.
    /// </summary>
    public void InvokeOnUnitTookDamage(Unit unit, int damage) => OnUnitTookDamage?.Invoke(unit, damage);

    /// <summary>
    /// Вызывает событие уничтожения юнита.
    /// </summary>
    public void InvokeOnUnitDestroyed(Unit unit) => OnUnitDestroyed?.Invoke(unit);

    /// <summary>
    /// Вызывает событие смены хода.
    /// </summary>
    public void InvokeOnTurnChanged() => OnTurnChanged?.Invoke();
}

/// <summary>
/// ViewModel для окна боя.
/// </summary>
public class BattleViewModel : ViewModelBase
{
    private readonly IBattleManager _battleManager;
    private readonly IGameSaveService? _saveService;
    private readonly IStatisticsService? _statisticsService;
    private readonly INavigationService? _navigationService;
    private readonly ITooltipService? _tooltipService;
    private readonly IUserService? _userService;
    private readonly IDeckService? _deckService;
    private IAIController? _aiController;
    private User? _currentUser;
    private ObservableCollection<BoardCell> _board = new();
    private ObservableCollection<Card> _playerHand = new();
    private Unit? _selectedUnit;
    private string _gameStatus = "Идёт игра";
    private bool _isPlayerTurn;
    private int _currentTurn;
    private int _sessionId;
    private GameState _gameState = new();
    private Card? _draggedCard;
    private string _tooltipText = string.Empty;
    private bool _showTooltip;
    private bool _isInteractionEnabled = true;
    private string _currentTurnIndicator = "Ваш ход";
    private string _difficulty = "Medium";
    private string _gameResult = string.Empty;
    private bool _showEnemyHand = false;
    private ObservableCollection<Card> _enemyHand = new();
    private bool _isInputBlocked = false;
    private string _turnInfo = "Ход: 1";

    public ICommand ReturnToMenuCommand { get; }

    /// <summary>
    /// События для анимаций.
    /// </summary>
    public BattleAnimationEvents AnimationEvents { get; } = new();

    // Кэшированные коллекции для рядов поля
    private readonly ObservableCollection<BoardCell> _boardRow0 = new();
    private readonly ObservableCollection<BoardCell> _boardRow1 = new();
    private readonly ObservableCollection<BoardCell> _boardRow2 = new();
    private readonly ObservableCollection<BoardCell> _boardRow3 = new();

    /// <summary>
    /// Клетки поля боя для рядов 0-3.
    /// </summary>
    public ObservableCollection<BoardCell> BoardCellsRow0 => _boardRow0;
    public ObservableCollection<BoardCell> BoardCellsRow1 => _boardRow1;
    public ObservableCollection<BoardCell> BoardCellsRow2 => _boardRow2;
    public ObservableCollection<BoardCell> BoardCellsRow3 => _boardRow3;

    /// <summary>
    /// Карта, которую перетаскивают.
    /// </summary>
    public Card? DraggedCard
    {
        get => _draggedCard;
        set => SetProperty(ref _draggedCard, value);
    }

    /// <summary>
    /// Количество карт в колоде игрока.
    /// </summary>
    public int PlayerDeckCount => _battleManager?.GameState?.PlayerDeck?.Count ?? 0;

    /// <summary>
    /// Количество карт в колоде противника.
    /// </summary>
    public int EnemyDeckCount => _battleManager?.GameState?.EnemyDeck?.Count ?? 0;

    /// <summary>
    /// Текст всплывающей подсказки.
    /// </summary>
    public string TooltipText
    {
        get => _tooltipText;
        set => SetProperty(ref _tooltipText, value);
    }

    /// <summary>
    /// Показывать ли подсказку.
    /// </summary>
    public bool ShowTooltip
    {
        get => _showTooltip;
        set => SetProperty(ref _showTooltip, value);
    }

    /// <summary>
    /// Все клетки поля боя (одним списком для UniformGrid).
    /// </summary>
    public ObservableCollection<BoardCell> AllBoardCells => _board;

    /// <summary>
    /// Разрешены ли действия игрока.
    /// </summary>
    public bool IsInteractionEnabled
    {
        get => _isInteractionEnabled;
        set => SetProperty(ref _isInteractionEnabled, value);
    }

    /// <summary>
    /// Текущий индикатор хода.
    /// </summary>
    public string CurrentTurnIndicator
    {
        get => _currentTurnIndicator;
        set => SetProperty(ref _currentTurnIndicator, value);
    }

    /// <summary>
    /// Информация о ходе (номер хода).
    /// </summary>
    public string TurnInfo
    {
        get => _turnInfo;
        set => SetProperty(ref _turnInfo, value);
    }

    /// <summary>
    /// Блокировка ввода.
    /// </summary>
    public bool IsInputBlocked
    {
        get => _isInputBlocked;
        set => SetProperty(ref _isInputBlocked, value);
    }

    /// <summary>
    /// Статус игры (Идёт игра / Победа / Поражение).
    /// </summary>
    public string GameStatus
    {
        get => _gameStatus;
        set => SetProperty(ref _gameStatus, value);
    }

    /// <summary>
    /// ID текущей игровой сессии.
    /// </summary>
    public int SessionId
    {
        get => _sessionId;
        set => SetProperty(ref _sessionId, value);
    }

    /// <summary>
    /// Игровое состояние.
    /// </summary>
    public GameState GameState => _gameState;

    /// <summary>
    /// Инициализирует новый экземпляр BattleViewModel.
    /// </summary>
    public BattleViewModel(
        IBattleManager battleManager,
        IGameSaveService? saveService = null,
        IStatisticsService? statisticsService = null,
        INavigationService? navigationService = null,
        ITooltipService? tooltipService = null,
        IUserService? userService = null,
        IDeckService? deckService = null)
    {
        _battleManager = battleManager;
        _saveService = saveService;
        _statisticsService = statisticsService;
        _navigationService = navigationService;
        _tooltipService = tooltipService;
        _userService = userService;
        _deckService = deckService;

        // Подписка на события ошибок
        if (_tooltipService != null)
        {
            _tooltipService.OnError += HandleTooltipError;
        }

        // Подписка на события анимаций от BattleManager
        if (_battleManager is BattleManager bm)
        {
            bm.AnimationEventHandlers.OnUnitPlaced += OnUnitPlacedHandler;
            bm.AnimationEventHandlers.OnCardPlayed += OnCardPlayedHandler;
            bm.AnimationEventHandlers.OnAttackOccurred += OnAttackOccurredHandler;
            bm.AnimationEventHandlers.OnUnitTookDamage += OnUnitTookDamageHandler;
            bm.AnimationEventHandlers.OnUnitDestroyed += OnUnitDestroyedHandler;
            bm.AnimationEventHandlers.OnTurnChanged += OnTurnChangedHandler;
        }

        PlayCardCommand = new RelayCommand(
            new Action<object?>(PlayCard),
            new Func<object?, bool>(CanPlayCard));
        SelectUnitCommand = new RelayCommand(SelectUnit);
        AttackCommand = new RelayCommand(Attack);
        EndTurnCommand = new AsyncRelayCommand(EndTurnAsync, CanExecuteEndTurn);
        SurrenderCommand = new AsyncRelayCommand(SurrenderAsync, CanSurrender);
        ReturnToMenuCommand = new RelayCommand(ReturnToMenu);
        DragDropCardCommand = new RelayCommand(
            new Action<object?>(DragDropCard),
            new Func<object?, bool>(CanDragDropCard));

        // Инициализация GameState в конструкторе, чтобы избежать null
        _gameState = new GameState();
    }

    private void HandleTooltipError(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            TooltipText = string.Empty;
            ShowTooltip = false;
        }
        else
        {
            TooltipText = message;
            ShowTooltip = true;
        }
    }

    /// <summary>
    /// Проверка возможности завершения хода.
    /// </summary>
    private bool CanExecuteEndTurn()
    {
        return _battleManager?.GameState != null && 
               !_battleManager.GameState.IsGameOver &&
               _battleManager.GameState.Turn.IsPlayerTurn;
    }

    /// <summary>
    /// Проверка возможности сдачи.
    /// </summary>
    private bool CanSurrender()
    {
        return _battleManager?.GameState != null && 
               !_battleManager.GameState.IsGameOver;
    }

    /// <summary>
    /// Показывает подсказку для карты.
    /// </summary>
    public void ShowCardTooltip(Card card)
    {
        // Показываем способность только для Support и Special
        bool canHaveAbilities = card.CardType == CardType.Support || card.CardType == CardType.Special;
        string? displayAbility = canHaveAbilities ? card.Ability : null;

        _tooltipService?.ShowCardTooltip(
            card.Name,
            card.Attack,
            card.Health,
            card.Range,
            displayAbility);
    }

    /// <summary>
    /// Скрывает подсказку.
    /// </summary>
    public void HideCardTooltip()
    {
        _tooltipService?.ClearAll();
    }

    /// <summary>
    /// Устанавливает пользователя и AIController (вызывается из BattleSetupViewModel).
    /// </summary>
    public void SetUserAndAI(User user, IAIController aiController)
    {
        _currentUser = user;
        _aiController = aiController;
    }

    /// <summary>
    /// Устанавливает сложность игры (вызывается из BattleSetupViewModel).
    /// </summary>
    public void SetDifficulty(string difficulty)
    {
        _difficulty = difficulty;
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Бой";

    /// <summary>
    /// Клетки игрового поля.
    /// </summary>
    public ObservableCollection<BoardCell> Board
    {
        get => _board;
        set => SetProperty(ref _board, value);
    }

    /// <summary>
    /// Карты в руке игрока.
    /// </summary>
    public ObservableCollection<Card> PlayerHand
    {
        get => _playerHand;
        set => SetProperty(ref _playerHand, value);
    }

    /// <summary>
    /// Количество карт в руке врага (скрыто).
    /// </summary>
    public int EnemyHandCount => _battleManager?.GameState?.EnemyHand?.Count ?? 0;

    /// <summary>
    /// Выбранный юнит.
    /// </summary>
    public Unit? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (SetProperty(ref _selectedUnit, value))
            {
                UpdateAvailableActions();
                OnPropertyChanged(nameof(SelectedUnit));
            }
        }
    }

    /// <summary>
    /// Доступные цели для атаки.
    /// </summary>
    public ObservableCollection<int> AvailableTargets { get; } = new();

    /// <summary>
    /// Результат игры.
    /// </summary>
    public string GameResult
    {
        get => _gameResult;
        set => SetProperty(ref _gameResult, value);
    }

    /// <summary>
    /// Показывать ли руку противника.
    /// </summary>
    public bool ShowEnemyHand
    {
        get => _showEnemyHand;
        set => SetProperty(ref _showEnemyHand, value);
    }

    /// <summary>
    /// Карты в руке противника (для визуализации при необходимости).
    /// </summary>
    public ObservableCollection<Card> EnemyHand
    {
        get => _enemyHand;
        set => SetProperty(ref _enemyHand, value);
    }

    /// <summary>
    /// Сейчас ход игрока.
    /// </summary>
    public bool IsPlayerTurn
    {
        get => _isPlayerTurn;
        set 
        {
            if (SetProperty(ref _isPlayerTurn, value))
            {
                OnPropertyChanged(nameof(IsInteractionEnabled));
                // Обновляем доступность всех карт при смене хода
                UpdateHandFromModel();
            }
        }
    }

    /// <summary>
    /// Номер текущего хода.
    /// </summary>
    public int CurrentTurn
    {
        get => _currentTurn;
        set => SetProperty(ref _currentTurn, value);
    }

    /// <summary>
    /// Команда перетаскивания карты.
    /// </summary>
    public ICommand DragDropCardCommand { get; }

    /// <summary>
    /// Команда розыгрыша карты.
    /// </summary>
    public ICommand PlayCardCommand { get; }

    /// <summary>
    /// Команда выбора юнита.
    /// </summary>
    public ICommand SelectUnitCommand { get; }

    /// <summary>
    /// Команда атаки.
    /// </summary>
    public ICommand AttackCommand { get; }

    /// <summary>
    /// Команда завершения хода.
    /// </summary>
    public AsyncRelayCommand EndTurnCommand { get; }

    /// <summary>
    /// Команда сдачи.
    /// </summary>
    public ICommand SurrenderCommand { get; }

    /// <summary>
    /// Инициализация игры.
    /// </summary>
    public async Task InitializeGameAsync(Deck playerDeck, Deck enemyDeck, string difficulty)
    {
        await _battleManager.InitializeAsync(playerDeck, enemyDeck, difficulty);
        _gameState = _battleManager.GameState;
        
        UpdateBoardFromModel();
        UpdateHandFromModel();
        UpdateTurnInfo();
    }

    /// <summary>
    /// Начинает перетаскивание карты.
    /// </summary>
    private void DragDropCard(object? parameter)
    {
        if (parameter is not Card card) return;
        
        DraggedCard = card;
        ShowTooltip = true;
        
        // Показываем способность только для Support и Special
        bool canHaveAbilities = card.CardType == CardType.Support || card.CardType == CardType.Special;
        string abilityText = (canHaveAbilities && !string.IsNullOrWhiteSpace(card.Ability)) 
            ? card.Ability 
            : "Нет способности";
            
        TooltipText = $"⚔️ {card.Attack} | 🛡️ {card.Health} | 📏 {card.Range}\n{abilityText}";
        
        // Автоматически скрывать подсказку через 1.5 сек
        Task.Delay(1500).ContinueWith(_ => Application.Current.Dispatcher.Invoke(() => ShowTooltip = false));
    }

    /// <summary>
    /// Проверка возможности перетаскивания.
    /// </summary>
    private bool CanDragDropCard(object? parameter)
    {
        return _isPlayerTurn && !_battleManager.GameState.IsGameOver && parameter is Card;
    }

    /// <summary>
    /// Вызывается при начале перетаскивания карты.
    /// </summary>
    public void StartedDraggingCard(Card card)
    {
        DraggedCard = card;
        ShowTooltip = true;

        // Показываем способность только для Support и Special
        bool canHaveAbilities = card.CardType == CardType.Support || card.CardType == CardType.Special;
        string abilityText = (canHaveAbilities && !string.IsNullOrWhiteSpace(card.Ability)) 
            ? card.Ability 
            : "Нет способности";

        TooltipText = $"⚔️ {card.Attack} | 🛡️ {card.Health} | 📏 {card.Range}\n{abilityText}";
        
        // Автоматически скрывать подсказку через 1.5 сек
        Task.Delay(1500).ContinueWith(_ => Application.Current.Dispatcher.Invoke(() => ShowTooltip = false));
    }

    /// <summary>
    /// Розыгрыш карты.
    /// </summary>
    private async void PlayCard(object? parameter)
    {
        if (_battleManager?.GameState?.Turn == null || !_battleManager.GameState.Turn.IsPlayerTurn)
        {
            System.Windows.MessageBox.Show("Сейчас не ваш ход!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (parameter is not Card card || DraggedCard == null)
        {
            return;
        }

        // Получаем координаты из параметра (object[] { row, column })
        if (parameter is not object[] targetArray || targetArray.Length < 2)
        {
            return;
        }

        int row = (int)targetArray[0];
        int column = (int)targetArray[1];

        // Проверка: позиция на стороне игрока (ряды 2-3)
        if (row < 2)
        {
            System.Windows.MessageBox.Show("Можно размещать юнитов только на своей стороне (ряды 2-3)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Проверка: клетка пуста
        var cell = _battleManager.GameState.Board.GetCell(row, column);
        if (cell == null || !cell.IsEmpty)
        {
            System.Windows.MessageBox.Show("Клетка занята!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Розыгрыш карты через BattleManager
        var result = await _battleManager.PlayCardAsync(card, row, column);

        if (result.Success)
        {
            // Обновление UI
            UpdateBoardFromModel();
            UpdateHandFromModel();
            SelectedUnit = null;
            DraggedCard = null;

            // Автосохранение
            await SaveGameStateAsync();
        }
        else
        {
            System.Windows.MessageBox.Show(result.Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Проверка возможности розыгрыша карты.
    /// </summary>
    public bool CanPlayCard(object? parameter)
    {
        return IsPlayerTurn && _battleManager?.GameState != null && !_battleManager.GameState.IsGameOver;
    }

    /// <summary>
    /// Выполняет розыгрыш карты.
    /// </summary>
    public async Task ExecutePlayCard(Card card, int row, int column)
    {
        if (!IsInteractionEnabled) return;

        // Блокируем взаимодействие на время розыгрыша и боя
        IsInteractionEnabled = false;

        try
        {
            // Проверка: сейчас ход игрока
            if (_battleManager?.GameState?.Turn == null || !_battleManager.GameState.Turn.IsPlayerTurn)
            {
                System.Windows.MessageBox.Show("Сейчас не ваш ход!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsInteractionEnabled = true;
                return;
            }

            // Проверка: карта в руке
            if (!_battleManager.GameState.PlayerHand.Cards.Contains(card))
            {
                System.Windows.MessageBox.Show("Этой карты нет в вашей руке!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsInteractionEnabled = true;
                return;
            }

            // Проверка: позиция на стороне игрока (ряды 2-3)
            if (row < 2)
            {
                System.Windows.MessageBox.Show("Можно размещать юнитов только на своей стороне (ряды 2-3)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsInteractionEnabled = true;
                return;
            }

            // Проверка: клетка пуста
            var cell = _battleManager.GameState.Board.GetCell(row, column);
            if (cell == null || !cell.IsEmpty)
            {
                System.Windows.MessageBox.Show("Клетка занята!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                IsInteractionEnabled = true;
                return;
            }

            // Розыгрыш карты через BattleManager
            var result = await _battleManager.PlayCardAsync(card, row, column);

            if (result.Success)
            {
                // Обновление UI
                UpdateBoardFromModel();
                UpdateHandFromModel();
                SelectedUnit = null;
                DraggedCard = null;
                
                // Автосохранение
                await SaveGameStateAsync();

                // АВТОЗАВЕРШЕНИЕ ХОДА ПОСЛЕ ВЫКЛАДЫВАНИЯ КАРТЫ
                if (EndTurnCommand.CanExecute(null))
                {
                    await EndTurnCommand.ExecuteAsync(null);
                }
                else
                {
                    IsInteractionEnabled = true;
                }
            }
            else
            {
                IsInteractionEnabled = true;
                System.Windows.MessageBox.Show(result.Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BattleViewModel] Ошибка в ExecutePlayCard: {ex.Message}");
            IsInteractionEnabled = true;
        }
    }

    /// <summary>
    /// Сохраняет состояние игры после розыгрыша карты.
    /// </summary>
    private async Task SaveGameStateAsync()
    {
        if (_saveService != null && _sessionId > 0)
        {
            try
            {
                await _saveService.SaveAfterPlayCardAsync(_gameState, _sessionId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Обрабатывает клик по клетке игрового поля.
    /// </summary>
    public async Task HandleBoardCellClick(int row, int column)
    {
        if (!_isPlayerTurn || _battleManager?.GameState == null || _battleManager.GameState.IsGameOver)
            return;

        // Если выбран юнит — проверяем атаку
        if (SelectedUnit != null)
        {
            // Проверяем атаку
            var targets = _battleManager.GetAvailableTargets(SelectedUnit.Id);
            var targetUnit = _battleManager.GameState.Board.GetCell(row, column)?.Unit;

            if (targetUnit != null && targets.Contains(targetUnit.Id))
            {
                var result = _battleManager.Attack(SelectedUnit.Id, targetUnit.Id);
                if (result.Success)
                {
                    UpdateBoardFromModel();
                    UpdateHandFromModel();
                    SelectedUnit = null;
                    AvailableTargets.Clear();
                    try
                    {
                        if (_saveService != null)
                        {
                            await _saveService.SaveAfterAttackAsync(_gameState, _sessionId);
                        }
                    }
                    catch (Exception saveEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка автосохранения при атаке: {saveEx.Message}");
                    }

                    var winResult = _battleManager.CheckWinCondition();
                    if (winResult != null)
                    {
                        GameResult = winResult;
                    }
                    
                    // Показываем результат атаки (только если юнит уничтожен)
                    if (!targetUnit.IsAlive)
                    {
                        System.Windows.MessageBox.Show($"Юнит уничтожен! Урон: {result.Damage}", "Атака", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(result.Error, "Ошибка", System.Windows.MessageBoxButton.OK);
                    SelectedUnit = null;
                    AvailableTargets.Clear();
                }
            }
            else
            {
                // Если кликнули не по цели — снимаем выделение
                SelectedUnit = null;
                AvailableTargets.Clear();
                UpdateBoardAvailability();
            }
        }
        else
        {
            // Выбираем юнита на своей стороне
            var cell = _battleManager.GameState.Board.GetCell(row, column);
            if (cell?.Unit != null && cell.Unit.IsPlayer)
            {
                SelectedUnit = cell.Unit;
                UpdateAvailableActions();
            }
        }
    }

    /// <summary>
    /// Выбор юнита.
    /// </summary>
    private void SelectUnit(object? parameter)
    {
        if (parameter is Unit unit)
        {
            SelectedUnit = unit;
            
            // Показать подсказку для юнита
            _tooltipService?.ShowUnitTooltip(
                unit.Name,
                unit.CurrentHealth,
                unit.MaxHealth,
                unit.Attack,
                unit.HasAttacked);
        }
    }

    /// <summary>
    /// Атака.
    /// </summary>
    private async void Attack(object? parameter)
    {
        if (SelectedUnit != null && parameter is int targetId)
        {
            var result = _battleManager.Attack(SelectedUnit.Id, targetId);
            if (result.Success)
            {
                UpdateBoardFromModel();
                UpdateHandFromModel();
                
                // Показываем результат атаки
                var target = _battleManager.GetUnitById(targetId);
                if (target != null && !target.IsAlive)
                {
                    _tooltipService?.ShowError($"⚔️ Юнит «{SelectedUnit.Name}» уничтожил «{target.Name}»! Урон: {result.Damage}");
                }
                else
                {
                    _tooltipService?.ShowError($"⚔️ Атака нанесла {result.Damage} урона!");
                }

                SelectedUnit = null;
                AvailableTargets.Clear();

                if (_saveService != null)
                {
                    try
                    {
                        await _saveService.SaveAfterAttackAsync(_gameState, _sessionId);
                    }
                    catch (Exception saveEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка автосохранения при атаке: {saveEx.Message}");
                    }
                }
            }
            else
            {
                // Показываем ошибку через TooltipService
                _tooltipService?.ShowError(result.Error);
            }
        }
    }

    /// <summary>
    /// Завершение хода.
    /// </summary>
    private async Task EndTurnAsync()
    {
        if (_battleManager?.GameState == null)
        {
            System.Windows.MessageBox.Show("Ошибка: состояние игры не инициализировано!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (_battleManager.GameState.IsGameOver)
        {
            System.Windows.MessageBox.Show("Игра завершена!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Проверка: это ход игрока
        if (!_battleManager.GameState.Turn.IsPlayerTurn)
        {
            System.Windows.MessageBox.Show("Сейчас не ваш ход!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Блокировка UI
        IsInteractionEnabled = false;
        CurrentTurnIndicator = "Ход противника...";
        
        // Вызов анимации смены хода
        AnimationEvents.InvokeOnTurnChanged();

        // Даем UI обновиться перед началом долгой операции
        await Task.Yield();

        try
        {
            // Завершение хода игрока (передача хода ИИ)
            await _battleManager.EndTurnAsync();
            UpdateBoardFromModel();

            // Ход ИИ
            if (_aiController != null)
            {
                CurrentTurnIndicator = "ИИ думает...";
                await Task.Delay(400);
                await _aiController.MakeTurnAsync(_battleManager);
            }

            // Завершение хода ИИ (возврат хода игроку)
            await _battleManager.CompleteEnemyTurnAsync();

            // Обновление UI после хода ИИ
            IsInteractionEnabled = true;
            CurrentTurnIndicator = "Ваш ход";
            SelectedUnit = null;
            AvailableTargets.Clear();
            UpdateBoardFromModel();
            UpdateHandFromModel();
            UpdateTurnInfo();

            // Вызов анимации смены хода
            AnimationEvents.InvokeOnTurnChanged();

            // Автосохранение после завершения хода
            if (_saveService != null)
            {
                try
                {
                    await _saveService.SaveAfterEndTurnAsync(_gameState, _sessionId);
                }
                catch (Exception saveEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка автосохранения: {saveEx.Message}");
                }
            }

            // Проверка условия победы
            var winResult = _battleManager.CheckWinCondition();
            
            if (!string.IsNullOrEmpty(winResult))
            {
                await EndGameAsync(winResult);
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EndTurnAsync: Ошибка - {ex}");
            System.Windows.MessageBox.Show($"Ошибка при завершении хода: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Восстанавливаем состояние чтобы игрок не застрял
            IsInteractionEnabled = true;
            IsPlayerTurn = true; 
            _battleManager.GameState.Turn.IsPlayerTurn = true;
            CurrentTurnIndicator = "Ваш ход";
            
            UpdateBoardFromModel();
            UpdateHandFromModel();
        }
    }

    private void ReturnToMenu(object? parameter)
    {
        _navigationService?.NavigateTo<MainViewModel>();
    }

    private async Task SurrenderAsync()
    {
        if (_battleManager == null)
        {
            System.Windows.MessageBox.Show("Ошибка: BattleManager не инициализирован!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _battleManager.Surrender();
        await EndGameAsync("Поражение");
    }

    /// <summary>
    /// Завершение игры с сохранением результата и переходом к экрану результатов.
    /// </summary>
    private async Task EndGameAsync(string result)
    {
        IsInteractionEnabled = false;
        IsInputBlocked = true;
        GameResult = result;

        try
        {
            // Обновляем статус в модели
            _battleManager.GameState.IsGameOver = true;
            _battleManager.GameState.GameResult = result;

            // Получаем детальную статистику
            var gameStats = _battleManager.GetGameEndStatistics(_difficulty);

            // Сохраняем результат игры в БД
            if (_currentUser != null && _saveService != null)
            {
                await _saveService.SaveGameResultAsync(_currentUser.Id, gameStats, _difficulty);
            }

            // Обновляем статистику игрока
            if (_currentUser != null && _statisticsService != null)
            {
                await _statisticsService.UpdateStatisticsAsync(_currentUser.Id, result);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BattleViewModel] Ошибка в EndGameAsync: {ex.Message}");
        }
    }

    /// <summary>
    /// Обновление доступных действий для выбранного юнита.
    /// </summary>
    private void UpdateAvailableActions()
    {
        if (SelectedUnit == null || _battleManager == null)
        {
            AvailableTargets.Clear();
            UpdateBoardAvailability();
            return;
        }

        var targets = _battleManager.GetAvailableTargets(SelectedUnit.Id);
        AvailableTargets.Clear();
        foreach (var target in targets)
        {
            AvailableTargets.Add(target);
        }

        UpdateBoardAvailability();
    }

    /// <summary>
    /// Обновляет доступность клеток на поле.
    /// </summary>
    private void UpdateBoardAvailability()
    {
        // Очистка всех флагов доступности
        foreach (var cell in _board)
        {
            cell.IsAvailableForMove = false;
            cell.IsAvailableForAttack = false;
        }

        // Установка флагов для доступных целей атаки
        foreach (var targetId in AvailableTargets)
        {
            var targetUnit = _battleManager?.GetUnitById(targetId);
            if (targetUnit != null)
            {
                var cellIndex = targetUnit.Row * 4 + targetUnit.Column;
                var cell = GetBoardCellByIndex(cellIndex);
                if (cell != null)
                {
                    cell.IsAvailableForAttack = true;
                }
            }
        }
    }

    /// <summary>
    /// Проверяет, не завершена ли игра.
    /// </summary>
    private void CheckGameOver()
    {
        var result = _battleManager?.CheckWinCondition();
        if (!string.IsNullOrEmpty(result))
        {
            System.Diagnostics.Debug.WriteLine($"[BattleViewModel] Игра окончена: {result}");
            _ = EndGameAsync(result);
        }
    }

    /// <summary>
    /// Получение клетки по индексу (row * 4 + column).
    /// </summary>
    private BoardCell? GetBoardCellByIndex(int index)
    {
        if (index < 0 || index >= _board.Count) return null;
        return _board[index];
    }

    /// <summary>
    /// Обновление поля из модели.
    /// </summary>
    public void UpdateBoardFromModel()
    {
        if (_battleManager?.GameState?.Board == null)
        {
            return;
        }

        var board = _battleManager.GameState.Board;
        
        _board.Clear();
        _boardRow0.Clear();
        _boardRow1.Clear();
        _boardRow2.Clear();
        _boardRow3.Clear();

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var cell = board.GetCell(row, col);
                
                if (cell == null)
                {
                    cell = new BoardCell { Row = row, Column = col, IsPlayerSide = row >= 2 };
                }

                _board.Add(cell);
                
                switch (row)
                {
                    case 0: _boardRow0.Add(cell); break;
                    case 1: _boardRow1.Add(cell); break;
                    case 2: _boardRow2.Add(cell); break;
                    case 3: _boardRow3.Add(cell); break;
                }
            }
        }

        CalculateTotalAttacks();
    }

    private int _playerTotalAttack;
    private int _enemyTotalAttack;

    /// <summary>
    /// Общая сила атаки игрока на поле.
    /// </summary>
    public int PlayerTotalAttack
    {
        get => _playerTotalAttack;
        set => SetProperty(ref _playerTotalAttack, value);
    }

    /// <summary>
    /// Общая сила атаки противника на поле.
    /// </summary>
    public int EnemyTotalAttack
    {
        get => _enemyTotalAttack;
        set => SetProperty(ref _enemyTotalAttack, value);
    }

    /// <summary>
    /// Пересчитывает общую силу атаки сторон на поле.
    /// </summary>
    private void CalculateTotalAttacks()
    {
        if (_battleManager?.GameState?.Board == null) return;

        var playerUnits = _battleManager.GameState.Board.GetPlayerUnits(true);
        var enemyUnits = _battleManager.GameState.Board.GetPlayerUnits(false);

        PlayerTotalAttack = playerUnits.Sum(u => u.Attack);
        EnemyTotalAttack = enemyUnits.Sum(u => u.Attack);
    }

    /// <summary>
    /// Обновление руки из модели.
    /// </summary>
    public void UpdateHandFromModel()
    {
        if (_battleManager?.GameState?.PlayerHand == null)
        {
            return;
        }

        var hand = _battleManager.GameState.PlayerHand;
        if (hand.Cards == null)
        {
            PlayerHand.Clear();
            OnPropertyChanged(nameof(PlayerHand));
            OnPropertyChanged(nameof(EnemyHandCount));
            return;
        }

        var newCards = hand.Cards.Where(c => !PlayerHand.Contains(c)).ToList();
        foreach (var card in newCards)
        {
            PlayerHand.Add(card);
            AnimationEvents.InvokeOnCardAddedToHand(card);
        }

        var cardsToRemove = PlayerHand.Where(c => !hand.Cards.Contains(c)).ToList();
        foreach (var card in cardsToRemove)
        {
            PlayerHand.Remove(card);
        }
        
        OnPropertyChanged(nameof(PlayerHand));
        OnPropertyChanged(nameof(EnemyHandCount));
        OnPropertyChanged(nameof(PlayerDeckCount));
        OnPropertyChanged(nameof(EnemyDeckCount));
    }

    /// <summary>
    /// Обновление информации о ходе.
    /// </summary>
    private void UpdateTurnInfo()
    {
        if (_battleManager?.GameState?.Turn == null) return;

        IsPlayerTurn = _battleManager.GameState.Turn.IsPlayerTurn;
        CurrentTurn = _battleManager.GameState.Turn.TurnNumber;
        TurnInfo = $"Ход {CurrentTurn}";
        
        OnPropertyChanged(nameof(IsInteractionEnabled));
        UpdateHandFromModel();
    }

    /// <summary>
    /// Инициализирует игру из сохранённой сессии.
    /// </summary>
    public async Task InitializeGameFromSessionAsync(GameState gameState)
    {
        _gameState = gameState;
        UpdateBoardFromModel();
        UpdateHandFromModel();
        UpdateTurnInfo();
        await Task.CompletedTask;
    }

    #region Анимации - обработчики событий

    private void OnUnitPlacedHandler(Unit unit)
    {
        AnimationEvents.InvokeOnUnitPlaced(unit);
    }

    private void OnCardPlayedHandler(Card card, int row, int col)
    {
        UpdateHandFromModel();
        AnimationEvents.InvokeOnCardPlayed(card, row, col);
    }

    private void OnAttackOccurredHandler(Unit attacker, Unit target, int damage)
    {
        AnimationEvents.InvokeOnAttackOccurred(attacker, target, damage);
    }

    private void OnUnitTookDamageHandler(Unit unit, int damage)
    {
        AnimationEvents.InvokeOnUnitTookDamage(unit, damage);
    }

    private void OnUnitDestroyedHandler(Unit unit)
    {
        AnimationEvents.InvokeOnUnitDestroyed(unit);
    }

    private void OnTurnChangedHandler()
    {
        AnimationEvents.InvokeOnTurnChanged();
    }

    #endregion
}