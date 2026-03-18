using Microsoft.Win32;

namespace Lame.Frontend.Services;

public interface ISystemIO
{
    public Task WriteAllBytesAsync(string filePath, byte[] data);

    public bool? OpenSaveFileDialog(SaveFileDialog dialog);

    public Task<byte[]> ReadAllBytesAsync(string filePath);
}