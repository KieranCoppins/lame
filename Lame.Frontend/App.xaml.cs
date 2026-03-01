using System.Configuration;
using System.Data;
using System.Windows;
using Lame.Backend.EntityFramework;
using Lame.Backend.Assets;
using Lame.Backend.Assets.LocalEF;
using Lame.Backend.Translations;
using Lame.Backend.Translations.LocalEF;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    public App()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={AppDbContext.GetConnectionString()}"));
        
        // Frontend Services
        services.AddSingleton<INavigationService, NavigationService>();
        
        // Backend Services
        services.AddScoped<IAssets, AssetsLocalEf>();
        services.AddScoped<ITranslations, TranslationsLocalEF>();
        
        // View Models
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<AssetLibraryViewModel>();
        services.AddTransient<AssetDetailsViewModel>();
        
        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();

        MainWindow = new MainWindow()
        {
            DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
        };
        MainWindow.Show();
        
        // Navigate to initial view
        var navigation = ServiceProvider.GetRequiredService<INavigationService>();
        navigation.NavigateTo(() => ServiceProvider.GetRequiredService<AssetLibraryViewModel>());
    }
}