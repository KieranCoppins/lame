using Lame.Frontend.Services;

namespace Lame.Backend.FileStorage.Local;

public class LocalFileStorage : IFileStorage
{
    private readonly string _baseDirectory;
    private readonly IUserSettingsService _userSettingsService;

    public LocalFileStorage(IUserSettingsService userSettingsService)
    {
        _userSettingsService = userSettingsService;

        _baseDirectory = Path.Combine(_userSettingsService.UserSettings.BaseDirectory, "Files");
    }

    public async Task<string> Save(byte[] data, string fileName)
    {
        // Ensure that the base directory exists whenever we go to write in case settings have changed
        Directory.CreateDirectory(_baseDirectory);

        var filePath = Path.Combine(_baseDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, data);

        return filePath;
    }

    public Task<byte[]> Get(string fileName)
    {
        var fullPath = Path.Combine(_baseDirectory, fileName);
        return File.ReadAllBytesAsync(fullPath);
    }
}