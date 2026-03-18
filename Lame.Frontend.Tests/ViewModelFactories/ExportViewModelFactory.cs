using Lame.Backend.Exports;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.Frontend.Services;
using Lame.Frontend.ViewModels;
using Moq;

namespace Lame.Frontend.Tests.ViewModelFactories;

public class ExportViewModelFactory
{
    public static ExportViewModel Create(
        INotificationService? notificationService = null,
        ILanguages? languagesService = null,
        IExports? exportsService = null,
        ITags? tagsService = null,
        ISystemIO? systemIo = null
    )
    {
        return new ExportViewModel(
            notificationService ?? new Mock<INotificationService>().Object,
            languagesService ?? new Mock<ILanguages>().Object,
            exportsService ?? new Mock<IExports>().Object,
            tagsService ?? new Mock<ITags>().Object,
            systemIo ?? new Mock<ISystemIO>().Object
        );
    }
}