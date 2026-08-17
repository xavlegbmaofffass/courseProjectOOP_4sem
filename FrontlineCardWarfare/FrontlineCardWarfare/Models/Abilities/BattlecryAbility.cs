using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Боевой клич — эффект, срабатывающий при розыгрыше карты на поле.
/// Может наносить урон, исцелять, давать баффы и т.д.
/// </summary>
public class BattlecryAbility : Ability
{
    /// <summary>
    /// Вложенная способность, которая будет выполнена при боевом кличе.
    /// </summary>
    public Ability? Effect { get; set; }

    public BattlecryAbility()
    {
        Type = AbilityType.Battlecry;
        Name = "Боевой клич";
    }

    public BattlecryAbility(Ability effect) : this()
    {
        Effect = effect;
        Description = effect.Description;
        Target = effect.Target;
        Value = effect.Value;
        Range = effect.Range;
    }

    public override (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board)
    {
        if (Effect == null)
        {
            return (false, "Эффект боевого клича не определён");
        }

        return Effect.Execute(source, target, board);
    }

    public override Ability Clone()
    {
        var clonedEffect = Effect?.Clone();
        return new BattlecryAbility(clonedEffect!)
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Value = Value,
            Range = Range,
            IsActive = IsActive
        };
    }
}
