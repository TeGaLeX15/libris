// ViewModels/BookDetailsViewModel.cs
using System;
using System.IO;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;

namespace Libris.ViewModels;

public partial class BookDetailsViewModel : ObservableObject
{
    [ObservableProperty]
    private Book? book;

    [ObservableProperty]
    private bool isOpen;

    public event EventHandler? CloseRequested;

    public event EventHandler<Book>? ReadRequested;

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
            catch
            {
                return null;
            }
        }
    }

    public string Title =>
        Book?.Title ?? "Unknown title";

    public string Author =>
        string.IsNullOrWhiteSpace(Book?.Author)
            ? "Unknown author"
            : Book.Author;

    public string Format =>
        string.IsNullOrWhiteSpace(Book?.FilePath)
            ? "Unknown"
            : Path.GetExtension(Book.FilePath)
                .TrimStart('.')
                .ToUpperInvariant();

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
            catch
            {
                return "Unknown";
            }
        }
    }

    public void Open(Book selectedBook)
    {
        Book = selectedBook;
        IsOpen = true;

        OnPropertyChanged(nameof(Cover));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Author));
        OnPropertyChanged(nameof(Format));
        OnPropertyChanged(nameof(FileName));
        OnPropertyChanged(nameof(FileSize));
    }

    [RelayCommand]
    private void Close()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ClosePanel()
    {
        IsOpen = false;
    }

    [RelayCommand]
    private void Read()
    {
        if (Book is null)
            return;

        ReadRequested?.Invoke(this, Book);
    }
}