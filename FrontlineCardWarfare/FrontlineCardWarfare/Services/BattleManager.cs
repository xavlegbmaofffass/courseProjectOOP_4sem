using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Repositories;

namespace FrontlineCardWarfare.Services;

    /// <summary>
    /// События для триггеров анимаций.
    /// </summary>
    public class AnimationEvents
    {
        public event Action<Unit>? OnUnitPlaced;
        public event Action<Card, int, int>? OnCardPlayed;
        public event Action<Unit, Unit, int>? OnAttackOccurred;
        public event Action<Unit, int>? OnUnitTookDamage;
        public event Action<Unit>? OnUnitDestroyed;
        public event Action? OnTurnChanged;

        /// <summary>
        /// Вызывает событие размещения юнита.
        /// </summary>
        public void InvokeOnUnitPlaced(Unit unit) => OnUnitPlaced?.Invoke(unit);

        /// <summary>
        /// Вызывает событие розыгрыша карты.
        /// </summary>
        public void InvokeOnCardPlayed(Card card, int row, int col) => OnCardPlayed?.Invoke(card, row, col);

        /// <summary>
        /// Вызывает событие атаки.
        /// </summary>
        public void InvokeOnAttackOccurred(Unit attacker, Unit target, int damage) => OnAttackOccurred?.Invoke(attacker, target, damage);

        /// <summary>
        /// Вызывает событие получения урона.
        /// </summary>
        public void InvokeOnUnitTookDamage(Unit unit, int damage) => OnUnitTookDamage?.Invoke(unit, damage);

        /// <summary>
        /// Вызывает событие уничтожения юнита.
        /// </summary>
        public void InvokeOnUnitDestroyed(Unit unit) => OnUnitDestroyed?.Invoke(unit);

        /// <summary>
        /// Вызывает событие смены хода.
        /// </summary>
        public void InvokeOnTurnChanged() => OnTurnChanged?.Invoke();
    }

