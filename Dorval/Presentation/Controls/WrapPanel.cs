using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Dorval.Presentation.Controls;

/// <summary>
/// Lays children left to right and wraps when the next one will not fit — what CSS
/// flex-wrap does for the tag pills. WinUI ships no wrap panel, and ItemsRepeater's
/// UniformGridLayout/FlowLayout do not wrap reliably inside a stacked panel on Uno.
/// Used as an ItemsControl.ItemsPanel, since ItemsRepeater only drives VirtualizingLayout.
/// </summary>
public partial class WrapPanel : Panel
{
    public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
        nameof(HorizontalSpacing), typeof(double), typeof(WrapPanel),
        new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
        nameof(VerticalSpacing), typeof(double), typeof(WrapPanel),
        new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var limit = double.IsInfinity(availableSize.Width) ? double.MaxValue : availableSize.Width;
        var lineWidth = 0d;
        var lineHeight = 0d;
        var widest = 0d;
        var stackedHeight = 0d;

        foreach (var child in Children)
        {
            child.Measure(new Size(limit, double.PositiveInfinity));
            var size = child.DesiredSize;

            var advance = lineWidth == 0 ? size.Width : lineWidth + HorizontalSpacing + size.Width;
            if (advance > limit && lineWidth > 0)
            {
                widest = Math.Max(widest, lineWidth);
                stackedHeight += lineHeight + VerticalSpacing;
                lineWidth = size.Width;
                lineHeight = size.Height;
            }
            else
            {
                lineWidth = advance;
                lineHeight = Math.Max(lineHeight, size.Height);
            }
        }

        return new Size(Math.Max(widest, lineWidth), stackedHeight + lineHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var x = 0d;
        var y = 0d;
        var lineHeight = 0d;

        foreach (var child in Children)
        {
            var size = child.DesiredSize;

            if (x > 0 && x + HorizontalSpacing + size.Width > finalSize.Width)
            {
                x = 0;
                y += lineHeight + VerticalSpacing;
                lineHeight = 0;
            }
            else if (x > 0)
            {
                x += HorizontalSpacing;
            }

            child.Arrange(new Rect(x, y, size.Width, size.Height));
            x += size.Width;
            lineHeight = Math.Max(lineHeight, size.Height);
        }

        return finalSize;
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((WrapPanel)d).InvalidateMeasure();
}
