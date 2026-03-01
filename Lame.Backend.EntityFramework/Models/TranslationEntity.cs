using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class TranslationEntity : Translation
{
    public AssetEntity Asset { get; set; }
}