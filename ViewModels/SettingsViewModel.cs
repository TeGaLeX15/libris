// ViewModels/SettingsViewModel.cs
using System.Collections.Generic;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Libris.Models;
using Libris.Services;
using SukiUI;
using SukiUI.Enums;

namespace Libris.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;

    private readonly SukiTheme _sukiTheme;

    public IReadOnlyList<string> AvailableThemes { get; } =
    [
        "System",
        "Light",
        "Dark"
    ];

    public IReadOnlyList<string> AvailableAccentColors { get; } =
    [
        "Blue",
        "Red",
        "Green",
        "Orange"
    ];

    public IReadOnlyList<string> AvailableFonts { get; } =
    [
        "Inter",
        "Arial",
        "Georgia",
        "Times New Roman"
    ];

    public IReadOnlyList<string> AvailableSortingOptions { get; } =
    [
        "Recently Added",
        "Title",
        "Author",
        "Progress"
    ];

    [ObservableProperty]
    private string theme = "System";

    [ObservableProperty]
    private string accentColor = "Blue";

    [ObservableProperty]
    private string defaultFont = "Inter";

    [ObservableProperty]
    private double fontSize = 16;

    [ObservableProperty]
    private double lineSpacing = 1.5;

    [ObservableProperty]
    private double readingWidth = 800;

    [ObservableProperty]
    private string defaultSorting = "Recently Added";

    [ObservableProperty]
    private bool showProgress = true;

    [ObservableProperty]
    private double coverSize = 180;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settings = _settingsService.Load();

        _sukiTheme = SukiTheme.GetInstance();

        Theme = _settings.Theme;
        AccentColor = _settings.AccentColor;
        DefaultFont = _settings.DefaultFont;
        FontSize = _settings.FontSize;
        LineSpacing = _settings.LineSpacing;
        ReadingWidth = _settings.ReadingWidth;
        DefaultSorting = _settings.DefaultSorting;
        ShowProgress = _settings.ShowProgress;
        CoverSize = _settings.CoverSize;

        ApplyTheme(Theme);
        ApplyAccentColor(AccentColor);
    }

    partial void OnThemeChanged(string value)
    {
        _settings.Theme = value;

        ApplyTheme(value);
        Save();
    }

    partial void OnAccentColorChanged(string value)
    {
        _settings.AccentColor = value;

        ApplyAccentColor(value);
        Save();
    }

    partial void OnDefaultFontChanged(string value)
    {
        _settings.DefaultFont = value;
        Save();
    }

    partial void OnFontSizeChanged(double value)
    {
        _settings.FontSize = value;
        Save();
    }

    partial void OnLineSpacingChanged(double value)
    {
        _settings.LineSpacing = value;
        Save();
    }

    partial void OnReadingWidthChanged(double value)
    {
        _settings.ReadingWidth = value;
        Save();
    }

    partial void OnDefaultSortingChanged(string value)
    {
        _settings.DefaultSorting = value;
        Save();
    }

    partial void OnShowProgressChanged(bool value)
    {
        _settings.ShowProgress = value;
        Save();
    }

    partial void OnCoverSizeChanged(double value)
    {
        _settings.CoverSize = value;
        Save();
    }

    private void ApplyTheme(string value)
    {
        switch (value)
        {
            case "Light":
                _sukiTheme.ChangeBaseTheme(ThemeVariant.Light);
                break;

            case "Dark":
                _sukiTheme.ChangeBaseTheme(ThemeVariant.Dark);
                break;

            default:
                _sukiTheme.ChangeBaseTheme(ThemeVariant.Default);
                break;
        }
    }

    private void ApplyAccentColor(string value)
    {
        switch (value)
        {
            case "Red":
                _sukiTheme.ChangeColorTheme(SukiColor.Red);
                break;

            case "Green":
                _sukiTheme.ChangeColorTheme(SukiColor.Green);
                break;

            case "Orange":
                _sukiTheme.ChangeColorTheme(SukiColor.Orange);
                break;

            default:
                _sukiTheme.ChangeColorTheme(SukiColor.Blue);
                break;
        }
    }

    private void Save()
    {
        _settingsService.Save(_settings);
    }
}