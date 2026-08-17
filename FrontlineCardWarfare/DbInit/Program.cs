using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare;

namespace DbInit
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Утилита для управления БД")
            {
                new Option<bool>(new[] { "--drop-database" }, "Удалить базу данных перед созданием"),
                new Option<bool>(new[] { "--seed" }, "Заполнить базу данных начальными данными")
            };

            rootCommand.SetHandler(async (dropDb, seed) =>
            {
                string connectionString = @"Server=(localdb)\MSSQLLocalDB;Database=FrontlineCardWarfare;Trusted_Connection=True;TrustServerCertificate=True;";
                string masterConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;";

                if (dropDb)
                {
                    await DropDatabaseAsync(masterConnectionString, "FrontlineCardWarfare");
                }

                // Создаем сервис-контейнер
                var services = new ServiceCollection();
                services.AddDbContext<GameDbContext>(options =>
                    options.UseSqlServer(connectionString));

                var provider = services.BuildServiceProvider();

                using var scope = provider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

                // Применяем миграции
                Console.WriteLine("Применение миграций...");
                await context.Database.MigrateAsync();
                Console.WriteLine("Миграции применены.");

                // Заполняем данными
                if (seed || !context.Cards.Any())
                {
                    Console.WriteLine("Инициализация данных...");
                    await SeedData.InitializeAsync(context);
                    Console.WriteLine("Данные инициализированы.");
                }

                Console.WriteLine("Готово!");
            },
            rootCommand.Children.OfType<Option<bool>>().First(o => o.Aliases.Contains("--drop-database")),
            rootCommand.Children.OfType<Option<bool>>().First(o => o.Aliases.Contains("--seed")));

            return await rootCommand.InvokeAsync(args);
        }

        static async Task DropDatabaseAsync(string connectionString, string dbName)
        {
            Console.WriteLine($"Проверка базы данных: {dbName}...");
            
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                
                using (var cmd = new SqlConnection(connectionString))
                {
                    // Сначала проверяем, существует ли БД
                    using (var checkCmd = new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{dbName}'", conn))
                    {
                        var count = (int)await checkCmd.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            Console.WriteLine("Удаление старой базы данных...");
                            conn.Close();
                            
                            using (var conn2 = new SqlConnection(connectionString))
                            {
                                await conn2.OpenAsync();
                                var cmd2 = new SqlCommand($@"
                                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                    DROP DATABASE [{dbName}];", conn2);
                                cmd2.CommandTimeout = 60;
                                await cmd2.ExecuteNonQueryAsync();
                                Console.WriteLine("База данных удалена.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("База данных не существует, пропускаем удаление.");
                        }
                    }
                }
            }
        }
    }
}