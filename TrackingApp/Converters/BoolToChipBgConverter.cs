using System.Globalization;

namespace TrackingApp.Converters;

/// <summary>
/// Returns a filled background color when the bool is true (chip is selected)
/// or a light outlined background when false. Use for chip/filter pill buttons.
/// </summary>
public class BoolToChipBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? Color.FromArgb("#2a3d66")   // selected  — dark navy
            : Color.FromArgb("#f0f2f8");  // unselected — very light blue-grey

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
