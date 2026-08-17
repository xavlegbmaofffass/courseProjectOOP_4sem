using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Способность, дарующая другую способность цели.
/// </summary>
public class GrantAbility : Ability
{
    public Ability? AbilityToGrant { get; set; }

    public GrantAbility()
    {
        Type = AbilityType.Buff;
        Target = AbilityTarget.AllyUnit;
        Name = "Дарование способности";
    }

    public override (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board)
    {
        if (target == null || AbilityToGrant == null)
        {
            return (false, "Цель или даруемая способность не найдены");
        }

        target.Abilities ??= new List<Ability>();
        target.Abilities.Add(AbilityToGrant.Clone());

        return (true, $"{target.Name} получил новую способность: {AbilityToGrant.Name}");
    }

    public override Ability Clone()
    {
        return new GrantAbility
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Value = Value,
            Range = Range,
            IsActive = IsActive,
            AbilityToGrant = AbilityToGrant?.Clone(),
            Target = Target
        };
    }
}
