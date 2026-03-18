namespace Lame.Backend.FileStorage.Local;

public class LocalFileStorage : IFileStorage
{
    private readonly string _baseDirectory;

    public LocalFileStorage()
    {
        var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _baseDirectory = Path.Combine(documentsFolder, "LAME\\Files");
        Directory.CreateDirectory(_baseDirectory);
    }

    public async Task<string> Save(byte[] data, string fileName)
    {
        var filePath = Path.Combine(_baseDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, data);

        return filePath;
    }

    public Task<byte[]> Get(string path)
    {
        return File.ReadAllBytesAsync(path);
    }
}