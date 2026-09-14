namespace Dorval.Models;

/// <summary>
/// Stand-in photography for the design's image slots.
/// Seeded so a given event, place or post always gets the same picture; every slot
/// paints a brand tint underneath, so the layout still reads correctly offline.
/// Swap this for the municipality's own media library.
/// </summary>
public static class Placeholders
{
    public static string For(string id) => $"https://picsum.photos/seed/dorval-{id}/720/540";
}
