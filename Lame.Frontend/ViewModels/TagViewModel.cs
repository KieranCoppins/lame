using Lame.DomainModel;

namespace Lame.Frontend.ViewModels;

public class TagViewModel
{
    private Tag Tag { get; }
    
    public string Name => Tag.Name;
    
    public TagViewModel(Tag tag)
    {
        Tag = tag;
    }
}