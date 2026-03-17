using System.IO;
using Lame.Backend.FileStorage;
using Lame.Backend.Translations;
using Lame.DomainModel;

namespace Lame.Frontend.Helpers;

public static class TranslationHelpers
{
    public static async Task CreateTranslation(
        ITranslations translationService,
        IFileStorage fileStorageService,
        AssetType owningAssetType,
        Translation translation)
    {
        if (owningAssetType == AssetType.Audio)
        {
            // For audio assets we need to save the file and set the content to the file path

            // TODO abstract this read to make this more testable
            var fileStream = File.OpenRead(translation.Content);

            var fileExtension = Path.GetExtension(translation.Content);
            var fileName = $"{translation.Id}{fileExtension}";

            translation.Content = await fileStorageService.Save(fileStream, fileName);
        }

        await translationService.Create(translation);
    }
}