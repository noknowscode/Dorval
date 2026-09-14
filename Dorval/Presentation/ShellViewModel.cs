namespace Dorval.Presentation;

/// <summary>
/// The chrome that outlives a page: the toast pill and the tab bar. Both sit in the
/// shell rather than in each page, so a tab switch only swaps what is above them.
/// </summary>
public partial class ShellViewModel
{
    public ShellViewModel(ToastService toasts, ShellChrome chrome, AppFilters filters)
    {
        Toasts = toasts;
        Chrome = chrome;
        Filters = filters;
    }

    /// <summary>Feeds the app-wide toast pill that lives above every screen.</summary>
    public ToastService Toasts { get; }

    /// <summary>Which tab the bar paints as active, and whether it is showing at all.</summary>
    public ShellChrome Chrome { get; }

    /// <summary>Holds the query the tab bar's search field edits and Discover reads.</summary>
    public AppFilters Filters { get; }
}
