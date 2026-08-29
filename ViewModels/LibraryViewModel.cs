// ViewModels/LibraryViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Libris.Models;
using Libris.Services;

namespace Libris.ViewModels;

/// <summary>
/// Определяет доступные варианты сортировки книг в библиотеке.
/// </summary>
public enum BookSortOption
{
    /// <summary>
    /// Сначала недавно добавленные книги.
    /// </summary>
    RecentlyAdded,

    /// <summary>
    /// Сортировка книг по названию от А до Я.
    /// </summary>
    TitleAscending,

    /// <summary>
    /// Сортировка книг по названию от Я до А.
    /// </summary>
    TitleDescending,

    /// <summary>
    /// Сортировка книг по автору от А до Я.
    /// </summary>
    AuthorAscending,

    /// <summary>
    /// Сортировка книг по автору от Я до А.
    /// </summary>
    AuthorDescending,

    /// <summary>
    /// Сортировка книг по прогрессу от большего к меньшему.
    /// </summary>
    ProgressDescending
}

/// <summary>
/// Представляет ViewModel библиотеки, отвечающую за загрузку,
/// поиск, сортировку, добавление и удаление книг.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;
    private readonly SettingsService _settingsService;
    private readonly AppData _appData;

    /// <summary>
    /// Содержит все книги, загруженные в библиотеку.
    /// </summary>
    public ObservableCollection<Book> Books { get; } = [];

    /// <summary>
    /// Содержит книги, отображаемые после применения поиска и сортировки.
    /// </summary>
    public ObservableCollection<Book> FilteredBooks { get; } = [];

    /// <summary>
    /// Содержит доступные варианты сортировки книг.
    /// </summary>
    public IReadOnlyList<BookSortOption> SortOptions { get; } =
    [
        BookSortOption.RecentlyAdded,
        BookSortOption.TitleAscending,
        BookSortOption.TitleDescending,
        BookSortOption.AuthorAscending,
        BookSortOption.AuthorDescending,
        BookSortOption.ProgressDescending
    ];

    /// <summary>
    /// Возникает при выборе пользователем книги.
    /// </summary>
    public event EventHandler<Book>? BookSelected;

    /// <summary>
    /// Указывает, пуста ли библиотека.
    /// </summary>
    [ObservableProperty]
    private bool isEmpty;

    /// <summary>
    /// Указывает, что поиск выполнен,
    /// но подходящие книги не найдены.
    /// </summary>
    [ObservableProperty]
    private bool isSearchEmpty;

    /// <summary>
    /// Содержит текущий поисковый запрос пользователя.
    /// </summary>
    [ObservableProperty]
    private string searchQuery = string.Empty;

    /// <summary>
    /// Определяет текущий способ сортировки книг.
    /// </summary>
    [ObservableProperty]
    private BookSortOption selectedSort;

    /// <summary>
    /// Инициализирует ViewModel библиотеки
    /// и загружает сохранённые книги.
    /// </summary>
    public LibraryViewModel(AppData appData)
    {
        ArgumentNullException.ThrowIfNull(appData);

        _appData = appData;
        _libraryService = new LibraryService();
        _settingsService = new SettingsService();

        SelectedSort = GetSortOption(
            _settingsService.Load().DefaultSorting);

        LoadBooks();
    }

    partial void OnSearchQueryChanged(string value)
    {
        UpdateFilteredBooks();
    }

    partial void OnSelectedSortChanged(BookSortOption value)
    {
        SaveDefaultSorting(value);
        UpdateFilteredBooks();
    }

    /// <summary>
    /// Выбирает книгу и уведомляет подписчиков о её выборе.
    /// </summary>
    public void SelectBook(Book? book)
    {
        if (book is null)
            return;

        BookSelected?.Invoke(this, book);
    }

    /// <summary>
    /// Загружает сохранённые книги через сервис библиотеки
    /// и восстанавливает сохранённый прогресс.
    /// </summary>
    private void LoadBooks()
    {
        Books.Clear();

        foreach (var book in _libraryService.Load())
        {
            RestoreProgress(book);
            Books.Add(book);
        }

        UpdateFilteredBooks();
    }

    /// <summary>
    /// Восстанавливает общий прогресс книги
    /// из сохранённых данных приложения.
    /// </summary>
    private void RestoreProgress(Book book)
    {
        var key = book.Id.ToString();

        if (!_appData.ReadingPositions.TryGetValue(
                key,
                out var position))
        {
            return;
        }

        book.Progress =
            Math.Clamp(
                position.Progress,
                0.0,
                1.0);
    }

    /// <summary>
    /// Импортирует книги из указанных файлов
    /// и добавляет их в библиотеку.
    /// </summary>
    public async Task AddBooksAsync(
        IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                continue;

            var book =
                await _libraryService.ImportAsync(filePath);

            if (book is null)
                continue;

            RestoreProgress(book);
            Books.Add(book);
        }

        UpdateFilteredBooks();
    }

    /// <summary>
    /// Удаляет указанную книгу из библиотеки.
    /// </summary>
    public void RemoveBook(Book? book)
    {
        if (book is null)
            return;

        _libraryService.Remove(book.Id);

        _appData.ReadingPositions.Remove(
            book.Id.ToString());

        Books.Remove(book);

        UpdateFilteredBooks();
    }

    /// <summary>
    /// Применяет текущий поисковый запрос и выбранную сортировку,
    /// после чего обновляет список книг, отображаемый в интерфейсе библиотеки.
    /// </summary>
    private void UpdateFilteredBooks()
    {
        var query = SearchQuery.Trim();

        IEnumerable<Book> result = Books;

        if (!string.IsNullOrWhiteSpace(query))
        {
            result = result.Where(book =>
                book.Title?.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase) == true
                ||
                book.Author?.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase) == true);
        }

        result = SelectedSort switch
        {
            BookSortOption.RecentlyAdded =>
                result.OrderByDescending(
                    book => book.AddedAt),

            BookSortOption.TitleAscending =>
                result.OrderBy(
                    book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            BookSortOption.TitleDescending =>
                result.OrderByDescending(
                    book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            BookSortOption.AuthorAscending =>
                result.OrderBy(
                    book => book.Author ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(
                    book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            BookSortOption.AuthorDescending =>
                result.OrderByDescending(
                    book => book.Author ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(
                    book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            BookSortOption.ProgressDescending =>
                result.OrderByDescending(
                    book => book.Progress)
                .ThenBy(
                    book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            _ => result
        };

        FilteredBooks.Clear();

        foreach (var book in result)
        {
            FilteredBooks.Add(book);
        }

        UpdateEmptyState(query);
    }

    /// <summary>
    /// Обновляет состояния пустой библиотеки
    /// и пустого результата поиска.
    /// </summary>
    private void UpdateEmptyState(string query)
    {
        IsEmpty = Books.Count == 0;

        IsSearchEmpty =
            !IsEmpty &&
            !string.IsNullOrWhiteSpace(query) &&
            FilteredBooks.Count == 0;
    }

    /// <summary>
    /// Преобразует значение сортировки из настроек
    /// в соответствующий вариант сортировки библиотеки.
    /// </summary>
    private static BookSortOption GetSortOption(
        string? sorting)
    {
        return sorting switch
        {
            "Title" =>
                BookSortOption.TitleAscending,

            "Author" =>
                BookSortOption.AuthorAscending,

            "Progress" =>
                BookSortOption.ProgressDescending,

            _ =>
                BookSortOption.RecentlyAdded
        };
    }

    /// <summary>
    /// Сохраняет выбранную пользователем сортировку
    /// как сортировку библиотеки по умолчанию.
    /// </summary>
    private void SaveDefaultSorting(
        BookSortOption sort)
    {
        var settings = _settingsService.Load();

        settings.DefaultSorting = sort switch
        {
            BookSortOption.TitleAscending =>
                "Title",

            BookSortOption.TitleDescending =>
                "Title",

            BookSortOption.AuthorAscending =>
                "Author",

            BookSortOption.AuthorDescending =>
                "Author",

            BookSortOption.ProgressDescending =>
                "Progress",

            _ =>
                "Recently Added"
        };

        _settingsService.Save(settings);
    }
}