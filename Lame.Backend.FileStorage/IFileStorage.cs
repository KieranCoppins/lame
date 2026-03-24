namespace Lame.Backend.FileStorage;

public interface IFileStorage
{
    Task<string> Save(byte[] data, string fileName);
    Task<byte[]> Get(string fileName);
}