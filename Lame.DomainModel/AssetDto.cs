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
}