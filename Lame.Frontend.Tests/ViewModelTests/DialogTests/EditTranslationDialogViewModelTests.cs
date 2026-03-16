using System.Collections.ObjectModel;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.TestingHelpers;
using Lame.Frontend.Tests.ViewModelFactories.Dialog;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests.DialogTests;

public class EditTranslationDialogViewModelTests
{
    [Fact]
    public async Task SaveChangesCommand_WhenTranslationContentUnchanged_ClosesDialogAndDoesNotCreateOrNotify()
    {
        // Arrange
        var translation = new TranslationDtoBuilder().WithContent("Original").Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "Original";

        // Act
        vm.SaveChangesCommand.Execute(null);
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        // Assert
        dialogService.Verify(x => x.CloseDialog(), Times.Once);
        translationsService.Verify(x => x.Create(It.IsAny<TranslationDto>()), Times.Never);
        notificationService.Verify(x => x.EmitNotification(It.IsAny<Notification>()), Times.Never);
    }

    [Fact]
    public async Task SaveChangesCommand_WhenSelectedTranslationContentMatches_ChangesTargetVersionAndEmitsSuccess()
    {
        // Arrange
        var translation = new TranslationDtoBuilder().WithContent("A").Build();
        var selectedTranslation = new TranslationDtoBuilder().WithContent("B").Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.SelectedTranslation = selectedTranslation;
        vm.Content = "B";

        // Act
        vm.SaveChangesCommand.Execute(null);
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        // Assert
        translationsService.Verify(x => x.SetTargetTranslation(selectedTranslation.Id), Times.Once);

        notificationService.Verify(x => x.EmitNotification(
            It.Is<Notification>(n => n.Type == NotificationType.Success)
        ), Times.Once);

        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task
        SaveChangesCommand_WhenCreatingNewTranslationWithMissingStatus_UsesEnglishMajorVersionAndIncrementsMinor()
    {
        // Arrange
        var translation = new TranslationDtoBuilder()
            .WithContent("Old")
            .WithStatus(TranslationStatus.Missing)
            .WithLanguageCode("fr")
            .Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        var englishTranslation = new TranslationDtoBuilder()
            .WithLanguageCode("en")
            .WithVersion(2, 0)
            .Build();

        translationsService
            .Setup(x => x.GetTargetedForAsset(translation.AssetId))
            .ReturnsAsync([englishTranslation]);

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "New";
        vm.HasMajorChanges = true;

        // Act
        vm.SaveChangesCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        translationsService.Verify(x => x.Create(It.Is<TranslationDto>(t =>
            t.Content == "New" &&
            t.MajorVersion == 2 &&
            t.MinorVersion == 0
        )), Times.Once);

        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Success)
            ),
            Times.Once);

        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task SaveChangesCommand_WhenEnglishTranslationNotFound_EmitsFailureNotificationAndDoesNotCloseDialog()
    {
        // Arrange
        var translation = new TranslationDtoBuilder()
            .WithContent("Old")
            .WithStatus(TranslationStatus.Missing)
            .WithLanguageCode("fr")
            .Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        translationsService
            .Setup(x => x.GetTargetedForAsset(translation.AssetId))
            .ReturnsAsync([]);

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "New";
        vm.HasMajorChanges = true;

        // Act
        vm.SaveChangesCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);

        dialogService.Verify(x => x.CloseDialog(), Times.Never);
    }

    [Fact]
    public async Task SaveChangesCommand_WhenExceptionThrownDuringCreate_EmitsFailureNotificationAndDoesNotCloseDialog()
    {
        // Arrange
        var translation = new TranslationDtoBuilder().WithContent("Old").Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        translationsService
            .Setup(x => x.Create(It.IsAny<TranslationDto>()))
            .ThrowsAsync(new Exception("fail"));

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "New";

        // Act
        vm.SaveChangesCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n => n.Type == NotificationType.Failure)
            ),
            Times.Once);

        dialogService.Verify(x => x.CloseDialog(), Times.Never);
    }

    [Fact]
    public async Task
        SaveChangesCommand_WhenHasMajorChangesAndStatusUpToDate_IncrementsMajorVersionAndResetsMinorVersion()
    {
        // Arrange
        var translation = new TranslationDtoBuilder()
            .WithContent("Old")
            .WithStatus(TranslationStatus.UpToDate)
            .WithVersion(1, 2)
            .Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        var existingVersions = new List<TranslationDto>
        {
            new TranslationDtoBuilder().WithVersion(2, 3).Build(),
            new TranslationDtoBuilder().WithVersion(1, 5).Build()
        };

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "New";
        vm.HasMajorChanges = true;
        vm.Translations = new ObservableCollection<TranslationDto>(existingVersions);

        // Act
        vm.SaveChangesCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        translationsService.Verify(x => x.Create(
            It.Is<TranslationDto>(t =>
                t.MajorVersion == 3 && t.MinorVersion == 0
            )), Times.Once);
    }

    [Fact]
    public async Task SaveChangesCommand_WhenHasNoMajorChanges_IncrementsMinorVersion()
    {
        // Arrange
        var translation = new TranslationDtoBuilder()
            .WithContent("Old")
            .WithStatus(TranslationStatus.UpToDate)
            .WithVersion(1, 2)
            .Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "New";
        vm.HasMajorChanges = false;

        // Act
        vm.SaveChangesCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        translationsService.Verify(x => x.Create(
            It.Is<TranslationDto>(t =>
                t.MajorVersion == 1 && t.MinorVersion == 3
            )), Times.Once);
    }

    [Fact]
    public async Task
        SaveChangesCommand_WhenStatusOutdatedAndHasMajorChanges_UsesEnglishMajorVersionAndIncrementsMinor()
    {
        // Arrange
        var translation = new TranslationDtoBuilder()
            .WithContent("Old")
            .WithStatus(TranslationStatus.Outdated)
            .WithLanguageCode("fr")
            .Build();

        var dialogService = new Mock<IDialogService>();
        var translationsService = new Mock<ITranslations>();
        var notificationService = new Mock<INotificationService>();

        var englishTranslation = new TranslationDtoBuilder()
            .WithLanguageCode("en")
            .WithVersion(5, 0)
            .Build();

        translationsService.Setup(x => x.GetTargetedForAsset(translation.AssetId))
            .ReturnsAsync([englishTranslation]);

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object,
            translationsService.Object,
            notificationService.Object
        );
        vm.Content = "Updated";
        vm.HasMajorChanges = true;

        // Act
        vm.SaveChangesCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.SaveChangesCommand).CommandTask!;

        translationsService.Verify(x => x.Create(
            It.Is<TranslationDto>(t =>
                t.MajorVersion == 5 && t.MinorVersion == 0
            )), Times.Once);
    }

    [Fact]
    public void CancelCommand_WhenExecuted_ClosesDialog()
    {
        // Arrange
        var translation = new TranslationDtoBuilder().Build();

        var dialogService = new Mock<IDialogService>();

        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            dialogService.Object
        );

        // Act
        vm.CancelCommand.Execute(null);

        // Assert
        dialogService.Verify(x => x.CloseDialog(), Times.Once);
    }

    [Fact]
    public async Task Constructor_WhenInitialized_LoadsTranslationVersions()
    {
        // Arrange
        var translation = new TranslationDtoBuilder().WithLanguageCode("en").Build();

        var versions = new List<TranslationDto>
        {
            translation,
            new TranslationDtoBuilder().WithLanguageCode("en").Build(),
            new TranslationDtoBuilder().WithLanguageCode("en").Build()
        };

        var translationsService = new Mock<ITranslations>();
        translationsService
            .Setup(x => x.GetAllForLanguageForAsset(translation.AssetId, translation.Language))
            .ReturnsAsync(versions);

        // Act
        var vm = EditTranslationDialogViewModelFactory.Create(
            translation,
            translationsService: translationsService.Object
        );

        await vm.TranslationVersionTask;

        // Assert
        Assert.Equal(3, vm.Translations.Count);
    }
}