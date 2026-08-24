// Views/MainWindow.axaml.cs
using Avalonia.Input;

namespace Libris.Views;

public partial class MainWindow : SukiUI.Controls.SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void DetailsBackdrop_PointerPressed(
        object? sender,
        PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel viewModel)
        {
            viewModel.CloseBookDetailsCommand.Execute(null);
        }
    }
}