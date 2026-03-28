using System.Text.Json;
using Lame.Backend.EntityFramework;
using Lame.Frontend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lame.Frontend.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly string _filePath;
    private readonly IServiceProvider _serviceProvider;

    public UserSettingsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
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

    public string SetBaseDirectory(string newDirectory)
    {
        var lastSegment =
            Path.GetFileName(Path.GetFullPath(newDirectory.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)));

        var dir = lastSegment.Equals("LAME", StringComparison.OrdinalIgnoreCase)
            ? newDirectory
            : Path.Combine(newDirectory, "LAME");


        // Ensure that the directory exists
        Directory.CreateDirectory(dir);

        UserSettings.BaseDirectory = dir;
        Save();

        // Apply database migration as the database might be out of date
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        return dir;
    }

    private UserSettings Load()
    {
        if (!File.Exists(_filePath))
        {
            var defaults = CreateDefault();
            SaveToFile(defaults);
            Directory.CreateDirectory(defaults.BaseDirectory);
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