// ViewModels/MainViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;
using Libris.Services;

namespace Libris.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly AppDataService _appDataService;
    private readonly AppData _appData;

    public LibraryViewModel Library { get; }
    public CollectionsViewModel Collections { get; }
    public SettingsViewModel Settings { get; }
    public BookDetailsViewModel BookDetails { get; }

    [ObservableProperty]
    private ReaderViewModel? reader;

    [ObservableProperty]
    private bool isBookDetailsOpen;

    [ObservableProperty]
    private bool isReaderOpen;

    [ObservableProperty]
    private double detailsPanelOffset = 420;

    [ObservableProperty]
    private double detailsOverlayOpacity;

    public MainViewModel(
        SettingsService settingsService,
        AppDataService appDataService,
        AppData appData)
    {
        _appDataService = appDataService;
        _appData = appData;

        Library = new LibraryViewModel();
        Collections = new CollectionsViewModel();
        Settings = new SettingsViewModel(settingsService);
        BookDetails = new BookDetailsViewModel();

        Library.BookSelected += OnBookSelected;
        Collections.BookSelected += OnBookSelected;
        BookDetails.ReadRequested += OnReadRequested;
        BookDetails.CloseRequested += OnCloseBookDetailsRequested;
    }

    private async void OnBookSelected(
        object? sender,
        Book book)
    {
        await OpenBookDetailsAsync(book);
    }

    private async void OnCloseBookDetailsRequested(
        object? sender,
        EventArgs e)
    {
        await CloseBookDetailsAsync();
    }

    private async Task OpenBookDetailsAsync(Book book)
    {
        BookDetails.Open(book);

        IsBookDetailsOpen = true;

        await AnimateDetailsAsync(
            targetOffset: 0,
            targetOpacity: 1);
    }

    [RelayCommand]
    private async Task CloseBookDetailsAsync()
    {
        if (!IsBookDetailsOpen)
            return;

        await AnimateDetailsAsync(
            targetOffset: 420,
            targetOpacity: 0);

        IsBookDetailsOpen = false;

        BookDetails.ClosePanel();
    }

    private async void OnReadRequested(
        object? sender,
        Book book)
    {
        await CloseBookDetailsAsync();

        Reader = new ReaderViewModel(
            book,
            CloseReader);

        IsReaderOpen = true;
    }

    public void CloseReader()
    {
        IsReaderOpen = false;
        Reader = null;
    }

    private async Task AnimateDetailsAsync(
        double targetOffset,
        double targetOpacity)
    {
        const int steps = 24;
        const int duration = 260;
        const int delay = duration / steps;

        double startOffset = DetailsPanelOffset;
        double startOpacity = DetailsOverlayOpacity;

        for (int i = 1; i <= steps; i++)
        {
            double progress = i / (double)steps;

            // Smooth ease-out.
            double eased =
                1 - Math.Pow(1 - progress, 3);

            DetailsPanelOffset =
                startOffset +
                (targetOffset - startOffset) * eased;

            DetailsOverlayOpacity =
                startOpacity +
                (targetOpacity - startOpacity) * eased;

            await Task.Delay(delay);
        }

        DetailsPanelOffset = targetOffset;
        DetailsOverlayOpacity = targetOpacity;
    }
}