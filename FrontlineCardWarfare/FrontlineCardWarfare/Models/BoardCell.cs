using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FrontlineCardWarfare.Models;

/// <summary>
/// Представляет клетку игрового поля 4×4.
/// </summary>
public class BoardCell : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        return false;
    }
    private Unit? _unit;
    private bool _isAvailableForMove;
    private bool _isAvailableForAttack;

    /// <summary>
    /// Row позиции клетки (0-3).
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Column позиции клетки (0-3).
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Юнит, находящийся на клетке.
    /// </summary>
    public Unit? Unit
    {
        get => _unit;
        set
        {
            // Отписка от старого юнита
            if (_unit != null)
            {
                _unit.PropertyChanged -= Unit_PropertyChanged;
            }

            if (_unit != value)
            {
                _unit = value;
                
                // Подписка на новый юнит
                if (_unit != null)
                {
                    _unit.PropertyChanged += Unit_PropertyChanged;
                }

                OnPropertyChanged(nameof(Unit));
                OnPropertyChanged(nameof(IsEmpty));
                OnPropertyChanged(nameof(UnitImagePath));
                OnPropertyChanged(nameof(HasUnit));
                OnPropertyChanged(nameof(CardTypeLabel));
            }
        }
    }

    private void Unit_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Перенаправляем изменения юнита в PropertyChanged для UI
        if (e.PropertyName == nameof(Unit.ImagePath) || e.PropertyName == nameof(Unit.CardType))
        {
            OnPropertyChanged(nameof(UnitImagePath));
            OnPropertyChanged(nameof(CardTypeLabel));
        }
    }

    /// <summary>
    /// Принадлежит ли клетка игроку (true) или врагу (false).
    /// </summary>
    public bool IsPlayerSide { get; set; }

    /// <summary>
    /// Пуста ли клетка.
    /// </summary>
    public bool IsEmpty => Unit == null;

    /// <summary>
    /// Доступна ли клетка для перемещения (для подсветки).
    /// </summary>
    public bool IsAvailableForMove
    {
        get => _isAvailableForMove;
        set
        {
            if (_isAvailableForMove != value)
            {
                _isAvailableForMove = value;
                OnPropertyChanged(nameof(IsAvailableForMove));
            }
        }
    }

    /// <summary>
    /// Доступна ли клетка для атаки (для подсветки).
    /// </summary>
    public bool IsAvailableForAttack
    {
        get => _isAvailableForAttack;
        set
        {
            if (_isAvailableForAttack != value)
            {
                _isAvailableForAttack = value;
                OnPropertyChanged(nameof(IsAvailableForAttack));
            }
        }
    }

    /// <summary>
    /// Уникальный идентификатор клетки.
    /// </summary>
    public int Id => Row * 4 + Column;

    /// <summary>
    /// Цвет рамки клетки в зависимости от типа юнита (для конвертера).
    /// </summary>
    public string CardTypeLabel => Unit?.CardType.ToString() ?? "Empty";

    /// <summary>
    /// Путь к изображению юнита на клетке.
    /// </summary>
    public string UnitImagePath => Unit?.ImagePath ?? string.Empty;

    /// <summary>
    /// Есть ли юнит на клетке.
    /// </summary>
    public bool HasUnit => Unit != null;

    /// <summary>
    /// Создаёт копию клетки.
    /// </summary>
    public BoardCell Clone()
    {
        return new BoardCell
        {
            Row = Row,
            Column = Column,
            Unit = Unit?.Clone(),
            IsPlayerSide = IsPlayerSide,
            IsAvailableForMove = IsAvailableForMove,
            IsAvailableForAttack = IsAvailableForAttack
        };
    }
}
