using System.Windows.Input;
using FrontlineCardWarfare.Commands;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Services;

namespace FrontlineCardWarfare.ViewModels;

/// <summary>
/// ViewModel окна настроек.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly IBackgroundMusicService _backgroundMusic;
    private GameSettings _settings = new();
    private int _musicVolume;
    private int _effectsVolume;
    private bool _showHints;
    private bool _isFullscreen;
    private int _soundVolume = 50;

    /// <summary>
    /// Инициализирует новый экземпляр SettingsViewModel.
    /// </summary>
    public SettingsViewModel(ISettingsService settingsService, INavigationService navigationService, IBackgroundMusicService backgroundMusic)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        _backgroundMusic = backgroundMusic;

        SaveCommand = new RelayCommand(SaveSettings);
        ResetCommand = new RelayCommand(ResetSettings);
        BackToMenuCommand = new RelayCommand(BackToMenu);
    }

    /// <summary>
    /// Заголовок окна.
    /// </summary>
    public override string Title => "Настройки";

    /// <summary>
    /// Настройки.
    /// </summary>
    public GameSettings Settings
    {
        get => _settings;
        set => SetProperty(ref _settings, value);
    }

    /// <summary>
    /// Громкость музыки.
    /// </summary>
    public int MusicVolume
    {
        get => _musicVolume;
        set => SetProperty(ref _musicVolume, value);
    }

    /// <summary>
    /// Громкость эффектов.
    /// </summary>
    public int EffectsVolume
    {
        get => _effectsVolume;
        set => SetProperty(ref _effectsVolume, value);
    }

    /// <summary>
    /// Громкость звука (общая).
    /// </summary>
    public int SoundVolume
    {
        get => _soundVolume;
        set => SetProperty(ref _soundVolume, value);
    }

    /// <summary>
    /// Показывать подсказки.
    /// </summary>
    public bool ShowHints
    {
        get => _showHints;
        set => SetProperty(ref _showHints, value);
    }

    /// <summary>
    /// Полноэкранный режим.
    /// </summary>
    public bool IsFullscreen
    {
        get => _isFullscreen;
        set => SetProperty(ref _isFullscreen, value);
    }

    /// <summary>
    /// Доступные уровни сложности.
    /// </summary>
    public List<string> Difficulties { get; } = new() { "Легкая", "Средняя", "Сложная" };

    /// <summary>
    /// Команда сохранения.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Команда сброса.
    /// </summary>
    public ICommand ResetCommand { get; }

    /// <summary>
    /// Команда возврата в меню.
    /// </summary>
    public ICommand BackToMenuCommand { get; }

    /// <summary>
    /// Активация — загрузка настроек.
    /// </summary>
    public override void OnActivated()
    {
        base.OnActivated();
        LoadSettings();
    }

    /// <summary>
    /// Загружает настройки.
    /// </summary>
    private void LoadSettings()
    {
        _settingsService.LoadSettings();
        var s = _settingsService.Settings;

        MusicVolume = s.MusicVolume;
        EffectsVolume = s.EffectsVolume;
        SoundVolume = Math.Max(MusicVolume, EffectsVolume); // Общая громкость = максимум из музыки и эффектов
        ShowHints = s.ShowHints;
        IsFullscreen = s.IsFullscreen;
    }

    /// <summary>
    /// Сохраняет настройки.
    /// </summary>
    private void SaveSettings(object? parameter)
    {
        // Синхронизируем MusicVolume и EffectsVolume с общей громкостью
        _settingsService.Settings.MusicVolume = SoundVolume;
        _settingsService.Settings.EffectsVolume = SoundVolume;
        _settingsService.Settings.ShowHints = ShowHints;
        _settingsService.Settings.IsFullscreen = IsFullscreen;

        _settingsService.SaveSettings();

        // Применяем громкость к фоновой музыке (0-100 → 0.0-1.0)
        _backgroundMusic.SetVolume(SoundVolume / 100.0);

        // Применение полноэкранного режима к главному окну
        // Примечание: AllowsTransparency нельзя менять после отображения окна,
        // поэтому мы управляем только WindowState и размером/позицией окна
        if (System.Windows.Application.Current.MainWindow is System.Windows.Window mainWindow)
        {
            if (IsFullscreen)
            {
                // Полноэкранный режим: максимизировать окно
                mainWindow.WindowStyle = System.Windows.WindowStyle.None;
                mainWindow.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                // Неполноэкранный режим: восстановить нормальный размер и позицию
                // WindowStyle остаётся None из-за AllowsTransparency="True"
                mainWindow.WindowState = System.Windows.WindowState.Normal;
                mainWindow.Width = 1100;
                mainWindow.Height = 750;
                mainWindow.Topmost = false;
            }
        }

        System.Windows.MessageBox.Show(
            "Настройки сохранены!",
            "Успех",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    /// <summary>
    /// Сбрасывает настройки.
    /// </summary>
    private void ResetSettings(object? parameter)
    {
        _settingsService.ResetToDefaults();
        LoadSettings();
    }

    /// <summary>
    /// Возврат в главное меню.
    /// </summary>
    private void BackToMenu(object? parameter)
    {
        _navigationService.NavigateTo<MainViewModel>();
    }
}
