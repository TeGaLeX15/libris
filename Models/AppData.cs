// Models/AppData.cs
namespace Libris.Models;

/// <summary>
/// Содержит данные приложения, которые сохраняются между запусками.
/// </summary>
public sealed class AppData
{
    /// <summary>
    /// Хранит название страницы, которая была открыта последней.
    /// </summary>
    public string LastOpenedPage { get; set; } = "Library";
}