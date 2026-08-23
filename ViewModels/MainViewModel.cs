// ViewModels/MainViewModel.cs
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
    }
}