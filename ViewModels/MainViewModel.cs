// ViewModels/MainViewModel.cs
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Models;
using Libris.Services;

namespace Libris.ViewModels;

/// <summary>
/// Главный ViewModel приложения, координирующий страницы библиотеки,
/// коллекций и настроек, а также панели информации о книге и режима чтения.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private const double DetailsPanelClosedOffset = 420;
    private const int AnimationSteps = 24;
    private const int AnimationDuration = 260;

    private readonly AppDataService _appDataService;
    private readonly AppData _appData;

    /// <summary>
    /// ViewModel страницы библиотеки.
    /// </summary>
    public LibraryViewModel Library { get; }

    /// <summary>
    /// ViewModel страницы коллекций.
    /// </summary>
    public CollectionsViewModel Collections { get; }

    /// <summary>
    /// ViewModel страницы настроек.
    /// </summary>
    public SettingsViewModel Settings { get; }

    /// <summary>
    /// ViewModel панели с подробной информацией о выбранной книге.
    /// </summary>
    public BookDetailsViewModel BookDetails { get; }

    /// <summary>
    /// Текущий ViewModel режима чтения.
    /// </summary>
    [ObservableProperty]
    private ReaderViewModel? reader;

    /// <summary>
    /// Определяет, открыта ли панель с информацией о книге.
    /// </summary>
    [ObservableProperty]
    private bool isBookDetailsOpen;

    /// <summary>
    /// Определяет, открыт ли режим чтения.
    /// </summary>
    [ObservableProperty]
    private bool isReaderOpen;

    /// <summary>
    /// Текущее горизонтальное смещение панели информации о книге.
    /// </summary>
    [ObservableProperty]
    private double detailsPanelOffset = DetailsPanelClosedOffset;

    /// <summary>
    /// Текущая прозрачность затемняющего слоя поверх содержимого.
    /// </summary>
    [ObservableProperty]
    private double detailsOverlayOpacity;

    /// <summary>
    /// Инициализирует главный ViewModel приложения.
    /// </summary>
    /// <param name="settingsService">
    /// Сервис для загрузки и сохранения настроек приложения.
    /// </param>
    /// <param name="appDataService">
    /// Сервис для загрузки и сохранения общих данных приложения.
    /// </param>
    /// <param name="appData">
    /// Загруженные данные приложения.
    /// </param>
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

    /// <summary>
    /// Обрабатывает выбор книги в библиотеке или коллекции.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="book">Выбранная книга.</param>
    private async void OnBookSelected(object? sender, Book book)
    {
        await OpenBookDetailsAsync(book);
    }

    /// <summary>
    /// Обрабатывает запрос на закрытие панели информации о книге.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="e">Аргументы события.</param>
    private async void OnCloseBookDetailsRequested(
        object? sender,
        EventArgs e)
    {
        await CloseBookDetailsAsync();
    }

    /// <summary>
    /// Открывает панель с подробной информацией о книге
    /// и запускает её анимацию появления.
    /// </summary>
    /// <param name="book">Книга, информацию о которой необходимо показать.</param>
    private async Task OpenBookDetailsAsync(Book book)
    {
        BookDetails.Open(book);
        IsBookDetailsOpen = true;

        await AnimateDetailsAsync(
            targetOffset: 0,
            targetOpacity: 1);
    }

    /// <summary>
    /// Закрывает панель с подробной информацией о книге
    /// и запускает её анимацию исчезновения.
    /// </summary>
    [RelayCommand]
    private async Task CloseBookDetailsAsync()
    {
        if (!IsBookDetailsOpen)
            return;

        await AnimateDetailsAsync(
            targetOffset: DetailsPanelClosedOffset,
            targetOpacity: 0);

        IsBookDetailsOpen = false;
        BookDetails.ClosePanel();
    }

    /// <summary>
    /// Обрабатывает запрос на открытие книги для чтения.
    /// Сначала закрывает панель информации, после чего открывает Reader.
    /// </summary>
    /// <param name="sender">Источник события.</param>
    /// <param name="book">Книга, которую необходимо открыть.</param>
    private async void OnReadRequested(object? sender, Book book)
    {
        await CloseBookDetailsAsync();

        Reader = new ReaderViewModel(
            book,
            CloseReader);

        IsReaderOpen = true;
    }

    /// <summary>
    /// Закрывает текущий режим чтения и освобождает его ViewModel.
    /// </summary>
    public void CloseReader()
    {
        IsReaderOpen = false;
        Reader = null;
    }

    /// <summary>
    /// Анимирует положение панели информации и прозрачность затемняющего слоя.
    /// Используется плавная функция ease-out.
    /// </summary>
    /// <param name="targetOffset">
    /// Конечное горизонтальное смещение панели.
    /// </param>
    /// <param name="targetOpacity">
    /// Конечная прозрачность затемняющего слоя.
    /// </param>
    private async Task AnimateDetailsAsync(
        double targetOffset,
        double targetOpacity)
    {
        var delay = AnimationDuration / AnimationSteps;
        var startOffset = DetailsPanelOffset;
        var startOpacity = DetailsOverlayOpacity;

        for (var step = 1; step <= AnimationSteps; step++)
        {
            var progress = step / (double)AnimationSteps;

            // Плавное замедление анимации к концу.
            var easedProgress = 1 - Math.Pow(1 - progress, 3);

            DetailsPanelOffset =
                startOffset +
                (targetOffset - startOffset) * easedProgress;

            DetailsOverlayOpacity =
                startOpacity +
                (targetOpacity - startOpacity) * easedProgress;

            await Task.Delay(delay);
        }

        DetailsPanelOffset = targetOffset;
        DetailsOverlayOpacity = targetOpacity;
    }
}