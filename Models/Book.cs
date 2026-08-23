// Models/Book.cs
using System;

namespace Libris.Models;

public sealed class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Path to the managed copy stored by Libris.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the imported file contents.
    /// Used to detect duplicate books.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    // Metadata

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Publisher { get; set; }

    public string? Language { get; set; }

    public string? Isbn { get; set; }

    public string? CoverPath { get; set; }

    public DateTime? PublishedAt { get; set; }

    // Library state

    /// <summary>
    /// Reading progress from 0.0 to 1.0.
    /// </summary>
    public double Progress { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
