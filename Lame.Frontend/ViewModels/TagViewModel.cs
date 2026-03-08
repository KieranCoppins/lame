using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

// TODO: Do without these specific viewmodels, they're just wrappers around the domain models and don't add much value.
// We should use converters instead to format domain model data
public class TagViewModel
{
    public TagViewModel(Tag tag)
    {
        Tag = tag;
    }

    public Tag Tag { get; }

    public string Name => Tag.Name;
    public Guid Id => Tag.Id;

    public override string ToString()
    {
        return Name;
    }
}