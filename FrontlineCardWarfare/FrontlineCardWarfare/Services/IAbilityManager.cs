using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Интерфейс менеджера эффектов.
/// </summary>
public interface IAbilityManager
{
    /// <summary>
    /// Применяет эффект к цели.
    /// </summary>
    (bool Success, string Message) ApplyEffect(Ability ability, Unit? source, Unit? target, Board? board);

    /// <summary>
    /// Разрешает все триггеры по условию (боевой клич, смерть и т.д.).
    /// </summary>
    List<string> ResolveTriggers(AbilityTrigger trigger, Unit? source, Board? board);

    /// <summary>
    /// Применяет ауры всех юнитов на поле.
    /// </summary>
    List<string> ApplyAllAuras(Board board);

    /// <summary>
    /// Размораживает юнитов в начале их хода.
    /// </summary>
    void UnfreezeUnit(Unit unit);
}

/// <summary>
/// Тип триггера способности.
/// </summary>
public enum AbilityTrigger
{
    /// <summary>
    /// При розыгрыше карты на поле (Battlecry).
    /// </summary>
    OnPlay,

    /// <summary>
    /// При смерти юнита (Deathrattle).
    /// </summary>
    OnDeath,

    /// <summary>
    /// В начале хода юнита.
    /// </summary>
    OnTurnStart,

    /// <summary>
    /// В конце хода юнита.
    /// </summary>
    OnTurnEnd,

    /// <summary>
    /// При атаке.
    /// </summary>
    OnAttack
}
