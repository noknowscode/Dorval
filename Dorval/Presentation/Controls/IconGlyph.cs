using System.Collections.Concurrent;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Path = Microsoft.UI.Xaml.Shapes.Path;

namespace Dorval.Presentation.Controls;

/// <summary>
/// A 24-unit stroked icon drawn from SVG path data and scaled to <see cref="Size"/>.
/// Scaling through a Viewbox keeps the stroke proportional the way an SVG does, so an
/// 18px and a 22px copy of the same glyph stay visually consistent.
/// </summary>
public sealed partial class IconGlyph : UserControl
{
    private const double DesignSize = 24d;

    private static readonly ConcurrentDictionary<string, Geometry> GeometryCache = new();

    private readonly Path _path = new()
    {
        Stretch = Stretch.None,
        StrokeLineJoin = PenLineJoin.Round,
        StrokeStartLineCap = PenLineCap.Round,
        StrokeEndLineCap = PenLineCap.Round,
    };

    private readonly Viewbox _viewbox;

    public IconGlyph()
    {
        _viewbox = new Viewbox
        {
            Stretch = Stretch.Uniform,
            Child = new Grid
            {
                Width = DesignSize,
                Height = DesignSize,
                Children = { _path },
            },
        };

        IsTabStop = false;
        IsHitTestVisible = false;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
        Content = _viewbox;

        ApplySize();
        ApplyPaint();
    }

    public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
        nameof(Data), typeof(string), typeof(IconGlyph),
        new PropertyMetadata(null, (d, _) => ((IconGlyph)d).ApplyGeometry()));

    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(double), typeof(IconGlyph),
        new PropertyMetadata(24d, (d, _) => ((IconGlyph)d).ApplySize()));

    public static readonly DependencyProperty IconStrokeProperty = DependencyProperty.Register(
        nameof(IconStroke), typeof(Brush), typeof(IconGlyph),
        new PropertyMetadata(null, (d, _) => ((IconGlyph)d).ApplyPaint()));

    public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(
        nameof(IconFill), typeof(Brush), typeof(IconGlyph),
        new PropertyMetadata(null, (d, _) => ((IconGlyph)d).ApplyPaint()));

    public static readonly DependencyProperty IconThicknessProperty = DependencyProperty.Register(
        nameof(IconThickness), typeof(double), typeof(IconGlyph),
        new PropertyMetadata(2d, (d, _) => ((IconGlyph)d).ApplyPaint()));

    /// <summary>SVG path data, expressed in a 0 0 24 24 view box.</summary>
    public string? Data
    {
        get => (string?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <summary>Rendered edge length in pixels.</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Brush? IconStroke
    {
        get => (Brush?)GetValue(IconStrokeProperty);
        set => SetValue(IconStrokeProperty, value);
    }

    public Brush? IconFill
    {
        get => (Brush?)GetValue(IconFillProperty);
        set => SetValue(IconFillProperty, value);
    }

    /// <summary>Stroke width in design units, before the scale down to <see cref="Size"/>.</summary>
    public double IconThickness
    {
        get => (double)GetValue(IconThicknessProperty);
        set => SetValue(IconThicknessProperty, value);
    }

    private void ApplyGeometry() => _path.Data = Data is { Length: > 0 } data ? Parse(data) : null;

    private void ApplySize()
    {
        _viewbox.Width = Size;
        _viewbox.Height = Size;
    }

    private void ApplyPaint()
    {
        _path.Stroke = IconStroke;
        _path.Fill = IconFill;
        _path.StrokeThickness = IconStroke is null ? 0 : IconThickness;
    }

    /// <summary>
    /// Turns SVG path data into a Geometry. There is no runtime string-to-Geometry
    /// conversion, so this round-trips through the XAML parser once per distinct glyph.
    /// </summary>
    private static Geometry Parse(string data) => GeometryCache.GetOrAdd(data, static d =>
    {
        var xaml = $"<Path xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" Data=\"{d}\" />";
        return ((Path)XamlReader.Load(xaml)).Data!;
    });
}
