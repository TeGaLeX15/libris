// Services/SettingsService.cs
using System;
using System.IO;
using System.Text.Json;
using Libris.Models;

namespace Libris.Services;

/// <summary>
/// Отвечает за загрузку и сохранение пользовательских настроек Libris.
/// </summary>
public sealed class SettingsService
{
    private const string LibraryDirectoryName = "Libris";
    private const string SettingsFileName = "settings.json";

    private readonly string _settingsDirectory;
    private readonly string _settingsFile;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Инициализирует сервис пользовательских настроек.
    /// </summary>
    public SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            LibraryDirectoryName);

        _settingsFile = Path.Combine(
            _settingsDirectory,
            SettingsFileName);
    }

    /// <summary>
    /// Загружает пользовательские настройки.
    /// </summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFile))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsFile);

            return JsonSerializer.Deserialize<AppSettings>(
                       json,
                       JsonOptions)
                   ?? new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
        catch (IOException)
        {
            return new AppSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    /// <summary>
    /// Сохраняет пользовательские настройки.
    /// </summary>
    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            Directory.CreateDirectory(_settingsDirectory);

            var json = JsonSerializer.Serialize(
                settings,
                JsonOptions);

            var temporaryFile = _settingsFile + ".tmp";

            File.WriteAllText(
                temporaryFile,
                json);

            File.Move(
                temporaryFile,
                _settingsFile,
                true);
        }
        catch (IOException)
        {
            // Ошибка записи настроек не должна приводить к падению приложения.
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа не должно приводить к падению приложения.
        }
    }
}