using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToNextDoseBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isNextDose && isNextDose)
            {
                // Fondo naranja claro para la siguiente dosis
                return Color.FromArgb("#FFF3E0");
            }
            // Fondo blanco para las demás
            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
