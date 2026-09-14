namespace Dorval.Models;

/// <summary>
/// Something happening in Dorval on a date, as it appears in listings and on the event page.
/// </summary>
public partial record DorvalEvent(
    string Id,
    string Title,
    string Date,
    string Time,
    string Place,
    bool IsFree,
    bool IsWeekend,
    bool IsToday,
    bool IsForKids,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> GoodToKnow)
{
    public string ImageUrl => Placeholders.For(Id);

    /// <summary>"Sat, Aug 14 · Sunset" — the one-line when, used everywhere but the hero card.</summary>
    public string WhenLine => $"{Date} · {Time}";

    public bool Matches(string query) =>
        string.IsNullOrEmpty(query) ||
        $"{Title} {Place}".Contains(query, StringComparison.OrdinalIgnoreCase);
}
