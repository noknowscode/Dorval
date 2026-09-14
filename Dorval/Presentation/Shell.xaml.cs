namespace Dorval.Presentation;

public sealed partial class Shell : UserControl, IContentControlProvider
{
    public Shell()
    {
        this.InitializeComponent();

        // Replay the entrance every time the toast comes back, not just the first time.
        ToastHost.RegisterPropertyChangedCallback(VisibilityProperty, (_, _) =>
        {
            if (ToastHost.Visibility == Visibility.Visible)
            {
                ToastIn.Begin();
            }
        });
    }

    public ContentControl ContentControl => Splash;
}
