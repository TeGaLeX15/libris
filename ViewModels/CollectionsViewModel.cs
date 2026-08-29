// ViewModels/CollectionsViewModel.cs
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Libris.Models;
using Libris.Services;

namespace Libris.ViewModels;

/// <summary>
/// Управляет страницей коллекций книг.
/// Отвечает за создание, переименование и удаление коллекций,
/// а также за добавление, удаление и поиск книг внутри коллекций.
/// </summary>
public partial class CollectionsViewModel : ViewModelBase
{
    private readonly CollectionService _collectionService;
    private readonly LibraryService _libraryService;

    /// <summary>
    /// Все коллекции, доступные пользователю.
    /// </summary>
    public ObservableCollection<BookCollection> Collections { get; } = [];

    /// <summary>
    /// Книги, входящие в выбранную коллекцию.
    /// </summary>
    public ObservableCollection<Book> CollectionBooks { get; } = [];

    /// <summary>
    /// Все книги библиотеки, которые ещё не добавлены
    /// в выбранную коллекцию.
    /// </summary>
    public ObservableCollection<Book> AvailableBooks { get; } = [];

    /// <summary>
    /// Доступные для добавления книги после применения поиска.
    /// </summary>
    public ObservableCollection<Book> FilteredAvailableBooks { get; } = [];

    /// <summary>
    /// Возникает при выборе книги пользователем.
    /// </summary>
    public event EventHandler<Book>? BookSelected;

    /// <summary>
    /// Текущая выбранная коллекция.
    /// </summary>
    [ObservableProperty]
    private BookCollection? selectedCollection;

    /// <summary>
    /// Название новой коллекции, введённое пользователем.
    /// </summary>
    [ObservableProperty]
    private string newCollectionName = string.Empty;

    /// <summary>
    /// Новое название коллекции при переименовании.
    /// </summary>
    [ObservableProperty]
    private string renameCollectionName = string.Empty;

    /// <summary>
    /// Поисковый запрос среди книг, доступных для добавления.
    /// </summary>
    [ObservableProperty]
    private string bookSearchQuery = string.Empty;

    /// <summary>
    /// Определяет, отсутствуют ли коллекции.
    /// </summary>
    [ObservableProperty]
    private bool isEmpty;

    /// <summary>
    /// Определяет, открыта ли панель добавления книг.
    /// </summary>
    [ObservableProperty]
    private bool isAddBooksOpen;

    /// <summary>
    /// Определяет, находится ли коллекция в режиме переименования.
    /// </summary>
    [ObservableProperty]
    private bool isRenaming;

    /// <summary>
    /// Возвращает значение, указывающее, содержит ли выбранная
    /// коллекция хотя бы одну книгу.
    /// </summary>
    public bool HasCollectionBooks =>
        CollectionBooks.Count > 0;

    /// <summary>
    /// Возвращает значение, указывающее, есть ли книги,
    /// доступные для добавления в выбранную коллекцию.
    /// </summary>
    public bool HasAvailableBooks =>
        FilteredAvailableBooks.Count > 0;

    /// <summary>
    /// Возвращает текст кнопки открытия или закрытия
    /// панели добавления книг.
    /// </summary>
    public string AddBooksButtonText =>
        IsAddBooksOpen ? "Close" : "Add books";

    /// <summary>
    /// Инициализирует ViewModel и загружает сохранённые коллекции.
    /// </summary>
    public CollectionsViewModel()
    {
        _collectionService = new CollectionService();
        _libraryService = new LibraryService();

        LoadCollections();
    }

    /// <summary>
    /// Обрабатывает изменение выбранной коллекции.
    /// </summary>
    /// <param name="value">Новая выбранная коллекция.</param>
    partial void OnSelectedCollectionChanged(BookCollection? value)
    {
        RenameCollectionName = value?.Name ?? string.Empty;
        IsRenaming = false;
        IsAddBooksOpen = false;
        BookSearchQuery = string.Empty;

        RefreshCollectionBooks();
        RefreshAvailableBooks();

        OnPropertyChanged(nameof(AddBooksButtonText));
    }

    /// <summary>
    /// Обновляет список доступных книг при изменении поискового запроса.
    /// </summary>
    /// <param name="value">Новое значение поискового запроса.</param>
    partial void OnBookSearchQueryChanged(string value)
    {
        ApplyBookFilter();
    }

    /// <summary>
    /// Обновляет текст кнопки добавления книг
    /// при изменении состояния панели.
    /// </summary>
    /// <param name="value">Новое состояние панели.</param>
    partial void OnIsAddBooksOpenChanged(bool value)
    {
        OnPropertyChanged(nameof(AddBooksButtonText));
    }

    /// <summary>
    /// Загружает все сохранённые коллекции.
    /// </summary>
    private void LoadCollections()
    {
        Collections.Clear();

        foreach (var collection in _collectionService.Load())
        {
            Collections.Add(collection);
        }

        IsEmpty = Collections.Count == 0;
        SelectedCollection = Collections.FirstOrDefault();

        RefreshState();
    }

    /// <summary>
    /// Обновляет книги, входящие в выбранную коллекцию.
    /// </summary>
    private void RefreshCollectionBooks()
    {
        CollectionBooks.Clear();

        if (SelectedCollection is null)
        {
            RefreshState();
            return;
        }

        var books = _libraryService.Load();

        foreach (var bookId in SelectedCollection.BookIds)
        {
            var book = books.FirstOrDefault(
                x => x.Id == bookId);

            if (book is not null)
            {
                CollectionBooks.Add(book);
            }
        }

        RefreshState();
    }

