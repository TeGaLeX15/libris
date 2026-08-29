// ViewModels/SettingsViewModel.cs
using System;
using System.Collections.Generic;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using Libris.Models;
using Libris.Services;
using SukiUI;
using SukiUI.Enums;

namespace Libris.ViewModels;

/// <summary>
/// ViewModel страницы настроек приложения.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _settings;
    private readonly SukiTheme _sukiTheme;

    /// <summary>
    /// Доступные варианты темы приложения.
    /// </summary>
    public IReadOnlyList<string> AvailableThemes { get; } =
    [
        "System",
        "Light",
        "Dark"
    ];

    /// <summary>
    /// Доступные цвета акцента.
    /// </summary>
    public IReadOnlyList<string> AvailableAccentColors { get; } =
    [
        "Blue",
        "Red",
        "Green",
        "Orange"
    ];

    /// <summary>
    /// Доступные шрифты режима чтения.
    /// </summary>
    public IReadOnlyList<string> AvailableFonts { get; } =
    [
        "Inter",
        "Arial",
        "Georgia",
        "Times New Roman"
    ];

    /// <summary>
    /// Доступные варианты сортировки библиотеки.
    /// </summary>
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
    private double readingWidth = 720;

    [ObservableProperty]
    private string defaultSorting = "Recently Added";

    [ObservableProperty]
    private bool showProgress = true;

    [ObservableProperty]
    private double coverSize = 160;

    /// <summary>
    /// Создаёт ViewModel настроек и загружает сохранённые значения.
    /// </summary>
    public SettingsViewModel(
        SettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        _settingsService = settingsService;
        _settings = _settingsService.Load();
        _sukiTheme = SukiTheme.GetInstance();

        Theme = NormalizeTheme(_settings.Theme);
        AccentColor = NormalizeAccentColor(_settings.AccentColor);
        DefaultFont = NormalizeFont(_settings.DefaultFont);

        FontSize = Math.Clamp(
            _settings.FontSize,
            10,
            48);

        LineSpacing = Math.Clamp(
            _settings.LineSpacing,
            1.0,
            3.0);

        ReadingWidth = Math.Clamp(
            _settings.ReadingWidth,
            400,
            1400);

        DefaultSorting = NormalizeSorting(
            _settings.DefaultSorting);

        ShowProgress = _settings.ShowProgress;

        CoverSize = Math.Clamp(
            _settings.CoverSize,
            100,
            400);

        ApplyTheme(Theme);
        ApplyAccentColor(AccentColor);
    }

    partial void OnThemeChanged(string value)
    {
        value = NormalizeTheme(value);

        if (Theme != value)
        {
            Theme = value;
            return;
        }

        _settings.Theme = value;

        ApplyTheme(value);
        Save();
    }

    partial void OnAccentColorChanged(string value)
    {
        value = NormalizeAccentColor(value);

        if (AccentColor != value)
        {
            AccentColor = value;
            return;
        }

        _settings.AccentColor = value;

        ApplyAccentColor(value);
        Save();
    }

    partial void OnDefaultFontChanged(string value)
    {
        value = NormalizeFont(value);

        if (DefaultFont != value)
        {
            DefaultFont = value;
            return;
        }

        _settings.DefaultFont = value;
        Save();
    }

    partial void OnFontSizeChanged(double value)
    {
        value = Math.Clamp(value, 10, 48);

        if (Math.Abs(FontSize - value) > double.Epsilon)
        {
            FontSize = value;
            return;
        }

        _settings.FontSize = value;
        Save();
    }

    partial void OnLineSpacingChanged(double value)
    {
        value = Math.Clamp(value, 1.0, 3.0);

        if (Math.Abs(LineSpacing - value) > double.Epsilon)
        {
            LineSpacing = value;
            return;
        }

        _settings.LineSpacing = value;
        Save();
    }

    partial void OnReadingWidthChanged(double value)
    {
        value = Math.Clamp(value, 400, 1400);

        if (Math.Abs(ReadingWidth - value) > double.Epsilon)
        {
            ReadingWidth = value;
            return;
        }

        _settings.ReadingWidth = value;
        Save();
    }

    partial void OnDefaultSortingChanged(string value)
    {
        value = NormalizeSorting(value);

        if (DefaultSorting != value)
        {
            DefaultSorting = value;
            return;
        }

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
        value = Math.Clamp(value, 100, 400);

        if (Math.Abs(CoverSize - value) > double.Epsilon)
        {
            CoverSize = value;
            return;
        }

        _settings.CoverSize = value;
        Save();
    }

    private void ApplyTheme(string value)
    {
        var theme = value switch
        {
            "Light" => ThemeVariant.Light,
            "Dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        _sukiTheme.ChangeBaseTheme(theme);
    }

    private void ApplyAccentColor(string value)
    {
        var color = value switch
        {
            "Red" => SukiColor.Red,
            "Green" => SukiColor.Green,
            "Orange" => SukiColor.Orange,
            _ => SukiColor.Blue
        };

        _sukiTheme.ChangeColorTheme(color);
    }

    private void Save()
    {
        _settingsService.Save(_settings);
    }

    private static string NormalizeTheme(string? value)
    {
        return value switch
        {
            "Light" => "Light",
            "Dark" => "Dark",
            _ => "System"
        };
    }

    private static string NormalizeAccentColor(string? value)
    {
        return value switch
        {
            "Red" => "Red",
            "Green" => "Green",
            "Orange" => "Orange",
            _ => "Blue"
        };
    }

    private static string NormalizeFont(string? value)
    {
        return value switch
        {
            "Arial" => "Arial",
            "Georgia" => "Georgia",
            "Times New Roman" => "Times New Roman",
            _ => "Inter"
        };
    }

    private static string NormalizeSorting(string? value)
    {
        return value switch
        {
            "Title" => "Title",
            "Author" => "Author",
            "Progress" => "Progress",
            _ => "Recently Added"
        };
    }
}