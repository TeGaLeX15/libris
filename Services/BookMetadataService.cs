// Services/BookMetadataService.cs
using System;
using System.Xml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using VersOne.Epub;

namespace Libris.Services;

/// <summary>
/// Отвечает за извлечение метаданных и обложек из файлов книг.
/// </summary>
public sealed class BookMetadataService
{
    private const string FictionBookNamespace =
        "http://www.gribuser.ru/xml/fictionbook/2.0";

    private const string XLinkNamespace =
        "http://www.w3.org/1999/xlink";

    /// <summary>
    /// Читает метаданные книги.
    /// </summary>
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
            return CreateFallbackMetadata(filePath);
        }
        catch (UnauthorizedAccessException)
        {
            return CreateFallbackMetadata(filePath);
        }
        catch (InvalidOperationException)
        {
            return CreateFallbackMetadata(filePath);
        }
        catch (FormatException)
        {
            return CreateFallbackMetadata(filePath);
        }
        catch (XmlException)
        {
            return CreateFallbackMetadata(filePath);
        }
    }

    private static async Task<BookMetadata> ReadEpubAsync(
        string filePath)
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

    private static BookMetadata ReadFb2(string filePath)
    {
        var document = XDocument.Load(filePath);

        var fictionBook = XNamespace.Get(
            FictionBookNamespace);

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

        var author = string.Join(
            " ",
            new[]
            {
                authorElement?
                    .Element(fictionBook + "first-name")?
                    .Value
                    .Trim(),

                authorElement?
                    .Element(fictionBook + "middle-name")?
                    .Value
                    .Trim(),

                authorElement?
                    .Element(fictionBook + "last-name")?
                    .Value
                    .Trim()
            }.Where(value =>
                !string.IsNullOrWhiteSpace(value)));

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

            var base64 = binary.Value.Trim();

            if (string.IsNullOrWhiteSpace(base64))
                return null;

            return Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static BookMetadata ReadPdfFallback(
        string filePath)
    {
        return CreateFallbackMetadata(filePath);
    }

    private static BookMetadata CreateFallbackMetadata(
        string? filePath)
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
    public string Title { get; init; } = string.Empty;

    public string? Author { get; init; }

    public string? Description { get; init; }

    public string? Publisher { get; init; }

    public string? Language { get; init; }

    public string? Isbn { get; init; }

    public DateTime? PublishedAt { get; init; }

    public string? CoverPath { get; init; }

    public byte[]? CoverBytes { get; init; }
}