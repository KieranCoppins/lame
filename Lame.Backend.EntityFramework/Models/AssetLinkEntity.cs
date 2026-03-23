using Lame.DomainModel;

namespace Lame.Backend.EntityFramework.Models;

public class AssetLinkEntity : AssetLink
{
    public AssetEntity AssetEntity { get; set; }
    public AssetEntity LinkedAssetEntity { get; set; }
}