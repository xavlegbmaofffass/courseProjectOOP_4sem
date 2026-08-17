using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel для экрана результатов игры.
/// </summary>
public class GameResultViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IGameSaveService _saveService;
    private readonly IStatisticsService _statisticsService;
    private readonly GameEndStatistics _gameStatistics;
    private readonly int _userId;
    private readonly string _difficulty;
    private string _resultTitle = string.Empty;
    private string _resultColor = string.Empty;
    private bool _isVictory;

    /// <summary>
    /// Инициализирует новый экземпляр GameResultViewModel.
    /// </summary>
    public GameResultViewModel(
        INavigationService navigationService,
        IGameSaveService saveService,
        IStatisticsService statisticsService,
        GameEndStatistics gameStatistics,
        int userId,
        string difficulty)
    {
        _navigationService = navigationService;
        _saveService = saveService;
        _statisticsService = statisticsService;
        _gameStatistics = gameStatistics;
        _userId = userId;
        _difficulty = difficulty;

        PlayAgainCommand = new AsyncRelayCommand(PlayAgainAsync);
        BackToMenuCommand = new RelayCommand(BackToMenu);

        InitializeResultDisplay();
    }

    private void InitializeResultDisplay()
    {
        _isVictory = _gameStatistics.Result.Contains("Победа", StringComparison.OrdinalIgnoreCase);
        
        ResultTitle = _isVictory ? "ПОБЕДА!" : (_gameStatistics.Result.Contains("Ничья") ? "НИЧЬЯ" : "ПОРАЖЕНИЕ");
        ResultColor = _isVictory ? "#4CAF50" : (_gameStatistics.Result.Contains("Ничья") ? "#FF9800" : "#F44336");
    }

    /// <summary>
    /// Заголовок результата.
    /// </summary>
    public string ResultTitle
    {
        get => _resultTitle;
        set => SetProperty(ref _resultTitle, value);
    }

    /// <summary>
    /// Цвет результата.
    /// </summary>
    public string ResultColor
    {
        get => _resultColor;
        set => SetProperty(ref _resultColor, value);
    }

    /// <summary>
    /// Количество ходов.
    /// </summary>
    public int TurnCount => _gameStatistics.TurnCount;

    /// <summary>
    /// Нанесённый урон игроком.
    /// </summary>
    public int PlayerDamageDealt => _gameStatistics.PlayerDamageDealt;

    /// <summary>
    /// Нанесённый урон противником.
    /// </summary>
    public int EnemyDamageDealt => _gameStatistics.EnemyDamageDealt;

    /// <summary>
    /// Убито юнитов игроком.
    /// </summary>
    public int PlayerUnitsKilled => _gameStatistics.PlayerUnitsKilled;

    /// <summary>
    /// Убито юнитов противником.
    /// </summary>
    public int EnemyUnitsKilled => _gameStatistics.EnemyUnitsKilled;

    /// <summary>
    /// Оставшиеся карты игрока.
    /// </summary>
    public int PlayerCardsRemaining => _gameStatistics.PlayerCardsRemaining;

    /// <summary>
    /// Оставшиеся карты противника.
    /// </summary>
    public int EnemyCardsRemaining => _gameStatistics.EnemyCardsRemaining;

    /// <summary>
    /// Уровень сложности.
    /// </summary>
    public string DifficultyDisplay => _difficulty switch
    {
        "Easy" => "Лёгкий",
        "Medium" => "Средний",
        "Hard" => "Сложный",
        _ => _difficulty
    };

    /// <summary>
    /// Команда «Сыграть ещё раз».
    /// </summary>
    public ICommand PlayAgainCommand { get; }

    /// <summary>
    /// Команда «В главное меню».
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Сыграть ещё раз.
    /// </summary>
    private async Task PlayAgainAsync()
    {
        try
        {
            // Сохраняем результат текущей игры
            await _saveService.SaveGameResultAsync(_userId, _gameStatistics, _difficulty);

            // Обновляем статистику
            await _statisticsService.UpdateStatisticsAsync(_userId, _gameStatistics.Result);

            // Возвращаемся к настройкам боя для новой игры
            _navigationService.NavigateTo<BattleSetupViewModel>();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu()
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
