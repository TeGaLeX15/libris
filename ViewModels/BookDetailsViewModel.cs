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

    public string Title =>
        Book?.Title ?? "Unknown title";

    public string Author =>
        string.IsNullOrWhiteSpace(Book?.Author)
            ? "Unknown author"
            : Book.Author;

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

    public string FileName =>
        string.IsNullOrWhiteSpace(Book?.FilePath)
            ? "Unknown"
            : Path.GetFileName(Book.FilePath);

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
    }
}