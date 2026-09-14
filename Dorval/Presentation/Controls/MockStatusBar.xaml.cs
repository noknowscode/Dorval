namespace Dorval.Presentation.Controls;

public sealed partial class MockStatusBar : UserControl
{
    public MockStatusBar()
    {
        this.InitializeComponent();
        IsHitTestVisible = false;

#if __ANDROID__ || __IOS__
        // The device draws its own; SafeArea reserves the space on the header instead.
        Visibility = Visibility.Collapsed;
#endif
    }
}
