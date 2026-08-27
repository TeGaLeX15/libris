// Models/BookCollection.cs
using System;
using System.Collections.Generic;

namespace Libris.Models;

/// <summary>
/// Представляет пользовательскую коллекцию книг.
/// Коллекция хранит идентификаторы входящих в неё книг.
/// </summary>
public sealed class BookCollection
{
    /// <summary>
    /// Уникальный идентификатор коллекции.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Название коллекции.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Идентификаторы книг, входящих в коллекцию.
    /// </summary>
    public List<Guid> BookIds { get; set; } = [];

    /// <summary>
    /// Дата и время создания коллекции.
    /// Хранится в формате UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}