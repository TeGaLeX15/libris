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
        var availableWidth = availableSize.Width;

        if (double.IsInfinity(availableWidth) || availableWidth <= 0)
            availableWidth = ItemWidth;

        var columns = CalculateColumns(availableWidth);

        var totalWidth =
            columns * ItemWidth +
            (columns - 1) * HorizontalSpacing;

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

        var totalHeight =
            rows * ItemHeight +
            Math.Max(0, rows - 1) * VerticalSpacing;

        return new Size(
            Math.Min(availableWidth, totalWidth),
            totalHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = CalculateColumns(finalSize.Width);

        var totalWidth =
            columns * ItemWidth +
            (columns - 1) * HorizontalSpacing;

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

        return new Size(
            Math.Min(finalSize.Width, totalWidth),
            finalSize.Height);
    }

    private int CalculateColumns(double width)
    {
        var columns = (int)(
            (width + HorizontalSpacing) /
            (ItemWidth + HorizontalSpacing));

        return Math.Max(1, columns);
    }
}