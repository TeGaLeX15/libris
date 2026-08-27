// Views/LibraryView.axaml.cs
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Libris.Models;
using Libris.ViewModels;

namespace Libris.Views;

/// <summary>
/// Представляет представление библиотеки книг.
/// Отвечает за взаимодействие с пользовательским интерфейсом библиотеки
/// и обработку выбора и добавления книг.
/// </summary>
public partial class LibraryView : UserControl
{
    /// <summary>
    /// Инициализирует новое представление библиотеки.
    /// </summary>
    public LibraryView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Обрабатывает нажатие на карточку книги
    /// и открывает выбранную книгу через модель представления.
    /// </summary>
    /// <param name="sender">Элемент управления, на котором произошло нажатие.</param>
    /// <param name="e">Аргументы события нажатия указателя.</param>
    private void BookCard_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (sender is not Control control)
            return;

        if (control.DataContext is not Book book)
            return;

        if (DataContext is not LibraryViewModel viewModel)
            return;

        viewModel.SelectBook(book);
    }

    /// <summary>
    /// Открывает системный диалог выбора файлов
    /// и добавляет выбранные книги в библиотеку.
    /// </summary>
    /// <param name="sender">Элемент управления, вызвавший событие.</param>
    /// <param name="e">Аргументы события маршрутизации.</param>
    private async void AddBooks_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not LibraryViewModel viewModel)
            return;

        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
            return;

        var files = await topLevel.StorageProvider
            .OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Add books",
                    AllowMultiple = true,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Books")
                        {
                            Patterns =
                            [
                                "*.epub",
                                "*.fb2",
                                "*.pdf",
                                "*.txt"
                            ]
                        }
                    ]
                });

        if (files.Count == 0)
            return;

        var filePaths = files
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => path!);

        await viewModel.AddBooksAsync(filePaths);
    }
}