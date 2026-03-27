using System.IO;
using System.Threading.Tasks;
using Lame.Backend.FileStorage;
using Lame.Backend.Translations;
using Lame.DomainModel;
using Lame.Frontend.Services;

namespace Lame.Frontend.Helpers;

public static class TranslationHelpers
{
    public static async Task CreateTranslation(
        ITranslations translationService,
        IFileStorage fileStorageService,
        ISystemIO systemIO,
        AssetType owningAssetType,
        Translation translation)
    {
        if (owningAssetType == AssetType.Audio)
        {
            // For audio assets we need to save the file and set the content to the file path
            var data = await systemIO.ReadAllBytesAsync(translation.Content);

            var fileExtension = Path.GetExtension(translation.Content);
            var fileName = $"{translation.Id}{fileExtension}";

            await fileStorageService.Save(data, fileName);
            translation.Content = fileName;
        }

        await translationService.Create(translation);
    }
}