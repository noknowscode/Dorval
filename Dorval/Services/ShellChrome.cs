using Uno.Extensions.Navigation;

namespace Dorval.Services;

/// <summary>
/// Where the tab bar is standing. The bar lives in the shell, above the navigation
/// frame, so it has no page to ask — it follows the route instead.
/// </summary>
/// <remarks>
/// This is a singleton on purpose. Navigation builds its own <see cref="ShellViewModel"/>
/// for the frame and the app resolves another for the shell, so any chrome state held on
/// the view model would exist twice and only one copy would ever hear a route change.
/// </remarks>
public sealed partial class ShellChrome : ObservableObject
{
    /// <summary>The five roots the tab bar can be sitting on.</summary>
    private static readonly string[] Roots = ["Today", "Discover", "Places", "Community", "Saved"];

    /// <summary>Everything else the frame can show. These are pushed over a root, so the bar steps aside.</summary>
    private static readonly string[] Leaves = ["Onboarding", "Event", "Post", "Reminder", "Safety"];

    public ShellChrome(IRouteNotifier notifier) => notifier.RouteChanged += OnRouteChanged;

    /// <summary>Which root the tab bar paints as active.</summary>
    [ObservableProperty]
    public partial string ActiveTab { get; set; } = "Today";

    /// <summary>The bar belongs to the roots; a pushed detail page gets the full height.</summary>
    [ObservableProperty]
    public partial bool ShowTabBar { get; set; }

#if DEBUG
    /// <summary>
    /// The live region navigator. Synthesized input cannot reach an Uno desktop window
    /// from a background process, so the transition harness in <c>tools/</c> drives a tab
    /// press through this instead of a pointer. Route changes are the only place a
    /// navigator is handed out, which is why it is captured here.
    /// </summary>
    internal INavigator? DebugNavigator { get; private set; }
#endif

    /// <summary>
    /// The route arrives as a path ("Community/Post"), so the destination is its last
    /// known segment. Anything unrecognised leaves the bar exactly as it is rather than
    /// guessing — an intermediate notification should not make it flicker.
    /// </summary>
    private void OnRouteChanged(object? sender, RouteChangedEventArgs e)
    {
#if DEBUG
        DebugNavigator = e.Navigator;
#endif

        var destination = (e.Navigator?.Route?.ToString() ?? string.Empty)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.Trim('-', '!', '.', ' '))
            .LastOrDefault(segment => Roots.Contains(segment) || Leaves.Contains(segment));

        if (destination is null)
        {
            return;
        }

        ShowTabBar = Roots.Contains(destination);

        if (ShowTabBar)
        {
            ActiveTab = destination;
        }
    }
}
