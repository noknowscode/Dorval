namespace Dorval.Services;

/// <summary>
/// Filter and selection state that outlives a single visit to a tab, so switching
/// away and back lands the resident where they left off.
/// </summary>
public sealed partial class AppFilters : ObservableObject
{
    public HashSet<string> DiscoverChips { get; } = ["This weekend"];

    /// <summary>
    /// The query typed into the tab bar. The bar lives in the shell and Discover lives
    /// in the frame, so the two never share a data context — they meet here instead.
    /// </summary>
    [ObservableProperty]
    public partial string Search { get; set; } = string.Empty;

    public string PlaceCategory { get; set; } = PlaceCategories.All;

    public string SelectedPlaceId { get; set; } = "surrey";

    public PostCategory? CommunityCategory { get; set; }

    public string SavedTab { get; set; } = "All";

    public const string KidsChip = "Kids";
    public const string TodayChip = "Today";
    public const string WeekendChip = "This weekend";
    public const string FreeChip = "Free";
}
