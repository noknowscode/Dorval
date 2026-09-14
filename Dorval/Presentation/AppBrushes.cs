using Microsoft.UI.Xaml.Media;

namespace Dorval.Presentation;

/// <summary>
/// Palette lookups for the few places that pick a brush in code — map pins, category
/// badges — so page code never names a colour value.
/// </summary>
internal static class AppBrushes
{
    public static Brush GreenDeep => Get("GreenDeepBrush");

    public static Brush Gold => Get("GoldBrush");

    public static Brush Hairline => Get("HairlineBrush");

    public static Brush GreenLine => Get("GreenLineBrush");

    public static Brush GreenSoft => Get("GreenSoftBrush");

    public static Brush Paper => Get("PaperBrush");

    public static Brush Card => Get("CardBrush");

    /// <summary>Badge background for a community post category.</summary>
    public static Brush CategoryBackground(PostCategory category) => category switch
    {
        PostCategory.Question => Get("GreenSoftBrush"),
        PostCategory.Free => Get("AmberBrush"),
        PostCategory.Help => Get("SkyBrush"),
        _ => Get("LilacBrush"),
    };

    /// <summary>Badge text colour for a community post category.</summary>
    public static Brush CategoryForeground(PostCategory category) => category switch
    {
        PostCategory.Question => Get("GreenDeepBrush"),
        PostCategory.Free => Get("AmberInkBrush"),
        PostCategory.Help => Get("SkyInkBrush"),
        _ => Get("LilacInkBrush"),
    };

    private static Brush Get(string key) => (Brush)Application.Current.Resources[key];
}
