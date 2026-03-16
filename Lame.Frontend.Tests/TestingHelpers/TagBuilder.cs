using Lame.DomainModel;

namespace Lame.Frontend.Tests.TestingHelpers;

public class TagBuilder
{
    private readonly Tag _tag;

    public TagBuilder()
    {
        _tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "TestTag"
        };
    }

    public Tag Build()
    {
        return _tag;
    }

    public TagBuilder WithId(Guid id)
    {
        _tag.Id = id;
        return this;
    }

    public TagBuilder WithName(string name)
    {
        _tag.Name = name;
        return this;
    }
}