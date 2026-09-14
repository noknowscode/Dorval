using System.Collections;

namespace Dorval.Presentation.Controls;

public sealed partial class CityMap : UserControl
{
    public CityMap() => this.InitializeComponent();

    public static readonly DependencyProperty PinsProperty = DependencyProperty.Register(
        nameof(Pins), typeof(IEnumerable), typeof(CityMap), new PropertyMetadata(null));

    /// <summary>The <see cref="MapPin"/> markers to drop on the map.</summary>
    public IEnumerable? Pins
    {
        get => (IEnumerable?)GetValue(PinsProperty);
        set => SetValue(PinsProperty, value);
    }
}
