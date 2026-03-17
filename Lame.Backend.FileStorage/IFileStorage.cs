namespace Lame.Backend.FileStorage;

public interface IFileStorage
{
    Task<string> Save(Stream stream, string fileName);
    Task<Stream> Get(string path);
}