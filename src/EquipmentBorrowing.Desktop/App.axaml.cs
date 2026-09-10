using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Interfaces;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

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
        var students = new[]
        {
            new Student(1, "2026-0001", "Ana Reyes", isAllowedToBorrow: true, maxActiveBorrowings: 2),
            new Student(2, "2026-0002", "Marco Santos", isAllowedToBorrow: false, maxActiveBorrowings: 2),
            new Student(3, "2026-0003", "Lia Cruz", isAllowedToBorrow: true, maxActiveBorrowings: 1)
        };

        var equipment = new[]
        {
            new Equipment(1, "CAM-001", "Digital Camera"),
            new Equipment(2, "MIC-001", "Wireless Microphone"),
            new Equipment(3, "TAB-001", "Drawing Tablet"),
            new Equipment(4, "LAP-001", "Laptop")
        };

        var services = new ServiceCollection();

        services.AddSingleton<IStudentRepository>(_ => new InMemoryStudentRepository(students));
        services.AddSingleton<IEquipmentRepository>(_ => new InMemoryEquipmentRepository(equipment));
        services.AddSingleton<IBorrowingRepository, InMemoryBorrowingRepository>();

        services.AddTransient<BorrowEquipmentService>();
        services.AddTransient<ReturnEquipmentService>();
        services.AddTransient<EquipmentCatalogService>();
        services.AddTransient<StudentCatalogService>();
        services.AddTransient<ActiveBorrowingsService>();

        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<BorrowingsViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
