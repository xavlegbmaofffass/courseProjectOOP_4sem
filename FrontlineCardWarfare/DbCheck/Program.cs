using System;
using System.Data.SqlClient;

string server = @"(localdb)\MSSQLLocalDB";
string connStr = $"Server={server};Database=FrontlineCardWarfare;Integrated Security=True;TrustServerCertificate=True;";

Console.WriteLine("Проверка данных в БД...\n");

try
{
    using (var conn = new SqlConnection(connStr))
    {
        conn.Open();
        
        // Проверка пользователей
        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users", conn))
        {
            var count = (int)cmd.ExecuteScalar();
            Console.WriteLine($"Пользователей: {count}");
            if (count == 0)
            {
                Console.WriteLine("  WARNING: Нет пользователей - SeedData не был выполнен!");
            }
        }
        
        // Проверка карт
        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Cards", conn))
        {
            var count = (int)cmd.ExecuteScalar();
            Console.WriteLine($"Карт: {count}");
            if (count == 0)
            {
                Console.WriteLine("  WARNING: Нет карт - SeedData не был выполнен!");
            }
        }
        
        // Проверка применённых миграций
        using (var cmd = new SqlCommand("SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId", conn))
        using (var reader = cmd.ExecuteReader())
        {
            Console.WriteLine("\nПрименённые миграции:");
            while (reader.Read())
            {
                Console.WriteLine($"  - {reader[0]}");
            }
        }
        
        // Проверка наличия миграции FixCardImagePaths
        using (var cmd = new SqlCommand("SELECT COUNT(*) FROM __EFMigrationsHistory WHERE MigrationId = '20260511000001_FixCardImagePaths'", conn))
        {
            var count = (int)cmd.ExecuteScalar();
            if (count == 0)
            {
                Console.WriteLine("\nWARNING: Миграция '20260511000001_FixCardImagePaths' НЕ применена!");
                Console.WriteLine("Это может вызвать ошибку при запуске приложения.");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
}