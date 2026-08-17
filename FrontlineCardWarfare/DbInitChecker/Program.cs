using System;
using System.Data.SqlClient;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using FrontlineCardWarfare.Data;

string server = @"(localdb)\MSSQLLocalDB";
string connStr = $"Server={server};Database=FrontlineCardWarfare;Integrated Security=True;TrustServerCertificate=True;";

Console.WriteLine("=== Инициализация БД FrontlineCardWarfare ===\n");

try
{
    var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
    optionsBuilder.UseSqlServer(connStr);

    using (var context = new GameDbContext(optionsBuilder.Options))
    {
        // 1. Применяем миграции
        Console.WriteLine("1. Применение миграций...");
        var pendingMigrations = context.Database.FindMigrations().Except(context.Database.GetAppliedMigrations());
        
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"   Найдено {pendingMigrations.Count()} неприменённых миграций");
            context.Database.Migrate();
            Console.WriteLine("   OK: Миграции применены");
        }
        else
        {
            Console.WriteLine("   OK: Все миграции уже применены");
        }
        
        // 2. Проверяем таблицы
        Console.WriteLine("\n2. Проверка таблиц:");
        var tables = new[] { "Users", "Cards", "Decks", "DeckCards", "GameSessions", "GameStatistics" };
        foreach (var table in tables)
        {
            var exists = context.Database.SqlQuery<int>($"SELECT COUNT(*) FROM sys.tables WHERE name = '{table}'").FirstOrDefault() > 0;
            Console.WriteLine($"   {(exists ? "OK" : "MISSING")}: {table}");
        }
        
        // 3. Проверяем данные
        Console.WriteLine("\n3. Проверка данных:");
        var userCount = context.Users.Count();
        var cardCount = context.Cards.Count();
        Console.WriteLine($"   Пользователей: {userCount}");
        Console.WriteLine($"   Карт: {cardCount}");
        
        if (userCount == 0 || cardCount == 0)
        {
            Console.WriteLine("\n4. Инициализация SeedData...");
            Console.WriteLine("   ВНИМАНИЕ: SeedData нужно запустить вручную через DbInit проект");
        }
        else
        {
            Console.WriteLine("   OK: Данные присутствуют");
        }
    }
    
    Console.WriteLine("\n=== Готово ===");
}
catch (Exception ex)
{
    Console.WriteLine($"\nERROR: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner: {ex.InnerException.Message}");
    }
}
