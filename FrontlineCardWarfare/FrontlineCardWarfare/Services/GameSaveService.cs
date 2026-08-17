using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Helpers;
using FrontlineCardWarfare.Models;
using FrontlineCardWarfare.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Services;

/// <summary>
/// Обёртка для данных сохранения с контрольной суммой.
/// </summary>
public class SaveDataWrapper
{
    public string Data { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public DateTime SavedAt { get; set; }
    public int SessionId { get; set; }
}

/// <summary>
/// Сервис сохранения и загрузки игровых сессий.
/// </summary>
public class GameSaveService : IGameSaveService
{
    private readonly GameDbContext _context;
    private readonly string _savePath;
    private readonly ICardRepository _cardRepository;
    private readonly Random _random;

    /// <summary>
    /// Инициализирует новый экземпляр GameSaveService.
    /// </summary>
    public GameSaveService(GameDbContext context, ICardRepository cardRepository)
    {
        _context = context;
        _cardRepository = cardRepository;
        _random = new Random();
        _savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FrontlineCardWarfare",
            "Saves");

        Directory.CreateDirectory(_savePath);
    }

    #region IGameSaveService Methods

    /// <summary>
    /// Сохраняет состояние игры в БД и файл.
    /// </summary>
    public async Task<bool> SaveGameAsync(GameSession session)
    {
        try
        {
            // Сериализация состояния
            var gameState = await CreateGameStateFromSessionAsync(session);
            var json = JsonSerializer.Serialize(gameState, GetJsonOptions());

            // Вычисление контрольной суммы
            var checksum = ComputeChecksum(json);

            // Обновление сессии
            session.BoardStateJson = json;
            session.Checksum = checksum;
            session.LastSavedAt = DateTime.UtcNow;

            // Сохранение в БД
            var existing = await _context.GameSessions.FindAsync(session.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(session);
            }
            else
            {
                _context.GameSessions.Add(session);
            }

            await _context.SaveChangesAsync();

            // Дополнительное сохранение в файл для резервной копии
            await SaveToFileAsync(session.Id, json, checksum);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения игры: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Загружает состояние игры по ID сессии.
    /// </summary>
    public async Task<GameState?> LoadGameAsync(int sessionId)
    {
        try
        {
            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null)
                return null;

            // Проверка контрольной суммы
            if (!VerifyChecksum(session.BoardStateJson, session.Checksum))
            {
                System.Diagnostics.Debug.WriteLine("Контрольная сумма не совпадает!");
                return null;
            }

            return JsonSerializer.Deserialize<GameState>(session.BoardStateJson, GetJsonOptions());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки игры: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Загружает последнее сохранение для игрока.
    /// </summary>
    public async Task<GameState?> LoadLastGameAsync(int playerId)
    {
        try
        {
            var session = await _context.GameSessions
                .Where(gs => gs.UserId == playerId && gs.GameResult == null)
                .OrderByDescending(gs => gs.LastSavedAt)
                .FirstOrDefaultAsync();

            if (session == null)
                return null;

            if (!VerifyChecksum(session.BoardStateJson, session.Checksum))
                return null;

            return JsonSerializer.Deserialize<GameState>(session.BoardStateJson, GetJsonOptions());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки последнего сохранения: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Удаляет сессию игры.
    /// </summary>
    public async Task<bool> DeleteGameAsync(int sessionId)
    {
        try
        {
            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null)
                return false;

            _context.GameSessions.Remove(session);
            await _context.SaveChangesAsync();

            // Удаление файла
            var filePath = Path.Combine(_savePath, $"save_{sessionId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка удаления игры: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Проверяет целостность данных сессии.
    /// </summary>
    public bool VerifyChecksum(GameSession session)
    {
        if (string.IsNullOrEmpty(session.BoardStateJson) || string.IsNullOrEmpty(session.Checksum))
            return false;

        return VerifyChecksum(session.BoardStateJson, session.Checksum);
    }

    /// <summary>
    /// Проверяет наличие незавершённых игр у игрока.
    /// </summary>
    public async Task<bool> HasActiveGamesAsync(int playerId)
    {
        try
        {
            return await _context.GameSessions
                .AnyAsync(gs => gs.UserId == playerId && gs.GameResult == null);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Получает список активных игр игрока.
    /// </summary>
    public async Task<List<GameSession>> GetActiveGamesAsync(int playerId)
    {
        try
        {
            return await _context.GameSessions
                .Where(gs => gs.UserId == playerId && gs.GameResult == null)
                .OrderByDescending(gs => gs.LastSavedAt)
                .ToListAsync();
        }
        catch
        {
            return new List<GameSession>();
        }
    }

    /// <summary>
    /// Получает список всех сохранённых игр пользователя.
    /// </summary>
    public async Task<List<GameSession>> GetSavedGamesAsync(int userId)
    {
        try
        {
            return await _context.GameSessions
                .Where(gs => gs.UserId == userId)
                .OrderByDescending(gs => gs.LastSavedAt)
                .ToListAsync();
        }
        catch
        {
            return new List<GameSession>();
        }
    }

    /// <summary>
    /// Инициализирует новую игровую сессию.
    /// </summary>
    public async Task<GameSession> InitializeGameAsync(int playerDeckId, string difficulty)
    {
        // Валидация сложности
        if (!IsValidDifficulty(difficulty))
        {
            throw new ArgumentException($"Недопустимый уровень сложности. Допустимые значения: Easy, Medium, Hard", nameof(difficulty));
        }

        // Загрузка колоды игрока с картами
        var playerDeck = await _context.Decks
            .Include(d => d.DeckCards)
            .ThenInclude(dc => dc.Card)
            .FirstOrDefaultAsync(d => d.Id == playerDeckId);

        if (playerDeck == null)
        {
            throw new InvalidOperationException($"Колода с ID {playerDeckId} не найдена");
        }

        // Валидация размера колоды игрока (20-30 карт)
        var playerCards = await GetCardsFromDeckAsync(playerDeck);
        ValidateDeckSize(playerCards.Count, "Колода игрока");

        // Генерация колоды ИИ на основе сложности
        var enemyCards = await GenerateEnemyCardsAsync(difficulty);

        // Создание игрового поля 4×3
        var board = new Board();

        // Перемешивание колод
        Shuffle(playerCards);
        Shuffle(enemyCards);

        // Стартовая рука (5 карт)
        var playerHand = new Hand();
        for (int i = 0; i < 5 && i < playerCards.Count; i++)
        {
            playerHand.AddCard(playerCards[i]);
        }

        var enemyHand = new Hand();
        for (int i = 0; i < 5 && i < enemyCards.Count; i++)
        {
            enemyHand.AddCard(enemyCards[i]);
        }

        // Сериализация рук
        var playerHandJson = JsonSerializer.Serialize(playerHand.Cards, GetJsonOptions());
        var enemyHandJson = JsonSerializer.Serialize(enemyHand.Cards, GetJsonOptions());
        
        // Создаем сводное состояние для контрольной суммы
        var gameStateData = new
        {
            Board = JsonSerializer.Serialize(board.Cells, GetJsonOptions()),
            PlayerHand = playerHandJson,
            EnemyHand = enemyHandJson,
            Difficulty = difficulty,
            Turn = 0
        };
        var gameStateJson = JsonSerializer.Serialize(gameStateData, GetJsonOptions());

        // Вычисление контрольной суммы
        var checksum = ComputeChecksum(gameStateJson);

        // Создание сессии игры
        var session = new GameSession
        {
            UserId = playerDeck.UserId,
            DeckId = playerDeck.Id,
            BoardStateJson = gameStateJson,
            PlayerHandJson = playerHandJson,
            EnemyHandJson = enemyHandJson,
            IsPlayerTurn = true,
            TurnNumber = 0,
            GameResult = null,
            LastSavedAt = DateTime.UtcNow,
            Checksum = checksum,
            Difficulty = difficulty
        };

        // Сохранение в БД
        await _context.GameSessions.AddAsync(session);
        await _context.SaveChangesAsync();

        return session;
    }

    #endregion

    #region Public Methods for Auto-Save

    /// <summary>
    /// Автосохранение после действия (обёртка с игрой).
    /// </summary>
    public async Task<bool> AutoSaveAsync(GameState gameState, int sessionId)
    {
        try
        {
            var session = await _context.GameSessions.FindAsync(sessionId);
            if (session == null)
                return false;

            // Обновление сессии из состояния игры
            session.BoardStateJson = JsonSerializer.Serialize(gameState, GetJsonOptions());
            session.Checksum = ComputeChecksum(session.BoardStateJson);
            session.LastSavedAt = DateTime.UtcNow;
            session.TurnNumber = gameState.Turn.TurnNumber;
            session.IsPlayerTurn = gameState.Turn.IsPlayerTurn;

            await _context.SaveChangesAsync();
            await SaveToFileAsync(sessionId, session.BoardStateJson, session.Checksum);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка автосохранения: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Автосохранение после розыгрыша карты.
    /// </summary>
    public async Task<bool> SaveAfterPlayCardAsync(GameState gameState, int sessionId)
    {
        return await AutoSaveAsync(gameState, sessionId);
    }

    /// <summary>
    /// Автосохранение после перемещения юнита.
    /// </summary>
    public async Task<bool> SaveAfterMoveUnitAsync(GameState gameState, int sessionId)
    {
        return await AutoSaveAsync(gameState, sessionId);
    }

    /// <summary>
    /// Автосохранение после атаки.
    /// </summary>
    public async Task<bool> SaveAfterAttackAsync(GameState gameState, int sessionId)
    {
        return await AutoSaveAsync(gameState, sessionId);
    }

    /// <summary>
    /// Автосохранение после завершения хода.
    /// </summary>
    public async Task<bool> SaveAfterEndTurnAsync(GameState gameState, int sessionId)
    {
        return await AutoSaveAsync(gameState, sessionId);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Вычисляет SHA-256 хеш для данных.
    /// </summary>
    private static string ComputeChecksum(string data)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Проверяет контрольную сумму.
    /// </summary>
    private static bool VerifyChecksum(string data, string expectedChecksum)
    {
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(expectedChecksum))
            return false;

        var actualChecksum = ComputeChecksum(data);
        return string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Создаёт GameState из GameSession.
    /// </summary>
    private async Task<GameState> CreateGameStateFromSessionAsync(GameSession session)
    {
        var gameState = new GameState
        {
            Board = LoadBoardFromJson(session.BoardStateJson),
            PlayerHand = LoadHandFromJson(session.PlayerHandJson ?? "[]"),
            EnemyHand = LoadHandFromJson(session.EnemyHandJson ?? "[]"),
            Turn = new GameTurn
            {
                TurnNumber = session.TurnNumber,
                IsPlayerTurn = session.IsPlayerTurn
            },
            PlayerDeckId = session.DeckId,
            EnemyDeckId = 0,
            Difficulty = session.Difficulty ?? "Medium",
            PlayerDeck = new List<Card>(),
            EnemyDeck = new List<Card>()
        };

        return await Task.FromResult(gameState);
    }

    /// <summary>
    /// Загружает Board из JSON.
    /// </summary>
    private Board LoadBoardFromJson(string json)
    {
        try
        {
            var board = JsonSerializer.Deserialize<Board>(json, GetJsonOptions()) ?? new Board();
            
            // Если BoardStateJson содержит полное состояние, извлекаем Board
            if (!string.IsNullOrEmpty(json))
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("Board", out var boardElement))
                {
                    var boardJson = boardElement.GetRawText();
                    board = JsonSerializer.Deserialize<Board>(boardJson, GetJsonOptions()) ?? new Board();
                }
            }

            return board;
        }
        catch
        {
            return new Board();
        }
    }

    /// <summary>
    /// Загружает Hand из JSON.
    /// </summary>
    private Hand LoadHandFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Hand>(json, GetJsonOptions()) ?? new Hand();
        }
        catch
        {
            return new Hand();
        }
    }

    /// <summary>
    /// Сохраняет данные в файл.
    /// </summary>
    private async Task SaveToFileAsync(int sessionId, string data, string checksum)
    {
        try
        {
            var saveData = new SaveDataWrapper
            {
                Data = data,
                Checksum = checksum,
                SavedAt = DateTime.UtcNow,
                SessionId = sessionId
            };

            var json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var filePath = Path.Combine(_savePath, $"save_{sessionId}.json");
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка записи файла: {ex.Message}");
        }
    }

    /// <summary>
    /// Настройки JSON сериализации.
    /// </summary>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = false,
            IncludeFields = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true
        };
    }

    private bool IsValidDifficulty(string difficulty)
    {
        return difficulty == "Easy" || difficulty == "Medium" || difficulty == "Hard";
    }

    private void ValidateDeckSize(int count, string deckName)
    {
        if (count < 20 || count > 30)
        {
            throw new InvalidOperationException($"{deckName} должна содержать от 20 до 30 карт. Текущее количество: {count}");
        }
    }

    private async Task<List<Card>> GenerateEnemyCardsAsync(string difficulty)
    {
        var allCards = await _cardRepository.GetAllAsync();
        var enemyCards = new List<Card>();
        int targetSize = difficulty switch
        {
            "Easy" => 20,
            "Medium" => 25,
            "Hard" => 30,
            _ => 25
        };

        var availableCards = difficulty switch
        {
            "Easy" => allCards.Where(c => c.Attack <= 4 && c.Health <= 4).ToList(),
            "Medium" => allCards.Where(c => c.Attack <= 6 && c.Health <= 6).ToList(),
            "Hard" => allCards.ToList(),
            _ => allCards.Where(c => c.Attack <= 6 && c.Health <= 6).ToList()
        };

        if (availableCards.Count == 0)
        {
            availableCards = allCards;
        }

        Shuffle(availableCards);

        int cardsToAdd = Math.Min(targetSize, availableCards.Count);
        for (int i = 0; i < cardsToAdd; i++)
        {
            enemyCards.Add(availableCards[i]);
        }

        while (enemyCards.Count < targetSize && availableCards.Count > 0)
        {
            var card = availableCards[_random.Next(availableCards.Count)];
            enemyCards.Add(card);
        }

        return enemyCards;
    }

    private async Task<List<Card>> GetCardsFromDeckAsync(Deck deck)
    {
        var cards = new List<Card>();
        foreach (var deckCard in deck.DeckCards)
        {
            if (deckCard.Card != null)
            {
                for (int i = 0; i < deckCard.Quantity; i++)
                {
                    cards.Add(deckCard.Card);
                }
            }
            else
            {
                var card = await _cardRepository.GetByIdAsync(deckCard.CardId);
                if (card != null)
                {
                    for (int i = 0; i < deckCard.Quantity; i++)
                    {
                        cards.Add(card);
                    }
                }
            }
        }
        return cards;
    }

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Сохраняет результат завершённой игры с подробной статистикой.
    /// </summary>
    public async Task<bool> SaveGameResultAsync(int userId, GameEndStatistics statistics, string difficulty)
    {
        try
        {
            var opponentName = difficulty switch
            {
                "Easy" => "ИИ: Рядовой (Easy)",
                "Medium" => "ИИ: Сержант (Medium)",
                "Hard" => "ИИ: Полковник (Hard)",
                _ => $"ИИ: {difficulty}"
            };

            // Создаём сессию с расширенным результатом
            var session = new GameSession
            {
                UserId = userId,
                GameResult = statistics.Result,
                Difficulty = difficulty,
                OpponentName = opponentName,
                LastSavedAt = DateTime.Now,
                IsPlayerTurn = false,
                TurnNumber = statistics.TurnCount,
                PlayerDamageDealt = statistics.PlayerDamageDealt,
                EnemyDamageDealt = statistics.EnemyDamageDealt,
                // Используем реальную длительность, если она доступна
                Duration = statistics.EndedAt - statistics.StartTime
            };

            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка сохранения результата игры: {ex.Message}");
            return false;
        }
    }

    #endregion
}