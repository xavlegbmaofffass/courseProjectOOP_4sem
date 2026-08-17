using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Сервис для управления подсказками и визуальной обратной связью.
/// </summary>
public class TooltipService : ITooltipService
{
    private readonly DispatcherTimer _tooltipTimer;
    private readonly DispatcherTimer _errorTimer;
    private bool _tooltipsEnabled = true;
    private string _currentError = string.Empty;

    /// <summary>
    /// Событие при показе ошибки.
    /// </summary>
    public event Action<string>? OnError;

    /// <summary>
    /// Включать/выключать подсказки.
    /// </summary>
    public bool TooltipsEnabled
    {
        get => _tooltipsEnabled;
        set => _tooltipsEnabled = value;
    }

    public TooltipService()
    {
        // Таймер для задержки подсказки (1.5 сек)
        _tooltipTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5)
        };
        _tooltipTimer.Tick += (s, e) => ShowQueuedTooltip();

        // Таймер для автоисчезновения ошибок (3 сек)
        _errorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _errorTimer.Tick += (s, e) => HideLastError();
    }

    private Queue<TooltipRequest> _tooltipQueue = new();

    /// <summary>
    /// Запрос на отображение подсказки.
    /// </summary>
    public class TooltipRequest
    {
        public string Content { get; set; } = string.Empty;
        public Point Position { get; set; }
        public double Width { get; set; } = 200;
        public double Height { get; set; } = 150;
    }

    /// <summary>
    /// Показывает подсказку для карты.
    /// </summary>
    public void ShowCardTooltip(string cardName, int attack, int health, int range, string? ability)
    {
        if (!_tooltipsEnabled) return;

        var content = $"<b>{cardName}</b>\n\n" +
                     $"⚔️ Атака: {attack}\n" +
                     $"❤️ Здоровье: {health}\n" +
                     $"📏 Дальность: {range}\n" +
                     (string.IsNullOrEmpty(ability) ? "" : $"✨ Способность: {ability}");

        ShowTooltip(content);
    }

    /// <summary>
    /// Показывает подсказку для юнита.
    /// </summary>
    public void ShowUnitTooltip(string unitName, int currentHealth, int maxHealth, int attack, bool hasAttacked)
    {
        if (!_tooltipsEnabled) return;

        var status = hasAttacked ? "✝️ Атаковал" : "⚔️ Готов к атаке";
        var content = $"<b>{unitName}</b>\n\n" +
                     $"❤️ Здоровье: {currentHealth}/{maxHealth}\n" +
                     $"⚔️ Атака: {attack}\n" +
                     $"{status}";

        ShowTooltip(content);
    }

    /// <summary>
    /// Показывает общую подсказку.
    /// </summary>
    private void ShowTooltip(string content)
    {
        _tooltipQueue.Clear();
        _tooltipQueue.Enqueue(new TooltipRequest { Content = content });
        _tooltipTimer.Stop();
        _tooltipTimer.Start();
    }

    private void ShowQueuedTooltip()
    {
        _tooltipTimer.Stop();
        
        if (_tooltipQueue.Count == 0) return;

        var request = _tooltipQueue.Dequeue();
        OnError?.Invoke(request.Content); // Для простоты используем это событие
    }

    /// <summary>
    /// Показывает ошибку с автоисчезновением (3 сек).
    /// </summary>
    public void ShowError(string message)
    {
        _currentError = message;
        OnError?.Invoke(message);
        
        _errorTimer.Stop();
        _errorTimer.Start();
    }

    private void HideLastError()
    {
        _errorTimer.Stop();
        _currentError = string.Empty;
        OnError?.Invoke(string.Empty); // Скрыть
    }

    /// <summary>
    /// Показывает ошибку атаки вне диапазона.
    /// </summary>
    public void ShowOutOfRangeError()
    {
        ShowError("Цель вне диапазона атаки!");
    }

    /// <summary>
    /// Показывает ошибку неверного хода.
    /// </summary>
    public void ShowNotYourTurnError()
    {
        ShowError("Сейчас не ваш ход!");
    }

    /// <summary>
    /// Показывает ошибку атаки союзника.
    /// </summary>
    public void ShowFriendlyFireError()
    {
        ShowError("Нельзя атаковать союзника!");
    }

    /// <summary>
    /// Показывает ошибку занятой клетки.
    /// </summary>
    public void ShowCellOccupiedError()
    {
        ShowError("Клетка занята!");
    }

    /// <summary>
    /// Очищает все подсказки.
    /// </summary>
    public void ClearAll()
    {
        _tooltipTimer.Stop();
        _errorTimer.Stop();
        _tooltipQueue.Clear();
        _currentError = string.Empty;
        OnError?.Invoke(string.Empty);
    }
}

/// <summary>
/// Контрол для отображения подсказки (не используется, оставлен для совместимости).
/// </summary>
public class TooltipControl : Control
{
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(string), typeof(TooltipControl));

    public string Content
    {
        get => (string)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    static TooltipControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TooltipControl),
            new FrameworkPropertyMetadata(typeof(TooltipControl)));
    }
}