/// <summary>
/// Менеджер боя — управляет игровой логикой.
/// </summary>
public class BattleManager : IBattleManager
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICardRepository _cardRepository;
    private readonly IAbilityManager _abilityManager;
    private GameState _gameState = new();
    private int _nextUnitId = 1;

    /// <summary>
    /// Текущее состояние игры.
    /// </summary>
    public GameState GameState => _gameState;

    /// <summary>
    /// Сообщения о применённых эффектах (для отображения в UI).
    /// </summary>
    public List<string> EffectMessages { get; } = new();

    /// <summary>
    /// События для анимаций.
    /// </summary>
    public AnimationEvents AnimationEventHandlers { get; } = new();

    /// <summary>
    /// Инициализирует новый экземпляр BattleManager.
    /// </summary>
    public BattleManager(IDeckRepository deckRepository, ICardRepository cardRepository, IAbilityManager abilityManager)
    {
        System.Diagnostics.Debug.WriteLine("[BattleManager] Конструктор вызван");
        _deckRepository = deckRepository;
        _cardRepository = cardRepository;
        _abilityManager = abilityManager;
    }

    /// <summary>
    /// Инициализирует новую игру.
    /// </summary>
    public async Task InitializeAsync(Deck playerDeck, Deck enemyDeck, string difficulty)
    {
        _gameState = new GameState
        {
            PlayerDeckId = playerDeck.Id,
            EnemyDeckId = enemyDeck.Id,
            Difficulty = difficulty
        };

        // Загрузка карт колод
        var playerCards = await GetCardsFromDeck(playerDeck);
        var enemyCards = await GetCardsFromDeck(enemyDeck);

        // Перемешивание колод
        Shuffle(playerCards);
        Shuffle(enemyCards);

        // Начальная рука (5 карт)
        for (int i = 0; i < 5 && i < playerCards.Count; i++)
        {
            _gameState.PlayerHand.AddCard(playerCards[i]);
        }

        for (int i = 0; i < 5 && i < enemyCards.Count; i++)
        {
            _gameState.EnemyHand.AddCard(enemyCards[i]);
        }

        // Оставшиеся карты в колодах
        for (int i = 5; i < playerCards.Count; i++)
        {
            _gameState.PlayerDeck.Add(playerCards[i]);
        }

        for (int i = 5; i < enemyCards.Count; i++)
        {
            _gameState.EnemyDeck.Add(enemyCards[i]);
        }

        // Игрок ходит первым
        _gameState.Turn.StartNewTurn(true);
    }

    /// <summary>
    /// Разыгрывает карту на поле.
    /// </summary>
    public async Task<(bool Success, string Error)> PlayCardAsync(Card card, int row, int column)
    {
        bool isPlayer = _gameState.Turn.IsPlayerTurn;

        // Проверка: карта в соответствующей руке
        var hand = isPlayer ? _gameState.PlayerHand : _gameState.EnemyHand;
        if (!hand.Cards.Contains(card))
        {
            return (false, "Карты нет в руке");
        }

        // Проверка: позиция на стороне владельца и ограничения по типу (ближний/дальний бой)
        bool isMelee = card.Range <= 1;
        if (isPlayer)
        {
            if (row < 2) return (false, "Можно размещать только на своей стороне");
            // Игрок: ряд 2 - ближний бой, ряд 3 - дальний бой
            if (isMelee && row != 2) return (false, "Воинов ближнего боя можно ставить только в передний ряд (ряд 2)");
            if (!isMelee && row != 3) return (false, "Воинов дальнего боя можно ставить только в задний ряд (ряд 3)");
        }
        else
        {
            if (row >= 2) return (false, "Враг размещает только на своей стороне");
            // Враг: ряд 1 - ближний бой, ряд 0 - дальний бой
            if (isMelee && row != 1) return (false, $"Воины ближнего боя врага должны быть в переднем ряду (ряд 1), а не {row}");
            if (!isMelee && row != 0) return (false, $"Воины дальнего боя врага должны быть в заднем ряду (ряд 0), а не {row}");
        }

        // Проверка: клетка пуста
        if (!_gameState.Board.GetCell(row, column)?.IsEmpty ?? true)
        {
            return (false, "Клетка занята");
        }

        // Создание юнита
        var unit = Unit.FromCard(card, isPlayer, row, column);
        unit.Id = _nextUnitId++;

        // Размещение на поле
        if (!_gameState.Board.PlaceUnit(unit, row, column))
        {
            return (false, "Не удалось разместить юнита");
        }

        // Удаление карты из руки
        hand.RemoveCard(card);

        // Поддержание 5 карт в руке (добор из колоды сразу после разыгрывания)
        if (isPlayer)
        {
            while (_gameState.PlayerHand.Count < 5 && _gameState.PlayerDeck.Count > 0)
            {
                var newCard = _gameState.PlayerDeck[0];
                _gameState.PlayerHand.AddCard(newCard);
                _gameState.PlayerDeck.RemoveAt(0);
            }
        }
        else
        {
            while (_gameState.EnemyHand.Count < 5 && _gameState.EnemyDeck.Count > 0)
            {
                var newCard = _gameState.EnemyDeck[0];
                _gameState.EnemyHand.AddCard(newCard);
                _gameState.EnemyDeck.RemoveAt(0);
            }
        }

        // Применение способностей Battlecry
        EffectMessages.Clear();
        var battlecry_messages = _abilityManager.ResolveTriggers(AbilityTrigger.OnPlay, unit, _gameState.Board);
        EffectMessages.AddRange(battlecry_messages);

        // Удаление мёртвых юнитов после способностей
        RemoveDeadUnits();

        // Запуск анимации размещения юнита
        AnimationEventHandlers.InvokeOnUnitPlaced(unit);
        AnimationEventHandlers.InvokeOnCardPlayed(card, row, column);

        // Автоматическое сражение на линии: атакует только НОВАЯ карта (в одну сторону)
        await ResolveOneWayCombatForUnitAsync(unit);

        return (true, string.Empty);
    }

    /// <summary>
    /// Разрешает одностороннее сражение для конкретного юнита (только при выкладывании).
    /// </summary>
    private async Task ResolveOneWayCombatForUnitAsync(Unit unit)
    {
        int col = unit.Column;
        
        // Ищем противника в этой же колонке (в любом ряду противоположной стороны)
        Unit? opponent = unit.IsPlayer 
            ? (_gameState.Board.GetCell(1, col)?.Unit ?? _gameState.Board.GetCell(0, col)?.Unit)
            : (_gameState.Board.GetCell(2, col)?.Unit ?? _gameState.Board.GetCell(3, col)?.Unit);

        // Если противника нет или атака равна 0 — ничего не происходит
        if (opponent == null || !opponent.IsAlive || unit.Attack == 0) return;

        // Задержка перед сражением, чтобы игрок увидел выложенную карту
        await Task.Delay(800);

        int damage = unit.Attack;
        opponent.TakeDamage(damage);

        // Запуск анимаций только для атакующего и цели (односторонне)
        AnimationEventHandlers.InvokeOnAttackOccurred(unit, opponent, damage);
        AnimationEventHandlers.InvokeOnUnitTookDamage(opponent, damage);

        // Подсчёт статистики урона
        if (unit.IsPlayer)
            _gameState.PlayerTotalDamageDealt += damage;
        else
            _gameState.EnemyTotalDamageDealt += damage;

        // Короткая задержка для визуализации получения урона
        await Task.Delay(600);

        // Проверка на смерть цели
        if (!opponent.IsAlive)
        {
            if (unit.IsPlayer) _gameState.KilledEnemyUnitsCount++;
            else _gameState.KilledPlayerUnitsCount++;
        }

        // Удаление мёртвых юнитов
        RemoveDeadUnits();
    }

    // PlayCard removed due to deadlocks. Use PlayCardAsync.

    /// <summary>
    /// Разрешает автоматические сражения на линиях (колонках) с задержкой для визуализации.
    /// </summary>
    public async Task ResolveLaneCombatAsync()
    {
        for (int col = 0; col < Board.Columns; col++)
        {
            // Ищем передних юнитов (ближе к центру)
            Unit? enemyUnit = _gameState.Board.GetCell(1, col)?.Unit ?? _gameState.Board.GetCell(0, col)?.Unit;
            Unit? playerUnit = _gameState.Board.GetCell(2, col)?.Unit ?? _gameState.Board.GetCell(3, col)?.Unit;

            // Если с одной из сторон нет юнитов в этой колонке — бой не происходит
            if (enemyUnit == null || playerUnit == null) continue;

            // Если оба юнита имеют 0 атаки — они не могут нанести урон друг другу
            if (enemyUnit.Attack == 0 && playerUnit.Attack == 0) continue;

            // Задержка перед сражением, чтобы игрок увидел карты
            await Task.Delay(800);

            // Повторно проверяем наличие юнитов и их живость
            if (enemyUnit.IsAlive && playerUnit.IsAlive)
            {
                int pDamage = playerUnit.Attack;
                int eDamage = enemyUnit.Attack;

                // Наносим урон одновременно (взаимный обмен ударами)
                enemyUnit.TakeDamage(pDamage);
                playerUnit.TakeDamage(eDamage);

                // Запуск анимаций атаки и получения урона для обоих сторон
                AnimationEventHandlers.InvokeOnAttackOccurred(playerUnit, enemyUnit, pDamage);
                AnimationEventHandlers.InvokeOnUnitTookDamage(enemyUnit, pDamage);
                
                AnimationEventHandlers.InvokeOnAttackOccurred(enemyUnit, playerUnit, eDamage);
                AnimationEventHandlers.InvokeOnUnitTookDamage(playerUnit, eDamage);

                // Подсчёт статистики урона
                _gameState.PlayerTotalDamageDealt += pDamage;
                _gameState.EnemyTotalDamageDealt += eDamage;

                // Короткая задержка для визуализации урона
                await Task.Delay(600);

                // Проверка на смерти после боя
                if (!enemyUnit.IsAlive) _gameState.KilledEnemyUnitsCount++;
                if (!playerUnit.IsAlive) _gameState.KilledPlayerUnitsCount++;

                // Очистка мёртвых юнитов с поля
                RemoveDeadUnits();
            }
        }
    }

    /// <summary>
    /// Уничтожает юнита с анимацией и задержкой.
    /// </summary>
    private async Task DestroyUnitWithAnimationAsync(Unit unit)
    {
        if (unit == null || !unit.IsAlive) return;

        // Сразу помечаем юнита как мертвого, чтобы избежать повторных атак на него
        unit.TakeDamage(unit.CurrentHealth);

        // Запуск анимации уничтожения (на стороне View это вызовет визуальный эффект)
        AnimationEventHandlers.InvokeOnUnitDestroyed(unit);

        // Короткая пауза для проигрывания анимации
        await Task.Delay(500);

        // Вызов способностей Deathrattle
        var deathrattle_messages = _abilityManager.ResolveTriggers(AbilityTrigger.OnDeath, unit, _gameState.Board);
        EffectMessages.AddRange(deathrattle_messages);

        // Удаление с поля
        _gameState.Board.Cells[unit.Row, unit.Column].Unit = null;

        // Подсчёт статистики
        if (unit.IsPlayer)
            _gameState.KilledPlayerUnitsCount++;
        else
            _gameState.KilledEnemyUnitsCount++;
    }

    /// <summary>
    /// Перемещает юнита (ФУНКЦИЯ ОТКЛЮЧЕНА ПО ТРЕБОВАНИЮ).
    /// </summary>
    public (bool Success, string Error) MoveUnit(int unitId, int toRow, int toColumn)
    {
        return (false, "Перемещение юнитов отключено");
    }

    /// <summary>
    /// Атакует цель юнитом.
    /// </summary>
    public (bool Success, string Error, int Damage, bool TargetDestroyed) Attack(int unitId, int targetUnitId)
    {
        if (!_gameState.Turn.IsPlayerTurn)
        {
            return (false, "Сейчас не ваш ход", 0, false);
        }

        var attacker = GetUnitById(unitId);
        var target = GetUnitById(targetUnitId);

        if (attacker == null || !attacker.IsPlayer)
        {
            return (false, "Атакующий юнит не найден", 0, false);
        }

        if (target == null)
        {
            return (false, "Цель не найдена", 0, false);
        }

        if (!attacker.CanAttack)
        {
            return (false, "Юнит не может атаковать в этом ходу", 0, false);
        }

        if (attacker.HasAttacked)
        {
            return (false, "Юнит уже атаковал в этом ходу", 0, false);
        }

        // Проверка дальности
        int distance = Math.Abs(attacker.Row - target.Row) + Math.Abs(attacker.Column - target.Column);
        if (distance > attacker.Range)
        {
            return (false, $"Цель слишком далеко. Дальность: {attacker.Range}, дистанция: {distance}", 0, false);
        }

        // Нанесение урона
        int damage = attacker.Attack;
        target.TakeDamage(damage);
        bool targetDestroyed = !target.IsAlive;

        // Запуск анимации атаки
        AnimationEventHandlers.InvokeOnAttackOccurred(attacker, target, damage);
        AnimationEventHandlers.InvokeOnUnitTookDamage(target, damage);

        // Подсчёт статистики
        if (attacker.IsPlayer)
        {
            _gameState.PlayerTotalDamageDealt += damage;
            if (targetDestroyed)
                _gameState.KilledEnemyUnitsCount++;
        }
        else
        {
            _gameState.EnemyTotalDamageDealt += damage;
            if (targetDestroyed)
                _gameState.KilledPlayerUnitsCount++;
        }

        // Ответный урон (если цель жива и в пределах досягаемости)
        if (target.IsAlive && distance <= target.Range)
        {
            int retaliatoryDamage = target.Attack;
            attacker.TakeDamage(retaliatoryDamage);

            // Запуск анимации ответного удара
            AnimationEventHandlers.InvokeOnAttackOccurred(target, attacker, retaliatoryDamage);
            AnimationEventHandlers.InvokeOnUnitTookDamage(attacker, retaliatoryDamage);
            
            // Подсчёт статистики ответного урона
            if (!attacker.IsPlayer)
            {
                _gameState.PlayerTotalDamageDealt += retaliatoryDamage;
                if (!attacker.IsAlive)
                    _gameState.KilledEnemyUnitsCount++;
            }
            else
            {
                _gameState.EnemyTotalDamageDealt += retaliatoryDamage;
                if (!attacker.IsAlive)
                    _gameState.KilledPlayerUnitsCount++;
            }
        }

        // Удаление умерших юнитов
        RemoveDeadUnits();

        // Устанавливаем флаг HasAttacked и сбрасываем CanAttack
        attacker.HasAttacked = true;
        attacker.CanAttack = false;

        return (true, string.Empty, damage, targetDestroyed);
    }

    /// <summary>
    /// Удаляет мёртвых юнитов и запускает анимации.
    /// </summary>
    private void RemoveDeadUnits()
    {
        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Columns; col++)
            {
                var unit = _gameState.Board.Cells[row, col].Unit;
                if (unit != null && !unit.IsAlive)
                {
                    // Вызов способностей Deathrattle перед удалением
                    var deathrattle_messages = _abilityManager.ResolveTriggers(AbilityTrigger.OnDeath, unit, _gameState.Board);
                    EffectMessages.AddRange(deathrattle_messages);

                    // Проверяем, жив ли юнит после способностей (например, после Возрождения)
                    if (!unit.IsAlive)
                    {
                        // Запуск анимации уничтожения
                        AnimationEventHandlers.InvokeOnUnitDestroyed(unit);
                        _gameState.Board.Cells[row, col].Unit = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Завершает ход.
    /// </summary>
    public async Task EndTurnAsync()
    {
        if (!_gameState.Turn.IsPlayerTurn)
        {
            throw new InvalidOperationException("Сейчас не ход игрока");
        }

        // Автоматическое сражение в конце хода игрока
        await ResolveLaneCombatAsync();

        // Устанавливаем флаг, что ход переходит к врагу (блокировка UI)
        _gameState.Turn.IsPlayerTurn = false;

        // Запуск анимации смены хода
        AnimationEventHandlers.InvokeOnTurnChanged();

        // Применение аур в конце хода
        EffectMessages.Clear();
        var auraMessages = _abilityManager.ApplyAllAuras(_gameState.Board);
        EffectMessages.AddRange(auraMessages);

        // Сброс флагов действий у юнитов игрока
        foreach (var unit in _gameState.Board.GetPlayerUnits(true))
        {
            unit.CanAttack = false;
            unit.HasAttacked = false;
            unit.IsFrozen = false;
        }

        // Сброс флагов у юнитов ИИ перед их ходом
        foreach (var unit in _gameState.Board.GetPlayerUnits(false))
        {
            unit.CanAttack = true;
            unit.HasAttacked = false;
            unit.IsFrozen = false;
        }

        // Увеличение счётчика ходов
        _gameState.Turn.TurnNumber++;

        // Добор карт противником до 5
        await EnemyDrawCardAsync();

        // Возврат управления игроку (в конце хода ИИ это сделает AIController)
        // Здесь только подготовка к ходу ИИ
    }

    /// <summary>
    /// Завершает ход ИИ и передаёт управление игроку.
    /// </summary>
    public async Task CompleteEnemyTurnAsync()
    {
        // Автоматическое сражение в конце хода ИИ
        await ResolveLaneCombatAsync();

        // Применение аур в конце хода ИИ
        EffectMessages.Clear();
        var auraMessages = _abilityManager.ApplyAllAuras(_gameState.Board);
        EffectMessages.AddRange(auraMessages);

        // Сброс флагов действий у юнитов ИИ
        foreach (var unit in _gameState.Board.GetPlayerUnits(false))
        {
            unit.CanAttack = false;
            unit.HasAttacked = false;
            unit.IsFrozen = false;
        }

        // Сброс флагов у юнитов игрока перед его ходом
        foreach (var unit in _gameState.Board.GetPlayerUnits(true))
        {
            unit.CanAttack = true;
            unit.HasAttacked = false;
            unit.IsFrozen = false;
        }

        // Передача хода игроку
        _gameState.Turn.IsPlayerTurn = true;

        // Добор карты игроком
        await PlayerDrawCardAsync();
    }

    /// <summary>
    /// Проверяет условие победы.
    /// </summary>
    public string? CheckWinCondition()
    {
        var playerUnits = _gameState.Board.GetPlayerUnits(true).ToList();
        var enemyUnits = _gameState.Board.GetPlayerUnits(false).ToList();

        bool playerHasUnits = playerUnits.Count > 0;
        bool playerHasCards = _gameState.PlayerHand.Count > 0 || _gameState.PlayerDeck.Count > 0;
        bool enemyHasUnits = enemyUnits.Count > 0;
        bool enemyHasCards = _gameState.EnemyHand.Count > 0 || _gameState.EnemyDeck.Count > 0;

        // Ничья: оба игрока одновременно теряют последних юнитов и карты
        if (!playerHasUnits && !enemyHasUnits && !playerHasCards && !enemyHasCards)
        {
            _gameState.GameResult = "Ничья";
            _gameState.IsGameOver = true;
            return "Ничья";
        }

        // Ничья по взаимному истощению ресурсов (нет возможности ходить)
        bool playerCanMove = playerHasCards && GetAvailableMovesForPlacement().Any(m => m.Row >= 2);
        bool enemyCanMove = enemyHasCards && GetAvailableMovesForPlacement().Any(m => m.Row < 2);
        
        // Если оба игрока не могут больше выставлять карты и нет возможности атаковать
        if (!playerCanMove && !enemyCanMove && !playerHasCards && !enemyHasCards)
        {
            int playerAttack = playerUnits.Sum(u => u.Attack);
            int enemyAttack = enemyUnits.Sum(u => u.Attack);

            if (playerAttack > enemyAttack)
            {
                _gameState.GameResult = "Победа";
            }
            else if (enemyAttack > playerAttack)
            {
                _gameState.GameResult = "Поражение";
            }
            else
            {
                _gameState.GameResult = "Ничья";
            }
            
            _gameState.IsGameOver = true;
            return _gameState.GameResult;
        }

        // Поражение: у игрока не осталось юнитов и карт
        if (!playerHasUnits && !playerHasCards)
        {
            _gameState.GameResult = "Поражение";
            _gameState.IsGameOver = true;
            return "Поражение";
        }

        // Победа: у врага не осталось юнитов и карт
        if (!enemyHasUnits && !enemyHasCards)
        {
            _gameState.GameResult = "Победа";
            _gameState.IsGameOver = true;
            return "Победа";
        }

        return null;
    }

    /// <summary>
    /// Получает детальную статистику завершённой игры.
    /// </summary>
    public GameEndStatistics GetGameEndStatistics(string difficulty)
    {
        return new GameEndStatistics
        {
            Result = _gameState.GameResult ?? "Не завершена",
            TurnCount = _gameState.Turn.TurnNumber,
            PlayerDamageDealt = _gameState.PlayerTotalDamageDealt,
            EnemyDamageDealt = _gameState.EnemyTotalDamageDealt,
            PlayerUnitsKilled = _gameState.KilledEnemyUnitsCount,
            EnemyUnitsKilled = _gameState.KilledPlayerUnitsCount,
            PlayerCardsRemaining = _gameState.PlayerHand.Count,
            EnemyCardsRemaining = _gameState.EnemyHand.Count,
            Difficulty = difficulty,
            StartTime = _gameState.StartTime,
            EndedAt = DateTime.Now
        };
    }

    /// <summary>
    /// Получает доступные цели для атаки юнита.
    /// </summary>
    public List<int> GetAvailableTargets(int unitId)
    {
        var unit = GetUnitById(unitId);
        if (unit == null || !unit.CanAttack || unit.HasAttacked)
            return new List<int>();

        var targets = new List<int>();
        var enemyUnits = _gameState.Board.GetPlayerUnits(false);

        foreach (var enemy in enemyUnits)
        {
            int distance = Math.Abs(unit.Row - enemy.Row) + Math.Abs(unit.Column - enemy.Column);
            if (distance <= unit.Range)
            {
                targets.Add(enemy.Id);
            }
        }

        return targets;
    }

    /// <summary>
    /// Сдаётся.
    /// </summary>
    public void Surrender()
    {
        _gameState.GameResult = "Поражение (сдача)";
    }

    #region Helper Methods

    private async Task<List<Card>> GetCardsFromDeck(Deck deck)
    {
        var cards = new List<Card>();
        foreach (var deckCard in deck.DeckCards)
        {
            var templateCard = await _cardRepository.GetByIdAsync(deckCard.CardId);
            if (templateCard != null)
            {
                for (int i = 0; i < deckCard.Quantity; i++)
                {
                    // Клонируем карту, чтобы каждый экземпляр в руке был уникальным объектом
                    cards.Add(templateCard.Clone());
                }
            }
        }
        return cards;
    }

    private void Shuffle<T>(List<T> list)
    {
        var random = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Получает юнита по ID.
    /// </summary>
    public Unit? GetUnitById(int unitId)
    {
        return _gameState.Board.GetAllAliveUnits().FirstOrDefault(u => u.Id == unitId);
    }

    /// <summary>
    /// Получает доступные клетки для размещения карт.
    /// </summary>
    public List<(int Row, int Column)> GetAvailableMovesForPlacement()
    {
        var availableCells = new List<(int Row, int Column)>();

        for (int row = 0; row < Board.Rows; row++)
        {
            for (int col = 0; col < Board.Columns; col++)
            {
                var cell = _gameState.Board.GetCell(row, col);
                if (cell?.IsEmpty == true)
                {
                    availableCells.Add((row, col));
                }
            }
        }

        return availableCells;
    }

    public async Task EnemyDrawCardAsync()
    {
        // Враг добирает до 5 карт
        while (_gameState.EnemyHand.Count < 5 && _gameState.EnemyDeck.Count > 0)
        {
            var card = _gameState.EnemyDeck[0];
            _gameState.EnemyHand.AddCard(card);
            _gameState.EnemyDeck.RemoveAt(0);
        }

        await Task.CompletedTask;
    }

    public async Task PlayerDrawCardAsync()
    {
        // Игрок добирает до 5 карт
        while (_gameState.PlayerHand.Count < 5 && _gameState.PlayerDeck.Count > 0)
        {
            var card = _gameState.PlayerDeck[0];
            _gameState.PlayerHand.AddCard(card);
            _gameState.PlayerDeck.RemoveAt(0);
        }

        await Task.CompletedTask;
    }

    #endregion
}
