using Lame.DomainModel;

namespace Lame.Frontend.Models;

public class PopulatedAssetLink : AssetLink
{
    public AssetDto Asset { get; set; }
    public AssetDto LinkedAsset { get; set; }
}