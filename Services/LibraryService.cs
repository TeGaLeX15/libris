// Services/LibraryService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.Json;
using Libris.Models;

namespace Libris.Services;

public sealed class LibraryService
{
    private const string BooksFolderName = "Books";
    private const string CoversFolderName = "Covers";
    private const string LibraryFileName = "library.json";

    private readonly string _libraryDirectory;
    private readonly string _booksDirectory;
    private readonly string _coversDirectory;
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

        _booksDirectory = Path.Combine(
            _libraryDirectory,
            BooksFolderName);

        _coversDirectory = Path.Combine(
            _libraryDirectory,
            CoversFolderName);

        _libraryFile = Path.Combine(
            _libraryDirectory,
            LibraryFileName);
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

            File.WriteAllText(
                _libraryFile,
                json);
        }
        catch
        {
            // Library persistence should never crash the application.
        }
    }

    public async Task<Book?> ImportAsync(string sourceFilePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                return null;

            if (!File.Exists(sourceFilePath))
                return null;

            var fileHash = CalculateFileHash(sourceFilePath);

            var books = Load().ToList();

            var existingBook = books.FirstOrDefault(book =>
                !string.IsNullOrWhiteSpace(book.FileHash) &&
                string.Equals(
                    book.FileHash,
                    fileHash,
                    StringComparison.OrdinalIgnoreCase));

            if (existingBook is not null)
                return null;

            var metadataService = new BookMetadataService();

            var metadata = await metadataService.ReadAsync(
                sourceFilePath);

            Directory.CreateDirectory(_booksDirectory);

            var extension = Path.GetExtension(sourceFilePath);

            var book = new Book
            {
                FileHash = fileHash,

                Title = string.IsNullOrWhiteSpace(metadata.Title)
                    ? Path.GetFileNameWithoutExtension(sourceFilePath)
                    : metadata.Title,

                Author = metadata.Author ?? string.Empty
            };

            var destinationFilePath = Path.Combine(
                _booksDirectory,
                $"{book.Id}{extension}");

            File.Copy(
                sourceFilePath,
                destinationFilePath);

            book.FilePath = destinationFilePath;

            if (metadata.CoverBytes is { Length: > 0 })
            {
                book.CoverPath = SaveCover(
                    book.Id,
                    metadata.CoverBytes);
            }

            books.Add(book);

            Save(books);

            return book;
        }
        catch
        {
            return null;
        }
    }

    public void Remove(Guid id)
    {
        var books = Load().ToList();

        var book = books.FirstOrDefault(
            x => x.Id == id);

        if (book is null)
            return;

        books.Remove(book);

        try
        {
            if (!string.IsNullOrWhiteSpace(book.FilePath) &&
                File.Exists(book.FilePath))
            {
                File.Delete(book.FilePath);
            }

            if (!string.IsNullOrWhiteSpace(book.CoverPath) &&
                File.Exists(book.CoverPath))
            {
                File.Delete(book.CoverPath);
            }
        }
        catch
        {
            // Removing a library entry should not crash the application.
        }

        Save(books);
    }

    private string SaveCover(
        Guid bookId,
        byte[] coverBytes)
    {
        Directory.CreateDirectory(_coversDirectory);

        var coverPath = Path.Combine(
            _coversDirectory,
            $"{bookId}.jpg");

        File.WriteAllBytes(
            coverPath,
            coverBytes);

        return coverPath;
    }

    private static string CalculateFileHash(
        string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();

        var hash = sha256.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }
}