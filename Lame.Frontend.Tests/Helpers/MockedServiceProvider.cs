using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Backend.FileStorage;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.Frontend.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Lame.Frontend.Tests.Helpers;

public class MockedServiceProvider
{
    public static IServiceProvider Get()
    {
        // Add mocks to the service provider for AssetDetailsViewModel as this view model constructs an instance
        // of the AssetDetailsViewModel through the service provider
        var services = new ServiceCollection();
        services.AddScoped<INavigationService>(p => new Mock<INavigationService>().Object);
        services.AddScoped<ITranslations>(p => new Mock<ITranslations>().Object);
        services.AddScoped<ITags>(p => new Mock<ITags>().Object);
        services.AddScoped<IAssets>(p => new Mock<IAssets>().Object);
        services.AddScoped<IDialogService>(p => new Mock<IDialogService>().Object);
        services.AddScoped<INotificationService>(p => new Mock<INotificationService>().Object);
        services.AddScoped<IFileStorage>(p => new Mock<IFileStorage>().Object);
        services.AddScoped<ISystemIO>(p => new Mock<ISystemIO>().Object);
        services.AddScoped<IAssetLinks>(p => new Mock<IAssetLinks>().Object);
        services.AddScoped<ILanguages>(p => new Mock<ILanguages>().Object);
        return services.BuildServiceProvider();
    }
}