// ViewModels/SettingsViewModel.cs
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
/// Отвечает за управление пользовательскими настройками,
/// их сохранение и применение визуальной темы Libris.
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
    /// Доступные цвета акцента приложения.
    /// </summary>
    public IReadOnlyList<string> AvailableAccentColors { get; } =
    [
        "Blue",
        "Red",
        "Green",
        "Orange"
    ];

    /// <summary>
    /// Доступные шрифты для режима чтения.
    /// </summary>
    public IReadOnlyList<string> AvailableFonts { get; } =
    [
        "Inter",
        "Arial",
        "Georgia",
        "Times New Roman"
    ];

    /// <summary>
    /// Доступные варианты сортировки книг по умолчанию.
    /// </summary>
    public IReadOnlyList<string> AvailableSortingOptions { get; } =
    [
        "Recently Added",
        "Title",
        "Author",
        "Progress"
    ];

    /// <summary>
    /// Текущая тема приложения.
    /// </summary>
    [ObservableProperty]
    private string theme = "System";

    /// <summary>
    /// Текущий цвет акцента приложения.
    /// </summary>
    [ObservableProperty]
    private string accentColor = "Blue";

    /// <summary>
    /// Шрифт, используемый по умолчанию в режиме чтения.
    /// </summary>
    [ObservableProperty]
    private string defaultFont = "Inter";

    /// <summary>
    /// Размер шрифта в режиме чтения.
    /// </summary>
    [ObservableProperty]
    private double fontSize = 16;

    /// <summary>
    /// Межстрочный интервал в режиме чтения.
    /// </summary>
    [ObservableProperty]
    private double lineSpacing = 1.5;

    /// <summary>
    /// Максимальная ширина области текста в режиме чтения.
    /// </summary>
    [ObservableProperty]
    private double readingWidth = 800;

    /// <summary>
    /// Сортировка книг, используемая по умолчанию в библиотеке.
    /// </summary>
    [ObservableProperty]
    private string defaultSorting = "Recently Added";

    /// <summary>
    /// Определяет, отображать ли прогресс чтения в интерфейсе библиотеки.
    /// </summary>
    [ObservableProperty]
    private bool showProgress = true;

    /// <summary>
    /// Размер обложек книг в библиотеке.
    /// </summary>
    [ObservableProperty]
    private double coverSize = 180;

    /// <summary>
    /// Инициализирует ViewModel настроек и загружает сохранённые значения.
    /// </summary>
    /// <param name="settingsService">
    /// Сервис для загрузки и сохранения настроек.
    /// </param>
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

    /// <summary>
    /// Применяет новую тему и сохраняет настройку.
    /// </summary>
    /// <param name="value">Название выбранной темы.</param>
    partial void OnThemeChanged(string value)
    {
        _settings.Theme = value;
        ApplyTheme(value);
        Save();
    }

    /// <summary>
    /// Применяет новый цвет акцента и сохраняет настройку.
    /// </summary>
    /// <param name="value">Название выбранного цвета.</param>
    partial void OnAccentColorChanged(string value)
    {
        _settings.AccentColor = value;
        ApplyAccentColor(value);
        Save();
    }

    /// <summary>
    /// Сохраняет выбранный шрифт.
    /// </summary>
    /// <param name="value">Название шрифта.</param>
    partial void OnDefaultFontChanged(string value)
    {
        _settings.DefaultFont = value;
        Save();
    }

    /// <summary>
    /// Сохраняет размер шрифта.
    /// </summary>
    /// <param name="value">Размер шрифта.</param>
    partial void OnFontSizeChanged(double value)
    {
        _settings.FontSize = value;
        Save();
    }

    /// <summary>
    /// Сохраняет межстрочный интервал.
    /// </summary>
    /// <param name="value">Новое значение межстрочного интервала.</param>
    partial void OnLineSpacingChanged(double value)
    {
        _settings.LineSpacing = value;
        Save();
    }

    /// <summary>
    /// Сохраняет ширину области чтения.
    /// </summary>
    /// <param name="value">Новая ширина области чтения.</param>
    partial void OnReadingWidthChanged(double value)
    {
        _settings.ReadingWidth = value;
        Save();
    }

    /// <summary>
    /// Сохраняет сортировку книг по умолчанию.
    /// </summary>
    /// <param name="value">Выбранный вариант сортировки.</param>
    partial void OnDefaultSortingChanged(string value)
    {
        _settings.DefaultSorting = value;
        Save();
    }

    /// <summary>
    /// Сохраняет настройку отображения прогресса чтения.
    /// </summary>
    /// <param name="value">Определяет, отображать ли прогресс.</param>
    partial void OnShowProgressChanged(bool value)
    {
        _settings.ShowProgress = value;
        Save();
    }

    /// <summary>
    /// Сохраняет размер обложек книг.
    /// </summary>
    /// <param name="value">Новый размер обложек.</param>
    partial void OnCoverSizeChanged(double value)
    {
        _settings.CoverSize = value;
        Save();
    }

    /// <summary>
    /// Применяет выбранную тему через SukiUI.
    /// </summary>
    /// <param name="value">Название темы.</param>
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

    /// <summary>
    /// Применяет выбранный цвет акцента через SukiUI.
    /// </summary>
    /// <param name="value">Название цвета.</param>
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

    /// <summary>
    /// Сохраняет текущие настройки на диск.
    /// </summary>
    private void Save()
    {
        _settingsService.Save(_settings);
    }
}