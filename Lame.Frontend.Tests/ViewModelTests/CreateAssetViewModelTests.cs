using System.Collections.ObjectModel;
using Lame.Backend.AssetLinks;
using Lame.Backend.Assets;
using Lame.Backend.Languages;
using Lame.Backend.Tags;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Commands;
using Lame.Frontend.Services;
using Lame.Frontend.Tests.ViewModelFactories;
using Lame.Frontend.ViewModels.Dialogs;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.ViewModelTests;

public class CreateAssetViewModelTests
{
    [Fact]
    public async Task OnNavigatedTo_WhenCalled_ShouldGetSupportedLanguages()
    {
        // Arrange
        var languages = new List<Language>
        {
            new LanguageBuilder().WithLanguageCode("en").Build(),
            new LanguageBuilder().WithLanguageCode("fr").Build(),
            new LanguageBuilder().WithLanguageCode("es").Build()
        };

        var languagesServiceMock = new Mock<ILanguages>();
        languagesServiceMock.Setup(x => x.Get()).ReturnsAsync(languages);

        var vm = CreateAssetViewModelFactory.Create(languagesService: languagesServiceMock.Object);

        // Act
        await vm.OnNavigatedTo();

        // Assert
        languagesServiceMock.Verify(s => s.Get(), Times.Once);
        Assert.Equal(languages.Count, vm.SupportedLanguagesCount);
    }

    [Fact]
    public async Task CreateAsset_WithValidFormData_CreatesAssetAndTranslationSuccessfully()
    {
        // Arrange
        var assetsService = new Mock<IAssets>();

        var createdAssetId = Guid.Empty;
        assetsService
            .Setup(s => s.Create(It.IsAny<Asset>()))
            .Callback<Asset>(a => createdAssetId = a.Id);


        var translationService = new Mock<ITranslations>();

        var vm = CreateAssetViewModelFactory.Create(
            assetsService: assetsService.Object,
            translationsService: translationService.Object);

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        // Act
        await vm.CreateAsset();

        // Assert
        assetsService.Verify(s => s.Create(
                It.Is<Asset>(a =>
                    a.InternalName == testInternalName &&
                    a.AssetType == testAssetType &&
                    a.ContextNotes == testContextNotes)
            ),
            Times.Once);

        translationService.Verify(s => s.Create(
                It.Is<Translation>(t =>
                    t.AssetId == createdAssetId &&
                    t.Language == "en" &&
                    t.Content == testEnglishContent)
            ),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsset_WithValidFormData_CreatesTagsSuccessfully()
    {
        // Arrange
        var tags = new List<Tag>
        {
            new TagBuilder().Build(),
            new TagBuilder().Build(),
            new TagBuilder().Build()
        };

        var tagsService = new Mock<ITags>();

        var assetsService = new Mock<IAssets>();

        var createdAssetId = Guid.Empty;
        assetsService
            .Setup(s => s.Create(It.IsAny<Asset>()))
            .Callback<Asset>(a => createdAssetId = a.Id);

        var vm = CreateAssetViewModelFactory.Create(
            tagsService: tagsService.Object,
            assetsService: assetsService.Object);

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        vm.Tags = new ObservableCollection<Tag>(tags);

        // Act
        await vm.CreateAsset();

        // Assert
        foreach (var tag in tags)
            tagsService.Verify(s => s.AddTagToResource(
                    tag,
                    createdAssetId,
                    ResourceType.Asset
                ),
                Times.Once);
    }

    [Fact]
    public async Task CreateAsset_WithValidFormData_CreatesAssetLinksSuccessfully()
    {
        // Arrange
        var linkedAssets = new List<AssetDto>
        {
            new AssetDtoBuilder().Build(),
            new AssetDtoBuilder().Build(),
            new AssetDtoBuilder().Build()
        };

        var assetsService = new Mock<IAssets>();
        var assetLinksService = new Mock<IAssetLinks>();

        var createdAssetId = Guid.Empty;
        assetsService
            .Setup(s => s.Create(It.IsAny<Asset>()))
            .Callback<Asset>(a => createdAssetId = a.Id);

        var vm = CreateAssetViewModelFactory.Create(
            assetsService: assetsService.Object,
            assetLinksService: assetLinksService.Object);

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        vm.AssetsToLink = new ObservableCollection<AssetDto>(linkedAssets);

        // Act
        await vm.CreateAsset();

        // Assert
        foreach (var asset in linkedAssets)
            assetLinksService.Verify(s => s.Create(
                    asset.Id,
                    createdAssetId
                ),
                Times.Once);
    }

    [Fact]
    public async Task CreateAsset_WithValidFormData_EmitsSuccessNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();

        var vm = CreateAssetViewModelFactory.Create(
            notificationService: notificationService.Object);

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        // Act
        await vm.CreateAsset();

        // Assert
        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n =>
                    n.Type == NotificationType.Success)
            ),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsset_WithValidFormData_ClearsForm()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        // Act
        await vm.CreateAsset();

