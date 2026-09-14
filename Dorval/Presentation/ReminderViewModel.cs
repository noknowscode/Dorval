using Microsoft.UI.Xaml.Media;

namespace Dorval.Presentation;

/// <summary>
/// A registration that opens later. The only thing to do here is ask to be told, so
/// the whole screen builds to that one button.
/// </summary>
public partial class ReminderViewModel : ObservableObject
{
    /// <summary>The pool photo at the top of the screen.</summary>
    public string ImageUrl => Placeholders.For("swim");

    [ObservableProperty]
    public partial bool IsReminderSet { get; set; }

    public string ReminderLabel => IsReminderSet ? "Reminder set ✓" : "Remind me";

    public Brush ReminderBackground => IsReminderSet ? AppBrushes.GreenSoft : AppBrushes.GreenDeep;

    public Brush ReminderForeground => IsReminderSet ? AppBrushes.GreenDeep : AppBrushes.Card;

    public string ReminderNote => IsReminderSet
        ? "We'll notify you Tuesday at 11:30 AM."
        : "We'll notify you 30 minutes before.";

    [RelayCommand]
    private void ToggleReminder() => IsReminderSet = !IsReminderSet;

    partial void OnIsReminderSetChanged(bool value)
    {
        OnPropertyChanged(nameof(ReminderLabel));
        OnPropertyChanged(nameof(ReminderBackground));
        OnPropertyChanged(nameof(ReminderForeground));
        OnPropertyChanged(nameof(ReminderNote));
    }
}
