namespace Dorval.Models;

/// <summary>
/// Why a neighbour posted. Drives the badge colour and the "how can we help?" tiles.
/// <c>Sell</c> is the model name; it reads as "Discussion" everywhere in the UI.
/// </summary>
public enum PostCategory
{
    Question,
    Free,
    Help,
    Sell,
}

public partial record CommunityPost(
    string Id,
    PostCategory Category,
    string Title,
    string Author,
    string Area,
    string Ago,
    string Meta,
    int ReplyCount,
    string Body)
{
    public string ImageUrl => Placeholders.For(Id);

    public string Initial => Author[..1];

    public string CategoryLabel => Category.ToString();
}

public record PostReply(string Author, string Role, string Ago, string Text)
{
    public string Initial => Author[..1];
}

/// <summary>One of the four "How can we help?" tiles, which double as category filters.</summary>
public record HelpTile(string Label, PostCategory Category, string IconData);

/// <summary>A line from the community ground rules.</summary>
public record GuidelineRule(string Text, string IconData);
