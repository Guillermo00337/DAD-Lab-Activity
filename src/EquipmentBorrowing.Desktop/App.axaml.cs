
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
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
    private ServiceProvider? _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                _services = ConfigureServices();

                // Display a loading window while PostgreSQL initializes.
                var loadingWindow = new Window
                {
                    Title = "Campus Equipment Borrowing System",
                    Width = 420,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new StackPanel
                    {
                        Margin = new Thickness(24),
                        Spacing = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "Campus Equipment Borrowing System",
                                FontSize = 20,
                                FontWeight = Avalonia.Media.FontWeight.SemiBold
                            },
                            new TextBlock
                            {
                                Text = "Connecting to PostgreSQL and loading data...",
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap
                            }
                        }
                    }
                };

                desktop.MainWindow = loadingWindow;

                desktop.Exit += (_, _) => _services?.Dispose();

                // Initialize the database without blocking Avalonia startup.
                _ = InitializeApplicationAsync(desktop, loadingWindow);
            }
            catch (Exception)
            {
                desktop.MainWindow = new Window
                {
                    Title = "Application Startup Error",
                    Width = 500,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new TextBlock
                    {
                        Margin = new Thickness(24),
                        Text = "The application could not start. Check your PostgreSQL configuration.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task InitializeApplicationAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        Window loadingWindow)
    {
        try
        {
            var services = _services
                ?? throw new InvalidOperationException(
                    "Application services are not configured.");

            await services
                .GetRequiredService<DatabaseInitializer>()
                .InitializeAsync();

            var mainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };

            // Show the main window before closing the loading window.
            mainWindow.Show();
            desktop.MainWindow = mainWindow;
            loadingWindow.Close();
        }
        catch (Exception)
        {
            loadingWindow.Title = "Database Initialization Error";
            loadingWindow.Content = new StackPanel
            {
                Margin = new Thickness(24),
                Spacing = 12,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Could not initialize the application database.",
                        FontSize = 20,
                        FontWeight = Avalonia.Media.FontWeight.SemiBold
                    },
                    new TextBlock
                    {
                        Text = "Check PostgreSQL and your connection string, then restart the application.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                }
            };
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        string connectionString =
            Environment.GetEnvironmentVariable(
                "EQUIPMENT_BORROWING_CONNECTION_STRING") ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "EQUIPMENT_BORROWING_CONNECTION_STRING is not set. " +
                "See docs/POSTGRESQL_SETUP.md for setup instructions.");
        }

        string logDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "EquipmentBorrowing");

        Directory.CreateDirectory(logDirectory);

        string logFile = Path.Combine(
            logDirectory,
            "efcore-postgresql-sql.log");

        var services = new ServiceCollection();

        services.AddDbContextFactory<EquipmentBorrowingDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .EnableDetailedErrors()
                .LogTo(
                    message => File.AppendAllText(logFile, message),
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
        services.AddTransient<StudentManagementService>();
        services.AddTransient<EquipmentManagementService>();
        services.AddTransient<BorrowingQueryService>();

        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<BorrowingsViewModel>();
        services.AddSingleton<StudentsViewModel>();
        services.AddSingleton<EquipmentManagementViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
