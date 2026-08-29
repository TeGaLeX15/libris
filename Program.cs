// Program.cs
using System;
using System.Runtime.InteropServices;
using Avalonia;

namespace Libris;

/// <summary>
/// Содержит точку входа и конфигурацию Avalonia-приложения Libris.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Запускает приложение Libris.
    /// </summary>
    /// <param name="args">Аргументы командной строки.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            ShowError(ex);
        }
    }

    /// <summary>
    /// Отображает пользователю сообщение о критической ошибке.
    /// </summary>
    private static void ShowError(Exception ex)
    {
        var message =
            "Libris crashed.\n\n" +
            ex + "\n\n" +
            "Press OK to close the application.";

        MessageBox(
            IntPtr.Zero,
            message,
            "Libris — Critical Error",
            0x00000010);
    }

    /// <summary>
    /// Отображает стандартное системное окно Windows.
    /// </summary>
    [DllImport(
        "user32.dll",
        CharSet = CharSet.Unicode)]
    private static extern int MessageBox(
        IntPtr hWnd,
        string text,
        string caption,
        uint type);

    /// <summary>
    /// Создаёт и настраивает экземпляр Avalonia-приложения.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}