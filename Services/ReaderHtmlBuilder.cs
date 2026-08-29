// Services/ReaderHtmlBuilder.cs
using System;
using System.Globalization;
using System.IO;
using System.Net;
using Avalonia.Styling;
using SukiUI;

namespace Libris.Services;

/// <summary>
/// Отвечает за формирование HTML-документа Reader.
/// </summary>
public static class ReaderHtmlBuilder
{
    /// <summary>
    /// Формирует HTML-документ для одной главы
    /// и сохраняет его в локальный файл.
    /// </summary>
    public static string BuildChapter(
        string bookTitle,
        ReaderChapter chapter,
        string fontFamily = "Inter",
        double fontSize = 18,
        double lineHeight = 1.65,
        double readingWidth = 760)
    {
        ArgumentNullException.ThrowIfNull(chapter);

        var safeTitle =
            WebUtility.HtmlEncode(bookTitle);

        var safeFont =
            WebUtility.HtmlEncode(fontFamily);

        var fontSizeValue =
            fontSize.ToString(
                CultureInfo.InvariantCulture);

        var lineHeightValue =
            lineHeight.ToString(
                CultureInfo.InvariantCulture);

        var readingWidthValue =
            readingWidth.ToString(
                CultureInfo.InvariantCulture);

        /*
         * Определяем текущую тему SukiUI
         * ДО создания HTML.
         *
         * Это важно: WebView сразу получит
         * правильный фон при открытии новой главы.
         */

        var sukiTheme =
            SukiTheme.GetInstance();

        var isDark =
            sukiTheme.ActiveBaseTheme ==
            ThemeVariant.Dark;

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

        var html =
            $$"""
            <!DOCTYPE html>

            <html lang="en">

            <head>

                <meta charset="utf-8">

                <meta
                    name="viewport"
                    content="width=device-width, initial-scale=1.0">

                <title>{{safeTitle}}</title>

                <style>

                    :root {

                        /*
                         * Цвета заранее устанавливаются
                         * правильными для текущей темы.
                         */

                        --background:
                            {{background}};

                        --foreground:
                            {{foreground}};

                        --muted:
                            {{muted}};

                        --accent:
                            {{accent}};

                        --scrollbar-thumb:
                            {{scrollbarThumb}};

                        --scrollbar-thumb-hover:
                            {{scrollbarThumbHover}};

                        --font-family:
                            "{{safeFont}}",
                            Inter,
                            system-ui,
                            sans-serif;

                        --font-size:
                            {{fontSizeValue}}px;

                        --line-height:
                            {{lineHeightValue}};

                        --reading-width:
                            {{readingWidthValue}}px;
                    }

                    /*
                     * Очень важно:
                     * фон задаётся уже на html,
                     * а не только после загрузки JS.
                     */

                    html {
                        background:
                            var(--background);

                        color:
                            var(--foreground);

                        scroll-behavior:
                            auto;
                    }

                    * {
                        box-sizing:
                            border-box;
                    }

                    body {

                        margin:
                            0;

                        padding:
                            0;

                        background:
                            var(--background);

                        color:
                            var(--foreground);

                        font-family:
                            var(--font-family);

                        font-size:
                            var(--font-size);

                        line-height:
                            var(--line-height);

                        /*
                         * Убираем стандартную анимацию
                         * цвета при загрузке страницы.
                         */

                        transition:
                            none;
                    }

                    #reader {

                        width:
                            min(
                                var(--reading-width),
                                calc(100% - 60px)
                            );

                        margin:
                            0 auto;

                        padding:
                            60px 0 140px;
                    }

                    #chapter {

                        margin:
                            0;
                    }

                    h1,
                    h2,
                    h3,
                    h4 {

                        line-height:
                            1.25;

                        margin-top:
                            0;

                        margin-bottom:
                            1.5em;
                    }

                    p {

                        margin:
                            0 0 1.2em;
                    }

                    img {

                        display:
                            block;

                        max-width:
                            100%;

                        height:
                            auto;

                        margin:
                            2em auto;
                    }

                    a {

                        color:
                            var(--accent);
                    }

                    blockquote {

                        margin:
                            1.5em 0;

                        padding-left:
                            1.2em;

                        border-left:
                            3px solid
                            rgba(128, 128, 128, 0.35);

                        color:
                            var(--muted);
                    }

                    pre,
                    code {

                        white-space:
                            pre-wrap;
                    }

                    .text-content {

                        white-space:
                            normal;
                    }

                    /*
                     * Скроллбар Reader.
                     */

                    ::-webkit-scrollbar {

                        width:
                            10px;
                    }

                    ::-webkit-scrollbar-track {

                        background:
                            var(--background);
                    }

                    ::-webkit-scrollbar-thumb {

                        background:
                            var(--scrollbar-thumb);

                        border-radius:
                            5px;
                    }

                    ::-webkit-scrollbar-thumb:hover {

                        background:
                            var(--scrollbar-thumb-hover);
                    }

                    @media (max-width: 700px) {

                        #reader {

                            width:
                                calc(100% - 32px);

                            padding-top:
                                30px;
                        }

                        body {

                            font-size:
                                calc(
                                    var(--font-size) - 1px
                                );
                        }
                    }

                </style>

            </head>

            <body>

                <main id="reader">

                    <article
                        id="chapter"
                        data-chapter="{{chapter.Index}}">

                        {{chapter.Html}}

                    </article>

                </main>

            </body>

            </html>
            """;

        if (string.IsNullOrWhiteSpace(
                chapter.BaseDirectory))
        {
            throw new InvalidOperationException(
                "Reader chapter does not have a base directory.");
        }

        Directory.CreateDirectory(
            chapter.BaseDirectory);

        var fileName =
            $"chapter-{chapter.Index}-{Guid.NewGuid():N}.html";

        var filePath =
            Path.Combine(
                chapter.BaseDirectory,
                fileName);

        File.WriteAllText(
            filePath,
            html);

        return filePath;
    }

    private static string GetAccentColor()
    {
        var application =
            Avalonia.Application.Current;

        if (application is null)
            return "#5B5BD6";

        if (application.Resources.TryGetResource(
                "AccentBrush",
                null,
                out var resource))
        {
            if (resource is Avalonia.Media.SolidColorBrush brush)
            {
                return ToCssColor(
                    brush.Color);
            }

            if (resource is Avalonia.Media.Color color)
            {
                return ToCssColor(
                    color);
            }
        }

        return "#5B5BD6";
    }

    private static string ToCssColor(
        Avalonia.Media.Color color)
    {
        return
            $"#{color.R:X2}" +
            $"{color.G:X2}" +
            $"{color.B:X2}";
    }
}