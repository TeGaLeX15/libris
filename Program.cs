// Program.cs
using Avalonia;
using System;
using System.Runtime.InteropServices;

namespace Libris;

/// <summary>
/// Содержит точку входа и конфигурацию Avalonia-приложения Libris.
/// </summary>
sealed class Program
{
    /// <summary>
    /// Запускает приложение Libris и обрабатывает необработанные исключения,
    /// возникшие во время работы приложения.
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
    /// Отображает пользователю сообщение о критической ошибке
    /// и завершении работы приложения.
    /// </summary>
    /// <param name="ex">Исключение, вызвавшее аварийное завершение.</param>
    private static void ShowError(Exception ex)
    {
        var message =
            "Libris crashed.\n\n" +
            ex + "\n\n" +
            "Press OK to close the application.";

        // Используется системное окно ошибки Windows.
        MessageBox(
            IntPtr.Zero,
            message,
            "Libris — Critical Error",
            0x00000010);
    }

    /// <summary>
    /// Отображает стандартное системное диалоговое окно Windows.
    /// </summary>
    /// <param name="hWnd">Дескриптор родительского окна.</param>
    /// <param name="text">Текст сообщения.</param>
    /// <param name="caption">Заголовок окна.</param>
    /// <param name="type">Тип и кнопки диалогового окна.</param>
    /// <returns>Идентификатор нажатой кнопки.</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(
        IntPtr hWnd,
        string text,
        string caption,
        uint type);

    /// <summary>
    /// Создаёт и настраивает экземпляр Avalonia-приложения Libris.
    /// </summary>
    /// <returns>Настроенный объект <see cref="AppBuilder"/>.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            // Инструменты разработчика доступны только в Debug-сборке.
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}