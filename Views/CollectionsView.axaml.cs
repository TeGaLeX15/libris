// Views/CollectionsView.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using Libris.Controls;
using Libris.Models;
using Libris.ViewModels;

namespace Libris.Views;

/// <summary>
/// Представляет представление страницы коллекций книг.
/// </summary>
public partial class CollectionsView : UserControl
{
    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CollectionsView"/>.
    /// </summary>
    public CollectionsView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Открывает книгу, переданную карточкой.
    /// </summary>
    private void BookCard_OpenBookRequested(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not BookCard bookCard)
            return;

        if (bookCard.DataContext is not Book book)
            return;

        if (DataContext is not CollectionsViewModel viewModel)
            return;

        viewModel.SelectBook(book);
    }
}