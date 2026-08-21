// Models/AppSettings.cs
namespace Libris.Models;

public sealed class AppSettings
{
    public string Theme { get; set; } = "System";

    public string AccentColor { get; set; } = "Blue";

    public string DefaultFont { get; set; } = "Inter";

    public double FontSize { get; set; } = 16;

    public double LineSpacing { get; set; } = 1.5;

    public double ReadingWidth { get; set; } = 720;

    public string DefaultSorting { get; set; } = "Recently Added";

    public bool ShowProgress { get; set; } = true;

    public double CoverSize { get; set; } = 160;
}