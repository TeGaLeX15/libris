// Models/AppSettings.cs
namespace Libris.Models;

/// <summary>
/// Содержит пользовательские настройки приложения Libris,
/// сохраняемые между запусками.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Определяет тему оформления приложения.
    /// </summary>
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Определяет основной акцентный цвет интерфейса.
    /// </summary>
    public string AccentColor { get; set; } = "Blue";

    /// <summary>
    /// Определяет шрифт, используемый для отображения текста книги.
    /// </summary>
    public string DefaultFont { get; set; } = "Inter";

    /// <summary>
    /// Определяет размер шрифта текста книги.
    /// </summary>
    public double FontSize { get; set; } = 16;

    /// <summary>
    /// Определяет межстрочный интервал текста книги.
    /// </summary>
    public double LineSpacing { get; set; } = 1.5;

    /// <summary>
    /// Определяет ширину области чтения.
    /// </summary>
    public double ReadingWidth { get; set; } = 720;

    /// <summary>
    /// Определяет стандартный способ сортировки книг.
    /// </summary>
    public string DefaultSorting { get; set; } = "Recently Added";

    /// <summary>
    /// Определяет, следует ли отображать прогресс чтения.
    /// </summary>
    public bool ShowProgress { get; set; } = true;

    /// <summary>
    /// Определяет размер обложки книги в библиотеке.
    /// </summary>
    public double CoverSize { get; set; } = 160;
}