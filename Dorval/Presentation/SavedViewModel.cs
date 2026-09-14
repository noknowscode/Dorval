namespace Dorval.Presentation;

/// <summary>
/// Everything the resident hearted, in one place. Events, places and posts share the
/// same store, so the tabs are a view over one list rather than three.
/// </summary>
public partial class SavedViewModel : ObservableObject
{
    private static readonly string[] TabLabels = ["All", "Events", "Places", "Posts"];

    private readonly IDorvalCatalog _catalog;
    private readonly SavedStore _saved;
    private readonly AppFilters _filters;
    private readonly ToastService _toasts;

    public SavedViewModel(IDorvalCatalog catalog, SavedStore saved, AppFilters filters, ToastService toasts)
    {
        _catalog = catalog;
        _saved = saved;
        _filters = filters;
        _toasts = toasts;

        Tabs = [.. TabLabels.Select(t => new FilterChip(t, t == filters.SavedTab, OnTabChanged))];

        _saved.Changed += (_, _) => Refresh();
        Refresh();
    }

    public ObservableCollection<FilterChip> Tabs { get; }

    public ObservableCollection<DorvalEvent> Events { get; } = [];

    public ObservableCollection<Place> Places { get; } = [];

    public ObservableCollection<CommunityPost> Posts { get; } = [];

    [ObservableProperty]
    public partial bool ShowEvents { get; set; }

    [ObservableProperty]
    public partial bool ShowPlaces { get; set; }

    [ObservableProperty]
    public partial bool ShowPosts { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    [RelayCommand]
    private void Unsave(string? id)
    {
        if (id is null)
        {
            return;
        }

        _saved.Toggle(id);
        _toasts.Show("Removed from Saved");
    }

    /// <summary>The tabs are a single choice, so unchecking the active one puts it back.</summary>
    private void OnTabChanged(FilterChip chip)
    {
        if (!chip.IsSelected)
        {
            chip.SetSilently(true);
            return;
        }

        foreach (var other in Tabs.Where(t => t != chip))
        {
            other.SetSilently(false);
        }

        _filters.SavedTab = chip.Label;
        Refresh();
    }

    private void Refresh()
    {
        var tab = _filters.SavedTab;

        Events.Clear();
        foreach (var e in _catalog.Events.Where(e => _saved.IsSaved(e.Id)))
        {
            Events.Add(e);
        }

        Places.Clear();
        foreach (var p in _catalog.Places.Where(p => _saved.IsSaved(p.Id)))
        {
            Places.Add(p);
        }

        Posts.Clear();
        foreach (var p in _catalog.Posts.Where(p => _saved.IsSaved(p.Id)))
        {
            Posts.Add(p);
        }

        ShowEvents = tab is "All" or "Events" && Events.Count > 0;
        ShowPlaces = tab is "All" or "Places" && Places.Count > 0;
        ShowPosts = tab is "All" or "Posts" && Posts.Count > 0;
        IsEmpty = !ShowEvents && !ShowPlaces && !ShowPosts;
    }
}
