// ViewModels/ReaderViewModel.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;

namespace Libris.ViewModels;

public partial class ReaderViewModel : ObservableObject
{
    private readonly Book _book;
    private readonly Action? _closeReader;

    public ReaderViewModel(
        Book book,
        Action? closeReader = null)
    {
        _book = book;
        _closeReader = closeReader;

        ReadingWidth = 760;
        FontSize = 18;
        LineHeight = 1.6;

        UpdateProgressText();
    }

    public Book Book => _book;

    public string Title => _book.Title;

    public string Author => _book.Author;

    public string? CoverPath => _book.CoverPath;

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

    [ObservableProperty]
    private double _readingWidth;

    [ObservableProperty]
    private double _fontSize;

    [ObservableProperty]
    private double _lineHeight;

    public double Progress
    {
        get => _book.Progress;
        set
        {
            var valueClamped = Math.Clamp(value, 0.0, 1.0);

            if (Math.Abs(_book.Progress - valueClamped) < 0.001)
                return;

            _book.Progress = valueClamped;

            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressText));
            OnPropertyChanged(nameof(PositionText));
        }
    }

    public string ProgressText =>
        $"{Math.Round(Progress * 100):0}%";

    public string PositionText =>
        $"Page 1 • {ProgressText}";

    [RelayCommand]
    private void Close()
    {
        _closeReader?.Invoke();
    }

    [RelayCommand]
    private void PreviousPage()
    {
        // TODO: перейти на предыдущую страницу.
    }

    [RelayCommand]
    private void NextPage()
    {
        // TODO: перейти на следующую страницу.
    }

    partial void OnReadingWidthChanged(double value)
    {
        OnPropertyChanged(nameof(ReadingWidth));
    }

    partial void OnFontSizeChanged(double value)
    {
        OnPropertyChanged(nameof(FontSize));
    }

    partial void OnLineHeightChanged(double value)
    {
        OnPropertyChanged(nameof(LineHeight));
    }

    private void UpdateProgressText()
    {
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(PositionText));
    }
}