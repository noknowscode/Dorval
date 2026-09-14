using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Dorval.Presentation.Controls;

/// <summary>
/// A fixed number of equal-width columns filled left to right — CSS
/// <c>grid-template-columns: repeat(n, 1fr)</c>. UniformGridLayout does not hold that
/// shape inside a stacked panel on Uno, so the intent tiles and the two-up segmented
/// control use this instead.
/// </summary>
public partial class ColumnsPanel : Panel
{
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        nameof(Columns), typeof(int), typeof(ColumnsPanel),
        new PropertyMetadata(2, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register(
        nameof(ColumnSpacing), typeof(double), typeof(ColumnsPanel),
        new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public static readonly DependencyProperty RowSpacingProperty = DependencyProperty.Register(
        nameof(RowSpacing), typeof(double), typeof(ColumnsPanel),
        new PropertyMetadata(0d, OnLayoutPropertyChanged));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var columns = Math.Max(1, Columns);
        var columnWidth = ColumnWidth(availableSize.Width, columns);

        var total = 0d;
        var rowHeight = 0d;
        var index = 0;

        foreach (var child in Children)
        {
            child.Measure(new Size(columnWidth, double.PositiveInfinity));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);

            if (++index % columns == 0)
            {
                total += rowHeight + RowSpacing;
                rowHeight = 0;
            }
        }

        // The last full row contributed a trailing gap; a partial row did not.
        total = rowHeight > 0 ? total + rowHeight : Math.Max(0, total - RowSpacing);

        return new Size(availableSize.Width, total);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = Math.Max(1, Columns);
        var columnWidth = ColumnWidth(finalSize.Width, columns);

        var y = 0d;
        var rowHeight = 0d;
        var index = 0;

        foreach (var child in Children)
        {
            var x = (index % columns) * (columnWidth + ColumnSpacing);
            child.Arrange(new Rect(x, y, columnWidth, child.DesiredSize.Height));
            rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);

            if (++index % columns == 0)
            {
                y += rowHeight + RowSpacing;
                rowHeight = 0;
            }
        }

        return finalSize;
    }

    private double ColumnWidth(double available, int columns)
    {
        if (double.IsInfinity(available))
        {
            return double.PositiveInfinity;
        }

        return Math.Max(0, (available - (ColumnSpacing * (columns - 1))) / columns);
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((ColumnsPanel)d).InvalidateMeasure();
}
