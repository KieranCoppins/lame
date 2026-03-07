using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

public class TagViewModel
{
    public TagViewModel(Tag tag)
    {
        Tag = tag;
    }

    public Tag Tag { get; }

    public string Name => Tag.Name;

    public override string ToString()
    {
        return Name;
    }
}