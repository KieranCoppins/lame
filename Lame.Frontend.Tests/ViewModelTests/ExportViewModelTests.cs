using Lame.Backend.Exports;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels.Exports;
using Lame.TestingHelpers;
using Microsoft.Win32;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class ExportViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_ShouldLoadLanguages()
    {
        // Arrange
        var languages = new List<Language>
        {
            new LanguageBuilder().WithLanguageCode("en").Build(),
            new LanguageBuilder().WithLanguageCode("fr").Build(),
            new LanguageBuilder().WithLanguageCode("es").Build()
        };

        var languagesServiceMock = new Mock<ILanguages>();
        languagesServiceMock.Setup(s => s.Get()).ReturnsAsync(languages);

        var vm = ExportViewModelFactory.Create(languagesService: languagesServiceMock.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        languagesServiceMock.Verify(s => s.Get(), Times.Once);
        Assert.Equal(3, vm.AvailableLanguages.Count);
    }

    [Fact]
    public void SetSelectedExportFormatCommand_WhenExecuted_ShouldSetSelectedExportFormat()
    {
        // Arrange
        var vm = ExportViewModelFactory.Create();

        // Act
        vm.SetSelectedExportFormatCommand.Execute(ExportFormatType.XLIFF);

        // Assert
        Assert.Equal(ExportFormatType.XLIFF, vm.SelectedExportFormat);
    }

    [Theory]
    [InlineData(ExportFormatType.XLIFF, typeof(ExportXliffViewModel))]
    [InlineData(ExportFormatType.JSON, typeof(ExportJsonViewModel))]
    public void SettingSelectedExportFormat_ToFormatType_ShouldSetFormatViewToCorrectViewModel(
        ExportFormatType exportFormatType,
        Type expectedViewModelType
    )
    {
        // Arrange
        var vm = ExportViewModelFactory.Create();

        // Act
        vm.SelectedExportFormat = exportFormatType;

        // Assert
        Assert.IsType(expectedViewModelType, vm.ExportFormatView);
    }

    [Fact]
    public void SettingSelectedExportFormat_ToInvalidFormat_ShouldSetFormatViewToNull()
    {
        // Arrange
        var vm = ExportViewModelFactory.Create();

        // Act
        vm.SelectedExportFormat = (ExportFormatType)999;

        // Assert
        Assert.Null(vm.ExportFormatView);
    }

    [Theory]
    [InlineData(ExportTagFilterType.All)]
    [InlineData(ExportTagFilterType.Any)]
    [InlineData(ExportTagFilterType.Only)]
    [InlineData((ExportTagFilterType)999)]
    public void SetSelectedExportTagFilterTypeCommand_WhenExecuted_HandlesVariousFilterTypes(
        ExportTagFilterType filterType)
    {
        // Arrange
        var vm = ExportViewModelFactory.Create();

        // Act
        vm.SetSelectedExportTagFilterTypeCommand.Execute(filterType);

        // Assert
        Assert.Equal(filterType, vm.SelectedExportTagFilter);
    }

    [Fact]
    public async Task GetTags_ReturnsTagsFromTagsService()
    {
        // Arrange
        var expectedTags = new List<Tag>
        {
            new TagBuilder().Build(),
            new TagBuilder().Build()
        };

        var tagsService = new Mock<ITags>();
        tagsService.Setup(t => t.Get()).ReturnsAsync(expectedTags);

        var vm = ExportViewModelFactory.Create(tagsService: tagsService.Object);

        // Act
        var result = await vm.GetTags();

        // Assert
        Assert.Equal(expectedTags, result);
    }

    [Fact]
    public async Task GetTags_ReturnsEmptyList_WhenTagsServiceReturnsEmpty()
    {
        // Arrange
        var tagsService = new Mock<ITags>();
        tagsService.Setup(t => t.Get()).ReturnsAsync(new List<Tag>());

        var vm = ExportViewModelFactory.Create(tagsService: tagsService.Object);

        // Act
        var result = await vm.GetTags();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTags_ThrowsException_WhenTagsServiceThrows()
    {
        // Arrange
        var tagsService = new Mock<ITags>();
        tagsService.Setup(t => t.Get()).ThrowsAsync(new Exception("error"));

        var vm = ExportViewModelFactory.Create(tagsService: tagsService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => vm.GetTags());
    }

    [Fact]
    public async Task Export_WhenExportFormatViewIsNull_ShouldEmitFailureNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var vm = ExportViewModelFactory.Create(notificationService.Object);
        vm.ExportFormatView = null;

        // Act
        vm.ExportCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ExportCommand).CommandTask!;

        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    [Fact]
    public async Task Export_WhenUserCancelsSaveDialog_ShouldNotExportOrNotify()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var systemIo = new Mock<ISystemIO>();
        systemIo.Setup(s => s.OpenSaveFileDialog(It.IsAny<SaveFileDialog>())).Returns(false);

        var vm = ExportViewModelFactory.Create(
            notificationService.Object,
            systemIo: systemIo.Object);

        vm.ExportFormatView = new TestExportFormatViewModel();

        // Act
        vm.ExportCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ExportCommand).CommandTask!;

        systemIo.Verify(s => s.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        notificationService.Verify(n => n.EmitNotification(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task Export_WhenExportSucceeds_ShouldWriteFileAndEmitSuccessNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var systemIo = new Mock<ISystemIO>();
        var exportsService = new Mock<IExports>();
        var fileData = new byte[] { 1, 2, 3 };
        var testFilePath = "test_export.xliff";

        systemIo
            .Setup(s => s.OpenSaveFileDialog(It.IsAny<SaveFileDialog>()))
            .Callback<SaveFileDialog>(dlg => dlg.FileName = testFilePath)
            .Returns(true);
        systemIo.Setup(s => s.WriteAllBytesAsync(It.IsAny<string>(), fileData)).Returns(Task.CompletedTask);
        exportsService.Setup(e => e.Export(It.IsAny<ExportOptions>())).ReturnsAsync(fileData);

        var vm = ExportViewModelFactory.Create(
            notificationService.Object,
            systemIo: systemIo.Object,
            exportsService: exportsService.Object);

        vm.ExportFormatView = new TestExportFormatViewModel();

        // Act
        vm.ExportCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ExportCommand).CommandTask!;

        systemIo.Verify(s => s.WriteAllBytesAsync(testFilePath, fileData), Times.Once);
        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task Export_WhenExportServiceThrows_ShouldEmitFailureNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var systemIo = new Mock<ISystemIO>();
        var exportsService = new Mock<IExports>();
        systemIo.Setup(s => s.OpenSaveFileDialog(It.IsAny<SaveFileDialog>())).Returns(true);
        exportsService.Setup(e => e.Export(It.IsAny<ExportOptions>())).ThrowsAsync(new Exception("export error"));

        var vm = ExportViewModelFactory.Create(
            notificationService.Object,
            systemIo: systemIo.Object,
            exportsService: exportsService.Object);

        vm.ExportFormatView = new TestExportFormatViewModel();

        // Act
        vm.ExportCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.ExportCommand).CommandTask!;

        notificationService.Verify(
            x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);
    }

    private class TestExportFormatViewModel : IExportOptionsViewModel
    {
        public ExportOptions GetExportOptions()
        {
            return new ExportOptions
            {
                Format = ExportFormatType.XLIFF,
                LanguageCode = "en",
                TagFilter = ExportTagFilterType.Any,
                Tags = [],
                TranslationStatusFilter = ExportTranslationStatusFilter.All
            };
        }
    }
}