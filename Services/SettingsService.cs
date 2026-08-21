// Services/SettingsService.cs
using System;
using System.IO;
using System.Text.Json;
using Libris.Models;

namespace Libris.Services;

public sealed class SettingsService
{
    private readonly string _settingsDirectory;
    private readonly string _settingsFile;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Libris");

        _settingsFile = Path.Combine(
            _settingsDirectory,
            "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFile))
                return new AppSettings();

            var json = File.ReadAllText(_settingsFile);

            return JsonSerializer.Deserialize<AppSettings>(
                       json,
                       JsonOptions)
                   ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(_settingsDirectory);

            var json = JsonSerializer.Serialize(
                settings,
                JsonOptions);

            File.WriteAllText(_settingsFile, json);
        }
        catch
        {
            // Settings should never be able to crash the application.
        }
    }
}