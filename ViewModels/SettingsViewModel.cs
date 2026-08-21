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
    private string theme;

    [ObservableProperty]
    private string accentColor;

    [ObservableProperty]
    private string defaultFont;

    [ObservableProperty]
    private double fontSize;

    [ObservableProperty]
    private double lineSpacing;

    [ObservableProperty]
    private double readingWidth;

    [ObservableProperty]
    private string defaultSorting;

    [ObservableProperty]
    private bool showProgress;

    [ObservableProperty]
    private double coverSize;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _settings = _settingsService.Load();

        _sukiTheme = SukiTheme.GetInstance();

        theme = _settings.Theme;
        accentColor = _settings.AccentColor;
        defaultFont = _settings.DefaultFont;
        fontSize = _settings.FontSize;
        lineSpacing = _settings.LineSpacing;
        readingWidth = _settings.ReadingWidth;
        defaultSorting = _settings.DefaultSorting;
        showProgress = _settings.ShowProgress;
        coverSize = _settings.CoverSize;

        ApplyTheme(theme);
        ApplyAccentColor(accentColor);
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