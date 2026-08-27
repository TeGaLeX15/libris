// Services/AppDataService.cs
using System;
using System.IO;
using System.Text.Json;

using Libris.Models;

namespace Libris.Services;

/// <summary>
/// Отвечает за сохранение и загрузку данных приложения Libris.
/// </summary>
public sealed class AppDataService
{
    private readonly string _dataDirectory;
    private readonly string _dataFile;

    /// <summary>
    /// Настройки сериализации данных приложения.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Инициализирует сервис хранения данных приложения.
    /// </summary>
    public AppDataService()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "Libris");

        _dataFile = Path.Combine(
            _dataDirectory,
            "data.json");
    }

    /// <summary>
    /// Загружает данные приложения из локального файла.
    /// </summary>
    /// <returns>
    /// Загруженные данные приложения или новый объект <see cref="AppData"/>,
    /// если файл отсутствует или его содержимое невозможно прочитать.
    /// </returns>
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
        catch (JsonException)
        {
            // Повреждённый JSON не должен приводить к падению приложения.
            return new AppData();
        }
        catch (IOException)
        {
            // Ошибки чтения файла не должны приводить к падению приложения.
            return new AppData();
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
            return new AppData();
        }
    }

    /// <summary>
    /// Сохраняет данные приложения в локальный JSON-файл.
    /// </summary>
    /// <param name="data">Данные приложения для сохранения.</param>
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
        catch (IOException)
        {
            // Ошибка записи данных не должна приводить к падению приложения.
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
        }
    }
}