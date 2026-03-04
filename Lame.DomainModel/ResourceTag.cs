namespace Lame.DomainModel;

public class ResourceTag
{
    public Guid TagId { get; set; }
    public Guid ResourceId { get; set; }
    public ResourceType ResourceType { get; set; }
}