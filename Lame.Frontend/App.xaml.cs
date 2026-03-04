using System.Windows;
using Lame.Backend.Assets;
using Lame.Backend.Assets.LocalEF;
using Lame.Backend.EntityFramework;
using Lame.Backend.Tags;
using Lame.Backend.Tags.LocalEF;
using Lame.Backend.Translations;
using Lame.Backend.Translations.LocalEF;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={AppDbContext.GetConnectionString()}"));

        // Frontend Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Backend Services
        services.AddScoped<IAssets, AssetsLocalEf>();
        services.AddScoped<ITranslations, TranslationsLocalEF>();
        services.AddScoped<ITags, TagsLocalEF>();

        // View Models
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<AssetLibraryViewModel>();
        services.AddTransient<AssetDetailsViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CreateAssetViewModel>();

        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
    }

    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        MainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
        };
        MainWindow.Show();

        // Navigate to initial view
        var navigation = ServiceProvider.GetRequiredService<INavigationService>();
        navigation.NavigateTo(() => ServiceProvider.GetRequiredService<DashboardViewModel>());

        // Handle any unhandled exceptions globally
        DispatcherUnhandledException += (sender, args) =>
        {
            var notificationService = ServiceProvider.GetRequiredService<INotificationService>();
            notificationService.EmitNotification(new Notification
            {
                Title = "An error occurred in the application",
                Message = args.Exception.Message,
                Type = NotificationType.Failure
            });

            args.Handled = false;
        };
    }
}