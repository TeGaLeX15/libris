// Controls/BookCard.axaml.cs
using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Libris.Models;

namespace Libris.Controls;

/// <summary>
/// Карточка книги в библиотеке.
/// </summary>
public partial class BookCard : UserControl
{
    /// <summary>
    /// Событие открытия книги.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? OpenBookRequested;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="BookCard"/>.
    /// </summary>
    public BookCard()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Обрабатывает нажатие на карточку книги.
    /// </summary>
    private void OpenBook_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not Book)
            return;

        OpenBookRequested?.Invoke(this, e);
    }
}

