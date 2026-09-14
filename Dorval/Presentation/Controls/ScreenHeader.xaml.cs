namespace Dorval.Presentation.Controls;

public sealed partial class ScreenHeader : UserControl
{
    public ScreenHeader()
    {
        this.InitializeComponent();
        ApplyEyebrow();
        ApplyBack();
    }

    public static readonly DependencyProperty EyebrowProperty = DependencyProperty.Register(
        nameof(Eyebrow), typeof(string), typeof(ScreenHeader),
        new PropertyMetadata(string.Empty, (d, _) => ((ScreenHeader)d).ApplyEyebrow()));

    public static readonly DependencyProperty ShowBackProperty = DependencyProperty.Register(
        nameof(ShowBack), typeof(bool), typeof(ScreenHeader),
        new PropertyMetadata(true, (d, _) => ((ScreenHeader)d).ApplyBack()));

    public static readonly DependencyProperty TrailingContentProperty = DependencyProperty.Register(
        nameof(TrailingContent), typeof(object), typeof(ScreenHeader), new PropertyMetadata(null));

    /// <summary>The small caps label that names the screen.</summary>
    public string Eyebrow
    {
        get => (string)GetValue(EyebrowProperty);
        set => SetValue(EyebrowProperty, value);
    }

    /// <summary>False on a screen reached without a back stack, keeping the title centred.</summary>
    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public object? TrailingContent
    {
        get => GetValue(TrailingContentProperty);
        set => SetValue(TrailingContentProperty, value);
    }

    private void ApplyEyebrow() => EyebrowText.Text = Eyebrow;

    private void ApplyBack() =>
        BackButton.Visibility = ShowBack ? Visibility.Visible : Visibility.Collapsed;
}
