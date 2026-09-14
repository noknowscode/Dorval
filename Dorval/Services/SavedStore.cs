namespace Dorval.Services;

/// <summary>
/// The single source of truth for what the resident has hearted. Events, places and
/// posts share one id space, so the Saved page and every heart icon agree without
/// passing state between pages.
/// </summary>
public sealed class SavedStore
{
    private readonly HashSet<string> _ids = ["cinema", "celebrates", "walk", "pine", "library", "bike"];

    /// <summary>Raised after any change, so open pages can re-read their lists.</summary>
    public event EventHandler? Changed;

    public bool IsSaved(string id) => _ids.Contains(id);

    public bool Toggle(string id)
    {
        var nowSaved = _ids.Add(id);
        if (!nowSaved)
        {
            _ids.Remove(id);
        }

        Changed?.Invoke(this, EventArgs.Empty);
        return nowSaved;
    }
}
