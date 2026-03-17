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

    public async Task<string> Save(Stream stream, string fileName)
    {
        var filePath = Path.Combine(_baseDirectory, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        await stream.CopyToAsync(fileStream);

        return filePath;
    }

    public Task<Stream> Get(string path)
    {
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }
}