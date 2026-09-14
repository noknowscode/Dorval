namespace Dorval.Presentation;

/// <summary>
/// Browse and search. The chips narrow the weekend rail; the search field in the tab
/// bar narrows everything at once.
/// </summary>
public partial class DiscoverViewModel : ObservableObject
{
    private const string DefaultSubtitle = "Events, activities and places in Dorval";

    private readonly IDorvalCatalog _catalog;
    private readonly AppFilters _filters;

    public DiscoverViewModel(IDorvalCatalog catalog, UserProfile profile, AppFilters filters)
    {
        _catalog = catalog;
        _filters = filters;

        // "Kids" only exists for a household that told us it has kids.
        var labels = new List<string> { AppFilters.TodayChip, AppFilters.WeekendChip };
        if (profile.IsFamilyMode)
        {
            labels.Add(AppFilters.KidsChip);
        }

        labels.Add(AppFilters.FreeChip);

        Chips = [.. labels.Select(label => new FilterChip(label, filters.DiscoverChips.Contains(label), OnChipChanged))];

        // The search field lives in the shell's tab bar, which outlives this page.
        filters.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AppFilters.Search))
            {
                Refresh();
            }
        };
        Refresh();
    }

    public ObservableCollection<FilterChip> Chips { get; }

    public ObservableCollection<DorvalEvent> WeekendEvents { get; } = [];

    public ObservableCollection<DorvalEvent> FreeEvents { get; } = [];

    [ObservableProperty]
    public partial string Subtitle { get; set; } = DefaultSubtitle;

    [ObservableProperty]
    public partial bool WeekendIsEmpty { get; set; }

    /// <summary>Drops every narrowing so the full listing comes back.</summary>
    [RelayCommand]
    private void SeeAll()
    {
        foreach (var chip in Chips)
        {
            chip.SetSilently(false);
        }

        _filters.DiscoverChips.Clear();

        // Clearing the query notifies, which refreshes on its own.
        _filters.Search = string.Empty;
        Refresh();
    }

    private void OnChipChanged(FilterChip chip)
    {
        if (chip.IsSelected)
        {
            _filters.DiscoverChips.Add(chip.Label);
        }
        else
        {
            _filters.DiscoverChips.Remove(chip.Label);
        }

        Refresh();
    }

    private bool IsOn(string label) => _filters.DiscoverChips.Contains(label);

    private void Refresh()
    {
        var query = _filters.Search.Trim();

        Subtitle = query.Length > 0 ? $"Results for “{query}”" : DefaultSubtitle;

        // "Today" replaces the weekend rail outright; the others narrow it.
        IEnumerable<DorvalEvent> weekend = IsOn(AppFilters.TodayChip)
            ? _catalog.Events.Where(e => e.IsToday && e.Matches(query))
            : _catalog.Events.Where(e => e.IsWeekend && e.Matches(query));

        if (IsOn(AppFilters.FreeChip))
        {
            weekend = weekend.Where(e => e.IsFree);
        }

        if (IsOn(AppFilters.KidsChip))
        {
            weekend = weekend.Where(e => e.IsForKids);
        }

        Fill(WeekendEvents, weekend);
        WeekendIsEmpty = WeekendEvents.Count == 0;

        Fill(FreeEvents, _catalog.Events
            .Where(e => e.IsFree && !e.IsWeekend && !e.IsToday && e.Matches(query))
            .Take(4));
    }

    private static void Fill(ObservableCollection<DorvalEvent> target, IEnumerable<DorvalEvent> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}
