// Controls/ResponsiveWrapPanel.cs
using System;

using Avalonia;
using Avalonia.Controls;

namespace Libris.Controls;

/// <summary>
/// Панель, которая автоматически размещает дочерние элементы
/// в несколько колонок в зависимости от доступной ширины.
/// </summary>
public class ResponsiveWrapPanel : Panel
{
    /// <summary>
    /// Определяет ширину одного элемента.
    /// </summary>
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(ItemWidth),
            174);

    /// <summary>
    /// Определяет высоту одного элемента.
    /// </summary>
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(ItemHeight),
            304);

    /// <summary>
    /// Определяет горизонтальное расстояние между элементами.
    /// </summary>
    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(HorizontalSpacing),
            16);

    /// <summary>
    /// Определяет вертикальное расстояние между элементами.
    /// </summary>
    public static readonly StyledProperty<double> VerticalSpacingProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(VerticalSpacing),
            20);

    /// <summary>
    /// Получает или задаёт ширину одного элемента.
    /// </summary>
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Получает или задаёт высоту одного элемента.
    /// </summary>
    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>
    /// Получает или задаёт горизонтальное расстояние между элементами.
    /// </summary>
    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    /// <summary>
    /// Получает или задаёт вертикальное расстояние между элементами.
    /// </summary>
    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    /// <summary>
    /// Вычисляет требуемый размер панели и измеряет все дочерние элементы
    /// с заданными размерами.
    /// </summary>
    /// <param name="availableSize">Доступный размер панели.</param>
    /// <returns>Размер, необходимый панели для размещения всех элементов.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var itemWidth = Math.Max(1, ItemWidth);
        var itemHeight = Math.Max(1, ItemHeight);
        var horizontalSpacing = Math.Max(0, HorizontalSpacing);
        var verticalSpacing = Math.Max(0, VerticalSpacing);

        var width = availableSize.Width;

        if (double.IsInfinity(width) || width <= 0)
            width = itemWidth;

        var columns = CalculateColumns(
            width,
            itemWidth,
            horizontalSpacing);

        foreach (var child in Children)
        {
            child.Measure(new Size(itemWidth, itemHeight));
        }

        var rows = Children.Count == 0
            ? 0
            : (int)Math.Ceiling(
                (double)Children.Count / columns);

        var desiredWidth =
            columns * itemWidth +
            Math.Max(0, columns - 1) * horizontalSpacing;

        var desiredHeight =
            rows * itemHeight +
            Math.Max(0, rows - 1) * verticalSpacing;

        return new Size(
            Math.Min(width, desiredWidth),
            desiredHeight);
    }

    /// <summary>
    /// Размещает дочерние элементы панели по рассчитанной сетке.
    /// </summary>
    /// <param name="finalSize">Фактический размер панели после измерения.</param>
    /// <returns>Фактический размер панели.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var itemWidth = Math.Max(1, ItemWidth);
        var itemHeight = Math.Max(1, ItemHeight);
        var horizontalSpacing = Math.Max(0, HorizontalSpacing);
        var verticalSpacing = Math.Max(0, VerticalSpacing);

        var columns = CalculateColumns(
            finalSize.Width,
            itemWidth,
            horizontalSpacing);

        for (var index = 0; index < Children.Count; index++)
        {
            var child = Children[index];

            var column = index % columns;
            var row = index / columns;

            var x =
                column * (itemWidth + horizontalSpacing);

            var y =
                row * (itemHeight + verticalSpacing);

            child.Arrange(
                new Rect(
                    x,
                    y,
                    itemWidth,
                    itemHeight));
        }

        return finalSize;
    }

    /// <summary>
    /// Вычисляет максимальное количество колонок,
    /// которое помещается в указанную ширину.
    /// </summary>
    /// <param name="width">Доступная ширина панели.</param>
    /// <param name="itemWidth">Ширина одного элемента.</param>
    /// <param name="horizontalSpacing">Расстояние между элементами.</param>
    /// <returns>Количество колонок.</returns>
    private static int CalculateColumns(
        double width,
        double itemWidth,
        double horizontalSpacing)
    {
        if (width <= itemWidth)
            return 1;

        var columns = (int)(
            (width + horizontalSpacing) /
            (itemWidth + horizontalSpacing));

        return Math.Max(1, columns);
    }
}