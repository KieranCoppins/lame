using Lame.DomainModel;

namespace Lame.Backend.Imports;

public interface IImports
{
    public Task<int> Import(ImportOptions importOptions);
}