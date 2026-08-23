// Services/AppDataService.cs
using System;
using System.IO;
using System.Text.Json;
using Libris.Models;

namespace Libris.Services;

public sealed class AppDataService
{
    private readonly string _dataDirectory;
    private readonly string _dataFile;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppDataService()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Libris");

        _dataFile = Path.Combine(
            _dataDirectory,
            "data.json");
    }

    public AppData Load()
    {
        try
        {
            if (!File.Exists(_dataFile))
                return new AppData();

            var json = File.ReadAllText(_dataFile);

            return JsonSerializer.Deserialize<AppData>(
                       json,
                       JsonOptions)
                   ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public void Save(AppData data)
    {
        try
        {
            Directory.CreateDirectory(_dataDirectory);

            var json = JsonSerializer.Serialize(
                data,
                JsonOptions);

            File.WriteAllText(_dataFile, json);
        }
        catch
        {
            // Application data should never be able to crash the application.
        }
    }
}