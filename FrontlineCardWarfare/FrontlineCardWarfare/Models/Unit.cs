using CommunityToolkit.Mvvm.ComponentModel;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models.Abilities;

namespace FrontlineCardWarfare.Models;

/// <summary>
/// Представляет юнита на игровом поле с поддержкой уведомлений об изменениях.
/// </summary>
public partial class Unit : ObservableObject
{
    private string _name = string.Empty;
    private int _currentHealth;
    private int _maxHealth;
    private int _attack;
    private string _imagePath = string.Empty;
    private int _row;
    private int _column;
    private bool _canAttack = true;
    private bool _canMove = true;
    private bool _hasMoved;
    private bool _hasAttacked;
    private bool _isFrozen;

    /// <summary>
    /// Уникальный идентификатор юнита в рамках сессии.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID карты, на основе которой создан юнит.
    /// </summary>
    public int CardId { get; set; }

    /// <summary>
    /// Название карты.
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Текущее здоровье.
    /// </summary>
    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            if (SetProperty(ref _currentHealth, value))
            {
                OnPropertyChanged(nameof(HealthPercent));
                OnPropertyChanged(nameof(HealthColor));
                OnPropertyChanged(nameof(IsAlive));
            }
        }
    }

    /// <summary>
    /// Максимальное здоровье.
    /// </summary>
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            if (SetProperty(ref _maxHealth, value))
            {
                OnPropertyChanged(nameof(HealthPercent));
                OnPropertyChanged(nameof(HealthColor));
            }
        }
    }

    /// <summary>
    /// Атака.
    /// </summary>
    public int Attack
    {
        get => _attack;
        set => SetProperty(ref _attack, value);
    }

    /// <summary>
    /// Дальность атаки.
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Текстовое описание способности (для отображения в UI).
    /// </summary>
    public string? Ability { get; set; }

    /// <summary>
    /// Список способностей юнита.
    /// </summary>
    public List<Ability>? Abilities { get; set; } = new();

    /// <summary>
    /// Тип карты.
    /// </summary>
    public CardType CardType { get; set; }

    /// <summary>
    /// Путь к изображению юнита.
    /// </summary>
    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    /// <summary>
    /// Принадлежит ли юнит игроку (true) или врагу (false).
    /// </summary>
    public bool IsPlayer { get; set; }

    /// <summary>
    /// Позиция Row на поле.
    /// </summary>
    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    /// <summary>
    /// Позиция Column на поле.
    /// </summary>
    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    /// <summary>
    /// Может ли юнит атаковать в этом ходу.
    /// </summary>
    public bool CanAttack
    {
        get => _canAttack;
        set => SetProperty(ref _canAttack, value);
    }

    /// <summary>
    /// Может ли юнит двигаться в этом ходу.
    /// </summary>
    public bool CanMove
    {
        get => _canMove;
        set => SetProperty(ref _canMove, value);
    }

    /// <summary>
    /// Перемещался ли юнит в этом ходу.
    /// </summary>
    public bool HasMoved
    {
        get => _hasMoved;
        set => SetProperty(ref _hasMoved, value);
    }

    /// <summary>
    /// Атаковал ли юнит в этом ходу.
    /// </summary>
    public bool HasAttacked
    {
        get => _hasAttacked;
        set => SetProperty(ref _hasAttacked, value);
    }

    /// <summary>
    /// Заморожен ли юнит (не может атаковать).
    /// </summary>
    public bool IsFrozen
    {
        get => _isFrozen;
        set => SetProperty(ref _isFrozen, value);
    }

    /// <summary>
    /// Процент здоровья (0.0 — 1.0) для привязки к ProgressBar.
    /// </summary>
    public double HealthPercent => MaxHealth > 0 ? (double)CurrentHealth / MaxHealth : 0;

    /// <summary>
    /// Цвет здоровья (для UI): зелёный > 60%, жёлтый 30-60%, красный < 30%.
    /// </summary>
    public string HealthColor => HealthPercent switch
    {
        > 0.6 => "#4CAF50",
        > 0.3 => "#FF9800",
        _ => "#F44336"
    };

    /// <summary>
    /// Жив ли юнит.
    /// </summary>
    public bool IsAlive => CurrentHealth > 0;

    /// <summary>
    /// Создаёт копию юнита.
    /// </summary>
    public Unit Clone()
    {
        return new Unit
        {
            Id = Id,
            CardId = CardId,
            Name = Name,
            CurrentHealth = CurrentHealth,
            MaxHealth = MaxHealth,
            Attack = Attack,
            Range = Range,
            Ability = Ability,
            Abilities = Abilities?.Select(a => a.Clone()).ToList(),
            CardType = CardType,
            ImagePath = ImagePath,
            IsPlayer = IsPlayer,
            Row = Row,
            Column = Column,
            CanAttack = CanAttack,
            CanMove = CanMove,
            HasMoved = HasMoved,
            HasAttacked = HasAttacked,
            IsFrozen = IsFrozen
        };
    }

    /// <summary>
    /// Создаёт юнита из карты.
    /// </summary>
    public static Unit FromCard(Card card, bool isPlayer, int row, int column)
    {
        // Используем изображение по умолчанию, если путь пустой
        string image_path = string.IsNullOrWhiteSpace(card.ImagePath) 
            ? "Resources/Images/default.png" 
            : card.ImagePath;

        // Ограничиваем систему способностей только для Support и Special
        bool canHaveAbilities = card.CardType == CardType.Support || card.CardType == CardType.Special;

        return new Unit
        {
            CardId = card.Id,
            Name = card.Name,
            CurrentHealth = card.Health,
            MaxHealth = card.Health,
            Attack = card.Attack,
            Range = card.Range,
            Ability = canHaveAbilities ? card.Ability : null,
            Abilities = canHaveAbilities ? ParseAbilities(card.Ability) : new List<Ability>(),
            CardType = card.CardType,
            ImagePath = image_path,
            IsPlayer = isPlayer,
            Row = row,
            Column = column,
            CanAttack = false,
            CanMove = false,
            HasMoved = false,
            HasAttacked = false
        };
    }

    /// <summary>
    /// Получение урона. Вызывает уведомления об изменении CurrentHealth и HealthPercent.
    /// </summary>
    public int TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        return CurrentHealth;
    }

    /// <summary>
    /// Исцеление. Вызывает уведомления об изменении CurrentHealth и HealthPercent.
    /// </summary>
    public int Heal(int amount)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        return CurrentHealth;
    }

    /// <summary>
    /// Разморозка юнита.
    /// </summary>
    public void Unfreeze()
    {
        IsFrozen = false;
        CanAttack = true;
    }

    /// <summary>
    /// Парсит текстовое описание способности в список объектов Ability.
    /// Упрощённая реализация — в будущем можно использовать JSON/XML.
    /// </summary>
    public static List<Ability> ParseAbilities(string? abilityText)
    {
        var abilities = new List<Ability>();

        if (string.IsNullOrWhiteSpace(abilityText))
            return abilities;

        var lower = abilityText.ToLower();

        // Жрец неоневого храма: "Исцеление: +3 к здоровью союзнику"
        if (lower.Contains("исцеление"))
        {
            abilities.Add(new BattlecryAbility(new HealAbility 
            { 
                Value = 3, 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Исцеляет союзника в той же колонке на 3" 
            }));
        }

        // Друид синтетического леса: "Природа: +2 к атаке и +2 к здоровью союзнику"
        // Генерал корпорации: "Тактика: +2 к атаке и +2 к здоровью союзнику"
        if (lower.Contains("природа") || lower.Contains("тактика"))
        {
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                AttackBonus = 2, 
                HealthBonus = 2, 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Даёт +2/+2 союзнику в той же колонке" 
            }));
        }

        // Маг кибер-башни: "Огонь: +3 к атаке союзнику"
        if (lower.Contains("огонь: +3"))
        {
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                AttackBonus = 3, 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Даёт +3 к атаке союзнику в той же колонке" 
            }));
        }

        // Чародей голографических иллюзий: "Проклятие на врага: +4 к атаке союзника"
        if (lower.Contains("проклятие"))
        {
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                AttackBonus = 4, 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Даёт +4 к атаке союзнику в той же колонке" 
            }));
        }

        // Некромант цифрового мира: "Воскрешение: возвращает юнита сразу после его смерти с 1 здоровьем"
        // Призыватель измерений: "Призыв: возвращает юнита сразу после его смерти с 1 здоровьем"
        if (lower.Contains("воскрешение") || (lower.Contains("призыв") && lower.Contains("смерти")))
        {
            abilities.Add(new BattlecryAbility(new GrantAbility 
            { 
                AbilityToGrant = new RebirthAbility(), 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Даёт возрождение союзнику в той же колонке" 
            }));
        }

        // Бард неон-клуба: "Вдохновение: +1 к атаке и +1 к здоровью всем соседним союзникам"
        if (lower.Contains("вдохновение"))
        {
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                AttackBonus = 1, 
                HealthBonus = 1, 
                Target = AbilityTarget.NeighborAllies,
                Description = "Даёт +1/+1 всем соседним союзникам" 
            }));
        }

        // Алхимик корпорации: "Зелье: случайный + к атаке/здоровью союзника"
        if (lower.Contains("зелье"))
        {
            var random = new Random();
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                AttackBonus = random.Next(1, 4), 
                HealthBonus = random.Next(1, 4), 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Даёт случайный бонус союзнику в той же колонке" 
            }));
        }

        // Знахарка полевого госпиталя: "Травы: + 2 к здоровью союзника"
        // Прорицатель сети: "Видение: +2 к здоровью союзника"
        if (lower.Contains("травы") || lower.Contains("видение"))
        {
            abilities.Add(new BattlecryAbility(new HealAbility 
            { 
                Value = 2, 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Исцеляет союзника в той же колонке на 2" 
            }));
        }

        // Король Неон-Сити: "Власть: все союзники +2/+2"
        if (lower.Contains("власть"))
        {
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                AttackBonus = 2, 
                HealthBonus = 2, 
                Target = AbilityTarget.AllAllies,
                Description = "Даёт +2/+2 всем союзникам" 
            }));
        }

        // Кибер-дракон: "Огненное дыхание: 3 урона всем врагам на поле боя"
        if (lower.Contains("дыхание"))
        {
            abilities.Add(new BattlecryAbility(new DamageAuraAbility 
            { 
                Value = 3, 
                Range = 10, // Весь экран
                Target = AbilityTarget.AllEnemies,
                Description = "Наносит 3 урона всем врагам" 
            }));
        }

        // Плазменный феникс: "Возрождение: при смерти возвращается с 1 здоровьем"
        if (lower.Contains("возрождение: при смерти"))
        {
            abilities.Add(new RebirthAbility());
        }

        // Кибер-голем: "Каменная кожа: +5 к здоровью союзника"
        if (lower.Contains("кожа"))
        {
            abilities.Add(new BattlecryAbility(new BuffAbility 
            { 
                HealthBonus = 5, 
                Target = AbilityTarget.SameColumnAlly,
                Description = "Даёт +5 к здоровью союзнику в той же колонке" 
            }));
        }

        return abilities;
    }
}
