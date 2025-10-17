using System.Globalization;

namespace TrackingApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isHistory)
            {
                // Si es historial (confirmado): verde, si es pendiente: azul
                return isHistory ? Color.FromArgb("#4CAF50") : Color.FromArgb("#2196F3");
            }
            return Color.FromArgb("#2196F3");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
