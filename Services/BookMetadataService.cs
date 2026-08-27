// Services/BookMetadataService.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using VersOne.Epub;

namespace Libris.Services;

/// <summary>
/// Отвечает за извлечение метаданных и обложек из файлов книг
/// поддерживаемых форматов.
/// </summary>
public sealed class BookMetadataService
{
    private const string FictionBookNamespace =
        "http://www.gribuser.ru/xml/fictionbook/2.0";

    private const string XLinkNamespace =
        "http://www.w3.org/1999/xlink";

    /// <summary>
    /// Читает метаданные книги из указанного файла.
    /// </summary>
    /// <param name="filePath">Путь к файлу книги.</param>
    /// <returns>
    /// Извлечённые метаданные книги или резервные метаданные,
    /// если файл не существует, формат не поддерживается
    /// или чтение метаданных завершилось ошибкой.
    /// </returns>
    public async Task<BookMetadata> ReadAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) ||
            !File.Exists(filePath))
        {
            return CreateFallbackMetadata(filePath);
        }

        var extension = Path.GetExtension(filePath);

        try
        {
            return extension.ToLowerInvariant() switch
            {
                ".epub" => await ReadEpubAsync(filePath),
                ".fb2" => ReadFb2(filePath),
                ".pdf" => ReadPdfFallback(filePath),
                ".txt" => CreateFallbackMetadata(filePath),
                _ => CreateFallbackMetadata(filePath)
            };
        }
        catch (IOException)
        {
            // Ошибки чтения файла не должны препятствовать импорту книги.
            return CreateFallbackMetadata(filePath);
        }
        catch (UnauthorizedAccessException)
        {
            // Отсутствие доступа к файлу не должно препятствовать импорту книги.
            return CreateFallbackMetadata(filePath);
        }
        catch (InvalidOperationException)
        {
            // Ошибки обработки содержимого книги не должны препятствовать импорту.
            return CreateFallbackMetadata(filePath);
        }
    }

    /// <summary>
    /// Извлекает метаданные и обложку из EPUB-файла.
    /// </summary>
    /// <param name="filePath">Путь к EPUB-файлу.</param>
    /// <returns>Метаданные книги, извлечённые из EPUB.</returns>
    private static async Task<BookMetadata> ReadEpubAsync(string filePath)
    {
        var epubBook = await EpubReader.ReadBookAsync(filePath);

        var title = string.IsNullOrWhiteSpace(epubBook.Title)
            ? Path.GetFileNameWithoutExtension(filePath)
            : epubBook.Title.Trim();

        var author = epubBook.Author?.Trim();

        return new BookMetadata
        {
            Title = title,
            Author = string.IsNullOrWhiteSpace(author)
                ? null
                : author,
            CoverBytes = ExtractEpubCover(epubBook)
        };
    }

    /// <summary>
    /// Извлекает изображение обложки из EPUB-книги.
    /// </summary>
    /// <param name="epubBook">Открытая EPUB-книга.</param>
    /// <returns>Данные изображения обложки или <see langword="null"/>,
    /// если обложку получить не удалось.</returns>
    private static byte[]? ExtractEpubCover(EpubBook epubBook)
    {
        try
        {
            return epubBook.CoverImage;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// Извлекает метаданные и обложку из FB2-файла.
    /// </summary>
    /// <param name="filePath">Путь к FB2-файлу.</param>
    /// <returns>Метаданные книги, извлечённые из FB2.</returns>
    private static BookMetadata ReadFb2(string filePath)
    {
        var document = XDocument.Load(filePath);
        var fictionBook = XNamespace.Get(FictionBookNamespace);

        var description = document
            .Descendants(fictionBook + "description")
            .FirstOrDefault();

        var title = description?
            .Descendants(fictionBook + "book-title")
            .FirstOrDefault()?
            .Value
            .Trim();

        var authorElement = description?
            .Descendants(fictionBook + "author")
            .FirstOrDefault();

        var firstName = authorElement?
            .Element(fictionBook + "first-name")?
            .Value
            .Trim();

        var middleName = authorElement?
            .Element(fictionBook + "middle-name")?
            .Value
            .Trim();

        var lastName = authorElement?
            .Element(fictionBook + "last-name")?
            .Value
            .Trim();

        var author = string.Join(
            " ",
            new[]
            {
                firstName,
                middleName,
                lastName
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new BookMetadata
        {
            Title = string.IsNullOrWhiteSpace(title)
                ? Path.GetFileNameWithoutExtension(filePath)
                : title,

            Author = string.IsNullOrWhiteSpace(author)
                ? null
                : author,

            CoverBytes = ExtractFb2Cover(
                document,
                fictionBook)
        };
    }

    /// <summary>
    /// Извлекает обложку из FB2-документа.
    /// </summary>
    /// <param name="document">XML-документ FB2.</param>
    /// <param name="fictionBook">XML-пространство имён FictionBook.</param>
    /// <returns>
    /// Данные изображения обложки или <see langword="null"/>,
    /// если обложка отсутствует или не может быть извлечена.
    /// </returns>
    private static byte[]? ExtractFb2Cover(
        XDocument document,
        XNamespace fictionBook)
    {
        try
        {
            var coverImage = document
                .Descendants(fictionBook + "coverpage")
                .Descendants(fictionBook + "image")
                .FirstOrDefault();

            if (coverImage is null)
                return null;

            var href = (string?)coverImage.Attribute(
                XName.Get(
                    "href",
                    XLinkNamespace));

            if (string.IsNullOrWhiteSpace(href))
                return null;

            var binaryId = href.TrimStart('#');

            var binary = document
                .Descendants(fictionBook + "binary")
                .FirstOrDefault(element =>
                    string.Equals(
                        (string?)element.Attribute("id"),
                        binaryId,
                        StringComparison.Ordinal));

            if (binary is null)
                return null;

            var base64 = binary.Value;

            if (string.IsNullOrWhiteSpace(base64))
                return null;

            return Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            // Некорректные Base64-данные не должны препятствовать импорту книги.
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// Возвращает резервные метаданные для PDF-файла.
    /// Полноценное извлечение метаданных PDF будет реализовано отдельно.
    /// </summary>
    /// <param name="filePath">Путь к PDF-файлу.</param>
    /// <returns>Резервные метаданные книги.</returns>
    private static BookMetadata ReadPdfFallback(string filePath)
    {
        return CreateFallbackMetadata(filePath);
    }

    /// <summary>
    /// Создаёт минимальный набор метаданных на основе имени файла.
    /// Используется, когда метаданные книги недоступны.
    /// </summary>
    /// <param name="filePath">Путь к файлу книги.</param>
    /// <returns>Резервные метаданные книги.</returns>
    private static BookMetadata CreateFallbackMetadata(string filePath)
    {
        return new BookMetadata
        {
            Title = string.IsNullOrWhiteSpace(filePath)
                ? "Unknown Book"
                : Path.GetFileNameWithoutExtension(filePath)
        };
    }
}

/// <summary>
/// Содержит метаданные, извлечённые из файла книги.
/// </summary>
public sealed class BookMetadata
{
    /// <summary>
    /// Название книги.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Автор книги.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Описание или аннотация книги.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Издательство книги.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// Язык книги.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Международный стандартный номер книги (ISBN).
    /// </summary>
    public string? Isbn { get; init; }

    /// <summary>
    /// Дата публикации книги.
    /// </summary>
    public DateTime? PublishedAt { get; init; }

    /// <summary>
    /// Путь к сохранённому файлу обложки.
    /// Заполняется после сохранения изображения обложки.
    /// </summary>
    public string? CoverPath { get; init; }

    /// <summary>
    /// Данные изображения обложки в памяти.
    /// Используются для последующего сохранения обложки.
    /// </summary>
    public byte[]? CoverBytes { get; init; }
}