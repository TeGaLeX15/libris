// Views/ReaderView.axaml.cs
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Libris.ViewModels;

namespace Libris.Views;

/// <summary>
/// Представляет интерфейс чтения книги.
/// </summary>
public partial class ReaderView : UserControl
{
    private ReaderViewModel? _reader;

    public ReaderView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;

        ReaderWebView.NavigationCompleted +=
            ReaderWebView_OnNavigationCompleted;

        ReaderWebView.WebMessageReceived +=
            ReaderWebView_OnWebMessageReceived;

        UpdateReaderChrome();

        var sukiTheme = SukiUI.SukiTheme.GetInstance();

        sukiTheme.OnBaseThemeChanged +=
            OnBaseThemeChanged;
    }

    private void OnBaseThemeChanged(ThemeVariant theme)
    {
        UpdateReaderChrome();

        if (_reader is not null)
        {
            _ = ApplyReaderSettingsAfterThemeChangedAsync();
        }
    }

    private async Task ApplyReaderSettingsAfterThemeChangedAsync()
    {
        await Task.Delay(1);

        if (_reader is null)
            return;

        await ApplyReaderSettingsAsync(
            ReaderWebView,
            _reader);
    }

    private void UpdateReaderChrome()
    {
        var sukiTheme =
            SukiUI.SukiTheme.GetInstance();

        var isDark =
            sukiTheme.ActiveBaseTheme ==
            ThemeVariant.Dark;

        /*
         * Важно:
         *
         * Панели НЕ прозрачные.
         * Они получают обычный SolidColorBrush.
         *
         * Это специально не ThemeBackgroundMediumBrush,
         * потому что WebView и Suki background renderer
         * могут давать нежелательную прозрачность.
         */

        if (isDark)
        {
            TopBar.Background =
                new SolidColorBrush(
                    Color.Parse("#141414"));

            BottomBar.Background =
                new SolidColorBrush(
                    Color.Parse("#141414"));

            ReaderArea.Background =
                new SolidColorBrush(
                    Color.Parse("#1A1A1A"));
        }
        else
        {
            TopBar.Background =
                new SolidColorBrush(
                    Color.Parse("#FFFFFF"));

            BottomBar.Background =
                new SolidColorBrush(
                    Color.Parse("#FFFFFF"));

            ReaderArea.Background =
                new SolidColorBrush(
                    Color.Parse("#FFFFFF"));
        }
    }

    private async void OnDataContextChanged(
        object? sender,
        EventArgs e)
    {
        if (_reader is not null)
        {
            _reader.PropertyChanged -=
                Reader_PropertyChanged;
        }

        _reader =
            DataContext as ReaderViewModel;

        if (_reader is null)
            return;

        _reader.PropertyChanged +=
            Reader_PropertyChanged;

        await _reader.LoadAsync();

        NavigateToCurrentChapter();
    }

    private void Reader_PropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName !=
            nameof(ReaderViewModel.ReaderHtml))
        {
            return;
        }

        NavigateToCurrentChapter();
    }

    private void NavigateToCurrentChapter()
    {
        if (_reader is null)
            return;

        if (string.IsNullOrWhiteSpace(
                _reader.ReaderHtml))
        {
            return;
        }

        var filePath =
            _reader.ReaderHtml;

        if (!File.Exists(filePath))
            return;

        /*
         * WebView перед навигацией может кратковременно
         * показать свой стандартный фон.
         *
         * Поэтому контейнер ReaderArea заранее имеет
         * правильный цвет темы.
         */

        var uri =
            new Uri(
                Path.GetFullPath(filePath));

        ReaderWebView.Navigate(uri);
    }

    private async void ReaderWebView_OnNavigationCompleted(
        object? sender,
        WebViewNavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
            return;

        if (sender is not NativeWebView webView)
            return;

        if (_reader is null)
            return;

        await ApplyReaderSettingsAsync(
            webView,
            _reader);
    }

    private void ReaderWebView_OnWebMessageReceived(
        object? sender,
        WebMessageReceivedEventArgs e)
    {
        if (_reader is null)
            return;

        if (!double.TryParse(
                e.Body,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var progress))
        {
            return;
        }

        _reader.UpdateChapterProgress(progress);
    }

    private static async Task ApplyReaderSettingsAsync(
        NativeWebView webView,
        ReaderViewModel reader)
    {
        var progress =
            reader.ChapterProgress.ToString(
                CultureInfo.InvariantCulture);

        var fontSize =
            reader.FontSize.ToString(
                CultureInfo.InvariantCulture);

        var lineHeight =
            reader.LineHeight.ToString(
                CultureInfo.InvariantCulture);

        var readingWidth =
            reader.ReadingWidth.ToString(
                CultureInfo.InvariantCulture);

        var sukiTheme =
            SukiUI.SukiTheme.GetInstance();

        var isDark =
            sukiTheme.ActiveBaseTheme ==
            ThemeVariant.Dark;

        /*
         * Эти значения должны совпадать
         * с начальными значениями ReaderHtmlBuilder.
         */

        var background =
            isDark
                ? "#1A1A1A"
                : "#FFFFFF";

        var foreground =
            isDark
                ? "#F3F3F3"
                : "#202124";

        var muted =
            isDark
                ? "#A1A1AA"
                : "#6B7280";

        var scrollbarThumb =
            isDark
                ? "#4A4A4A"
                : "#C8C8C8";

        var scrollbarThumbHover =
            isDark
                ? "#626262"
                : "#AAAAAA";

        var accent =
            GetAccentColor();

        var script =
            $$"""
            (() => {

                const root =
                    document.documentElement;

                const body =
                    document.body;

                const readerElement =
                    document.getElementById('reader');

                if (!root ||
                    !body ||
                    !readerElement) {
                    return;
                }

                /*
                 * Цветовая схема Reader.
                 */

                root.style.setProperty(
                    '--background',
                    '{{background}}');

                root.style.setProperty(
                    '--foreground',
                    '{{foreground}}');

                root.style.setProperty(
                    '--muted',
                    '{{muted}}');

                root.style.setProperty(
                    '--accent',
                    '{{accent}}');

                root.style.setProperty(
                    '--scrollbar-thumb',
                    '{{scrollbarThumb}}');

                root.style.setProperty(
                    '--scrollbar-thumb-hover',
                    '{{scrollbarThumbHover}}');

                /*
                 * Применяем фон сразу.
                 */

                root.style.backgroundColor =
                    '{{background}}';

                body.style.backgroundColor =
                    '{{background}}';

                body.style.color =
                    '{{foreground}}';

                /*
                 * Настройки текста.
                 */

                body.style.fontSize =
                    '{{fontSize}}px';

                body.style.lineHeight =
                    '{{lineHeight}}';

                readerElement.style.width =
                    'min({{readingWidth}}px, calc(100% - 60px))';

                /*
                 * Восстановление позиции.
                 */

                const savedProgress =
                    {{progress}};

                const restorePosition = () => {

                    const maxScroll =
                        Math.max(
                            0,
                            document.documentElement.scrollHeight -
                            window.innerHeight);

                    window.scrollTo(
                        0,
                        maxScroll * savedProgress);
                };

                restorePosition();

                setTimeout(
                    restorePosition,
                    100);

                setTimeout(
                    restorePosition,
                    500);

                /*
                 * Отслеживание прогресса.
                 */

                let lastSent = -1;

                let timeout = null;

                const sendProgress = () => {

                    const maxScroll =
                        Math.max(
                            0,
                            document.documentElement.scrollHeight -
                            window.innerHeight);

                    const current =
                        maxScroll <= 0
                            ? 0
                            : Math.max(
                                0,
                                Math.min(
                                    1,
                                    window.scrollY /
                                    maxScroll));

                    if (
                        Math.abs(
                            current - lastSent) < 0.002
                    ) {
                        return;
                    }

                    lastSent = current;

                    invokeCSharpAction(
                        String(current));
                };

                window.addEventListener(
                    'scroll',
                    () => {

                        if (timeout !== null)
                            return;

                        timeout =
                            setTimeout(() => {

                                timeout = null;

                                sendProgress();

                            }, 150);

                    },
                    { passive: true });

                sendProgress();

            })();
            """;

        await webView.InvokeScript(script);
    }

    private static string GetAccentColor()
    {
        var application =
            Application.Current;

        if (application is null)
            return "#5B5BD6";

        if (application.Resources.TryGetResource(
                "AccentBrush",
                null,
                out var resource))
        {
            if (resource is SolidColorBrush brush)
            {
                return ToCssColor(
                    brush.Color);
            }

            if (resource is Color color)
            {
                return ToCssColor(
                    color);
            }
        }

        /*
         * Если конкретного AccentBrush в SukiUI
         * нет, используем стандартный fallback.
         */

        return "#5B5BD6";
    }

    private static string ToCssColor(
        Color color)
    {
        return
            $"#{color.R:X2}" +
            $"{color.G:X2}" +
            $"{color.B:X2}";
    }

    private void OnUnloaded(
        object? sender,
        RoutedEventArgs e)
    {
        _reader?.SavePosition();
    }
}