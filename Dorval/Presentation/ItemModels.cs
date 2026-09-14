using System.Windows.Input;
using Microsoft.UI.Xaml.Media;

namespace Dorval.Presentation;

/// <summary>
/// A filter pill. Bound two-way to a ToggleButton, so the toggle itself holds the
/// state and the owning page just reacts to the change.
/// </summary>
public sealed partial class FilterChip : ObservableObject
{
    private readonly Action<FilterChip>? _onChanged;
    private bool _suppress;

    public FilterChip(string label, bool isSelected = false, Action<FilterChip>? onChanged = null)
    {
        Label = label;

        // Seed the initial value before wiring the callback: the owner is usually still
        // inside its own constructor, so calling back now reaches half-built state.
        SetSilently(isSelected);
        _onChanged = onChanged;
    }

    public string Label { get; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>Sets the value without calling back — for when the owner is the one changing it.</summary>
    public void SetSilently(bool value)
    {
        if (IsSelected == value)
        {
            return;
        }

        _suppress = true;
        IsSelected = value;
        _suppress = false;
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (!_suppress)
        {
            _onChanged?.Invoke(this);
        }
    }
}

/// <summary>A marker on the stylised map, placed by its top-left offset in design space.</summary>
public sealed partial record MapPin(string Id, string Name, double X, double Y, bool IsSelected, ICommand Select)
{
    /// <summary>Shifts the 30x36 pin so its point lands on the coordinate.</summary>
    public Thickness Offset => new(X - 15, Y - 36, 0, 0);

    public Brush Fill => IsSelected ? AppBrushes.Gold : AppBrushes.GreenDeep;
}

/// <summary>
/// A place as it appears in the list: the selected one opens into a full card, the
/// rest stay compact rows.
/// </summary>
public sealed partial record PlaceListItem(Place Place, bool IsSelected, ICommand Select)
{
    public string Name => Place.Name;

    public string Away => Place.Away;

    public string OpenLine => Place.OpenLine;

    public string ImageUrl => Place.ImageUrl;

    public IReadOnlyList<string> Tags => Place.Tags;

    public bool IsExpanded => IsSelected;

    public bool IsCompact => !IsSelected;

    public Brush BorderBrush => IsSelected ? AppBrushes.GreenLine : AppBrushes.Hairline;
}

/// <summary>One of the four "How can we help?" tiles, which double as category filters.</summary>
public sealed partial record HelpTileItem(HelpTile Tile, bool IsActive, ICommand Select)
{
    public string Label => Tile.Label;

    public string IconData => Tile.IconData;

    public Brush Background => IsActive ? AppBrushes.GreenSoft : AppBrushes.Paper;

    public Brush BorderBrush => IsActive ? AppBrushes.GreenLine : AppBrushes.Hairline;
}
