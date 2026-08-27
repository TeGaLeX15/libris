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
    AuthorDescending
}

/// <summary>
/// Представляет ViewModel библиотеки, отвечающую за загрузку,
/// поиск, сортировку, добавление и удаление книг.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

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
        BookSortOption.TitleAscending,
        BookSortOption.TitleDescending,
        BookSortOption.AuthorAscending,
        BookSortOption.AuthorDescending
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
    /// Указывает, что поиск выполнен, но подходящие книги не найдены.
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
    private BookSortOption selectedSort = BookSortOption.TitleAscending;

    /// <summary>
    /// Инициализирует ViewModel библиотеки и загружает сохранённые книги.
    /// </summary>
    public LibraryViewModel()
    {
        _libraryService = new LibraryService();

        LoadBooks();
    }

    /// <summary>
    /// Обновляет отображаемый список книг при изменении поискового запроса.
    /// </summary>
    /// <param name="value">Новое значение поискового запроса.</param>
    partial void OnSearchQueryChanged(string value)
    {
        UpdateFilteredBooks();
    }

    /// <summary>
    /// Обновляет отображаемый список книг при изменении способа сортировки.
    /// </summary>
    /// <param name="value">Новый вариант сортировки.</param>
    partial void OnSelectedSortChanged(BookSortOption value)
    {
        UpdateFilteredBooks();
    }

    /// <summary>
    /// Выбирает книгу и уведомляет подписчиков о её выборе.
    /// </summary>
    /// <param name="book">Выбранная книга.</param>
    public void SelectBook(Book? book)
    {
        if (book is null)
            return;

        BookSelected?.Invoke(this, book);
    }

    /// <summary>
    /// Загружает сохранённые книги через сервис библиотеки.
    /// </summary>
    private void LoadBooks()
    {
        Books.Clear();

        foreach (var book in _libraryService.Load())
        {
            Books.Add(book);
        }

        UpdateFilteredBooks();
    }

    /// <summary>
    /// Импортирует книги из указанных файлов и добавляет их в библиотеку.
    /// </summary>
    /// <param name="filePaths">Пути к файлам книг для импорта.</param>
    /// <returns>Задача, представляющая асинхронную операцию импорта.</returns>
    public async Task AddBooksAsync(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                continue;

            var book = await _libraryService.ImportAsync(filePath);

            if (book is null)
                continue;

            Books.Add(book);
        }

        UpdateFilteredBooks();
    }

    /// <summary>
    /// Удаляет указанную книгу из библиотеки.
    /// </summary>
    /// <param name="book">Книга, которую необходимо удалить.</param>
    public void RemoveBook(Book? book)
    {
        if (book is null)
            return;

        _libraryService.Remove(book.Id);
        Books.Remove(book);

        UpdateFilteredBooks();
    }

    /// <summary>
    /// Применяет текущий поисковый запрос и выбранную сортировку,
    /// после чего обновляет список книг, отображаемый в интерфейсе.
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
    /// Обновляет состояния пустой библиотеки и пустого результата поиска,
    /// используемые интерфейсом для отображения соответствующих состояний.
    /// </summary>
    /// <param name="query">Нормализованный поисковый запрос.</param>
    private void UpdateEmptyState(string query)
    {
        IsEmpty = Books.Count == 0;

        IsSearchEmpty =
            !IsEmpty &&
            !string.IsNullOrWhiteSpace(query) &&
            FilteredBooks.Count == 0;
    }
}