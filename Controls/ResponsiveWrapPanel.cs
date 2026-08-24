// Controls/ResponsiveWrapPanel.cs
using System;

using Avalonia;
using Avalonia.Controls;

namespace Libris.Controls;

public class ResponsiveWrapPanel : Panel
{
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(ItemWidth),
            174);

    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(ItemHeight),
            304);

    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(HorizontalSpacing),
            16);

    public static readonly StyledProperty<double> VerticalSpacingProperty =
        AvaloniaProperty.Register<ResponsiveWrapPanel, double>(
            nameof(VerticalSpacing),
            20);

    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = availableSize.Width;

        if (double.IsInfinity(width) || width <= 0)
        {
            width = ItemWidth;
        }

        var columns = CalculateColumns(width);

        foreach (var child in Children)
        {
            child.Measure(
                new Size(
                    ItemWidth,
                    ItemHeight));
        }

        var rows = Children.Count == 0
            ? 0
            : (int)Math.Ceiling(
                (double)Children.Count / columns);

        var desiredWidth =
            columns * ItemWidth +
            Math.Max(0, columns - 1) * HorizontalSpacing;

        var desiredHeight =
            rows * ItemHeight +
            Math.Max(0, rows - 1) * VerticalSpacing;

        return new Size(
            Math.Min(width, desiredWidth),
            desiredHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = CalculateColumns(finalSize.Width);

        for (var index = 0; index < Children.Count; index++)
        {
            var child = Children[index];

            var column = index % columns;
            var row = index / columns;

            var x =
                column * (ItemWidth + HorizontalSpacing);

            var y =
                row * (ItemHeight + VerticalSpacing);

            child.Arrange(
                new Rect(
                    x,
                    y,
                    ItemWidth,
                    ItemHeight));
        }

        return finalSize;
    }

    private int CalculateColumns(double width)
    {
        if (width <= ItemWidth)
            return 1;

        var columns = (int)(
            (width + HorizontalSpacing) /
            (ItemWidth + HorizontalSpacing));

        return Math.Max(1, columns);
    }
}
