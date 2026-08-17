using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrontlineCardWarfare.Data;

/// <summary>
/// Сущность колоды карт.
/// </summary>
public class Deck
{
    /// <summary>
    /// Уникальный идентификатор колоды.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор владельца колоды.
    /// </summary>
    [ForeignKey("User")]
    public int UserId { get; set; }

    /// <summary>
    /// Название колоды.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания колоды.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Навигационное свойство для пользователя.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Карты в колоде.
    /// </summary>
    public ICollection<DeckCard> DeckCards { get; set; } = new List<DeckCard>();

    /// <summary>
    /// Возвращает строковое представление колоды (название).
    /// </summary>
    public override string ToString() => Name;
}
