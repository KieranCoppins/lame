using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels.Dialogs;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class SettingsViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_LoadsLanguages()
    {
        // Arrange
        var languages = new List<Language>
        {
            new LanguageBuilder().WithLanguageCode("en").Build(),
            new LanguageBuilder().WithLanguageCode("fr").Build(),
            new LanguageBuilder().WithLanguageCode("es").Build()
        };

        var languagesService = new Mock<ILanguages>();
        languagesService.Setup(x => x.Get()).ReturnsAsync(languages);

        var vm = SettingsViewModelFactory.Create(languagesService: languagesService.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        Assert.Equal(3, vm.SupportedLanguages.Count);
    }

    [Fact]
    public void OpenLanguageDialogCommand_WhenExecuted_ShouldOpenLanguageDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var vm = SettingsViewModelFactory.Create(dialogService.Object);

        // Act
        vm.OpenAddLanguageDialogCommand.Execute(null);

        // Assert
        dialogService.Verify(x => x.ShowDialog<AddSupportedLanguageDialogViewModel>(), Times.Once);
    }
}