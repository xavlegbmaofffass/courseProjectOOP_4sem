using Microsoft.EntityFrameworkCore;

namespace FrontlineCardWarfare.Data;

/// <summary>
/// Контекст базы данных для игрового приложения. 
/// </summary>
public class GameDbContext : DbContext
{
    /// <summary>
    /// Строка подключения по умолчанию.
    /// </summary>
    private const string DefaultConnectionString = "Data Source=game.db";

    /// <summary>
    /// Таблица пользователей.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Таблица карт.
    /// </summary>
    public DbSet<Card> Cards { get; set; } = null!;

    /// <summary>
    /// Таблица колод.
    /// </summary>
    public DbSet<Deck> Decks { get; set; } = null!;

    /// <summary>
    /// Таблица связей колод и карт.
    /// </summary>
    public DbSet<DeckCard> DeckCards { get; set; } = null!;

    /// <summary>
    /// Таблица игровых сессий.
    /// </summary>
    public DbSet<GameSession> GameSessions { get; set; } = null!;

    /// <summary>
    /// Таблица игровой статистики.
    /// </summary>
    public DbSet<GameStatistics> GameStatistics { get; set; } = null!;

    /// <summary>
    /// Инициализирует новый экземпляр GameDbContext.
    /// </summary>
    public GameDbContext()
    {
    }

    /// <summary>
    /// Инициализирует новый экземпляр GameDbContext с опциями.
    /// </summary>
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Настройка конфигурации базы данных.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(DefaultConnectionString);
        }
    }

    /// <summary>
    /// Настройка модели при создании.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка сущности User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasDefaultValue(UserRole.Player);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsBlocked).HasDefaultValue(false);
        });

        // Настройка сущности Card
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CardType).HasDefaultValue(CardType.Melee);
        });

        // Настройка сущности Deck
        modelBuilder.Entity<Deck>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Decks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка сущности DeckCard
        modelBuilder.Entity<DeckCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(e => e.Deck)
                .WithMany(d => d.DeckCards)
                .HasForeignKey(e => e.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Card)
                .WithMany()
                .HasForeignKey(e => e.CardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Настройка сущности GameSession
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.LastSavedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Настройка сущности GameStatistics
        modelBuilder.Entity<GameStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Wins).HasDefaultValue(0);
            entity.Property(e => e.Losses).HasDefaultValue(0);
            entity.Property(e => e.TotalGames).HasDefaultValue(0);
        });
    }
}
