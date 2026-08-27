// ViewModels/ReaderViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;

namespace Libris.ViewModels;

/// <summary>
/// ViewModel режима чтения выбранной книги.
/// Отвечает за отображение книги, параметры чтения и прогресс.
/// </summary>
public partial class ReaderViewModel : ObservableObject
{
    private readonly Book _book;
    private readonly Action? _closeReader;

    /// <summary>
    /// Создаёт ViewModel режима чтения.
    /// </summary>
    /// <param name="book">Книга, открытая для чтения.</param>
    /// <param name="closeReader">
    /// Действие, вызываемое при закрытии режима чтения.
    /// </param>
    public ReaderViewModel(
        Book book,
        Action? closeReader = null)
    {
        _book = book;
        _closeReader = closeReader;

        ReadingWidth = 760;
        FontSize = 18;
        LineHeight = 1.6;
    }

    /// <summary>
    /// Книга, открытая в режиме чтения.
    /// </summary>
    public Book Book => _book;

    /// <summary>
    /// Название открытой книги.
    /// </summary>
    public string Title => _book.Title;

    /// <summary>
    /// Автор открытой книги.
    /// </summary>
    public string Author => _book.Author;

    /// <summary>
    /// Путь к обложке открытой книги.
    /// </summary>
    public string? CoverPath => _book.CoverPath;

    /// <summary>
    /// Содержимое книги, отображаемое в режиме чтения.
    /// Временно содержит демонстрационный текст.
    /// В дальнейшем здесь будет загружаться содержимое выбранной книги.
    /// </summary>
    public string Content { get; } = """
        Chapter One

        The room was quiet.

        Beyond the window, the evening light slowly disappeared
        behind the buildings. A faint glow remained in the sky,
        illuminating the streets below.

        He opened the book again and continued reading.

        Every page brought another detail into the story.

        The characters became more familiar, the world around
        them more believable, and the distance between the reader
        and the story gradually disappeared.

        This is temporary reader content.

        Later, Libris will load the actual contents of the selected
        book and render it according to the selected reading settings.
        """;

    /// <summary>
    /// Максимальная ширина области текста при чтении.
    /// </summary>
    [ObservableProperty]
    private double readingWidth;

    /// <summary>
    /// Размер шрифта текста книги.
    /// </summary>
    [ObservableProperty]
    private double fontSize;

    /// <summary>
    /// Межстрочный интервал текста книги.
    /// </summary>
    [ObservableProperty]
    private double lineHeight;

    /// <summary>
    /// Текущий прогресс чтения книги от 0 до 1.
    /// </summary>
    public double Progress
    {
        get => _book.Progress;
        set
        {
            var clampedValue = Math.Clamp(value, 0.0, 1.0);

            if (Math.Abs(_book.Progress - clampedValue) < 0.001)
                return;

            _book.Progress = clampedValue;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(PositionText));
        }
    }

    /// <summary>
    /// Текущий прогресс чтения в процентном формате.
    /// </summary>
    public string ProgressText =>
        $"{Math.Round(Progress * 100):0}%";

    /// <summary>
    /// Текущая позиция чтения.
    /// Временный вариант до реализации реальной пагинации.
    /// </summary>
    public string PositionText =>
        $"Page 1 • {ProgressText}";

    /// <summary>
    /// Закрывает режим чтения.
    /// </summary>
    [RelayCommand]
    private void Close()
    {
        _closeReader?.Invoke();
    }

    /// <summary>
    /// Переходит на предыдущую страницу книги.
    /// </summary>
    [RelayCommand]
    private void PreviousPage()
    {
        // TODO: Реализовать переход на предыдущую страницу.
    }

    /// <summary>
    /// Переходит на следующую страницу книги.
    /// </summary>
    [RelayCommand]
    private void NextPage()
    {
        // TODO: Реализовать переход на следующую страницу.
    }
}