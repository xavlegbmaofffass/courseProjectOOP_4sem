using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Способность исцеления — восстанавливает здоровье союзному юниту.
/// </summary>
public class HealAbility : Ability
{
    public HealAbility()
    {
        Type = AbilityType.Heal;
        Target = AbilityTarget.AllyUnit;
        Name = "Исцеление";
    }

    public override (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board)
    {
        if (target == null || !target.IsAlive)
        {
            return (false, "Цель не найдена или мертва");
        }

        int healed = target.Heal(Value);
        return (true, $"{target.Name} восстановил {healed} здоровья");
    }

    public override Ability Clone()
    {
        return new HealAbility
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
