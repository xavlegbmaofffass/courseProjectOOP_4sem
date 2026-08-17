using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Предсмертный хрип — эффект, срабатывающий при смерти юнита.
/// </summary>
public class DeathrattleAbility : Ability
{
    /// <summary>
    /// Вложенная способность, которая будет выполнена при смерти.
    /// </summary>
    public Ability? Effect { get; set; }

    public DeathrattleAbility()
    {
        Type = AbilityType.Deathrattle;
        Name = "Предсмертный хрип";
    }

    public DeathrattleAbility(Ability effect) : this()
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
            return (false, "Эффект предсмертного хрипа не определён");
        }

        return Effect.Execute(source, target, board);
    }

    public override Ability Clone()
    {
        var clonedEffect = Effect?.Clone();
        return new DeathrattleAbility(clonedEffect!)
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
