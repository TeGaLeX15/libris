// Models/AppData.cs
using System.Collections.Generic;

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

    /// <summary>
    /// Сохраняемые позиции чтения для книг.
    /// Ключом является идентификатор книги.
    /// </summary>
    public Dictionary<string, ReaderPosition> ReadingPositions { get; set; } = [];
}

/// <summary>
/// Позиция чтения конкретной книги.
/// </summary>
public sealed class ReaderPosition
{
    /// <summary>
    /// Индекс текущей главы.
    /// </summary>
    public int Chapter { get; set; }

    /// <summary>
    /// Прогресс внутри текущей главы от 0 до 1.
    /// </summary>
    public double ChapterProgress { get; set; }

    /// <summary>
    /// Общий прогресс чтения книги от 0 до 1.
    /// </summary>
    public double Progress { get; set; }
}