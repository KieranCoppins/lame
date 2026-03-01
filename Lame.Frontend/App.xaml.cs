using System.Configuration;
using System.Data;
using System.Windows;
using Lame.Backend.EntityFramework;
using Lame.Backend.Assets;
using Lame.Backend.Assets.LocalEF;
using Lame.Backend.Translations;
using Lame.Backend.Translations.LocalEF;
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
        
        services.AddScoped<IAssets, AssetsLocalEf>();
        services.AddScoped<ITranslations, TranslationsLocalEF>();
        
        // Build the service provider
        ServiceProvider = services.BuildServiceProvider();
    }
}