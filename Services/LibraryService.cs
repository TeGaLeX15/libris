// Services/LibraryService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

using Libris.Models;

namespace Libris.Services;

/// <summary>
/// Отвечает за хранение библиотеки книг, импорт и удаление книг,
/// а также сохранение связанных с книгами файлов и обложек.
/// </summary>
public sealed class LibraryService
{
    private const string LibraryDirectoryName = "Libris";
    private const string BooksFolderName = "Books";
    private const string CoversFolderName = "Covers";
    private const string LibraryFileName = "library.json";

    private readonly string _libraryDirectory;
    private readonly string _booksDirectory;
    private readonly string _coversDirectory;
    private readonly string _libraryFile;

    private readonly BookMetadataService _metadataService;

    /// <summary>
    /// Настройки сериализации библиотеки.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Инициализирует сервис библиотеки и определяет
    /// расположение файлов приложения.
    /// </summary>
    public LibraryService()
    {
        _libraryDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            LibraryDirectoryName);

        _booksDirectory = Path.Combine(
            _libraryDirectory,
            BooksFolderName);

        _coversDirectory = Path.Combine(
            _libraryDirectory,
            CoversFolderName);

        _libraryFile = Path.Combine(
            _libraryDirectory,
            LibraryFileName);

        _metadataService = new BookMetadataService();
    }

    /// <summary>
    /// Загружает все книги из локального хранилища библиотеки.
    /// </summary>
    /// <returns>
    /// Список сохранённых книг или пустой список,
    /// если библиотека отсутствует или её невозможно прочитать.
    /// </returns>
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
        catch (JsonException)
        {
            // Повреждённый JSON не должен приводить к падению приложения.
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
    /// Сохраняет переданный список книг в локальное хранилище.
    /// </summary>
    /// <param name="books">Книги для сохранения.</param>
    public void Save(IEnumerable<Book> books)
    {
        ArgumentNullException.ThrowIfNull(books);

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
        catch (IOException)
        {
            // Ошибка записи библиотеки не должна приводить к падению приложения.
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно приводить к падению приложения.
        }
    }

    /// <summary>
    /// Импортирует книгу в библиотеку.
    /// Создаёт управляемую копию файла, извлекает метаданные
    /// и сохраняет обложку, если она доступна.
    /// </summary>
    /// <param name="sourceFilePath">
    /// Путь к исходному файлу книги.
    /// </param>
    /// <returns>
    /// Импортированная книга или <see langword="null"/>,
    /// если файл невозможно импортировать или такая книга уже существует.
    /// </returns>
    public async Task<Book?> ImportAsync(string sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            return null;

        if (!File.Exists(sourceFilePath))
            return null;

        try
        {
            var fileHash = CalculateFileHash(sourceFilePath);
            var books = Load();

            // Не добавляем книгу, если файл с таким же содержимым
            // уже присутствует в библиотеке.
            var existingBook = books.FirstOrDefault(book =>
                !string.IsNullOrWhiteSpace(book.FileHash) &&
                string.Equals(
                    book.FileHash,
                    fileHash,
                    StringComparison.OrdinalIgnoreCase));

            if (existingBook is not null)
                return null;

            var metadata = await _metadataService.ReadAsync(
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

            var updatedBooks = books.ToList();
            updatedBooks.Add(book);

            Save(updatedBooks);

            return book;
        }
        catch (IOException)
        {
            // Ошибки работы с файлами не должны приводить к падению приложения.
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлам не должно приводить к падению приложения.
            return null;
        }
        catch (JsonException)
        {
            // Ошибки чтения или сериализации данных не должны приводить
            // к падению приложения.
            return null;
        }
    }

    /// <summary>
    /// Удаляет книгу из библиотеки и удаляет её управляемую копию
    /// и сохранённую обложку.
    /// </summary>
    /// <param name="id">Идентификатор удаляемой книги.</param>
    public void Remove(Guid id)
    {
        var books = Load().ToList();

        var book = books.FirstOrDefault(
            x => x.Id == id);

        if (book is null)
            return;

        try
        {
            DeleteFileIfExists(book.FilePath);
            DeleteFileIfExists(book.CoverPath);
        }
        catch (IOException)
        {
            // Ошибка удаления связанных файлов не должна приводить
            // к падению приложения.
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлам не должно приводить
            // к падению приложения.
        }

        books.Remove(book);
        Save(books);
    }

    /// <summary>
    /// Сохраняет обложку книги в локальное хранилище.
    /// </summary>
    /// <param name="bookId">Идентификатор книги.</param>
    /// <param name="coverBytes">Данные изображения обложки.</param>
    /// <returns>Полный путь к сохранённому файлу обложки.</returns>
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

    /// <summary>
    /// Удаляет файл, если путь указан и файл существует.
    /// </summary>
    /// <param name="filePath">Путь к удаляемому файлу.</param>
    private static void DeleteFileIfExists(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        if (!File.Exists(filePath))
            return;

        File.Delete(filePath);
    }

    /// <summary>
    /// Вычисляет SHA-256 хеш содержимого файла.
    /// Используется для обнаружения дубликатов книг.
    /// </summary>
    /// <param name="filePath">Путь к файлу.</param>
    /// <returns>SHA-256 хеш в виде шестнадцатеричной строки.</returns>
    private static string CalculateFileHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();

        var hash = sha256.ComputeHash(stream);

        return Convert.ToHexString(hash);
    }
}