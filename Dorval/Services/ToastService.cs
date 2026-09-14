using Microsoft.UI.Xaml;

namespace Dorval.Services;

/// <summary>
/// The dark pill that confirms an action. One at a time, 1.8s, app-wide — a new
/// message restarts the clock rather than queueing behind the old one.
/// </summary>
public sealed partial class ToastService : ObservableObject
{
    private DispatcherTimer? _timer;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsShowing { get; set; }

    public void Show(string message)
    {
        Message = message;
        IsShowing = true;

        _timer ??= CreateTimer();
        _timer.Stop();
        _timer.Start();
    }

    private DispatcherTimer CreateTimer()
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1800) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            IsShowing = false;
        };
        return timer;
    }
}
