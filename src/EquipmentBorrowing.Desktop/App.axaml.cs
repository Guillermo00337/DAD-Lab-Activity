using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

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
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _services.GetRequiredService<MainWindowViewModel>()
                };
                desktop.Exit += (_, _) => _services.Dispose();
                desktop.MainWindow.Show();
            }
            catch (Exception ex)
            {
                desktop.MainWindow = new Window
                {
                    Title = "Application Startup Error",
                    Width = 500,
                    Height = 200,
                    Content = new TextBlock
                    {
                        Margin = new Thickness(24),
                        Text = $"The application could not start: {ex.Message}",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                };
                desktop.MainWindow.Show();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5080/")
        });
        services.AddSingleton<BorrowingApiClient>();

        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<BorrowingsViewModel>();
        services.AddSingleton<StudentsViewModel>();
        services.AddSingleton<EquipmentManagementViewModel>();
        services.AddSingleton<HistoryViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}