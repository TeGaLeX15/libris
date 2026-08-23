// ViewModels/MainViewModel.cs
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

    [ObservableProperty]
    private AppPage currentPage = AppPage.Library;

    [ObservableProperty]
    private ViewModelBase currentViewModel;

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

        currentViewModel = Library;

        RestoreLastPage();
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentPage = AppPage.Library;
        CurrentViewModel = Library;

        SaveLastPage();
    }

    [RelayCommand]
    private void NavigateToCollections()
    {
        CurrentPage = AppPage.Collections;
        CurrentViewModel = Collections;

        SaveLastPage();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = AppPage.Settings;
        CurrentViewModel = Settings;

        SaveLastPage();
    }

    private void RestoreLastPage()
    {
        switch (_appData.LastOpenedPage)
        {
            case "Collections":
                CurrentPage = AppPage.Collections;
                CurrentViewModel = Collections;
                break;

            case "Settings":
                CurrentPage = AppPage.Settings;
                CurrentViewModel = Settings;
                break;

            default:
                CurrentPage = AppPage.Library;
                CurrentViewModel = Library;
                break;
        }
    }

    private void SaveLastPage()
    {
        _appData.LastOpenedPage = CurrentPage.ToString();
        _appDataService.Save(_appData);
    }
}