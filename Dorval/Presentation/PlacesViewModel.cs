namespace Dorval.Presentation;

/// <summary>
/// The map and the places on it. One place is always selected: it leads the list as a
/// full card and turns gold on the map, so the map and the list never disagree.
/// </summary>
public partial class PlacesViewModel : ObservableObject
{
    private readonly IDorvalCatalog _catalog;
    private readonly AppFilters _filters;
    private readonly ToastService _toasts;

    public PlacesViewModel(IDorvalCatalog catalog, AppFilters filters, ToastService toasts)
    {
        _catalog = catalog;
        _filters = filters;
        _toasts = toasts;

        Categories =
        [
            .. PlaceCategories.Ordered.Select(c => new FilterChip(c, c == filters.PlaceCategory, OnCategoryChanged))
        ];

        Refresh();
    }

    public ObservableCollection<FilterChip> Categories { get; }

    public ObservableCollection<PlaceListItem> Places { get; } = [];

    public ObservableCollection<MapPin> Pins { get; } = [];

    [RelayCommand]
    private void Locate() => _toasts.Show("Centred on your location");

    [RelayCommand]
    private void SelectPlace(string? id)
    {
        if (id is null || _filters.SelectedPlaceId == id)
        {
            return;
        }

        _filters.SelectedPlaceId = id;
        Refresh();
    }

    /// <summary>Category chips pick exactly one, so unchecking the active one puts it back.</summary>
    private void OnCategoryChanged(FilterChip chip)
    {
        if (!chip.IsSelected)
        {
            chip.SetSilently(true);
            return;
        }

        foreach (var other in Categories.Where(c => c != chip))
        {
            other.SetSilently(false);
        }

        _filters.PlaceCategory = chip.Label;
        Refresh();
    }

    private void Refresh()
    {
        var visible = _catalog.Places.Where(p => p.InCategory(_filters.PlaceCategory)).ToList();

        // Keep the selection if it survived the filter, otherwise fall to the first place.
        var selectedId = visible.Any(p => p.Id == _filters.SelectedPlaceId)
            ? _filters.SelectedPlaceId
            : visible.FirstOrDefault()?.Id;
        _filters.SelectedPlaceId = selectedId ?? string.Empty;

        Places.Clear();
        foreach (var place in visible.OrderByDescending(p => p.Id == selectedId))
        {
            Places.Add(new PlaceListItem(place, place.Id == selectedId, SelectPlaceCommand));
        }

        Pins.Clear();
        foreach (var place in visible)
        {
            Pins.Add(new MapPin(place.Id, place.Name, place.PinX, place.PinY, place.Id == selectedId, SelectPlaceCommand));
        }
    }
}
