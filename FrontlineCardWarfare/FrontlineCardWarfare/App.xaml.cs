using System;
using System.Windows;
using FrontlineCardWarfare.Data;
using FrontlineCardWarfare.Repositories;
using FrontlineCardWarfare.Services;
using FrontlineCardWarfare.ViewModels;
using FrontlineCardWarfare.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace FrontlineCardWarfare;

public partial class App : Application
{
    public IServiceProvider ServiceProvider => _serviceProvider;
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var connectionString = "Data Source=game.db";
        services.AddDbContext<GameDbContext>(options => { options.UseSqlite(connectionString); });
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IDeckRepository, DeckRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IDeckService, DeckService>();
        services.AddScoped<IBattleManager, BattleManager>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IAbilityManager, AbilityManager>();
        services.AddScoped<ITooltipService, TooltipService>();
        services.AddScoped<IBattleAnimationService, BattleAnimationService>();
        services.AddTransient<IAIController, AIController>();
        services.AddSingleton<IGameSaveService, GameSaveService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IBackgroundMusicService, BackgroundMusicService>();
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<MainWindow>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<CollectionViewModel>();
        services.AddTransient<DeckBuilderViewModel>();
        services.AddTransient<BattleViewModel>();
        services.AddTransient<BattleSetupViewModel>();
        services.AddTransient<AdminViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<RulesViewModel>();
        services.AddTransient<LoginView>();
        services.AddTransient<RegisterView>();
        services.AddTransient<MainView>();
        services.AddTransient<CollectionView>();
        services.AddTransient<DeckBuilderView>();
        services.AddTransient<BattleView>();
        services.AddTransient<BattleSetupView>();
        services.AddTransient<AdminView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<RulesView>();
        services.AddTransient<GameResultView>();
    }

    public T GetService<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;
        TaskScheduler.UnobservedTaskException += App_UnobservedTaskException;

        try 
        { 
            await DatabaseInitializer.InitializeAsync(_serviceProvider); 
        }
        catch (Exception ex) 
        { 
            MessageBox.Show($"Ошибка инициализации БД: {ex.Message}\n{ex.StackTrace}", "Критическая ошибка"); 
            Shutdown(1); 
            return; 
        }

        try {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow; // Устанавливаем главное окно приложения

            var settingsService = _serviceProvider.GetService<ISettingsService>();
            settingsService?.LoadSettings();

            if (settingsService?.Settings.IsFullscreen == true) 
            { 
                mainWindow.WindowStyle = WindowStyle.None; 
                mainWindow.WindowState = WindowState.Maximized; 
            }

            var musicService = _serviceProvider.GetService<IBackgroundMusicService>();
            if (musicService != null) 
            { 
                musicService.SetVolume(settingsService?.Settings.MusicVolume / 100.0 ?? 0.5); 
                musicService.Play(); 
            }

            // Инициализируем начальную навигацию
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            navigationService.NavigateTo<LoginViewModel>();

            mainWindow.Show();
        }
        catch (Exception ex) 
        { 
            string message = ex.Message;
            string stackTrace = ex.StackTrace ?? "";
            
            if (ex.InnerException != null)
            {
                message += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                stackTrace = $"--- Внутренний стек ---\n{ex.InnerException.StackTrace}\n\n--- Внешний стек ---\n{stackTrace}";
                
                if (ex.InnerException.InnerException != null)
                {
                    message += $"\nЕще глубже: {ex.InnerException.InnerException.Message}";
                }
            }
            MessageBox.Show($"Ошибка создания главного окна: {message}\n\n{stackTrace}", "Критическая ошибка"); 
            Shutdown(1); 
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) 
    { 
        // Временно отключаем подавление ошибок для отладки
        MessageBox.Show($"Произошла ошибка в основном потоке: {e.Exception.Message}\n{e.Exception.StackTrace}", "Ошибка Dispatcher");
        // e.Handled = true; 
    }
    private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e) { }
    private void App_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) { e.SetObserved(); }
}
