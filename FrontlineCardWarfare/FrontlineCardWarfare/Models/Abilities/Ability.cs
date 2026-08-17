namespace FrontlineCardWarfare.Models.Abilities;

/// <summary>
/// Тип способности карты.
/// </summary>
public enum AbilityType
{
    /// <summary>
    /// Боевой клич — эффект при розыгрыше карты на поле.
    /// </summary>
    Battlecry,

    /// <summary>
    /// Предсмертный хрип — эффект при смерти юнита.
    /// </summary>
    Deathrattle,

    /// <summary>
    /// Аура — постоянный эффект, действующий на соседние юниты.
    /// </summary>
    Aura,

    /// <summary>
    /// Пассивная — постоянный эффект на самого юнита.
    /// </summary>
    Passive,

    /// <summary>
    /// Активная — эффект, активируемый игроком вручную.
    /// </summary>
    Active,

    /// <summary>
    /// Исцеление — восстанавливает здоровье союзному юниту.
    /// </summary>
    Heal,

    /// <summary>
    /// Заморозка — предотвращает атаку цели в следующем ходу.
    /// </summary>
    Freeze,

    /// <summary>
    /// Призыв — вызывает дополнительного юнита на поле.
    /// </summary>
    Summon,

    /// <summary>
    /// Урон — наносит урон выбранной цели.
    /// </summary>
    Damage,

    /// <summary>
    /// Бафф — увеличивает характеристики цели.
    /// </summary>
    Buff,

    /// <summary>
    /// Воскрешение — возвращает юнита к жизни.
    /// </summary>
    Resurrect
}

/// <summary>
/// Цель способности.
/// </summary>
public enum AbilityTarget
{
    /// <summary>
    /// Нет цели (пассивная/аура).
    /// </summary>
    None,

    /// <summary>
    /// Вражеский юнит.
    /// </summary>
    EnemyUnit,

    /// <summary>
    /// Союзный юнит.
    /// </summary>
    AllyUnit,

    /// <summary>
    /// Все вражеские юниты.
    /// </summary>
    AllEnemies,

    /// <summary>
    /// Все союзные юниты.
    /// </summary>
    AllAllies,

    /// <summary>
    /// Сам юнит.
    /// </summary>
    Self,

    /// <summary>
    /// Союзник в той же колонке.
    /// </summary>
    SameColumnAlly,

    /// <summary>
    /// Все соседние союзники.
    /// </summary>
    NeighborAllies
}

/// <summary>
/// Базовый класс для всех способностей карт.
/// </summary>
public abstract class Ability
{
    /// <summary>
    /// Уникальный идентификатор способности.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название способности.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание эффекта.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Тип способности.
    /// </summary>
    public AbilityType Type { get; set; }

    /// <summary>
    /// Цель способности.
    /// </summary>
    public AbilityTarget Target { get; set; }

    /// <summary>
    /// Значение эффекта (урон, лечение, бонус и т.д.).
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Радиус действия (для ауры и целевых способностей).
    /// </summary>
    public int Range { get; set; } = 1;

    /// <summary>
    /// Выполняется ли способность.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Применяет способность к цели.
    /// </summary>
    /// <param name="source">Юнит-источник способности.</param>
    /// <param name="target">Юнит-цель (может быть null для пассивных).</param>
    /// <param name="board">Игровое поле (для глобальных эффектов).</param>
    /// <returns>Результат применения (успех/ошибка).</returns>
    public abstract (bool Success, string Message) Execute(Unit? source, Unit? target, Board? board);

    /// <summary>
    /// Создаёт копию способности.
    /// </summary>
    public abstract Ability Clone();
}
