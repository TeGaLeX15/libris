// Models/Book.cs
using System;

namespace Libris.Models;

public sealed class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FilePath { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? CoverPath { get; set; }

    public double Progress { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}