// Models/Book.cs

using System;

namespace Libris.Models;

/// <summary>
/// Представляет книгу, хранящуюся в библиотеке Libris.
/// Содержит сведения о файле, метаданные книги и состояние чтения.
/// </summary>
public sealed class Book
{
    /// <summary>
    /// Уникальный идентификатор книги в библиотеке.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Путь к управляемой копии файла книги,
    /// хранящейся в директории Libris.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 хеш содержимого импортированного файла.
    /// Используется для обнаружения дубликатов книг.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    // Метаданные книги

    /// <summary>
    /// Название книги.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Автор книги.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Описание или аннотация книги.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Издательство книги.
    /// </summary>
    public string? Publisher { get; set; }

    /// <summary>
    /// Язык, на котором написана книга.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Международный стандартный номер книги (ISBN).
    /// </summary>
    public string? Isbn { get; set; }

    /// <summary>
    /// Путь к файлу обложки книги.
    /// </summary>
    public string? CoverPath { get; set; }

    /// <summary>
    /// Дата публикации книги.
    /// Если дата неизвестна, значение равно <see langword="null"/>.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    // Состояние книги в библиотеке

    /// <summary>
    /// Текущий прогресс чтения книги.
    /// Значение должно находиться в диапазоне от 0.0 до 1.0,
    /// где 0.0 означает отсутствие прогресса, а 1.0 — полностью прочитанную книгу.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Дата и время добавления книги в библиотеку.
    /// Хранится в формате UTC.
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}