        // Assert
        Assert.Equal(string.Empty, vm.InternalName);
        Assert.Equal(default, vm.SelectedAssetType);
        Assert.Equal(string.Empty, vm.ContextNotes);
        Assert.Equal(string.Empty, vm.EnglishContent);
        Assert.Empty(vm.AssetsToLink);
        Assert.Empty(vm.Tags);
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public async Task CreateAsset_WithInValidFormData_EmitsValidationNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();

        var vm = CreateAssetViewModelFactory.Create(
            notificationService: notificationService.Object);

        // Act
        await vm.CreateAsset();

        // Assert
        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n =>
                    n.Type == NotificationType.Failure &&
                    n.Title == "Validation error")
            ),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsset_ServiceThrows_EmitsErrorNotification()
    {
        // Arrange
        var notificationService = new Mock<INotificationService>();
        var assetsService = new Mock<IAssets>();
        assetsService.Setup(s => s.Create(It.IsAny<Asset>())).ThrowsAsync(new Exception("Test exception"));

        var vm = CreateAssetViewModelFactory.Create(
            assetsService: assetsService.Object,
            notificationService: notificationService.Object);


        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        // Act
        await vm.CreateAsset();

        // Assert
        notificationService.Verify(x => x.EmitNotification(
                It.Is<Notification>(n =>
                    n.Type == NotificationType.Failure &&
                    n.Title == "Error creating asset")
            ),
            Times.Once);
    }

    [Fact]
    public async Task LinkToAsset_WhenCalled_AddsAssetToAssetToLink()
    {
        // Arrange
        var assetToLink = new AssetDtoBuilder().Build();

        var vm = CreateAssetViewModelFactory.Create();

        // Act
        await vm.LinkToAsset(assetToLink);

        // Assert
        Assert.Contains(assetToLink, vm.AssetsToLink);
    }

    [Fact]
    public void SettingInternalName_WithValidInternalName_SetsValueAndClearsErrors()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testInternalName = "Test Internal Name";

        // Act
        vm.InternalName = testInternalName;

        // Assert
        Assert.Equal(testInternalName, vm.InternalName);
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public void SettingInternalName_WithBlank_SetsValueAndAddsError()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testInternalName = "";

        // Act
        vm.InternalName = testInternalName;

        // Assert
        Assert.Equal(testInternalName, vm.InternalName);
        Assert.True(vm.HasErrors);
        Assert.Contains(
            "Asset Name is required.",
            vm.GetErrors(
                nameof(vm.InternalName)
            ).Cast<string>()
        );
    }

    [Fact]
    public void SettingInternalName_WithWhitespace_SetsValueAndAddsError()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testInternalName = "     ";

        // Act
        vm.InternalName = testInternalName;

        // Assert
        Assert.Equal(testInternalName, vm.InternalName);
        Assert.True(vm.HasErrors);
        Assert.Contains(
            "Asset Name is required.",
            vm.GetErrors(
                nameof(vm.InternalName)
            ).Cast<string>()
        );
    }

    [Fact]
    public void SettingEnglishContent_WithValidContent_SetsValueAndClearsErrors()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testEnglishContent = "Test Content";

        // Act
        vm.EnglishContent = testEnglishContent;

        // Assert
        Assert.Equal(testEnglishContent, vm.EnglishContent);
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public void SettingEnglishContent_WithBlank_SetsValueAndAddsError()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testEnglishContent = "";

        // Act
        vm.EnglishContent = testEnglishContent;

        // Assert
        Assert.Equal(testEnglishContent, vm.EnglishContent);
        Assert.True(vm.HasErrors);
        Assert.Contains(
            "Content is required.",
            vm.GetErrors(
                nameof(vm.EnglishContent)
            ).Cast<string>()
        );
    }

    [Fact]
    public void SettingEnglishContent_WithWhitespace_SetsValueAndAddsError()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testEnglishContent = "      ";

        // Act
        vm.EnglishContent = testEnglishContent;

        // Assert
        Assert.Equal(testEnglishContent, vm.EnglishContent);
        Assert.True(vm.HasErrors);
        Assert.Contains(
            "Content is required.",
            vm.GetErrors(
                nameof(vm.EnglishContent)
            ).Cast<string>()
        );
    }

    [Fact]
    public void SettingCreatingAsset_WithTrue_SetsValueAndRaisesCommandCanExecuteChanged()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var canExecuteChangedRaised = false;
        vm.CreateAssetCommand.CanExecuteChanged += (_, _) => canExecuteChangedRaised = true;

        // Act
        vm.CreatingAsset = true;

        // Assert
        Assert.True(vm.CreatingAsset);
        Assert.True(canExecuteChangedRaised);
    }

    [Fact]
    public void SettingCreatingAsset_WithFalse_SetsValueAndRaisesCommandCanExecuteChanged()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var canExecuteChangedRaised = false;
        vm.CreateAssetCommand.CanExecuteChanged += (_, _) => canExecuteChangedRaised = true;

        // Act
        vm.CreatingAsset = false;

        // Assert
        Assert.False(vm.CreatingAsset);
        Assert.True(canExecuteChangedRaised);
    }

    [Fact]
    public async Task CreateAssetCommand_Execute_CallsCreateAsset()
    {
        // Arrange
        var assetsService = new Mock<IAssets>();

        var vm = CreateAssetViewModelFactory.Create(assetsService: assetsService.Object);

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        // Act
        vm.CreateAssetCommand.Execute(null);

        // Assert
        await ((AsyncRelayCommand)vm.CreateAssetCommand).CommandTask!;
        assetsService.Verify(x => x.Create(It.IsAny<Asset>()), Times.Once);
    }

    [Fact]
    public void ClearFormCommand_Execute_ClearsForm()
    {
        // Arrange
        var vm = CreateAssetViewModelFactory.Create();

        var testInternalName = "TestAsset";
        var testAssetType = AssetType.Text;
        var testContextNotes = "Test context notes";
        var testEnglishContent = "Some test content";

        vm.InternalName = testInternalName;
        vm.SelectedAssetType = testAssetType;
        vm.ContextNotes = testContextNotes;
        vm.EnglishContent = testEnglishContent;

        // Act
        vm.ClearFormCommand.Execute(null);

        // Assert
        Assert.Equal(string.Empty, vm.InternalName);
        Assert.Equal(default, vm.SelectedAssetType);
        Assert.Equal(string.Empty, vm.ContextNotes);
        Assert.Equal(string.Empty, vm.EnglishContent);
        Assert.Empty(vm.AssetsToLink);
        Assert.Empty(vm.Tags);
        Assert.False(vm.HasErrors);
    }

    [Fact]
    public void RemoveAssetLinkCommand_Execute_RemovesAssetFromAssetsToLink()
    {
        // Arrange
        var assetToLink = new AssetDtoBuilder().Build();

        var vm = CreateAssetViewModelFactory.Create();
        vm.AssetsToLink.Add(assetToLink);

        // Act
        vm.RemoveAssetLinkCommand.Execute(assetToLink);

        // Assert
        Assert.DoesNotContain(assetToLink, vm.AssetsToLink);
    }

    [Fact]
    public void OpenLinkAssetDialogCommand_Execute_OpensDialog()
    {
        // Arrange
        var dialogService = new Mock<IDialogService>();

        var vm = CreateAssetViewModelFactory.Create(dialogService: dialogService.Object);

        // Act
        vm.OpenLinkAssetDialogCommand.Execute(null);

        // Assert
        dialogService.Verify(x =>
                x.ShowDialog<LinkAssetsDialogViewModel>(
                    It.IsAny<Func<AssetDto, Task>>()
                ),
            Times.Once);
    }
}