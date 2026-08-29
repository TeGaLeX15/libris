// ViewModels/ReaderViewModel.cs
using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;
using Libris.Services;

namespace Libris.ViewModels;

/// <summary>
/// ViewModel режима чтения книги.
/// </summary>
public partial class ReaderViewModel : ObservableObject
{
    private readonly Book _book;
    private readonly Action? _closeReader;
    private readonly BookReaderService _readerService;
    private readonly SettingsService _settingsService;
    private readonly AppDataService _appDataService;
    private readonly AppData _appData;

    private ReaderDocument? _document;

    public ReaderViewModel(
        Book book,
        SettingsService settingsService,
        AppDataService appDataService,
        AppData appData,
        Action? closeReader = null)
    {
        ArgumentNullException.ThrowIfNull(book);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(appDataService);
        ArgumentNullException.ThrowIfNull(appData);

        _book = book;
        _settingsService = settingsService;
        _appDataService = appDataService;
        _appData = appData;
        _closeReader = closeReader;

        _readerService = new BookReaderService();

        var settings = _settingsService.Load();

        ReadingWidth =
            Math.Clamp(
                settings.ReadingWidth,
                400,
                1400);

        FontSize =
            Math.Clamp(
                settings.FontSize,
                10,
                48);

        LineHeight =
            Math.Clamp(
                settings.LineSpacing,
                1.0,
                3.0);

        FontFamily =
            string.IsNullOrWhiteSpace(settings.DefaultFont)
                ? "Inter"
                : settings.DefaultFont;
    }

    /// <summary>
    /// Открытая книга.
    /// </summary>
    public Book Book => _book;

    public string Title => _book.Title;

    public string Author =>
        string.IsNullOrWhiteSpace(_book.Author)
            ? "Unknown author"
            : _book.Author;

    /// <summary>
    /// HTML текущей главы Reader.
    /// </summary>
    [ObservableProperty]
    private string readerHtml = string.Empty;

    [ObservableProperty]
    private double readingWidth;

    [ObservableProperty]
    private double fontSize;

    [ObservableProperty]
    private double lineHeight;

    [ObservableProperty]
    private string fontFamily;

    /// <summary>
    /// Текущий индекс главы.
    /// </summary>
    [ObservableProperty]
    private int currentChapter;

    /// <summary>
    /// Прогресс внутри текущей главы.
    /// </summary>
    [ObservableProperty]
    private double chapterProgress;

    /// <summary>
    /// Количество глав.
    /// </summary>
    public int ChapterCount =>
        _document?.Chapters.Count ?? 0;

    /// <summary>
    /// Общий прогресс книги.
    /// </summary>
    public double Progress
    {
        get
        {
            if (_document is null ||
                _document.Chapters.Count == 0)
            {
                return _book.Progress;
            }

            var progress =
                (CurrentChapter + ChapterProgress) /
                _document.Chapters.Count;

            return Math.Clamp(progress, 0.0, 1.0);
        }
    }

    public string ProgressText =>
        $"{Math.Round(Progress * 100):0}%";

    public string PositionText
    {
        get
        {
            if (_document is null)
                return "Loading…";

            return
                $"Chapter {CurrentChapter + 1} " +
                $"of {ChapterCount}";
        }
    }

    public string CurrentChapterTitle
    {
        get
        {
            if (_document is null ||
                CurrentChapter < 0 ||
                CurrentChapter >=
                _document.Chapters.Count)
            {
                return "Reading";
            }

            return
                _document.Chapters[
                    CurrentChapter].Title;
        }
    }

    public bool CanGoPrevious =>
        CurrentChapter > 0;

    public bool CanGoNext =>
        _document is not null &&
        CurrentChapter <
        _document.Chapters.Count - 1;

    /// <summary>
    /// Загружает книгу и восстанавливает последнюю позицию.
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            _document =
                await _readerService.LoadAsync(
                    _book.FilePath);

            RestorePosition();

