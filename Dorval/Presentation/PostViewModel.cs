using Microsoft.UI.Xaml.Media;

namespace Dorval.Presentation;

/// <summary>
/// One community post and its thread. Replying, marking helpful and bookmarking all
/// happen in place — nothing here navigates away.
/// </summary>
public partial class PostViewModel : ObservableObject
{
    private readonly IDorvalCatalog _catalog;
    private readonly SavedStore _saved;
    private readonly EngagementStore _engagement;
    private readonly ToastService _toasts;
    private readonly int _seedReplyCount;

    public PostViewModel(
        CommunityPost post,
        IDorvalCatalog catalog,
        SavedStore saved,
        EngagementStore engagement,
        ToastService toasts)
    {
        Post = post;
        _catalog = catalog;
        _saved = saved;
        _engagement = engagement;
        _toasts = toasts;

        _seedReplyCount = catalog.RepliesFor(post.Id).Count;
        Replies = [.. catalog.RepliesFor(post.Id)];

        IsBookmarked = saved.IsSaved(post.Id);
        IsHelpful = engagement.HasVoted(post.Id);
    }

    public CommunityPost Post { get; }

    public ObservableCollection<PostReply> Replies { get; }

    public Brush CategoryBackground => AppBrushes.CategoryBackground(Post.Category);

    public Brush CategoryForeground => AppBrushes.CategoryForeground(Post.Category);

    public string AuthorLine => $"{Post.Area} · {Post.Ago}";

    /// <summary>The seeded thread size, plus anything added in this session.</summary>
    public string ReplyCountLabel => $"{Post.ReplyCount + Replies.Count - _seedReplyCount} replies";

    [ObservableProperty]
    public partial bool IsHelpful { get; set; }

    [ObservableProperty]
    public partial bool IsBookmarked { get; set; }

    [ObservableProperty]
    public partial string ReplyText { get; set; } = string.Empty;

    public string HelpfulLabel => $"Helpful {_engagement.CountFor(Post.Id)}";

    public Brush HelpfulBackground => IsHelpful ? AppBrushes.GreenSoft : new SolidColorBrush(Microsoft.UI.Colors.Transparent);

    public Brush HelpfulForeground => IsHelpful ? AppBrushes.GreenDeep : (Brush)Application.Current.Resources["InkMutedBrush"];

    public Brush? BookmarkFill => IsBookmarked ? AppBrushes.GreenDeep : null;

    [RelayCommand]
    private void ToggleHelpful()
    {
        _engagement.ToggleVote(Post.Id);
        IsHelpful = _engagement.HasVoted(Post.Id);
    }

    [RelayCommand]
    private void ToggleBookmark()
    {
        IsBookmarked = _saved.Toggle(Post.Id);
        _toasts.Show(IsBookmarked ? "Post saved" : "Removed from Saved");
    }

    [RelayCommand]
    private void ShowMore() => _toasts.Show("Report · Mute · Copy link");

    [RelayCommand]
    private void SendReply()
    {
        var text = (ReplyText ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            return;
        }

        var reply = new PostReply("You", "Dorval Resident", "just now", text);
        _catalog.AddReply(Post.Id, reply);
        Replies.Add(reply);

        ReplyText = string.Empty;
        OnPropertyChanged(nameof(ReplyCountLabel));
        _toasts.Show("Reply posted");
    }

    partial void OnIsHelpfulChanged(bool value)
    {
        OnPropertyChanged(nameof(HelpfulLabel));
        OnPropertyChanged(nameof(HelpfulBackground));
        OnPropertyChanged(nameof(HelpfulForeground));
    }

    partial void OnIsBookmarkedChanged(bool value) => OnPropertyChanged(nameof(BookmarkFill));
}
