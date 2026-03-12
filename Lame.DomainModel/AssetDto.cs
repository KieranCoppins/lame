namespace Lame.DomainModel;

public class AssetDto
{
    public required Guid Id { get; set; }
    public required string InternalName { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public required AssetType AssetType { get; set; }
    public string? ContextNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public int NumTranslations { get; set; }

    // A hack to make the asset show up in the ui as the internal name instead of the class name when binding to a list of assets
    public override string ToString()
    {
        return InternalName;
    }
}