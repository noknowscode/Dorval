using Microsoft.UI.Xaml.Data;

namespace Dorval.Presentation;

/// <summary>
/// Resolves a <see cref="PostCategory"/> to its badge colours. Pass "Foreground" as the
/// converter parameter for the text colour; anything else gives the fill.
/// </summary>
public sealed partial class CategoryBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not PostCategory category)
        {
            return null;
        }

        return parameter as string == "Foreground"
            ? AppBrushes.CategoryForeground(category)
            : AppBrushes.CategoryBackground(category);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}

/// <summary>Hides an element when a string is empty, for optional lines of metadata.</summary>
public sealed partial class EmptyStringToCollapsedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
