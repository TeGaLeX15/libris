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

public partial class LibraryViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public ObservableCollection<Book> Books { get; } = [];
    public ObservableCollection<Book> FilteredBooks { get; } = [];

    public IReadOnlyList<string> SortOptions { get; } =
    [
        "Title: A–Z",
        "Title: Z–A",
        "Author: A–Z",
        "Author: Z–A"
    ];

    [ObservableProperty]
    private bool isEmpty;

    [ObservableProperty]
    private bool isSearchEmpty;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string selectedSort = "Title: A–Z";

    public LibraryViewModel()
    {
        _libraryService = new LibraryService();

        LoadBooks();
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedSortChanged(string value)
    {
        ApplyFilter();
    }

    private void LoadBooks()
    {
        Books.Clear();

        foreach (var book in _libraryService.Load())
        {
            Books.Add(book);
        }

        ApplyFilter();
    }

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

        ApplyFilter();
    }

    public void RemoveBook(Book? book)
    {
        if (book is null)
            return;

        _libraryService.Remove(book.Id);
        Books.Remove(book);

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = SearchQuery.Trim();

        IEnumerable<Book> result = Books;

        // Search
        if (!string.IsNullOrWhiteSpace(query))
        {
            result = result.Where(book =>
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

        // Sorting
        result = SelectedSort switch
        {
            "Title: A–Z" => result
                .OrderBy(book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            "Title: Z–A" => result
                .OrderByDescending(book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            "Author: A–Z" => result
                .OrderBy(book => book.Author ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            "Author: Z–A" => result
                .OrderByDescending(book => book.Author ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(book => book.Title ?? string.Empty,
                    StringComparer.CurrentCultureIgnoreCase),

            _ => result
        };

        FilteredBooks.Clear();

        foreach (var book in result)
        {
            FilteredBooks.Add(book);
        }

        IsEmpty = Books.Count == 0;

        IsSearchEmpty =
            !IsEmpty &&
            !string.IsNullOrWhiteSpace(query) &&
            FilteredBooks.Count == 0;
    }
}
