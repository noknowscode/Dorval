namespace Dorval.Presentation;

/// <summary>
/// Neighbours helping neighbours. The four intent tiles double as the filter for the
/// feed below, so choosing what you want to do also shows you who already did it.
/// </summary>
public partial class CommunityViewModel : ObservableObject
{
    private readonly IDorvalCatalog _catalog;
    private readonly AppFilters _filters;
    private readonly ToastService _toasts;

    public CommunityViewModel(IDorvalCatalog catalog, AppFilters filters, ToastService toasts)
    {
        _catalog = catalog;
        _filters = filters;
        _toasts = toasts;

        Refresh();
    }

    public ObservableCollection<HelpTileItem> HelpTiles { get; } = [];

    public ObservableCollection<CommunityPost> Nearby { get; } = [];

    [ObservableProperty]
    public partial string NearbyTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NearbyLink { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmptyLine { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool NearbyIsEmpty { get; set; }

    [RelayCommand]
    private void ToggleCategory(HelpTile? tile)
    {
        if (tile is null)
        {
            return;
        }

        _filters.CommunityCategory = _filters.CommunityCategory == tile.Category ? null : tile.Category;
        Refresh();
    }

    [RelayCommand]
    private void ClearCategory()
    {
        _filters.CommunityCategory = null;
        Refresh();
    }

    [RelayCommand]
    private void Compose() => _toasts.Show("Composer opens here");

    private void Refresh()
    {
        var active = _filters.CommunityCategory;

        HelpTiles.Clear();
        foreach (var tile in _catalog.HelpTiles)
        {
            HelpTiles.Add(new HelpTileItem(tile, tile.Category == active, ToggleCategoryCommand));
        }

        Nearby.Clear();
        foreach (var post in _catalog.Posts.Where(p => p.Id != "swim" && (active is null || p.Category == active)))
        {
            Nearby.Add(post);
        }

        NearbyIsEmpty = Nearby.Count == 0;
        NearbyTitle = active switch
        {
            null => "Nearby activity",
            PostCategory.Sell => "Discussions nearby",
            _ => $"{active} posts nearby",
        };
        NearbyLink = active is null ? "See all" : "Clear";
        EmptyLine = $"No {Describe(active)} posts nearby yet.";
    }

    /// <summary>"Sell" is the model's word for it; residents see "discussion".</summary>
    private static string Describe(PostCategory? category) => category switch
    {
        null => string.Empty,
        PostCategory.Sell => "discussion",
        _ => category.ToString()!.ToLowerInvariant(),
    };
}
