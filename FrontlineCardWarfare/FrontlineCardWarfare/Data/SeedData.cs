using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;

namespace FrontlineCardWarfare.Data;

/// <summary>
/// Класс для наполнения базы данных начальными данными.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Наполняет базу данных начальными данными.
    /// </summary>
    public static async Task InitializeAsync(GameDbContext context)
    {
        // 1. Добавляем карты, если их нет
        if (!context.Cards.Any())
        {
            var cards = GetPredefinedCards();
            context.Cards.AddRange(cards);
            await context.SaveChangesAsync();
        }

        // 2. Добавляем пользователей, если их нет
        if (!context.Users.Any(u => u.Username == "admin"))
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = PasswordHelper.HashPassword("1234567890"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsBlocked = false
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }

        if (!context.Users.Any(u => u.Username == "player"))
        {
            var player = new User
            {
                Username = "player",
                PasswordHash = PasswordHelper.HashPassword("player123"),
                Role = UserRole.Player,
                CreatedAt = DateTime.UtcNow,
                IsBlocked = false
            };
            context.Users.Add(player);
            await context.SaveChangesAsync();
        }

        // 3. Получаем игрока и карты из БД (после сохранения, чтобы получить ID)
        var currentPlayer = context.Users.FirstOrDefault(u => u.Username == "player");
        if (currentPlayer == null) return;

        var allCards = context.Cards.ToList();

        // 4. Добавляем стартовую колоду
        if (!context.Decks.Any(d => d.UserId == currentPlayer.Id && d.Name == "Стартовая колода"))
        {
            var starterDeck = new Deck
            {
                UserId = currentPlayer.Id,
                Name = "Стартовая колода",
                CreatedAt = DateTime.UtcNow,
                DeckCards = new List<DeckCard>()
            };

            for (int i = 0; i < 10 && i < allCards.Count; i++)
            {
                starterDeck.DeckCards.Add(new DeckCard
                {
                    CardId = allCards[i].Id,
                    Quantity = 2
                });
            }

            context.Decks.Add(starterDeck);
            await context.SaveChangesAsync();
        }

        // 5. Добавляем статистику
        if (!context.GameStatistics.Any(s => s.UserId == currentPlayer.Id))
        {
            context.GameStatistics.Add(new GameStatistics
            {
                UserId = currentPlayer.Id,
                Wins = 0,
                Losses = 0,
                TotalGames = 0,
                LastPlayedAt = null
            });
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Возвращает список предопределённых карт.
    /// </summary>
    private static List<Card> GetPredefinedCards()
    {
        var cards = new List<Card>();

        // === Карты ближнего боя (Melee) ===
        cards.Add(new Card
        {
            Name = "Корпоративный пехотинец",
            CardType = CardType.Melee,
            Attack = 3,
            Health = 4,
            Range = 1,
            Ability = null,
            Description = "Обычный боец ближнего боя",
            ImagePath = "Resources/Images/infantry.png"
        });

        cards.Add(new Card
        {
            Name = "Нео-рыцарь",
            CardType = CardType.Melee,
            Attack = 4,
            Health = 5,
            Range = 1,
            Ability = null,
            Description = "Закалённый в боях воин",
            ImagePath = "Resources/Images/knight.png"
        });

        cards.Add(new Card
        {
            Name = "Берсерк из пустошей",
            CardType = CardType.Melee,
            Attack = 6,
            Health = 3,
            Range = 1,
            Ability = null,
            Description = "Неистовый воин, становящийся сильнее при ранении",
            ImagePath = "Resources/Images/berserker.png"
        });

        cards.Add(new Card
        {
            Name = "Щитоносец СБ",
            CardType = CardType.Melee,
            Attack = 2,
            Health = 7,
            Range = 1,
            Ability = null,
            Description = "Защищает союзников своим щитом",
            ImagePath = "Resources/Images/shieldbearer.png"
        });

        cards.Add(new Card
        {
            Name = "Паладин нового мира",
            CardType = CardType.Melee,
            Attack = 4,
            Health = 4,
            Range = 1,
            Ability = null,
            Description = "Святой воин, несущий свет",
            ImagePath = "Resources/Images/paladin.png"
        });

        cards.Add(new Card
        {
            Name = "Тень корпорации",
            CardType = CardType.Melee,
            Attack = 5,
            Health = 3,
            Range = 1,
            Ability = null,
            Description = "Мастер скрытных убийств",
            ImagePath = "Resources/Images/assassin.png"
        });

        cards.Add(new Card
        {
            Name = "Гвардеец Неон-Сити",
            CardType = CardType.Melee,
            Attack = 3,
            Health = 5,
            Range = 1,
            Ability = null,
            Description = "Верный страж",
            ImagePath = "Resources/Images/guardsman.png"
        });

        cards.Add(new Card
        {
            Name = "Кибер-викинг",
            CardType = CardType.Melee,
            Attack = 5,
            Health = 4,
            Range = 1,
            Ability = null,
            Description = "Воин севера с боевым кличем",
            ImagePath = "Resources/Images/viking.png"
        });

        cards.Add(new Card
        {
            Name = "Нео-самурай",
            CardType = CardType.Melee,
            Attack = 5,
            Health = 3,
            Range = 1,
            Ability = null,
            Description = "Воин чести с катаной",
            ImagePath = "Resources/Images/samurai.png"
        });

        cards.Add(new Card
        {
            Name = "Гладиатор арены",
            CardType = CardType.Melee,
            Attack = 4,
            Health = 4,
            Range = 1,
            Ability = null,
            Description = "Боец арены",
            ImagePath = "Resources/Images/gladiator.png"
        });

        // === Карты дальнего боя (Ranged) ===
        cards.Add(new Card
        {
            Name = "Стрелок пустошей",
            CardType = CardType.Ranged,
            Attack = 3,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Меткий стрелок с луком",
            ImagePath = "Resources/Images/archer.png"
        });

        cards.Add(new Card
        {
            Name = "Арбалетчик СБ",
            CardType = CardType.Ranged,
            Attack = 4,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Стреляет из арбалета",
            ImagePath = "Resources/Images/crossbowman.png"
        });

        cards.Add(new Card
        {
            Name = "Мушкетёр Неон-Сити",
            CardType = CardType.Ranged,
            Attack = 4,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Вооружён мушкетом",
            ImagePath = "Resources/Images/musketeer.png"
        });

        cards.Add(new Card
        {
            Name = "Снайпер тени",
            CardType = CardType.Ranged,
            Attack = 5,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Мастер дальней стрельбы",
            ImagePath = "Resources/Images/sniper.png"
        });

        cards.Add(new Card
        {
            Name = "Охотник за головами",
            CardType = CardType.Ranged,
            Attack = 3,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Опытный следопыт",
            ImagePath = "Resources/Images/hunter.png"
        });

        cards.Add(new Card
        {
            Name = "Эльфийский стрелок",
            CardType = CardType.Ranged,
            Attack = 4,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Эльф с волшебным луком",
            ImagePath = "Resources/Images/elf_archer.png"
        });

        cards.Add(new Card
        {
            Name = "Драгун корпорации",
            CardType = CardType.Ranged,
            Attack = 3,
            Health = 4,
            Range = 2,
            Ability = null,
            Description = "Кавалерист с копьём",
            ImagePath = "Resources/Images/dragoon.png"
        });

        cards.Add(new Card
        {
            Name = "Наёмник пустошей",
            CardType = CardType.Ranged,
            Attack = 4,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Стрелок за деньги",
            ImagePath = "Resources/Images/mercenary.png"
        });

        cards.Add(new Card
        {
            Name = "Разведчик сети",
            CardType = CardType.Ranged,
            Attack = 2,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Быстрый разведчик",
            ImagePath = "Resources/Images/scout.png"
        });

        cards.Add(new Card
        {
            Name = "Бомбардир тяжёлой артиллерии",
            CardType = CardType.Ranged,
            Attack = 3,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Метает бомбы",
            ImagePath = "Resources/Images/bombardier.png"
        });

        // === Осадные карты (Siege) ===
        cards.Add(new Card
        {
            Name = "Кибер-катапульта",
            CardType = CardType.Siege,
            Attack = 4,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Метает камни на большое расстояние",
            ImagePath = "Resources/Images/catapult.png"
        });

        cards.Add(new Card
        {
            Name = "Электро-баллиста",
            CardType = CardType.Siege,
            Attack = 5,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Стреляет огромными болтами",
            ImagePath = "Resources/Images/ballista.png"
        });

        cards.Add(new Card
        {
            Name = "Голографический требушет",
            CardType = CardType.Siege,
            Attack = 6,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Мощная осадная машина",
            ImagePath = "Resources/Images/trebuchet.png"
        });

        cards.Add(new Card
        {
            Name = "Плазменная пушка",
            CardType = CardType.Siege,
            Attack = 5,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Стреляет ядрами",
            ImagePath = "Resources/Images/cannon.png"
        });

        cards.Add(new Card
        {
            Name = "Миномёт теневых операций",
            CardType = CardType.Siege,
            Attack = 4,
            Health = 2,
            Range = 2,
            Ability = null,
            Description = "Стреляет по навесной траектории",
            ImagePath = "Resources/Images/mortar.png"
        });

        cards.Add(new Card
        {
            Name = "Ракетная установка «Буря»",
            CardType = CardType.Siege,
            Attack = 3,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Запускает ракеты",
            ImagePath = "Resources/Images/rocket_launcher.png"
        });

        cards.Add(new Card
        {
            Name = "Огнемёт «Инферно»",
            CardType = CardType.Siege,
            Attack = 4,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Метает огонь",
            ImagePath = "Resources/Images/flamethrower.png"
        });

        cards.Add(new Card
        {
            Name = "Инженер корпорации",
            CardType = CardType.Siege,
            Attack = 2,
            Health = 4,
            Range = 2,
            Ability = null,
            Description = "Чинит технику",
            ImagePath = "Resources/Images/engineer.png"
        });

        cards.Add(new Card
        {
            Name = "Сапёр",
            CardType = CardType.Siege,
            Attack = 3,
            Health = 3,
            Range = 1,
            Ability = null,
            Description = "Устанавливает мины",
            ImagePath = "Resources/Images/sapper.png"
        });

        cards.Add(new Card
        {
            Name = "Артиллерист",
            CardType = CardType.Siege,
            Attack = 4,
            Health = 3,
            Range = 2,
            Ability = null,
            Description = "Обслуживает артиллерию",
            ImagePath = "Resources/Images/artilleryman.png"
        });

        // === Карты поддержки (Support) ===
        cards.Add(new Card
        {
            Name = "Жрец неоневого храма",
            CardType = CardType.Support,
            Attack = 1,
            Health = 4,
            Range = 2,
            Ability = "Исцеление: +3 к здоровью союзнику",
            Description = "Лечит раненых",
            ImagePath = "Resources/Images/priest.png"
        });

        cards.Add(new Card
        {
            Name = "Друид синтетического леса",
            CardType = CardType.Support,
            Attack = 2,
            Health = 4,
            Range = 2,
            Ability = "Природа: +2 к атаке и +2 к здоровью союзнику",
            Description = "Черпает силу природы",
            ImagePath = "Resources/Images/druid.png"
        });

        cards.Add(new Card
        {
            Name = "Маг кибер-башни",
            CardType = CardType.Support,
            Attack = 3,
            Health = 3,
            Range = 2,
            Ability = "Огонь: +3 к атаке союзнику",
            Description = "Повелитель стихий",
            ImagePath = "Resources/Images/mage.png"
        });

        cards.Add(new Card
        {
            Name = "Чародей голографических иллюзий",
            CardType = CardType.Support,
            Attack = 2,
            Health = 3,
            Range = 2,
            Ability = "Проклятие на врага: +4 к атаке союзника",
            Description = "Накладывает проклятия",
            ImagePath = "Resources/Images/enchanter.png"
        });

        cards.Add(new Card
        {
            Name = "Некромант цифрового мира",
            CardType = CardType.Support,
            Attack = 2,
            Health = 4,
            Range = 2,
            Ability = "Воскрешение: возвращает юнита сразу после его смерти с 1 здоровьем",
            Description = "Поднимает мёртвых",
            ImagePath = "Resources/Images/necromancer.png"
        });

        cards.Add(new Card
        {
            Name = "Бард неон-клуба",
            CardType = CardType.Support,
            Attack = 1,
            Health = 4,
            Range = 2,
            Ability = "Вдохновение: +1 к атаке и +1 к здоровью всем соседним союзникам",
            Description = "Поёт боевые песни",
            ImagePath = "Resources/Images/bard.png"
        });

        cards.Add(new Card
        {
            Name = "Алхимик корпорации",
            CardType = CardType.Support,
            Attack = 2,
            Health = 3,
            Range = 2,
            Ability = "Зелье: случайный + к атаке/здоровью союзника",
            Description = "Варит зелья",
            ImagePath = "Resources/Images/alchemist.png"
        });

        cards.Add(new Card
        {
            Name = "Знахарка полевого госпиталя",
            CardType = CardType.Support,
            Attack = 1,
            Health = 5,
            Range = 2,
            Ability = "Травы:  + 2 к здоровью союзника",
            Description = "Лечит травами",
            ImagePath = "Resources/Images/healer.png"
        });

        cards.Add(new Card
        {
            Name = "Прорицатель сети",
            CardType = CardType.Support,
            Attack = 1,
            Health = 3,
            Range = 2,
            Ability = "Видение: +2 к здоровью союзника",
            Description = "Видит будущее",
            ImagePath = "Resources/Images/soothsayer.png"
        });

        cards.Add(new Card
        {
            Name = "Призыватель измерений",
            CardType = CardType.Support,
            Attack = 2,
            Health = 4,
            Range = 2,
            Ability = "Призыв: возвращает юнита сразу после его смерти с 1 здоровьем",
            Description = "Призывает существ",
            ImagePath = "Resources/Images/summoner.png"
        });

        // === Особенные карты (Special) ===
        cards.Add(new Card
        {
            Name = "Король Неон-Сити",
            CardType = CardType.Special,
            Attack = 4,
            Health = 6,
            Range = 1,
            Ability = "Власть: все союзники +2/+2",
            Description = "Правитель королевства",
            ImagePath = "Resources/Images/king.png"
        });

        cards.Add(new Card
        {
            Name = "Генерал корпорации",
            CardType = CardType.Special,
            Attack = 4,
            Health = 5,
            Range = 1,
            Ability = "Тактика: +2 к атаке и +2 к здоровью союзника",
            Description = "Командует армией",
            ImagePath = "Resources/Images/general.png"
        });

        cards.Add(new Card
        {
            Name = "Кибер-дракон",
            CardType = CardType.Special,
            Attack = 6,
            Health = 7,
            Range = 2,
            Ability = "Огненное дыхание: 3 урона всем врагам на поле боя",
            Description = "Древнее существо",
            ImagePath = "Resources/Images/dragon.png"
        });

        cards.Add(new Card
        {
            Name = "Плазменный феникс",
            CardType = CardType.Special,
            Attack = 4,
            Health = 4,
            Range = 2,
            Ability = "Возрождение: при смерти возвращается с 1 здоровьем",
            Description = "Птица огня",
            ImagePath = "Resources/Images/phoenix.png"
        });

        cards.Add(new Card
        {
            Name = "Кибер-голем",
            CardType = CardType.Special,
            Attack = 3,
            Health = 8,
            Range = 1,
            Ability = "Каменная кожа: +5 к здоровью союзника",
            Description = "Ожившая статуя",
            ImagePath = "Resources/Images/golem.png"
        });

        return cards;
    }
}