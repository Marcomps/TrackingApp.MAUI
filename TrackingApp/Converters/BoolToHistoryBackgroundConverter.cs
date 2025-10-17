using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToHistoryBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHistory)
            {
                // Si es historial (confirmado): verde claro, si es pendiente: gris claro
                return isHistory ? Color.FromArgb("#e8f5e9") : Color.FromArgb("#f7f7f7");
            }
            return Color.FromArgb("#f7f7f7");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
