using Lame.Backend.FileStorage;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Helpers;
using Lame.Frontend.Services;
using Lame.TestingHelpers;
using Moq;

namespace Lame.Frontend.Tests.HelpersTests;

public class TranslationHelpersTests
{
    [Fact]
    public async Task CreateTranslation_WhenAssetTypeIsAudio_SavesFileAndSetsContentAndCallsCreate()
    {
        // Arrange
        var translationService = new Mock<ITranslations>();
        var fileStorageService = new Mock<IFileStorage>();
        var systemIO = new Mock<ISystemIO>();
        var translation = new TranslationDtoBuilder()
            .WithContent("audio.mp3")
            .WithId(Guid.NewGuid())
            .Build();

        var fileData = new byte[] { 1, 2, 3 };
        systemIO.Setup(x => x.ReadAllBytesAsync("audio.mp3")).ReturnsAsync(fileData);
        fileStorageService.Setup(x => x.Save(fileData, $"{translation.Id}.mp3")).ReturnsAsync("stored_path.mp3");

        // Act
        await TranslationHelpers.CreateTranslation(
            translationService.Object,
            fileStorageService.Object,
            systemIO.Object,
            AssetType.Audio,
            translation
        );

        // Assert
        Assert.Equal("stored_path.mp3", translation.Content);
        translationService.Verify(x => x.Create(translation), Times.Once);
    }

    [Fact]
    public async Task CreateTranslation_WhenAssetTypeIsNotAudio_DoesNotSaveFileAndCallsCreateWithOriginalContent()
    {
        // Arrange
        var translationService = new Mock<ITranslations>();
        var fileStorageService = new Mock<IFileStorage>();
        var systemIO = new Mock<ISystemIO>();
        var translation = new TranslationDtoBuilder()
            .WithContent("text.txt")
            .WithId(Guid.NewGuid())
            .Build();

        // Act
        await TranslationHelpers.CreateTranslation(
            translationService.Object,
            fileStorageService.Object,
            systemIO.Object,
            AssetType.Text,
            translation
        );

        // Assert
        fileStorageService.Verify(x => x.Save(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
        translationService.Verify(x => x.Create(translation), Times.Once);
        Assert.Equal("text.txt", translation.Content);
    }

    [Fact]
    public async Task CreateTranslation_WhenReadAllBytesThrows_ThrowsExceptionAndDoesNotCallCreate()
    {
        // Arrange
        var translationService = new Mock<ITranslations>();
        var fileStorageService = new Mock<IFileStorage>();
        var systemIO = new Mock<ISystemIO>();
        var translation = new TranslationDtoBuilder()
            .WithContent("audio.mp3")
            .WithId(Guid.NewGuid())
            .Build();

        systemIO.Setup(x => x.ReadAllBytesAsync("audio.mp3")).ThrowsAsync(new IOException("read error"));

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(async () =>
            await TranslationHelpers.CreateTranslation(
                translationService.Object,
                fileStorageService.Object,
                systemIO.Object,
                AssetType.Audio,
                translation
            )
        );

        translationService.Verify(x => x.Create(It.IsAny<Translation>()), Times.Never);
    }

    [Fact]
    public async Task CreateTranslation_WhenSaveThrows_ThrowsExceptionAndDoesNotCallCreate()
    {
        // Arrange
        var translationService = new Mock<ITranslations>();
        var fileStorageService = new Mock<IFileStorage>();
        var systemIO = new Mock<ISystemIO>();
        var translation = new TranslationDtoBuilder()
            .WithContent("audio.mp3")
            .WithId(Guid.NewGuid())
            .Build();

        var fileData = new byte[] { 1, 2, 3 };
        systemIO.Setup(x => x.ReadAllBytesAsync("audio.mp3")).ReturnsAsync(fileData);
        fileStorageService.Setup(x => x.Save(fileData, $"{translation.Id}.mp3"))
            .ThrowsAsync(new Exception("save error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await TranslationHelpers.CreateTranslation(
                translationService.Object,
                fileStorageService.Object,
                systemIO.Object,
                AssetType.Audio,
                translation
            )
        );

        translationService.Verify(x => x.Create(It.IsAny<Translation>()), Times.Never);
    }

    [Fact]
    public async Task CreateTranslation_WhenCreateThrows_ThrowsException()
    {
        // Arrange
        var translationService = new Mock<ITranslations>();
        var fileStorageService = new Mock<IFileStorage>();
        var systemIO = new Mock<ISystemIO>();
        var translation = new TranslationDtoBuilder()
            .WithContent("text.txt")
            .WithId(Guid.NewGuid())
            .Build();

        translationService.Setup(x => x.Create(translation)).ThrowsAsync(new Exception("create error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await TranslationHelpers.CreateTranslation(
                translationService.Object,
                fileStorageService.Object,
                systemIO.Object,
                AssetType.Text,
                translation
            )
        );
    }
}