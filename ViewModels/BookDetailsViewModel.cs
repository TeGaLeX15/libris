// ViewModels/BookDetailsViewModel.cs
using System;
using System.IO;

using Avalonia.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Libris.Models;

namespace Libris.ViewModels;

/// <summary>
/// Представляет состояние панели с подробной информацией о выбранной книге.
/// </summary>
public partial class BookDetailsViewModel : ObservableObject
{
    /// <summary>
    /// Текущая выбранная книга.
    /// </summary>
    [ObservableProperty]
    private Book? book;

    /// <summary>
    /// Определяет, открыта ли панель с подробностями книги.
    /// </summary>
    [ObservableProperty]
    private bool isOpen;

    /// <summary>
    /// Возникает при запросе закрытия панели с подробностями.
    /// </summary>
    public event EventHandler? CloseRequested;

    /// <summary>
    /// Возникает при запросе открытия выбранной книги для чтения.
    /// </summary>
    public event EventHandler<Book>? ReadRequested;

    /// <summary>
    /// Возвращает обложку текущей книги.
    /// </summary>
    public Bitmap? Cover
    {
        get
        {
            if (Book is null ||
                string.IsNullOrWhiteSpace(Book.CoverPath))
            {
                return null;
            }

            try
            {
                return new Bitmap(Book.CoverPath);
            }
            catch (IOException)
            {
                // Отсутствующая или недоступная обложка не должна
                // приводить к падению приложения.
                return null;
            }
            catch (ArgumentException)
            {
                // Некорректный путь или формат изображения.
                return null;
            }
        }
    }

    /// <summary>
    /// Возвращает название текущей книги.
    /// </summary>
    public string Title =>
        Book?.Title ?? "Unknown title";

    /// <summary>
    /// Возвращает имя автора текущей книги.
    /// </summary>
    public string Author =>
        string.IsNullOrWhiteSpace(Book?.Author)
            ? "Unknown author"
            : Book.Author;

    /// <summary>
    /// Возвращает формат файла текущей книги.
    /// </summary>
    public string Format =>
        string.IsNullOrWhiteSpace(Book?.FilePath)
            ? "Unknown"
            : Path.GetExtension(Book.FilePath)
                .TrimStart('.')
                .ToUpperInvariant();

    /// <summary>
    /// Возвращает имя файла текущей книги.
    /// </summary>
    public string FileName =>
        string.IsNullOrWhiteSpace(Book?.FilePath)
            ? "Unknown"
            : Path.GetFileName(Book.FilePath);

    /// <summary>
    /// Возвращает размер файла текущей книги в удобном для отображения формате.
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
                // Ошибка доступа к файлу не должна приводить
                // к падению приложения.
                return "Unknown";
            }
            catch (UnauthorizedAccessException)
            {
                // Файл может существовать, но быть недоступным.
                return "Unknown";
            }
        }
    }

    /// <summary>
    /// Открывает панель с информацией о выбранной книге.
    /// </summary>
    /// <param name="selectedBook">Книга для отображения.</param>
    public void Open(Book selectedBook)
    {
        ArgumentNullException.ThrowIfNull(selectedBook);

        Book = selectedBook;
        IsOpen = true;

        NotifyBookPropertiesChanged();
    }

    /// <summary>
    /// Закрывает панель с подробностями книги.
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Закрывает панель и очищает выбранную книгу.
    /// </summary>
    public void ClosePanel()
    {
        IsOpen = false;
        Book = null;

        NotifyBookPropertiesChanged();
    }

    /// <summary>
    /// Запрашивает открытие текущей книги в режиме чтения.
    /// </summary>
    [RelayCommand]
    private void Read()
    {
        if (Book is null)
            return;

        ReadRequested?.Invoke(this, Book);
    }

    /// <summary>
    /// Уведомляет интерфейс об изменении вычисляемых свойств,
    /// зависящих от текущей книги.
    /// </summary>
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