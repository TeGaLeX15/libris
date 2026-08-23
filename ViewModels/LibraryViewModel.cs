// ViewModels/LibraryViewModel.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Libris.Models;
using Libris.Services;

namespace Libris.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    private readonly LibraryService _libraryService;

    public ObservableCollection<Book> Books { get; } = [];

    [ObservableProperty]
    private bool isEmpty;

    public LibraryViewModel()
    {
        _libraryService = new LibraryService();
        LoadBooks();
    }

    private void LoadBooks()
    {
        Books.Clear();

        foreach (var book in _libraryService.Load())
        {
            Books.Add(book);
        }

        UpdateEmptyState();
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

        UpdateEmptyState();
    }

    public void RemoveBook(Book? book)
    {
        if (book is null)
            return;

        _libraryService.Remove(book.Id);
        Books.Remove(book);

        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        IsEmpty = Books.Count == 0;
    }
}