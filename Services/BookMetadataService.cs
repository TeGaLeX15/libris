// Services/BookMetadataService.cs
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using VersOne.Epub;

namespace Libris.Services;

public sealed class BookMetadataService
{
    public async Task<BookMetadata> ReadAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return CreateFallbackMetadata(filePath);

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
        catch
        {
            // Metadata extraction should never prevent a book from being imported.
            return CreateFallbackMetadata(filePath);
        }
    }

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

    private static byte[]? ExtractEpubCover(EpubBook epubBook)
    {
        try
        {
            return epubBook.CoverImage;
        }
        catch
        {
            return null;
        }
    }

    private static BookMetadata ReadFb2(string filePath)
    {
        var document = XDocument.Load(filePath);

        XNamespace fictionBook =
            "http://www.gribuser.ru/xml/fictionbook/2.0";

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

            CoverBytes = ExtractFb2Cover(document, fictionBook)
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
                    "http://www.w3.org/1999/xlink"));

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
        catch
        {
            return null;
        }
    }

    private static BookMetadata ReadPdfFallback(string filePath)
    {
        return CreateFallbackMetadata(filePath);
    }

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
