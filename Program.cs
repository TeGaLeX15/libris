using Avalonia;
using System;
using System.Runtime.InteropServices;

namespace Libris;

sealed class Program
{
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

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(
        IntPtr hWnd,
        string text,
        string caption,
        uint type);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
