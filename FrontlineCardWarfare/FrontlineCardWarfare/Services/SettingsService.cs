using System.IO;
using System.Text.Json;
using FrontlineCardWarfare.Models;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса настроек.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Текущие настройки.
    /// </summary>
    GameSettings Settings { get; }

    /// <summary>
    /// Загружает настройки из файла.
    /// </summary>
    void LoadSettings();

    /// <summary>
    /// Сохраняет настройки в файл.
    /// </summary>
    void SaveSettings();

    /// <summary>
    /// Сбрасывает настройки к значениям по умолчанию.
    /// </summary>
    void ResetToDefaults();

    /// <summary>
    /// Включает/выключает подсказки.
    /// </summary>
    void SetShowHints(bool showHints);
}

/// <summary>
/// Сервис настроек — загрузка/сохранение настроек в JSON.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly string _settingsPath;
    private GameSettings _settings = new();

    /// <summary>
    /// Текущие настройки.
    /// </summary>
    public GameSettings Settings => _settings;

    /// <summary>
    /// Инициализирует новый экземпляр SettingsService.
    /// </summary>
    public SettingsService()
    {
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FrontlineCardWarfare",
            "settings.json");
    }

    /// <summary>
    /// Загружает настройки из файла.
    /// </summary>
    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var loaded = JsonSerializer.Deserialize<GameSettings>(json);
                    if (loaded != null)
                    {
                        _settings = loaded;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки настроек: {ex.Message}");
            _settings = new GameSettings();
        }
    }

    /// <summary>
    /// Сохраняет настройки в файл.
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения настроек: {ex.Message}");
        }
    }

    /// <summary>
    /// Сбрасывает настройки к значениям по умолчанию.
    /// </summary>
    public void ResetToDefaults()
    {
        _settings = new GameSettings();
        SaveSettings();
    }

    /// <summary>
    /// Включает/выключает подсказки.
    /// </summary>
    public void SetShowHints(bool showHints)
    {
        _settings.ShowHints = showHints;
        SaveSettings();
    }
}
