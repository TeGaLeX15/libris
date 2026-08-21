// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Libris.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private AppPage currentPage = AppPage.Library;

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentPage = AppPage.Library;
    }

    [RelayCommand]
    private void NavigateToCollections()
    {
        CurrentPage = AppPage.Collections;
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPage = AppPage.Settings;
    }
}