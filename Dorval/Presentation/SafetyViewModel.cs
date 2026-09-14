using Uno.Extensions.Navigation;

namespace Dorval.Presentation;

/// <summary>
/// The ground rules. Reached two ways: as the last step of onboarding, where agreeing
/// starts the app, or from Community, where it is a reminder and just closes.
/// </summary>
public partial class SafetyViewModel
{
    private readonly UserProfile _profile;
    private readonly INavigator _navigator;
    private readonly ToastService _toasts;
    private readonly bool _isOnboarding;

    public SafetyViewModel(IDorvalCatalog catalog, UserProfile profile, INavigator navigator, ToastService toasts)
    {
        _profile = profile;
        _navigator = navigator;
        _toasts = toasts;

        _isOnboarding = !profile.HasCompletedOnboarding;
        Rules = catalog.Guidelines;
    }

    public IReadOnlyList<GuidelineRule> Rules { get; }

    public string AgreeLabel => _isOnboarding ? "I understand, let’s go" : "Got it";

    [RelayCommand]
    private void ShowGuidelines() => _toasts.Show("Opening community guidelines");

    /// <summary>
    /// Agreeing has to record the agreement and move on together, so this is a command
    /// rather than a Navigation.Request in markup.
    /// </summary>
    [RelayCommand]
    private async Task AgreeAsync()
    {
        if (_isOnboarding)
        {
            _profile.CompleteOnboarding();
            await _navigator.NavigateViewModelAsync<TodayViewModel>(this, qualifier: Qualifiers.ClearBackStack);
        }
        else
        {
            await _navigator.NavigateBackAsync(this);
        }
    }
}
