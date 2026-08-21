// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Libris.Services;

namespace Libris.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;

    public LibraryViewModel Library { get; }

    public CollectionsViewModel Collections { get; }

    public SettingsViewModel Settings { get; }

    [ObservableProperty]
    private AppPage currentPage = AppPage.Library;

    [ObservableProperty]
    private ViewModelBase currentViewModel;

    public MainViewModel()
    {
        _settingsService = new SettingsService();

        Library = new LibraryViewModel();
        Collections = new CollectionsViewModel();

        Settings = new SettingsViewModel(_settingsService);

        currentViewModel = Library;
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentPage = AppPage.Library;
        CurrentViewModel = Library;
    }

    [RelayCommand]
    private void NavigateToCollections()
    {
        CurrentPage = AppPage.Collections;
        CurrentViewModel = Collections;
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = AppPage.Settings;
        CurrentViewModel = Settings;
    }
}