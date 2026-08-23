// Views/LibraryView.axaml.cs
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Interactivity;
using Libris.ViewModels;

namespace Libris.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
    }

    private async void AddBooks_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
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

        var paths = files
            .Select(file => file.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToList();

        if (DataContext is LibraryViewModel viewModel)
        {
            viewModel.AddBooks(paths);
        }
    }
}