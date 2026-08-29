using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Libris.Services;
using Libris.ViewModels;
using Libris.Views;

namespace Libris;

/// <summary>
/// Представляет точку входа и корневой объект Avalonia-приложения Libris.
/// Отвечает за загрузку XAML, инициализацию сервисов
/// и создание главного окна приложения.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Сервис пользовательских настроек приложения.
    /// </summary>
    public SettingsService SettingsService { get; private set; } = null!;

    /// <summary>
    /// Сервис хранения данных приложения.
    /// </summary>
    public AppDataService AppDataService { get; private set; } = null!;

    /// <summary>
    /// Загружает XAML-разметку приложения.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Выполняет инициализацию приложения после запуска Avalonia.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SettingsService = new SettingsService();
            AppDataService = new AppDataService();

            var appData = AppDataService.Load();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(
                    SettingsService,
                    AppDataService,
                    appData)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}