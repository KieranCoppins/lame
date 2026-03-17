using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.DomainModel;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels.Exports;
using Lame.TestingHelpers;
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
        var tagsService = new Mock<ITags>();
        tagsService.Setup(t => t.Get()).ThrowsAsync(new Exception("error"));

        var vm = ExportViewModelFactory.Create(tagsService: tagsService.Object);

        await Assert.ThrowsAsync<Exception>(() => vm.GetTags());
    }
}