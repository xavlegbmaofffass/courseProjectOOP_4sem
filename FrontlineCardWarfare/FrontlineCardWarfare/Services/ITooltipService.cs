namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс сервиса подсказок и визуальной обратной связи.
/// </summary>
public interface ITooltipService
{
    /// <summary>
    /// Включать/выключать подсказки.
    /// </summary>
    bool TooltipsEnabled { get; set; }

    /// <summary>
    /// Событие при показе ошибки/подсказки.
    /// </summary>
    event Action<string>? OnError;

    /// <summary>
    /// Показывает подсказку для карты.
    /// </summary>
    void ShowCardTooltip(string cardName, int attack, int health, int range, string? ability);

    /// <summary>
    /// Показывает подсказку для юнита.
    /// </summary>
    void ShowUnitTooltip(string unitName, int currentHealth, int maxHealth, int attack, bool hasAttacked);

    /// <summary>
    /// Показывает ошибку атаки вне диапазона.
    /// </summary>
    void ShowOutOfRangeError();

    /// <summary>
    /// Показывает ошибку неверного хода.
    /// </summary>
    void ShowNotYourTurnError();

    /// <summary>
    /// Показывает ошибку атаки союзника.
    /// </summary>
    void ShowFriendlyFireError();

    /// <summary>
    /// Показывает ошибку занятой клетки.
    /// </summary>
    void ShowCellOccupiedError();

    /// <summary>
    /// Показывает произвольную ошибку.
    /// </summary>
    void ShowError(string message);

    /// <summary>
    /// Очищает все подсказки.
    /// </summary>
    void ClearAll();
}
