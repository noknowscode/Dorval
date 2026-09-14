using Windows.Storage;

namespace Dorval.Services;

/// <summary>
/// What onboarding learned about the household. Family mode is what surfaces the
/// "For your family" card on Today and the Kids filter on Discover.
/// </summary>
public sealed class UserProfile
{
    private const string OnboardedKey = "dorval.onboarded";
    private const string LookingForKey = "dorval.lookingFor";
    private const string AgesKey = "dorval.ages";

    public const string ForMyself = "Me";
    public const string ForMyFamily = "My family";

    public static IReadOnlyList<string> AgeBands { get; } = ["0-2", "3-5", "6-8", "9-12", "13-17"];

    private readonly HashSet<string> _ages;

    /// <summary>Readable before the DI container exists, so routing can pick the first screen.</summary>
    public static bool IsOnboarded => ApplicationData.Current.LocalSettings.Values[OnboardedKey] as bool? ?? false;

    public UserProfile()
    {
        var settings = ApplicationData.Current.LocalSettings.Values;
        HasCompletedOnboarding = IsOnboarded;
        LookingFor = settings[LookingForKey] as string ?? ForMyFamily;
        _ages = [.. (settings[AgesKey] as string ?? "3-5").Split(',', StringSplitOptions.RemoveEmptyEntries)];
    }

    public bool HasCompletedOnboarding { get; private set; }

    public string LookingFor { get; private set; }

    public bool IsFamilyMode => LookingFor == ForMyFamily;

    public bool HasAge(string band) => _ages.Contains(band);

    public void SetLookingFor(string value) => LookingFor = value;

    public void ToggleAge(string band)
    {
        if (!_ages.Add(band))
        {
            _ages.Remove(band);
        }
    }

    public void CompleteOnboarding()
    {
        HasCompletedOnboarding = true;

        var settings = ApplicationData.Current.LocalSettings.Values;
        settings[OnboardedKey] = true;
        settings[LookingForKey] = LookingFor;
        settings[AgesKey] = string.Join(',', _ages);
    }
}
