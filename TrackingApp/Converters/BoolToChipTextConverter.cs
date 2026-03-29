using System.Globalization;

namespace TrackingApp.Converters;

/// <summary>
/// Returns White when the bool is true (chip selected) or a dark color when false.
/// Use paired with BoolToChipBgConverter for chip/filter pill buttons.
/// </summary>
public class BoolToChipTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? Colors.White             // selected  — white text on dark bg
            : Color.FromArgb("#2a3d66"); // unselected — dark navy text on light bg

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
