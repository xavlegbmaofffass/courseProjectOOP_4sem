namespace FrontlineCardWarfare.Data;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Типы карт в игре.
/// </summary>
public enum CardType
{
    Melee = 0,      // Ближний бой
    Ranged = 1,     // Дальний бой
    Siege = 2,      // Осадная
    Support = 3,    // Поддержка
    Special = 4     // Особенная
}

/// <summary>
/// Сущность карты.
/// </summary>
public class Card : INotifyPropertyChanged
{
    /// <summary>
    /// Уникальный идентификатор карты.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название карты.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Путь к изображению карты.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Атака карты.
    /// </summary>
    public int Attack { get; set; }

    /// <summary>
    /// Здоровье карты.
    /// </summary>
    public int Health { get; set; }

    /// <summary>
    /// Дальность атаки.
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Способность карты (текстовое описание).
    /// </summary>
    public string? Ability { get; set; }

    /// <summary>
    /// Тип карты.
    /// </summary>
    public CardType CardType { get; set; }

    /// <summary>
    /// Описание карты для подсказки.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Создаёт копию карты.
    /// </summary>
    public Card Clone()
    {
        return new Card
        {
            Id = Id,
            Name = Name,
            ImagePath = ImagePath,
            Attack = Attack,
            Health = Health,
            Range = Range,
            Ability = Ability,
            CardType = CardType,
            Description = Description
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
