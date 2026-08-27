// Converters/BookCoverConverter.cs

using System;
using System.Collections.Concurrent;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Libris.Converters;

/// <summary>
/// Преобразует путь к обложке книги в объект <see cref="Bitmap"/>,
/// который может быть использован элементом изображения в Avalonia.
/// </summary>
public sealed class BookCoverConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap> BitmapCache = [];

    /// <summary>
    /// Преобразует путь к файлу изображения в загруженную обложку книги.
    /// </summary>
    /// <param name="value">Путь к файлу изображения.</param>
    /// <param name="targetType">Тип значения, ожидаемый целевым свойством.</param>
    /// <param name="parameter">Дополнительный параметр конвертера.</param>
    /// <param name="culture">Текущая культура интерфейса.</param>
    /// <returns>
    /// Загруженное изображение, если путь корректен и файл удалось открыть;
    /// в противном случае <see langword="null"/>.
    /// </returns>
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not string path ||
            string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (BitmapCache.TryGetValue(path, out var cachedBitmap))
        {
            return cachedBitmap;
        }

        try
        {
            var bitmap = new Bitmap(path);

            return BitmapCache.GetOrAdd(path, bitmap);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Обратное преобразование не поддерживается,
    /// поскольку изображение не преобразуется обратно в путь к файлу.
    /// </summary>
    /// <param name="value">Значение, полученное от целевого свойства.</param>
    /// <param name="targetType">Тип исходного значения.</param>
    /// <param name="parameter">Дополнительный параметр конвертера.</param>
    /// <param name="culture">Текущая культура интерфейса.</param>
    /// <returns>Метод не возвращает значение, поскольку операция не поддерживается.</returns>
    /// <exception cref="NotSupportedException">
    /// Всегда возникает при попытке выполнить обратное преобразование.
    /// </exception>
    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException(
            "Обратное преобразование изображения в путь к файлу не поддерживается.");
    }
}