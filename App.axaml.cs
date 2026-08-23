using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Libris.Services;
using Libris.ViewModels;
using Libris.Views;

namespace Libris;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = new SettingsService();
            var appDataService = new AppDataService();

            var appData = appDataService.Load();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(
                    settingsService,
                    appDataService,
                    appData)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}