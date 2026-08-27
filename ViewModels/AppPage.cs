// ViewModels/AppPage.cs
namespace Libris.ViewModels;

/// <summary>
/// Определяет основные страницы приложения Libris.
/// </summary>
public enum AppPage
{
    /// <summary>
    /// Страница библиотеки с доступными книгами.
    /// </summary>
    Library,

    /// <summary>
    /// Страница пользовательских коллекций книг.
    /// </summary>
    Collections,

    /// <summary>
    /// Страница настроек приложения.
    /// </summary>
    Settings
}