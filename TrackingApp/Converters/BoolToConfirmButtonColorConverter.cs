using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToConfirmButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isNextDose && isNextDose)
            {
                // Botón naranja para la siguiente dosis (más visible)
                return Color.FromArgb("#FF6B35");
            }
            // Botón verde para las demás dosis pendientes
            return Color.FromArgb("#4CAF50");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
