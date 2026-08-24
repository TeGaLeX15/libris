// Services/CollectionService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Libris.Models;

namespace Libris.Services;

public sealed class CollectionService
{
    private const string LibraryDirectoryName = "Libris";
    private const string CollectionsFileName = "collections.json";

    private readonly string _libraryDirectory;
    private readonly string _collectionsFile;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

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
        catch
        {
            return [];
        }
    }

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
        catch
        {
            // Collection persistence should never crash the application.
        }
    }

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

    public void Update(BookCollection collection)
    {
        var collections = Load().ToList();

        var existing = collections.FirstOrDefault(
            x => x.Id == collection.Id);

        if (existing is null)
            return;

        existing.Name = collection.Name.Trim();
        existing.BookIds = collection.BookIds.Distinct().ToList();

        Save(collections);
    }

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