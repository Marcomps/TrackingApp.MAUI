using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToNextDoseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isNextDose && isNextDose)
            {
                // Texto naranja para la siguiente dosis
                return Color.FromArgb("#FF6B35");
            }
            // Texto normal oscuro para las demás
            return Color.FromArgb("#333333");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
