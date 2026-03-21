namespace Lame.DomainModel;

public class AssetLink
{
    public Guid AssetEntityId { get; set; }
    public Guid LinkedContentId { get; set; }

    /// <summary>
    ///     True if the asset and the linked asset has been marked as being synced. This is set to false when one of the assets
    ///     receives a major change in the english translation.
    ///     Synced assets do not require to be on the same version. They are marked as synced by the user
    /// </summary>
    public bool Synced { get; set; }
}