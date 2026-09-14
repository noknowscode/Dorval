namespace Dorval.Models;

/// <summary>
/// A municipal place — park, pool, library, museum. <see cref="PinX"/>/<see cref="PinY"/>
/// are coordinates in the stylised map's 343x150 design space.
/// </summary>
public partial record Place(
    string Id,
    string Name,
    string Kind,
    string Away,
    string OpenLine,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Categories,
    double PinX,
    double PinY)
{
    public string ImageUrl => Placeholders.For(Id);

    public bool InCategory(string category) =>
        category == PlaceCategories.All || Categories.Contains(category);
}

public static class PlaceCategories
{
    public const string All = "All";

    public static IReadOnlyList<string> Ordered { get; } =
        ["All", "Parks", "Pools", "Culture", "Sports"];
}
