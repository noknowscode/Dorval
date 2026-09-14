using Microsoft.UI.Xaml.Media;

namespace Dorval.Presentation.Controls;

public sealed partial class AppTabBar : UserControl
{
    private const string Today = "Today";
    private const string Discover = "Discover";

    private static readonly GridLength Fill = new(1, GridUnitType.Star);
    private static readonly GridLength Compact = new(36, GridUnitType.Pixel);
    private static readonly GridLength Hug = GridLength.Auto;

    private bool _syncingSearch;

    public AppTabBar()
    {
        this.InitializeComponent();

        TodayDateText.Text = DateTime.Now.ToString("ddd, MMM d", System.Globalization.CultureInfo.GetCultureInfo("en-CA"));
        SearchBox.TextChanged += OnSearchBoxTextChanged;

        Apply();
    }

    public static readonly DependencyProperty ActiveTabProperty = DependencyProperty.Register(
        nameof(ActiveTab), typeof(string), typeof(AppTabBar),
        new PropertyMetadata(Today, (d, _) => ((AppTabBar)d).Apply()));

    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText), typeof(string), typeof(AppTabBar),
        new PropertyMetadata(string.Empty, (d, _) => ((AppTabBar)d).OnSearchTextChanged()));

    /// <summary>Which root the tab bar is currently sitting under: Today, Discover, Places, Community or Saved.</summary>
    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    /// <summary>Two-way with the search field inside the Discover pill.</summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    private void OnClearSearch(object sender, RoutedEventArgs e) => SearchText = string.Empty;

    private void OnSearchBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_syncingSearch)
        {
            return;
        }

        SearchText = SearchBox.Text;
        UpdateClearButton();
    }

    private void OnSearchTextChanged()
    {
        if (SearchBox.Text != SearchText)
        {
            _syncingSearch = true;
            SearchBox.Text = SearchText ?? string.Empty;
            _syncingSearch = false;
        }

        UpdateClearButton();
    }

    private void UpdateClearButton() =>
        ClearSearchButton.Visibility = string.IsNullOrEmpty(SearchText) ? Visibility.Collapsed : Visibility.Visible;

    private void Apply()
    {
        var active = ActiveTab ?? Today;
        var onToday = active == Today;
        var onDiscover = active == Discover;
        var expanded = onToday || onDiscover;

        TodayPill.Visibility = onToday ? Visibility.Visible : Visibility.Collapsed;
        TodayTab.Visibility = onToday ? Visibility.Collapsed : Visibility.Visible;
        SearchPill.Visibility = onDiscover ? Visibility.Visible : Visibility.Collapsed;
        DiscoverTab.Visibility = onDiscover ? Visibility.Collapsed : Visibility.Visible;

        // The expanded root takes the space; the rest shrink and drop their labels.
        Col0.Width = onToday ? Hug : onDiscover ? Compact : Fill;
        Col1.Width = Fill;
        Col2.Width = onDiscover ? Compact : Fill;
        Col3.Width = onDiscover ? Compact : Fill;
        Col4.Width = onDiscover ? Compact : Fill;

        var iconSize = onDiscover ? 18d : 22d;
        var labels = expanded ? Visibility.Collapsed : Visibility.Visible;

        Paint(TodayGlyph, TodayLabel, active == Today, iconSize, labels, filled: false);
        Paint(DiscoverGlyph, DiscoverLabel, active == Discover, iconSize, labels, filled: false);
        Paint(PlacesGlyph, PlacesLabel, active == "Places", iconSize, labels, filled: false);
        Paint(CommunityGlyph, CommunityLabel, active == "Community", iconSize, labels, filled: false);
        Paint(SavedGlyph, SavedLabel, active == "Saved", iconSize, labels, filled: active == "Saved");

        UpdateClearButton();
    }

    private void Paint(IconGlyph glyph, TextBlock label, bool isActive, double size, Visibility labelVisibility, bool filled)
    {
        var brush = (Brush)Application.Current.Resources[isActive ? "OnGreenBrush" : "GreenMutedBrush"];

        glyph.Size = size;
        glyph.IconStroke = brush;
        glyph.IconFill = filled ? brush : null;

        label.Foreground = brush;
        label.Visibility = labelVisibility;
    }
}
