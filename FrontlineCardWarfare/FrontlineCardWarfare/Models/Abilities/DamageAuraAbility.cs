using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Аура урона — наносит урон всем врагам в радиусе действия каждый ход.
/// </summary>
public class DamageAuraAbility : Ability
{
    public DamageAuraAbility()
    {
        Type = AbilityType.Aura;
        Target = AbilityTarget.AllEnemies;
        Name = "Аура урона";
    }

    public override (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board)
    {
        if (source == null || board == null)
        {
            return (false, "Источник не найден");
        }

        int affectedCount = 0;
        var allUnits = board.GetAllAliveUnits();

        foreach (var unit in allUnits)
        {
            // Проверяем, является ли юнит врагом источника
            if (unit.IsPlayer != source.IsPlayer)
            {
                int distance = Math.Abs(unit.Row - source.Row) + Math.Abs(unit.Column - source.Column);
                if (distance <= Range)
                {
                    unit.TakeDamage(Value);
                    affectedCount++;
                }
            }
        }

        return (true, $"Аура урона поразила {affectedCount} врагов");
    }

    public override Ability Clone()
    {
        return new DamageAuraAbility
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
