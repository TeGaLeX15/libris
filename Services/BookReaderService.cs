// Services/BookReaderService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VersOne.Epub;

namespace Libris.Services;

/// <summary>
/// Загружает содержимое книг и преобразует его
/// в структуру, пригодную для отображения Reader.
/// </summary>
public sealed class BookReaderService
{
    /// <summary>
    /// Загружает книгу.
    /// </summary>
    public async Task<ReaderDocument> LoadAsync(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Book file was not found.",
                filePath);
        }

        var extension = Path.GetExtension(filePath);

        return extension.ToLowerInvariant() switch
        {
            ".epub" => await LoadEpubAsync(filePath),
            ".txt" => await LoadTextAsync(filePath),
            _ => throw new NotSupportedException(
                $"Reader does not support '{extension}' yet.")
        };
    }

    private static async Task<ReaderDocument> LoadEpubAsync(
        string filePath)
    {
        var epubBook =
            await EpubReader.ReadBookAsync(filePath);

        var bookId =
            Path.GetFileNameWithoutExtension(filePath);

        var readerDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "Libris",
                "Reader",
                SanitizeFileName(bookId));

        if (Directory.Exists(readerDirectory))
        {
            Directory.Delete(
                readerDirectory,
                recursive: true);
        }

        Directory.CreateDirectory(readerDirectory);

        var imagesDirectory =
            Path.Combine(
                readerDirectory,
                "images");

        Directory.CreateDirectory(imagesDirectory);

        var imageLookup =
            SaveImages(
                epubBook.Content.Images.Local,
                imagesDirectory);

        var chapters = new List<ReaderChapter>();

        for (var index = 0;
             index < epubBook.ReadingOrder.Count;
             index++)
        {
            var contentFile =
                epubBook.ReadingOrder[index];

            var html = contentFile.Content;

            if (string.IsNullOrWhiteSpace(html))
                continue;

            html = PrepareChapterHtml(
                html,
                imageLookup);

            chapters.Add(
                new ReaderChapter
                {
                    Index = chapters.Count,
                    Title = ExtractChapterTitle(
                        html,
                        index + 1),
                    Html = html,
                    BaseDirectory = readerDirectory
                });
        }

        if (chapters.Count == 0)
        {
            throw new InvalidOperationException(
                "The EPUB does not contain readable content.");
        }

        return new ReaderDocument
        {
            Title =
                string.IsNullOrWhiteSpace(epubBook.Title)
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : epubBook.Title,

            Author = epubBook.Author,

            Chapters = chapters
        };
    }

    private static async Task<ReaderDocument> LoadTextAsync(
        string filePath)
    {
        var text =
            await File.ReadAllTextAsync(filePath);

        var escaped =
            WebUtility.HtmlEncode(text);

        var title =
            Path.GetFileNameWithoutExtension(filePath);

        var html =
            $"""
            <article class="chapter">
                <h1>{WebUtility.HtmlEncode(title)}</h1>
                <div class="text-content">
                    {escaped.Replace(
                        Environment.NewLine,
                        "<br>")}
                </div>
            </article>
            """;

        var readerDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "Libris",
                "Reader",
                SanitizeFileName(title));

        Directory.CreateDirectory(
            readerDirectory);

        var chapter = new ReaderChapter
        {
            Index = 0,
            Title = title,
            Html = html,
            BaseDirectory = readerDirectory
        };

        return new ReaderDocument
        {
            Title = title,
            Author = null,
            Chapters = [chapter]
        };
    }

    private static Dictionary<string, string> SaveImages(
        IEnumerable<EpubLocalByteContentFile> images,
        string imagesDirectory)
    {
        var lookup =
            new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

        var usedNames =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var image in images)
        {
            if (image.Content is not { Length: > 0 })
                continue;

            var normalizedPath =
                NormalizePath(image.Key);

            if (string.IsNullOrWhiteSpace(normalizedPath))
                continue;

            var originalName =
                Path.GetFileName(normalizedPath);

            if (string.IsNullOrWhiteSpace(originalName))
                continue;

            var fileName =
                CreateUniqueFileName(
                    originalName,
                    usedNames);

            var targetPath =
                Path.Combine(
                    imagesDirectory,
                    fileName);

            File.WriteAllBytes(
                targetPath,
                image.Content);

            lookup[normalizedPath] =
                Path.Combine(
                    "images",
                    fileName)
                .Replace(
                    '\\',
                    '/');
        }

        return lookup;
    }

    private static string CreateUniqueFileName(
        string originalName,
        HashSet<string> usedNames)
    {
        var name =
            Path.GetFileNameWithoutExtension(
                originalName);

        var extension =
            Path.GetExtension(originalName);

        var candidate =
            originalName;

        var index = 1;

        while (!usedNames.Add(candidate))
        {
            candidate =
                $"{name}_{index}{extension}";

            index++;
        }

        return candidate;
    }

    private static string PrepareChapterHtml(
        string html,
        IReadOnlyDictionary<string, string> imageLookup)
    {
        return Regex.Replace(
            html,
            """
            (?<attribute>\b(?:src|href)\s*=\s*)
            (?<quote>["'])
            (?<path>.*?)
            \k<quote>
            """,
            match =>
            {
                var attribute =
                    match.Groups["attribute"].Value;

                var quote =
                    match.Groups["quote"].Value;

                var path =
                    match.Groups["path"].Value;

                if (!IsImagePath(path))
                    return match.Value;

                var normalizedPath =
                    NormalizePath(
                        Uri.UnescapeDataString(
                            path
                                .Split('#')[0]
                                .Split('?')[0]));

                if (imageLookup.TryGetValue(
                    normalizedPath,
                    out var relativePath))
                {
                    return
                        attribute +
                        quote +
                        relativePath +
                        quote;
                }

                var fileName =
                    Path.GetFileName(
                        normalizedPath);

                if (string.IsNullOrWhiteSpace(fileName))
                    return match.Value;

                var fallback =
                    imageLookup.FirstOrDefault(
                        pair =>
                            string.Equals(
                                Path.GetFileName(
                                    NormalizePath(pair.Key)),
                                fileName,
                                StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(
                    fallback.Value))
                {
                    return
                        attribute +
                        quote +
                        fallback.Value +
                        quote;
                }

                return match.Value;
            },
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline |
            RegexOptions.IgnorePatternWhitespace);
    }

    private static bool IsImagePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (path.StartsWith(
                "data:",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var extension =
            Path.GetExtension(
                path
                    .Split('#')[0]
                    .Split('?')[0]);

        return
            extension.Equals(
                ".jpg",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".jpeg",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".png",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".gif",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".webp",
                StringComparison.OrdinalIgnoreCase)
            ||
            extension.Equals(
                ".svg",
                StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        var normalized =
            path
                .Replace('\\', '/')
                .TrimStart('/');

        var segments =
            new List<string>();

        foreach (var segment in normalized.Split('/'))
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (segments.Count > 0)
                {
                    segments.RemoveAt(
                        segments.Count - 1);
                }

                continue;
            }

            segments.Add(segment);
        }

        return string.Join(
            "/",
            segments);
    }

    private static string SanitizeFileName(
        string value)
    {
        foreach (var invalidChar
                 in Path.GetInvalidFileNameChars())
        {
            value =
                value.Replace(
                    invalidChar,
                    '_');
        }

        return string.IsNullOrWhiteSpace(value)
            ? "book"
            : value;
    }

    private static string ExtractChapterTitle(
        string html,
        int fallbackNumber)
    {
        var match =
            Regex.Match(
                html,
                @"<h1[^>]*>(.*?)</h1>",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline);

        if (match.Success)
        {
            var title =
                Regex.Replace(
                    match.Groups[1].Value,
                    "<.*?>",
                    string.Empty);

            title =
                WebUtility
                    .HtmlDecode(title)
                    .Trim();

            if (!string.IsNullOrWhiteSpace(title))
                return title;
        }

        match =
            Regex.Match(
                html,
                @"<title[^>]*>(.*?)</title>",
                RegexOptions.IgnoreCase |
                RegexOptions.Singleline);

        if (match.Success)
        {
            var title =
                Regex.Replace(
                    match.Groups[1].Value,
                    "<.*?>",
                    string.Empty);

            title =
                WebUtility
                    .HtmlDecode(title)
                    .Trim();

            if (!string.IsNullOrWhiteSpace(title))
                return title;
        }

        return $"Chapter {fallbackNumber}";
    }
}

/// <summary>
/// Загруженный документ книги.
/// </summary>
public sealed class ReaderDocument
{
    public string Title { get; init; } = string.Empty;

    public string? Author { get; init; }

    public IReadOnlyList<ReaderChapter> Chapters { get; init; } =
        [];
}

/// <summary>
/// Отдельная глава книги.
/// </summary>
public sealed class ReaderChapter
{
    public int Index { get; init; }

    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// HTML-содержимое главы без встроенных Base64-изображений.
    /// </summary>
    public string Html { get; init; } = string.Empty;

    /// <summary>
    /// Каталог, относительно которого должны разрешаться
    /// локальные ресурсы главы.
    /// </summary>
    public string BaseDirectory { get; init; } = string.Empty;
}