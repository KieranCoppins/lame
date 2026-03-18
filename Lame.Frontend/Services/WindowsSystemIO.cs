using System.IO;
using Microsoft.Win32;

namespace Lame.Frontend.Services;

public class WindowsSystemIO : ISystemIO
{
    public Task WriteAllBytesAsync(string filePath, byte[] data)
    {
        return File.WriteAllBytesAsync(filePath, data);
    }

    public bool? OpenSaveFileDialog(SaveFileDialog dialog)
    {
        return dialog.ShowDialog();
    }

    public Task<byte[]> ReadAllBytesAsync(string filePath)
    {
        return File.ReadAllBytesAsync(filePath);
    }
}