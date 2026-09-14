namespace Dorval.Services;

/// <summary>
/// "Helpful" votes on community posts: the seeded count from other residents plus
/// whether this resident has added theirs.
/// </summary>
public sealed class EngagementStore
{
    private readonly Dictionary<string, int> _baseCounts = new()
    {
        ["swim"] = 8,
        ["pool"] = 3,
        ["bike"] = 1,
        ["washer"] = 0,
    };

    private readonly HashSet<string> _myVotes = [];

    public bool HasVoted(string postId) => _myVotes.Contains(postId);

    public int CountFor(string postId) =>
        (_baseCounts.TryGetValue(postId, out var seed) ? seed : 0) + (HasVoted(postId) ? 1 : 0);

    public void ToggleVote(string postId)
    {
        if (!_myVotes.Add(postId))
        {
            _myVotes.Remove(postId);
        }
    }
}
