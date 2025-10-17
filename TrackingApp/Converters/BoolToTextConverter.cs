using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConfirmed)
            {
                return isConfirmed ? "✓" : "Confirmar";
            }
            return "Confirmar";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
