using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Libris.Services;
using Libris.ViewModels;
using Libris.Views;

namespace Libris;

/// <summary>
/// Представляет точку входа и корневой объект Avalonia-приложения Libris.
/// Отвечает за загрузку XAML, инициализацию сервисов и создание главного окна.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Загружает XAML-разметку приложения и связанные с ней ресурсы.
    /// Вызывается Avalonia при запуске приложения.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Выполняет завершающую инициализацию приложения после запуска Avalonia.
    /// Создаёт необходимые сервисы, загружает сохранённые данные
    /// и устанавливает главное окно приложения для desktop-режима.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Создание сервисов, используемых ViewModel приложения.
            var settingsService = new SettingsService();
            var appDataService = new AppDataService();

            // Загрузка сохранённых данных приложения.
            var appData = appDataService.Load();

            // Создание главного окна и передача корневой ViewModel.
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