            RebuildReaderHtml();
            NotifyChapterChanged();
        }
        catch (Exception ex) when (
            ex is IOException
            or UnauthorizedAccessException
            or InvalidOperationException
            or NotSupportedException)
        {
            ReaderHtml =
                BuildErrorHtml(
                    "Unable to open this book.",
                    ex.Message);

            OnPropertyChanged(
                nameof(PositionText));
        }
    }

    /// <summary>
    /// Сохраняет текущую позицию чтения.
    /// </summary>
    public void SavePosition()
    {
        if (_document is null)
            return;

        var key = _book.Id.ToString();

        _appData.ReadingPositions[key] =
            new ReaderPosition
            {
                Chapter = CurrentChapter,
                ChapterProgress =
                    Math.Clamp(
                        ChapterProgress,
                        0.0,
                        1.0)
            };

        _book.Progress = Progress;

        _appDataService.Save(_appData);
    }

    /// <summary>
    /// Обновляет позицию внутри текущей главы.
    /// Вызывается ReaderView при прокрутке WebView.
    /// </summary>
    public void UpdateChapterProgress(
        double progress)
    {
        if (_document is null)
            return;

        ChapterProgress =
            Math.Clamp(progress, 0.0, 1.0);

        _book.Progress = Progress;

        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressText));

        SavePosition();
    }

    [RelayCommand]
    private void Close()
    {
        SavePosition();
        _closeReader?.Invoke();
    }

    /// <summary>
    /// Переходит к предыдущей главе.
    /// </summary>
    [RelayCommand]
    private void PreviousPage()
    {
        if (!CanGoPrevious)
            return;

        SavePosition();

        CurrentChapter--;
        ChapterProgress = 0;

        RebuildReaderHtml();
        NotifyChapterChanged();
    }

    /// <summary>
    /// Переходит к следующей главе.
    /// </summary>
    [RelayCommand]
    private void NextPage()
    {
        if (!CanGoNext)
            return;

        SavePosition();

        CurrentChapter++;
        ChapterProgress = 0;

        RebuildReaderHtml();
        NotifyChapterChanged();
    }

    partial void OnCurrentChapterChanged(int value)
    {
        NotifyChapterChanged();
    }

    partial void OnChapterProgressChanged(double value)
    {
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressText));
    }

    partial void OnFontSizeChanged(double value)
    {
        value = Math.Clamp(value, 10, 48);

        if (Math.Abs(FontSize - value) >
            double.Epsilon)
        {
            FontSize = value;
            return;
        }

        RebuildReaderHtml();
    }

    partial void OnLineHeightChanged(double value)
    {
        value = Math.Clamp(value, 1.0, 3.0);

        if (Math.Abs(LineHeight - value) >
            double.Epsilon)
        {
            LineHeight = value;
            return;
        }

        RebuildReaderHtml();
    }

    partial void OnReadingWidthChanged(double value)
    {
        value = Math.Clamp(value, 400, 1400);

        if (Math.Abs(ReadingWidth - value) >
            double.Epsilon)
        {
            ReadingWidth = value;
            return;
        }

        RebuildReaderHtml();
    }

    partial void OnFontFamilyChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FontFamily = "Inter";
            return;
        }

        RebuildReaderHtml();
    }

    private void RestorePosition()
    {
        if (_document is null ||
            _document.Chapters.Count == 0)
        {
            CurrentChapter = 0;
            ChapterProgress = 0;
            return;
        }

        var key = _book.Id.ToString();

        if (!_appData.ReadingPositions.TryGetValue(
                key,
                out var position))
        {
            CurrentChapter = 0;
            ChapterProgress = 0;
            return;
        }

        CurrentChapter =
            Math.Clamp(
                position.Chapter,
                0,
                _document.Chapters.Count - 1);

        ChapterProgress =
            Math.Clamp(
                position.ChapterProgress,
                0.0,
                1.0);
    }

    private void RebuildReaderHtml()
    {
        if (_document is null ||
            CurrentChapter < 0 ||
            CurrentChapter >=
            _document.Chapters.Count)
        {
            return;
        }

        ReaderHtml =
            ReaderHtmlBuilder.BuildChapter(
                _document.Title,
                _document.Chapters[CurrentChapter],
                FontFamily,
                FontSize,
                LineHeight,
                ReadingWidth);
    }

    private void NotifyChapterChanged()
    {
        OnPropertyChanged(
            nameof(CurrentChapterTitle));

        OnPropertyChanged(
            nameof(PositionText));

        OnPropertyChanged(
            nameof(CanGoPrevious));

        OnPropertyChanged(
            nameof(CanGoNext));

        OnPropertyChanged(
            nameof(ChapterCount));

        OnPropertyChanged(
            nameof(Progress));

        OnPropertyChanged(
            nameof(ProgressText));
    }

    private static string BuildErrorHtml(
        string title,
        string message)
    {
        var safeTitle =
            System.Net.WebUtility.HtmlEncode(title);

        var safeMessage =
            System.Net.WebUtility.HtmlEncode(message);

        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">

                <style>
                    html,
                    body {
                        height: 100%;
                    }

                    body {
                        margin: 0;

                        display: flex;
                        align-items: center;
                        justify-content: center;

                        background: transparent;
                        color: #777;

                        font-family:
                            system-ui,
                            sans-serif;

                        text-align: center;
                    }

                    h1 {
                        color: #555;
                        font-size: 24px;
                    }

                    p {
                        max-width: 600px;
                        line-height: 1.5;
                    }
                </style>
            </head>

            <body>
                <div>
                    <h1>{{safeTitle}}</h1>
                    <p>{{safeMessage}}</p>
                </div>
            </body>

            </html>
            """;
    }
}
