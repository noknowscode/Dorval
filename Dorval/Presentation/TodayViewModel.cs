using Uno.Extensions.Navigation;

namespace Dorval.Presentation;

/// <summary>
/// The home screen: what is on today, what is on this weekend, and the one deadline
/// worth knowing about.
/// </summary>
public partial class TodayViewModel
{
    private readonly ToastService _toasts;
    private readonly AppFilters _filters;
    private readonly INavigator _navigator;

    public TodayViewModel(
        IDorvalCatalog catalog,
        UserProfile profile,
        ToastService toasts,
        AppFilters filters,
        INavigator navigator)
    {
        _toasts = toasts;
        _filters = filters;
        _navigator = navigator;

        Hero = catalog.Event("cinema")!;
        Happening = catalog.Event("market")!;
        Upcoming = catalog.Event("walk")!;

        ShowFamilyCard = profile.IsFamilyMode;
        KidsThisWeekend = catalog.Events.Count(e => e is { IsWeekend: true, IsForKids: true });
    }

    /// <summary>The weekend headline, given the full-bleed card.</summary>
    public DorvalEvent Hero { get; }

    public DorvalEvent Happening { get; }

    public DorvalEvent Upcoming { get; }

    public bool ShowFamilyCard { get; }

    public int KidsThisWeekend { get; }

    public string TodayLabel => $"TODAY · {Happening.Date.ToUpperInvariant()}";

    public string FamilyLine => $"{KidsThisWeekend} activities this weekend";

    [RelayCommand]
    private void RingBell() => _toasts.Show("No new notifications");

    /// <summary>
    /// Jumps to Discover already filtered to the weekend's kids activities. The filter
    /// and the navigation have to land together, so this is one command rather than a
    /// Navigation.Request in markup.
    /// </summary>
    [RelayCommand]
    private async Task ShowFamilyPicksAsync()
    {
        _filters.DiscoverChips.Clear();
        _filters.DiscoverChips.Add(AppFilters.WeekendChip);
        _filters.DiscoverChips.Add(AppFilters.KidsChip);

        await _navigator.NavigateViewModelAsync<DiscoverViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }
}
