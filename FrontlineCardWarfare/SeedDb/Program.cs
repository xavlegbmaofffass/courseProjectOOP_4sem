using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare;

var services = new ServiceCollection();
services.AddDbContext<GameDbContext>(options =>
    options.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=FrontlineCardWarfare;Trusted_Connection=True;TrustServerCertificate=True;"));

var provider = services.BuildServiceProvider();

using var scope = provider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();

// Применяем миграции
Console.WriteLine("Применение миграций...");
await context.Database.MigrateAsync();
Console.WriteLine("Миграции применены.");

// Заполняем данными
Console.WriteLine("Инициализация данных...");
await SeedData.InitializeAsync(context);
Console.WriteLine("Данные инициализированы.");

Console.WriteLine("Готово!");