// Views/MainWindow.axaml.cs
using Avalonia.Input;

namespace Libris.Views;

/// <summary>
/// Представляет главное окно приложения Libris.
/// </summary>
public partial class MainWindow : SukiUI.Controls.SukiWindow
{
    /// <summary>
    /// Инициализирует главное окно приложения.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Обрабатывает нажатие на фон панели с подробной информацией о книге
    /// и закрывает панель деталей.
    /// </summary>
    /// <param name="sender">Элемент, вызвавший событие.</param>
    /// <param name="e">Аргументы события нажатия указателя.</param>
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