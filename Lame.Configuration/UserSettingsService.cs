using System.IO;
using System.Text.Json;
using Lame.Frontend.Models;

namespace Lame.Frontend.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly string _filePath;

    public UserSettingsService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LAME");

        Directory.CreateDirectory(folder);

        _filePath = Path.Combine(folder, "usersettings.json");

        UserSettings = Load();
    }

    public UserSettings UserSettings { get; }

    public void Save()
    {
        SaveToFile(UserSettings);
    }

    private UserSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            var defaults = CreateDefault();
            SaveToFile(defaults);
            return defaults;
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<UserSettings>(json)
               ?? CreateDefault();
    }

    private UserSettings CreateDefault()
    {
        var documents = Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments);

        return new UserSettings
        {
            BaseDirectory = Path.Combine(documents, "LAME")
        };
    }

    private void SaveToFile(UserSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(_filePath, json);
    }
}