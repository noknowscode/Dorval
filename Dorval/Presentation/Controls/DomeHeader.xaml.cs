namespace Dorval.Presentation.Controls;

public sealed partial class DomeHeader : UserControl
{
    public DomeHeader()
    {
        this.InitializeComponent();

        // The curve has to match the header's measured height. ActualHeight does not
        // push updates through a binding here, so take it from the layout pass.
        Body.SizeChanged += (_, e) => Dome.Height = e.NewSize.Height;
    }

    public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(
        nameof(HeaderContent), typeof(object), typeof(DomeHeader), new PropertyMetadata(null));

    /// <summary>What sits under the status row: a greeting, or a title and one action.</summary>
    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }
}
