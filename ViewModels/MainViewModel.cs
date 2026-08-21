using CommunityToolkit.Mvvm.ComponentModel;

namespace Libris.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Привет, Libris!";
}