    /// <summary>
    /// Обновляет список книг, доступных для добавления
    /// в выбранную коллекцию.
    /// </summary>
    private void RefreshAvailableBooks()
    {
        AvailableBooks.Clear();

        if (SelectedCollection is null)
        {
            FilteredAvailableBooks.Clear();
            RefreshState();
            return;
        }

        var books = _libraryService.Load();

        foreach (var book in books)
        {
            if (!SelectedCollection.BookIds.Contains(book.Id))
            {
                AvailableBooks.Add(book);
            }
        }

        ApplyBookFilter();
    }

    /// <summary>
    /// Применяет поисковый запрос к списку доступных книг.
    /// </summary>
    private void ApplyBookFilter()
    {
        FilteredAvailableBooks.Clear();

        var query = BookSearchQuery.Trim();

        IEnumerable<Book> books = AvailableBooks;

        if (!string.IsNullOrWhiteSpace(query))
        {
            books = books.Where(book =>
                book.Title.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase)
                ||
                book.Author.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase));
        }

        foreach (var book in books)
        {
            FilteredAvailableBooks.Add(book);
        }

        RefreshState();
    }

    /// <summary>
    /// Обновляет вычисляемые свойства состояния интерфейса.
    /// </summary>
    private void RefreshState()
    {
        IsEmpty = Collections.Count == 0;

        OnPropertyChanged(nameof(HasCollectionBooks));
        OnPropertyChanged(nameof(HasAvailableBooks));
    }

    /// <summary>
    /// Выбирает книгу и уведомляет подписчиков о выборе.
    /// </summary>
    /// <param name="book">Выбранная книга.</param>
    public void SelectBook(Book? book)
    {
        if (book is null)
            return;

        BookSelected?.Invoke(this, book);
    }

    /// <summary>
    /// Открывает или закрывает панель добавления книг.
    /// </summary>
    [RelayCommand]
    private void ToggleAddBooks()
    {
        if (SelectedCollection is null)
            return;

        IsAddBooksOpen = !IsAddBooksOpen;

        if (!IsAddBooksOpen)
            return;

        BookSearchQuery = string.Empty;
        RefreshAvailableBooks();
    }

    /// <summary>
    /// Закрывает панель добавления книг и очищает поисковый запрос.
    /// </summary>
    [RelayCommand]
    private void CloseAddBooks()
    {
        IsAddBooksOpen = false;
        BookSearchQuery = string.Empty;
    }

    /// <summary>
    /// Переводит выбранную коллекцию в режим переименования.
    /// </summary>
    [RelayCommand]
    private void StartRename()
    {
        if (SelectedCollection is null)
            return;

        RenameCollectionName = SelectedCollection.Name;
        IsRenaming = true;
    }

    /// <summary>
    /// Отменяет переименование коллекции.
    /// </summary>
    [RelayCommand]
    private void CancelRename()
    {
        RenameCollectionName =
            SelectedCollection?.Name ?? string.Empty;

        IsRenaming = false;
    }

    /// <summary>
    /// Создаёт новую коллекцию.
    /// </summary>
    [RelayCommand]
    private void CreateCollection()
    {
        var name = NewCollectionName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        var alreadyExists = Collections.Any(collection =>
            string.Equals(
                collection.Name,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
            return;

        var collection = _collectionService.Create(name);

        if (collection is null)
            return;

        Collections.Add(collection);

        NewCollectionName = string.Empty;
        IsEmpty = false;
        SelectedCollection = collection;
    }

    /// <summary>
    /// Переименовывает выбранную коллекцию.
    /// </summary>
    [RelayCommand]
    private void RenameCollection()
    {
        if (SelectedCollection is null)
            return;

        var name = RenameCollectionName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        var alreadyExists = Collections.Any(collection =>
            collection.Id != SelectedCollection.Id &&
            string.Equals(
                collection.Name,
                name,
                StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
            return;

        SelectedCollection.Name = name;

        _collectionService.Update(
            SelectedCollection);

        OnPropertyChanged(nameof(SelectedCollection));

        IsRenaming = false;
    }

    /// <summary>
    /// Удаляет выбранную коллекцию.
    /// Удаление коллекции не удаляет содержащиеся в ней книги.
    /// </summary>
    [RelayCommand]
    private void DeleteCollection()
    {
        if (SelectedCollection is null)
            return;

        var collection = SelectedCollection;

        _collectionService.Delete(collection.Id);

        Collections.Remove(collection);

        IsEmpty = Collections.Count == 0;
        SelectedCollection = Collections.FirstOrDefault();

        IsAddBooksOpen = false;
        IsRenaming = false;

        RefreshState();
    }

    /// <summary>
    /// Добавляет книгу в выбранную коллекцию.
    /// </summary>
    /// <param name="book">Книга для добавления.</param>
    [RelayCommand]
    private void AddBook(Book? book)
    {
        if (book is null || SelectedCollection is null)
            return;

        if (SelectedCollection.BookIds.Contains(book.Id))
            return;

        SelectedCollection.BookIds.Add(book.Id);

        _collectionService.Update(
            SelectedCollection);

        RefreshCollectionBooks();
        RefreshAvailableBooks();
    }

    /// <summary>
    /// Удаляет книгу из выбранной коллекции.
    /// </summary>
    /// <param name="book">Книга для удаления.</param>
    [RelayCommand]
    private void RemoveBook(Book? book)
    {
        if (book is null || SelectedCollection is null)
            return;

        if (!SelectedCollection.BookIds.Remove(book.Id))
            return;

        _collectionService.Update(
            SelectedCollection);

        RefreshCollectionBooks();
        RefreshAvailableBooks();
    }
}