// Models/Book.cs
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Libris.Models;

/// <summary>
/// Представляет книгу, хранящуюся в библиотеке Libris.
/// Содержит сведения о файле, метаданные книги и состояние чтения.
/// </summary>
public partial class Book : ObservableObject
{
    /// <summary>
    /// Уникальный идентификатор книги в библиотеке.
    /// </summary>
    [ObservableProperty]
    private Guid id = Guid.NewGuid();

    /// <summary>
    /// Путь к управляемой копии файла книги,
    /// хранящейся в директории Libris.
    /// </summary>
    [ObservableProperty]
    private string filePath = string.Empty;

    /// <summary>
    /// SHA-256 хеш содержимого импортированного файла.
    /// Используется для обнаружения дубликатов книг.
    /// </summary>
    [ObservableProperty]
    private string fileHash = string.Empty;

    /// <summary>
    /// Название книги.
    /// </summary>
    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>
    /// Автор книги.
    /// </summary>
    [ObservableProperty]
    private string author = string.Empty;

    /// <summary>
    /// Описание или аннотация книги.
    /// </summary>
    [ObservableProperty]
    private string? description;

    /// <summary>
    /// Издательство книги.
    /// </summary>
    [ObservableProperty]
    private string? publisher;

    /// <summary>
    /// Язык, на котором написана книга.
    /// </summary>
    [ObservableProperty]
    private string? language;

    /// <summary>
    /// Международный стандартный номер книги (ISBN).
    /// </summary>
    [ObservableProperty]
    private string? isbn;

    /// <summary>
    /// Путь к файлу обложки книги.
    /// </summary>
    [ObservableProperty]
    private string? coverPath;

    /// <summary>
    /// Дата публикации книги.
    /// Если дата неизвестна, значение равно null.
    /// </summary>
    [ObservableProperty]
    private DateTime? publishedAt;

    /// <summary>
    /// Текущий прогресс чтения книги.
    /// Значение находится в диапазоне от 0.0 до 1.0.
    /// </summary>
    [ObservableProperty]
    private double progress;

    /// <summary>
    /// Дата и время добавления книги в библиотеку.
    /// Хранится в формате UTC.
    /// </summary>
    [ObservableProperty]
    private DateTime addedAt = DateTime.UtcNow;
}