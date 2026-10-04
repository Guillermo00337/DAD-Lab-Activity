using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ServiceProvider services = ConfigureServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        string databasePath = DatabasePath.GetDatabasePath();

        string sqlLogPath = Path.Combine(Path.GetDirectoryName(databasePath)!, "efcore-sql.log");

        services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
            options
                .UseSqlite($"Data Source={databasePath}")
                .LogTo(
                    message => File.AppendAllText(sqlLogPath, message),
                    new[] { DbLoggerCategory.Database.Command.Name },
                    LogLevel.Information));
        services.AddSingleton<DatabaseInitializer>();

        services.AddSingleton<IStudentRepository, EfStudentRepository>();
        services.AddSingleton<IEquipmentRepository, EfEquipmentRepository>();
        services.AddSingleton<IBorrowingRepository, EfBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();
        services.AddTransient<EquipmentCatalogService>();
        services.AddTransient<StudentCatalogService>();
        services.AddTransient<ActiveBorrowingsService>();

        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<BorrowingsViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider.GetRequiredService<DatabaseInitializer>()
            .InitializeAsync()
            .GetAwaiter()
            .GetResult();

        return serviceProvider;
    }
}
