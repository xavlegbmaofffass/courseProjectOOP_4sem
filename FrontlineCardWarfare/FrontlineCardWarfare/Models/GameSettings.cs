namespace FrontlineCardWarfare.Models;

/// <summary>
/// Настройки приложения.
/// </summary>
public class GameSettings
{
    /// <summary>
    /// Громкость музыки (0-100).
    /// </summary>
    public int MusicVolume { get; set; } = 50;

    /// <summary>
    /// Громкость звуковых эффектов (0-100).
    /// </summary>
    public int EffectsVolume { get; set; } = 50;

    /// <summary>
    /// Показывать подсказки во время боя.
    /// </summary>
    public bool ShowHints { get; set; } = true;

    /// <summary>
    /// Полноэкранный режим.
    /// </summary>
    public bool IsFullscreen { get; set; } = false;

    /// <summary>
    /// Ширина окна.
    /// </summary>
    public int WindowWidth { get; set; } = 1280;

    /// <summary>
    /// Высота окна.
    /// </summary>
    public int WindowHeight { get; set; } = 800;

    /// <summary>
    /// Показывать анимации.
    /// </summary>
    public bool ShowAnimations { get; set; } = true;

    /// <summary>
    /// Показывать эффекты способностей.
    /// </summary>
    public bool ShowAbilityEffects { get; set; } = true;
}
