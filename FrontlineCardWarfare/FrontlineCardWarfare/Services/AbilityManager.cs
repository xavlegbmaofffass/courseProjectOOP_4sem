using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Менеджер эффектов — управляет применением способностей и разрешением триггеров.
/// </summary>
public class AbilityManager : IAbilityManager
{
    private readonly List<Ability> _activeEffects = new();

    /// <summary>
    /// Применяет эффект к цели.
    /// </summary>
    public (bool Success, string Message) ApplyEffect(Ability ability, Unit? source, Unit? target, Board? board)
    {
        if (!ability.IsActive)
        {
            return (false, "Способность неактивна");
        }

        try
        {
            var result = ability.Execute(source, target, board);

            if (result.Success)
            {
                // Для постоянных эффектов добавляем в список активных
                if (ability.Type is AbilityType.Aura or AbilityType.Passive)
                {
                    _activeEffects.Add(ability);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            return (false, $"Ошибка применения эффекта: {ex.Message}");
        }
    }

    /// <summary>
    /// Разрешает все триггеры по условию.
    /// </summary>
    public List<string> ResolveTriggers(AbilityTrigger trigger, Unit? source, Board? board)
    {
        var messages = new List<string>();

        if (source == null)
            return messages;

        // Получаем способности юнита
        var abilities = source.Abilities ?? new List<Ability>();

        foreach (var ability in abilities)
        {
            if (!ability.IsActive)
                continue;

            // Проверяем, соответствует ли тип способности триггеру
            bool shouldTrigger = trigger switch
            {
                AbilityTrigger.OnPlay => ability.Type == AbilityType.Battlecry,
                AbilityTrigger.OnDeath => ability.Type == AbilityType.Deathrattle,
                AbilityTrigger.OnTurnStart => ability.Type == AbilityType.Passive,
                AbilityTrigger.OnTurnEnd => ability.Type == AbilityType.Aura,
                AbilityTrigger.OnAttack => ability.Type is AbilityType.Freeze or AbilityType.Damage,
                _ => false
            };

            if (shouldTrigger)
            {
                // Определяем цель в зависимости от типа способности
                Unit? target = DetermineTarget(ability, source, board);

                var result = ApplyEffect(ability, source, target, board);
                if (result.Success)
                {
                    messages.Add(result.Message);
                }
            }
        }

        return messages;
    }

    /// <summary>
    /// Применяет ауры всех юнитов на поле.
    /// </summary>
    public List<string> ApplyAllAuras(Board board)
    {
        var messages = new List<string>();
        var units = board.GetAllAliveUnits();

        foreach (var unit in units)
        {
            var auras = unit.Abilities?.Where(a => a.Type == AbilityType.Aura).ToList() ?? new List<Ability>();

            foreach (var aura in auras)
            {
                var result = ApplyEffect(aura, unit, null, board);
                if (result.Success)
                {
                    messages.Add(result.Message);
                }
            }
        }

        return messages;
    }

    /// <summary>
    /// Размораживает юнит в начале его хода.
    /// </summary>
    public void UnfreezeUnit(Unit unit)
    {
        if (unit.IsFrozen)
        {
            unit.IsFrozen = false;
            unit.CanAttack = true;
        }
    }

    /// <summary>
    /// Определяет цель способности на основе типа.
    /// </summary>
    private Unit? DetermineTarget(Ability ability, Unit source, Board? board)
    {
        return ability.Target switch
        {
            AbilityTarget.None => null,
            AbilityTarget.Self => source,
            AbilityTarget.EnemyUnit => FindNearestEnemy(source, board),
            AbilityTarget.AllyUnit => FindNearestAlly(source, board),
            AbilityTarget.SameColumnAlly => FindSameColumnAlly(source, board),
            AbilityTarget.AllEnemies => null, // Обрабатывается внутри способности
            AbilityTarget.AllAllies => null, // Обрабатывается внутри способности
            AbilityTarget.NeighborAllies => null, // Обрабатывается внутри способности
            _ => null
        };
    }

    /// <summary>
    /// Находит союзника в той же колонке.
    /// </summary>
    private Unit? FindSameColumnAlly(Unit source, Board? board)
    {
        if (board == null) return null;

        // В той же колонке, но в другом ряду
        for (int r = 0; r < Board.Rows; r++)
        {
            if (r == source.Row) continue;
            
            var unit = board.GetCell(r, source.Column)?.Unit;
            if (unit != null && unit.IsAlive && unit.IsPlayer == source.IsPlayer)
            {
                return unit;
            }
        }

        return null;
    }

    /// <summary>
    /// Находит ближайшего врага.
    /// </summary>
    private Unit? FindNearestEnemy(Unit source, Board? board)
    {
        if (board == null) return null;

        Unit? nearest = null;
        int minDistance = int.MaxValue;

        foreach (var unit in board.GetAllAliveUnits())
        {
            if (unit.IsPlayer != source.IsPlayer)
            {
                int distance = Math.Abs(unit.Row - source.Row) + Math.Abs(unit.Column - source.Column);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = unit;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// Находит ближайшего союзника.
    /// </summary>
    private Unit? FindNearestAlly(Unit source, Board? board)
    {
        if (board == null) return null;

        Unit? nearest = null;
        int minDistance = int.MaxValue;

        foreach (var unit in board.GetAllAliveUnits())
        {
            if (unit.IsPlayer == source.IsPlayer && unit.Id != source.Id)
            {
                int distance = Math.Abs(unit.Row - source.Row) + Math.Abs(unit.Column - source.Column);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = unit;
                }
            }
        }

        return nearest;
    }
}
