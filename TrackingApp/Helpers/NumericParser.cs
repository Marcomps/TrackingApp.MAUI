using System.Globalization;

namespace TrackingApp.Helpers
{
    public static class NumericParser
    {
        public static bool TryParseDouble(string input, out double result)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                result = 0;
                return false;
            }

            // Normalize comma to dot
            string normalizedInput = input.Replace(',', '.');

            return double.TryParse(
                normalizedInput, 
                NumberStyles.Any, 
                CultureInfo.InvariantCulture, 
                out result);
        }
    }
}