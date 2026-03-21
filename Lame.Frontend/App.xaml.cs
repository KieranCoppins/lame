using System.Windows;
using Lame.Backend.AssetLinks;
using Lame.Backend.AssetLinks.LocalEF;
using Lame.Backend.Assets;
using Lame.Backend.Assets.LocalEF;
using Lame.Backend.EntityFramework;
using Lame.Backend.Exports;
using Lame.Backend.Exports.Exporters;
using Lame.Backend.Exports.LocalEF;
using Lame.Backend.FileStorage;
using Lame.Backend.FileStorage.Local;
using Lame.Backend.Imports;
using Lame.Backend.Imports.LocalEF;
using Lame.Backend.Languages;
using Lame.Backend.Languages.LocalEF;
using Lame.Backend.Statistics;
using Lame.Backend.Statistics.LocalEF;
using Lame.Backend.Tags;
using Lame.Backend.Tags.LocalEF;
using Lame.Backend.Translations;
using Lame.Backend.Translations.LocalEF;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Lame.Frontend.ViewModels.Dialogs;
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
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ISystemIO, WindowsSystemIO>();

        // Backend Services
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IAssets, AssetsLocalEf>();
        services.AddScoped<IAssetLinks, AssetLinksLocalEF>();
        services.AddScoped<ITranslations, TranslationsLocalEF>();
        services.AddScoped<ITags, TagsLocalEF>();
        services.AddScoped<ILanguages, LanguagesLocalEF>();
        services.AddScoped<IStatistics, StatisticsLocalEF>();
        services.AddScoped<IExports, ExportsLocalEF>();
        services.AddScoped<IImports, ImportsLocalEF>();
        services.AddTransient<IExporterFactory, ExporterFactory>();
        services.AddTransient<JsonExporter>();
        services.AddTransient<Xliff12Exporter>();

        // View Models
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<AssetLibraryViewModel>();
        services.AddTransient<AssetDetailsViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CreateAssetViewModel>();
        services.AddTransient<LinkAssetsDialogViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ExportViewModel>();
        services.AddTransient<EditTranslationDialogViewModel>();

        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
    }

    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Apply DB migrations
        using (var scope = ServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

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