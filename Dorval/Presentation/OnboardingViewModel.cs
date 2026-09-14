using Uno.Extensions.Navigation;

namespace Dorval.Presentation;

/// <summary>
/// The one question worth asking before anything else: who is this for? The answer
/// decides whether Today and Discover surface family content at all.
/// </summary>
public partial class OnboardingViewModel : ObservableObject
{
    private readonly UserProfile _profile;
    private readonly INavigator _navigator;

    public OnboardingViewModel(UserProfile profile, INavigator navigator)
    {
        _profile = profile;
        _navigator = navigator;

        Audiences =
        [
            new FilterChip(UserProfile.ForMyself, profile.LookingFor == UserProfile.ForMyself, OnAudienceChanged),
            new FilterChip(UserProfile.ForMyFamily, profile.LookingFor == UserProfile.ForMyFamily, OnAudienceChanged),
        ];

        Ages = [.. UserProfile.AgeBands.Select(band => new FilterChip(band, profile.HasAge(band), OnAgeChanged))];

        IsFamily = profile.IsFamilyMode;
        Console.WriteLine("[dorval] OnboardingViewModel constructed");
    }

    /// <summary>The illustration at the top of the question.</summary>
    public string ImageUrl => Placeholders.For("family");

    public ObservableCollection<FilterChip> Audiences { get; }

    public ObservableCollection<FilterChip> Ages { get; }

    /// <summary>The age picker only exists once someone says they have a household.</summary>
    [ObservableProperty]
    public partial bool IsFamily { get; set; }

    /// <summary>
    /// Skipping still counts as onboarding — we just keep the defaults — so this
    /// records and navigates as one action.
    /// </summary>
    [RelayCommand]
    private async Task SkipAsync()
    {
        _profile.CompleteOnboarding();
        await _navigator.NavigateViewModelAsync<TodayViewModel>(this, qualifier: Qualifiers.ClearBackStack);
    }

    private void OnAudienceChanged(FilterChip chip)
    {
        if (!chip.IsSelected)
        {
            chip.SetSilently(true);
            return;
        }

        foreach (var other in Audiences.Where(a => a != chip))
        {
            other.SetSilently(false);
        }

        _profile.SetLookingFor(chip.Label);
        IsFamily = _profile.IsFamilyMode;
    }

    private void OnAgeChanged(FilterChip chip) => _profile.ToggleAge(chip.Label);
}
