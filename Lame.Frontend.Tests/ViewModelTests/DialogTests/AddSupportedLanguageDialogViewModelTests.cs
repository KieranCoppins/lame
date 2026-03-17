using Lame.Backend.Languages;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories.Dialog;
using Lame.Frontend.ViewModels;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests.DialogTests;

public class AddSupportedLanguageDialogViewModelTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("english", "en")]
    [InlineData("EnglIsh", "en")]
    [InlineData("Fre", "fr")]
    [InlineData("Span", "es")]
    public async Task SearchLanguages_SearchTerm_ReturnsExpectedResult(
        string searchTerm,
        string topLanguageCode
    )
    {
        // Arrange
        var vm = AddSupportedLanguageDialogViewModelFactory.Create();

        // Act
        var result = (await vm.SearchLanguages(searchTerm)).Cast<LanguageViewModel>().ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(topLanguageCode, result.First().LanguageCode);
    }

    [Fact]
    public void CancelCommand_Execute_ShouldCloseDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();
        var vm = AddSupportedLanguageDialogViewModelFactory.Create(dialogService.Object);

        // Act
        vm.CancelCommand.Execute(null);

        // Assert
        dialogService.Verify(ds => ds.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task AddLanguagesCommand_WithSelectedLanguage_RegistersLanguage()
    {
        // Arrange
        var languagesService = new Mock<ILanguages>();

        var language = new LanguageBuilder().WithLanguageCode("es").Build();

        var vm = AddSupportedLanguageDialogViewModelFactory.Create(
            languageService: languagesService.Object
        );

        vm.SelectedLanguage = new LanguageViewModel(language);

        // Act
        vm.AddLanguageCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.AddLanguageCommand).CommandTask!;

        languagesService.Verify(ds => ds.RegisterLanguage(
                It.Is<Language>(l => l.LanguageCode == language.LanguageCode)
            ),
            Times.Once);
    }

    [Fact]
    public async Task AddLanguagesCommand_WithSelectedLanguage_EmitsSuccessNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();

        var language = new LanguageBuilder().WithLanguageCode("es").Build();

        var vm = AddSupportedLanguageDialogViewModelFactory.Create(
            notificationService: notificationService.Object
        );

        vm.SelectedLanguage = new LanguageViewModel(language);

        // Act
        vm.AddLanguageCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.AddLanguageCommand).CommandTask!;

        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task AddLanguagesCommand_WithSelectedLanguage_ClosesDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();

        var language = new LanguageBuilder().WithLanguageCode("es").Build();

        var vm = AddSupportedLanguageDialogViewModelFactory.Create(
            dialogService.Object
        );

        vm.SelectedLanguage = new LanguageViewModel(language);

        // Act
        vm.AddLanguageCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.AddLanguageCommand).CommandTask!;

        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task AddLanguagesCommand_WithoutSelectedLanguage_EmitsFailureNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();

        var vm = AddSupportedLanguageDialogViewModelFactory.Create(
            notificationService: notificationService.Object
        );

        // Act
        vm.AddLanguageCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.AddLanguageCommand).CommandTask!;

        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);
    }
}