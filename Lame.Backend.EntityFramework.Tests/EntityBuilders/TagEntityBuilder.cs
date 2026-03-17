using Lame.Backend.EntityFramework.Models;

namespace Lame.Backend.EntityFramework.Tests.EntityBuilders;

public class TagEntityBuilder
{
    private readonly TagEntity _tag;

    public TagEntityBuilder()
    {
        _tag = new TagEntity
        {
            Id = Guid.NewGuid(),
            Name = "Tag1",
            Translations = [],
            Assets = []
        };
    }

    public TagEntity Build()
    {
        return _tag;
    }

    public TagEntityBuilder WithId(Guid id)
    {
        _tag.Id = id;
        return this;
    }

    public TagEntityBuilder WithName(string name)
    {
        _tag.Name = name;
        return this;
    }

    public TagEntityBuilder AddTranslation(TranslationEntity translation)
    {
        _tag.Translations.Add(translation);
        return this;
    }

    public TagEntityBuilder AddAsset(AssetEntity asset)
    {
        _tag.Assets.Add(asset);
        return this;
    }
}