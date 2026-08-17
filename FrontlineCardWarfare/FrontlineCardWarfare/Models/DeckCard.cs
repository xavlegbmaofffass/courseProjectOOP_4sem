using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace FrontlineCardWarfare.Data;

/// <summary>
/// Сущность связи колоды и карты.
/// </summary>
public class DeckCard : INotifyPropertyChanged
{
    private int _quantity;

    /// <summary>
    /// Уникальный идентификатор записи.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор колоды.
    /// </summary>
    [ForeignKey("Deck")]
    public int DeckId { get; set; }

    /// <summary>
    /// Идентификатор карты.
    /// </summary>
    [ForeignKey("Card")]
    public int CardId { get; set; }

    /// <summary>
    /// Количество копий карты в колоде.
    /// </summary>
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Навигационное свойство для колоды.
    /// </summary>
    public Deck? Deck { get; set; }

    /// <summary>
    /// Навигационное свойство для карты.
    /// </summary>
    public Card? Card { get; set; }

    /// <summary>
    /// Событие изменения свойства.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Вызывает событие изменения свойства.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
