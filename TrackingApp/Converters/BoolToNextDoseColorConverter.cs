using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToNextDoseColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isNextDose && isNextDose)
            {
                // Naranja para la siguiente dosis
                return Color.FromArgb("#FF6B35");
            }
            // Gris claro para las demás dosis pendientes
            return Color.FromArgb("#e0e0e0");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
