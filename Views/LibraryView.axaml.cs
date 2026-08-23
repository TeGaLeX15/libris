// Views/LibraryView.axaml.cs
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Libris.ViewModels;

namespace Libris.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
    }

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