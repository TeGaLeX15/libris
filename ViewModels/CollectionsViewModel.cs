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

public partial class CollectionsViewModel : ViewModelBase
{
    private readonly CollectionService _collectionService;
    private readonly LibraryService _libraryService;

    public ObservableCollection<BookCollection> Collections { get; } = [];
    public ObservableCollection<Book> CollectionBooks { get; } = [];
    public ObservableCollection<Book> AvailableBooks { get; } = [];
    public ObservableCollection<Book> FilteredAvailableBooks { get; } = [];

    [ObservableProperty]
    private BookCollection? selectedCollection;

    [ObservableProperty]
    private Book? selectedAvailableBook;

    [ObservableProperty]
    private string newCollectionName = string.Empty;

    [ObservableProperty]
    private string renameCollectionName = string.Empty;

    [ObservableProperty]
    private string bookSearchQuery = string.Empty;

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private bool isAddBooksOpen;

    public CollectionsViewModel()
    {
        _collectionService = new CollectionService();
        _libraryService = new LibraryService();

        LoadCollections();
    }

    partial void OnSelectedCollectionChanged(BookCollection? value)
    {
        RenameCollectionName = value?.Name ?? string.Empty;

        RefreshCollectionBooks();
        RefreshAvailableBooks();

        SelectedAvailableBook = null;
        IsAddBooksOpen = false;
    }

    partial void OnBookSearchQueryChanged(string value)
    {
        ApplyBookFilter();
    }

    private void LoadCollections()
    {
        Collections.Clear();

        foreach (var collection in _collectionService.Load())
        {
            Collections.Add(collection);
        }

        IsEmpty = Collections.Count == 0;
        SelectedCollection = Collections.FirstOrDefault();
    }

    private void RefreshCollectionBooks()
    {
        CollectionBooks.Clear();

        if (SelectedCollection is null)
            return;

        var books = _libraryService.Load();

        foreach (var bookId in SelectedCollection.BookIds)
        {
            var book = books.FirstOrDefault(x => x.Id == bookId);

            if (book is not null)
            {
                CollectionBooks.Add(book);
            }
        }
    }

    private void RefreshAvailableBooks()
    {
        AvailableBooks.Clear();

        if (SelectedCollection is null)
        {
            FilteredAvailableBooks.Clear();
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

    private void ApplyBookFilter()
    {
        FilteredAvailableBooks.Clear();

        var query = BookSearchQuery.Trim();

        IEnumerable<Book> books = AvailableBooks;

        if (!string.IsNullOrWhiteSpace(query))
        {
            books = books.Where(book =>
                (!string.IsNullOrWhiteSpace(book.Title) &&
                 book.Title.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase))
                ||
                (!string.IsNullOrWhiteSpace(book.Author) &&
                 book.Author.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var book in books)
        {
            FilteredAvailableBooks.Add(book);
        }
    }

    [RelayCommand]
    private void ToggleAddBooks()
    {
        IsAddBooksOpen = !IsAddBooksOpen;

        if (IsAddBooksOpen)
        {
            BookSearchQuery = string.Empty;
            SelectedAvailableBook = null;

            RefreshAvailableBooks();
        }
    }

    [RelayCommand]
    private void CreateCollection()
    {
        var name = NewCollectionName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        if (Collections.Any(x =>
                string.Equals(
                    x.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var collection = _collectionService.Create(name);

        if (collection is null)
            return;

        Collections.Add(collection);

        NewCollectionName = string.Empty;
        IsEmpty = false;
        SelectedCollection = collection;
    }

    [RelayCommand]
    private void RenameCollection()
    {
        if (SelectedCollection is null)
            return;

        var name = RenameCollectionName.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        if (Collections.Any(x =>
                x.Id != SelectedCollection.Id &&
                string.Equals(
                    x.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        SelectedCollection.Name = name;

        _collectionService.Update(SelectedCollection);

        OnPropertyChanged(nameof(SelectedCollection));
    }

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
    }

    [RelayCommand]
    private void AddSelectedBook()
    {
        if (SelectedAvailableBook is null)
            return;

        AddBook(SelectedAvailableBook);

        SelectedAvailableBook = null;
    }

    private void AddBook(Book? book)
    {
        if (book is null || SelectedCollection is null)
            return;

        if (SelectedCollection.BookIds.Contains(book.Id))
            return;

        SelectedCollection.BookIds.Add(book.Id);

        _collectionService.Update(SelectedCollection);

        RefreshCollectionBooks();
        RefreshAvailableBooks();
    }

    [RelayCommand]
    private void RemoveBook(Book? book)
    {
        if (book is null || SelectedCollection is null)
            return;

        SelectedCollection.BookIds.Remove(book.Id);

        _collectionService.Update(SelectedCollection);

        RefreshCollectionBooks();
        RefreshAvailableBooks();
    }
}
