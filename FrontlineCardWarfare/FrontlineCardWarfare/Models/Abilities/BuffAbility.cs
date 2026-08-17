using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Способность усиления — увеличивает атаку и/или здоровье юнита.
/// </summary>
public class BuffAbility : Ability
{
    /// <summary>
    /// Бонус к атаке.
    /// </summary>
    public int AttackBonus { get; set; }

    /// <summary>
    /// Бонус к здоровью (текущему и максимальному).
    /// </summary>
    public int HealthBonus { get; set; }

    public BuffAbility()
    {
        Type = AbilityType.Buff;
        Target = AbilityTarget.AllyUnit;
        Name = "Усиление";
    }

    public override (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board)
    {
        if (source == null || board == null)
        {
            return (false, "Источник не найден");
        }

        if (Target == AbilityTarget.AllAllies)
        {
            int count = 0;
            foreach (var unit in board.GetPlayerUnits(source.IsPlayer))
            {
                ApplyBuff(unit);
                count++;
            }
            return (true, $"Усиление применено ко всем союзникам ({count})");
        }

        if (Target == AbilityTarget.NeighborAllies)
        {
            int count = 0;
            var neighbors = GetNeighbors(source, board);
            foreach (var unit in neighbors)
            {
                if (unit.IsPlayer == source.IsPlayer)
                {
                    ApplyBuff(unit);
                    count++;
                }
            }
            return (true, $"Вдохновение усилило {count} соседей");
        }

        if (target == null || !target.IsAlive)
        {
            return (false, "Цель не найдена или мертва");
        }

        ApplyBuff(target);
        return (true, $"{target.Name} получил +{AttackBonus}/+{HealthBonus}");
    }

    private void ApplyBuff(Unit target)
    {
        target.Attack += AttackBonus;
        if (HealthBonus > 0)
        {
            target.MaxHealth += HealthBonus;
            target.CurrentHealth += HealthBonus;
        }
    }

    private List<Unit> GetNeighbors(Unit source, Board board)
    {
        var neighbors = new List<Unit>();
        int[] dr = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dc = { 0, 0, -1, 1, -1, 1, -1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int r = source.Row + dr[i];
            int c = source.Column + dc[i];

            if (r >= 0 && r < Board.Rows && c >= 0 && c < Board.Columns)
            {
                var unit = board.GetCell(r, c)?.Unit;
                if (unit != null && unit.IsAlive)
                {
                    neighbors.Add(unit);
                }
            }
        }
        return neighbors;
    }

    public override Ability Clone()
    {
        return new BuffAbility
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Value = Value,
            Range = Range,
            IsActive = IsActive,
            AttackBonus = AttackBonus,
            HealthBonus = HealthBonus,
            Target = Target
        };
    }
}
