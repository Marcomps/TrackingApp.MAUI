using System.Globalization;
using TrackingApp.Models;

namespace TrackingApp.Converters
{
    public class TipoAlimentoIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TipoAlimentacion tipo)
            {
                return tipo switch
                {
                    TipoAlimentacion.Lactancia => "🤱",
                    TipoAlimentacion.Formula   => "🍼",
                    TipoAlimentacion.Solido    => "🥣",
                    _                          => "🍽️"
                };
            }
            return "🍽️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
