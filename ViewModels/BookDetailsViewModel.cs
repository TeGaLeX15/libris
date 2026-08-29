// ViewModels/BookDetailsViewModel.cs
using System;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;

namespace Libris.ViewModels;

/// <summary>
/// Представляет состояние панели подробной информации о выбранной книге.
/// </summary>
public partial class BookDetailsViewModel : ObservableObject
{
    [ObservableProperty]
    private Book? book;

    [ObservableProperty]
    private bool isOpen;

    private Bitmap? _cover;

    /// <summary>
    /// Возникает при запросе закрытия панели.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Возникает при запросе открытия книги для чтения.
    /// </summary>
    public event EventHandler<Book>? ReadRequested;

    /// <summary>
    /// Обложка текущей книги.
    /// </summary>
    public Bitmap? Cover => _cover;

    /// <summary>
    /// Название книги.
    /// </summary>
    public string Title =>
        Book?.Title ?? "Unknown title";

    /// <summary>
    /// Автор книги.
    /// </summary>
    public string Author =>
        string.IsNullOrWhiteSpace(Book?.Author)
            ? "Unknown author"
            : Book.Author;

    /// <summary>
    /// Формат файла книги.
    /// </summary>
    public string Format
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Book?.FilePath))
                return "Unknown";

            return Path
                .GetExtension(Book.FilePath)
                .TrimStart('.')
                .ToUpperInvariant();
        }
    }

    /// <summary>
    /// Имя файла книги.
    /// </summary>
    public string FileName =>
        string.IsNullOrWhiteSpace(Book?.FilePath)
            ? "Unknown"
            : Path.GetFileName(Book.FilePath);

    /// <summary>
    /// Размер файла книги.
    /// </summary>
    public string FileSize
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Book?.FilePath))
                return "Unknown";

            try
            {
                var bytes = new FileInfo(Book.FilePath).Length;

                return bytes switch
                {
                    >= 1024L * 1024L * 1024L =>
                        $"{bytes / 1024d / 1024d / 1024d:F1} GB",

                    >= 1024L * 1024L =>
                        $"{bytes / 1024d / 1024d:F1} MB",

                    >= 1024L =>
                        $"{bytes / 1024d:F0} KB",

                    _ =>
                        $"{bytes} B"
                };
            }
            catch (IOException)
            {
                return "Unknown";
            }
            catch (UnauthorizedAccessException)
            {
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// Прогресс чтения книги от 0 до 1.
    /// </summary>
    public double Progress =>
        Math.Clamp(Book?.Progress ?? 0.0, 0.0, 1.0);

    /// <summary>
    /// Прогресс чтения книги в процентах.
    /// </summary>
    public string ProgressText =>
        $"{Math.Round(Progress * 100):0}%";

    /// <summary>
    /// Текстовое состояние чтения книги.
    /// </summary>
    public string ReadingStatus
    {
        get
        {
            if (Progress <= 0)
                return "Not started";

            if (Progress >= 1)
                return "Finished";

            return "In progress";
        }
    }

    /// <summary>
    /// Открывает панель информации о книге.
    /// </summary>
    public void Open(Book selectedBook)
    {
        ArgumentNullException.ThrowIfNull(selectedBook);

        DisposeCover();

        Book = selectedBook;
        IsOpen = true;

        LoadCover();
        NotifyBookPropertiesChanged();
    }

    /// <summary>
    /// Закрывает панель.
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Полностью закрывает панель и освобождает ресурсы.
    /// </summary>
    public void ClosePanel()
    {
        IsOpen = false;
        Book = null;

        DisposeCover();
        NotifyBookPropertiesChanged();
    }

    /// <summary>
    /// Запрашивает открытие книги в Reader.
    /// </summary>
    [RelayCommand]
    private void Read()
    {
        if (Book is null)
            return;

        ReadRequested?.Invoke(this, Book);
    }

    private void LoadCover()
    {
        if (string.IsNullOrWhiteSpace(Book?.CoverPath))
            return;

        try
        {
            if (File.Exists(Book.CoverPath))
            {
                _cover = new Bitmap(Book.CoverPath);
            }
        }
        catch (IOException)
        {
            _cover = null;
        }
        catch (UnauthorizedAccessException)
        {
            _cover = null;
        }
        catch (ArgumentException)
        {
            _cover = null;
        }
    }

    private void DisposeCover()
    {
        _cover?.Dispose();
        _cover = null;
    }

    private void NotifyBookPropertiesChanged()
    {
        OnPropertyChanged(nameof(Cover));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Author));
        OnPropertyChanged(nameof(Format));
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(FileSize));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ReadingStatus));
    }

    partial void OnBookChanged(Book? value)
    {
        NotifyBookPropertiesChanged();

        if (value is not null)
        {
            value.PropertyChanged += Book_PropertyChanged;
        }
    }

    private void Book_PropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(Book.Progress))
            return;

        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ReadingStatus));
    }
}