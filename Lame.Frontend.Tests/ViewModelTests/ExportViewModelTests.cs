using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Tests.TestingHelpers;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels.Exports;
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
}