// Services/LibraryService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Libris.Models;

namespace Libris.Services;

public sealed class LibraryService
{
    private readonly string _libraryDirectory;
    private readonly string _libraryFile;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public LibraryService()
    {
        _libraryDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "Libris");

        _libraryFile = Path.Combine(
            _libraryDirectory,
            "library.json");
    }

    public IReadOnlyList<Book> Load()
    {
        try
        {
            if (!File.Exists(_libraryFile))
                return [];

            var json = File.ReadAllText(_libraryFile);

            return JsonSerializer.Deserialize<List<Book>>(
                       json,
                       JsonOptions)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<Book> books)
    {
        try
        {
            Directory.CreateDirectory(_libraryDirectory);

            var json = JsonSerializer.Serialize(
                books,
                JsonOptions);

            File.WriteAllText(_libraryFile, json);
        }
        catch
        {
            // Library persistence should never crash the application.
        }
    }

    public void Add(Book book)
    {
        var books = Load().ToList();

        if (books.Any(x =>
                string.Equals(
                    x.FilePath,
                    book.FilePath,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        books.Add(book);
        Save(books);
    }

    public void Remove(Guid id)
    {
        var books = Load()
            .Where(x => x.Id != id)
            .ToList();

        Save(books);
    }
}