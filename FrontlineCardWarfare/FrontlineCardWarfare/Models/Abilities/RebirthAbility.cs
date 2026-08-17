using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Способность возрождения — возвращает юнита к жизни с 1 здоровьем при смерти.
/// </summary>
public class RebirthAbility : Ability
{
    public RebirthAbility()
    {
        Type = AbilityType.Deathrattle;
        Target = AbilityTarget.Self;
        Name = "Возрождение";
        Description = "Возвращается с 1 здоровьем после смерти";
    }

    public override (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board)
    {
        if (source == null)
        {
            return (false, "Источник не найден");
        }

        source.CurrentHealth = 1;
        // Удаляем саму способность, чтобы не возрождаться бесконечно (если нужно)
        // Для Феникса может быть бесконечно, но обычно один раз.
        // User didn't specify, but usually it's once.
        IsActive = false; 

        return (true, $"{source.Name} возродился!");
    }

    public override Ability Clone()
    {
        return new RebirthAbility
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
