using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Dorval.Presentation.Controls;

public sealed partial class SlotImage : UserControl
{
    public SlotImage()
    {
        this.InitializeComponent();
        ApplyTint();
        ApplyRadius();
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(string), typeof(SlotImage),
        new PropertyMetadata(null, (d, _) => ((SlotImage)d).ApplySource()));

    public static readonly DependencyProperty TintProperty = DependencyProperty.Register(
        nameof(Tint), typeof(Brush), typeof(SlotImage),
        new PropertyMetadata(null, (d, _) => ((SlotImage)d).ApplyTint()));

    public static readonly DependencyProperty SlotCornerRadiusProperty = DependencyProperty.Register(
        nameof(SlotCornerRadius), typeof(CornerRadius), typeof(SlotImage),
        new PropertyMetadata(default(CornerRadius), (d, _) => ((SlotImage)d).ApplyRadius()));

    /// <summary>Absolute or ms-appx URL of the photo.</summary>
    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Brand colour shown under the photo.</summary>
    public Brush? Tint
    {
        get => (Brush?)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    public CornerRadius SlotCornerRadius
    {
        get => (CornerRadius)GetValue(SlotCornerRadiusProperty);
        set => SetValue(SlotCornerRadiusProperty, value);
    }

    private void ApplySource() =>
        Photo.Source = Uri.TryCreate(Source, UriKind.Absolute, out var uri) ? new BitmapImage(uri) : null;

    private void ApplyTint() => Frame.Background = Tint;

    private void ApplyRadius() => Frame.CornerRadius = SlotCornerRadius;
}
