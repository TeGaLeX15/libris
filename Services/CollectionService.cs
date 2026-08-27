// Services/CollectionService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Libris.Models;

namespace Libris.Services;

/// <summary>
/// Отвечает за создание, загрузку, изменение и удаление
/// пользовательских коллекций книг.
/// </summary>
public sealed class CollectionService
{
    private const string LibraryDirectoryName = "Libris";
    private const string CollectionsFileName = "collections.json";

    private readonly string _libraryDirectory;
    private readonly string _collectionsFile;

    /// <summary>
    /// Настройки сериализации коллекций.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Инициализирует сервис хранения коллекций.
    /// </summary>
    public CollectionService()
    {
        _libraryDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            LibraryDirectoryName);

        _collectionsFile = Path.Combine(
            _libraryDirectory,
            CollectionsFileName);
    }

    /// <summary>
    /// Загружает все сохранённые коллекции.
    /// </summary>
    /// <returns>
    /// Список коллекций или пустой список,
    /// если файл отсутствует или его невозможно прочитать.
    /// </returns>
    public IReadOnlyList<BookCollection> Load()
    {
        try
        {
            if (!File.Exists(_collectionsFile))
                return [];

            var json = File.ReadAllText(_collectionsFile);

            return JsonSerializer.Deserialize<List<BookCollection>>(
                       json,
                       JsonOptions)
                   ?? [];
        }
        catch (JsonException)
        {
            // Повреждённые данные коллекций не должны приводить к падению приложения.
            return [];
        }
        catch (IOException)
        {
            // Ошибка чтения файла не должна приводить к падению приложения.
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
            return [];
        }
    }

    /// <summary>
    /// Сохраняет коллекции в локальный JSON-файл.
    /// </summary>
    /// <param name="collections">Коллекции для сохранения.</param>
    public void Save(IEnumerable<BookCollection> collections)
    {
        try
        {
            Directory.CreateDirectory(_libraryDirectory);

            var json = JsonSerializer.Serialize(
                collections,
                JsonOptions);

            File.WriteAllText(
                _collectionsFile,
                json);
        }
        catch (IOException)
        {
            // Ошибка записи коллекций не должна приводить к падению приложения.
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
        }
    }

    /// <summary>
    /// Создаёт новую коллекцию.
    /// </summary>
    /// <param name="name">Название новой коллекции.</param>
    /// <returns>
    /// Созданная коллекция или <see langword="null"/>,
    /// если название пустое.
    /// </returns>
    public BookCollection? Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var collections = Load().ToList();

        var collection = new BookCollection
        {
            Name = name.Trim()
        };

        collections.Add(collection);
        Save(collections);

        return collection;
    }

    /// <summary>
    /// Обновляет существующую коллекцию.
    /// </summary>
    /// <param name="collection">Коллекция с обновлёнными данными.</param>
    public void Update(BookCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        var collections = Load().ToList();

        var existing = collections.FirstOrDefault(
            x => x.Id == collection.Id);

        if (existing is null)
            return;

        existing.Name = collection.Name.Trim();

        existing.BookIds = collection.BookIds
            .Distinct()
            .ToList();

        Save(collections);
    }

    /// <summary>
    /// Удаляет коллекцию по её идентификатору.
    /// Удаление коллекции не удаляет сами книги из библиотеки.
    /// </summary>
    /// <param name="collectionId">Идентификатор удаляемой коллекции.</param>
    public void Delete(Guid collectionId)
    {
        var collections = Load().ToList();

        var collection = collections.FirstOrDefault(
            x => x.Id == collectionId);

        if (collection is null)
            return;

        collections.Remove(collection);
        Save(collections);
    }

    /// <summary>
    /// Добавляет книгу в указанную коллекцию.
    /// Если книга уже находится в коллекции, операция игнорируется.
    /// </summary>
    /// <param name="collectionId">Идентификатор коллекции.</param>
    /// <param name="bookId">Идентификатор добавляемой книги.</param>
    public void AddBook(Guid collectionId, Guid bookId)
    {
        var collections = Load().ToList();

        var collection = collections.FirstOrDefault(
            x => x.Id == collectionId);

        if (collection is null)
            return;

        if (collection.BookIds.Contains(bookId))
            return;

        collection.BookIds.Add(bookId);

        Save(collections);
    }

    /// <summary>
    /// Удаляет книгу из указанной коллекции.
    /// </summary>
    /// <param name="collectionId">Идентификатор коллекции.</param>
    /// <param name="bookId">Идентификатор удаляемой книги.</param>
    public void RemoveBook(Guid collectionId, Guid bookId)
    {
        var collections = Load().ToList();

        var collection = collections.FirstOrDefault(
            x => x.Id == collectionId);

        if (collection is null)
            return;

        collection.BookIds.Remove(bookId);

        Save(collections);
    }
}