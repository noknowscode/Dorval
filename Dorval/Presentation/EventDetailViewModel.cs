using Microsoft.UI.Xaml.Media;

namespace Dorval.Presentation;

/// <summary>
/// One event, with the three things a resident does next: save it, put it in a
/// calendar, or send it to someone.
/// </summary>
public partial class EventDetailViewModel : ObservableObject
{
    private readonly SavedStore _saved;
    private readonly ToastService _toasts;

    public EventDetailViewModel(DorvalEvent evt, SavedStore saved, ToastService toasts)
    {
        Event = evt;
        _saved = saved;
        _toasts = toasts;

        IsSaved = saved.IsSaved(evt.Id);
    }

    public DorvalEvent Event { get; }

    [ObservableProperty]
    public partial bool IsSaved { get; set; }

    [ObservableProperty]
    public partial bool IsOnCalendar { get; set; }

    public string SaveLabel => IsSaved ? "Saved" : "Save";

    public Brush SaveBackground => IsSaved ? AppBrushes.GreenSoft : AppBrushes.Card;

    public Brush? HeartFill => IsSaved ? AppBrushes.GreenDeep : null;

    public string CalendarLabel => IsOnCalendar ? "Added ✓" : "Add to calendar";

    [RelayCommand]
    private void ToggleSave()
    {
        IsSaved = _saved.Toggle(Event.Id);
        _toasts.Show(IsSaved ? "Saved to your list" : "Removed from Saved");
    }

    [RelayCommand]
    private void AddToCalendar()
    {
        if (IsOnCalendar)
        {
            return;
        }

        IsOnCalendar = true;
        _toasts.Show("Added to your calendar");
    }

    [RelayCommand]
    private void Share() => _toasts.Show("Share sheet opened");

    partial void OnIsSavedChanged(bool value)
    {
        OnPropertyChanged(nameof(SaveLabel));
        OnPropertyChanged(nameof(SaveBackground));
        OnPropertyChanged(nameof(HeartFill));
    }

    partial void OnIsOnCalendarChanged(bool value) => OnPropertyChanged(nameof(CalendarLabel));
}
