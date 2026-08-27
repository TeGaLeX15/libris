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

    /// <summary>
    /// Настройки сериализации пользовательских настроек.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Инициализирует сервис пользовательских настроек
    /// и определяет расположение файла настроек.
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
    /// Загружает пользовательские настройки из локального JSON-файла.
    /// </summary>
    /// <returns>
    /// Загруженные настройки или настройки по умолчанию,
    /// если файл отсутствует или его невозможно прочитать.
    /// </returns>
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
        catch (JsonException)
        {
            // Повреждённый файл настроек не должен приводить к падению приложения.
            return new AppSettings();
        }
        catch (IOException)
        {
            // Ошибка чтения файла не должна приводить к падению приложения.
            return new AppSettings();
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
            return new AppSettings();
        }
    }

    /// <summary>
    /// Сохраняет пользовательские настройки в локальный JSON-файл.
    /// </summary>
    /// <param name="settings">Настройки для сохранения.</param>
    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            Directory.CreateDirectory(_settingsDirectory);

            var json = JsonSerializer.Serialize(
                settings,
                JsonOptions);

            File.WriteAllText(
                _settingsFile,
                json);
        }
        catch (IOException)
        {
            // Ошибка записи настроек не должна приводить к падению приложения.
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
        }
    }
}