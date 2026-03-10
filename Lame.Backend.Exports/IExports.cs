using Lame.DomainModel;

namespace Lame.Backend.Exports;

public interface IExports
{
    public Task<byte[]> Export(ExportOptions exportOptions);
}