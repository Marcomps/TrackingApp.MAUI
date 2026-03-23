using System.Globalization;

namespace TrackingApp.Helpers
{
    /// <summary>
    /// Utility class for parsing numeric strings with support for both
    /// dot (.) and comma (,) as decimal separators.
    /// </summary>
    public static class NumericParser
    {
        /// <summary>
        /// Tries to parse a string to double, accepting both dot (.) and comma (,) as decimal separators.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        /// <param name="result">The parsed result if successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public static bool TryParseDouble(string? input, out double result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Normalizar: reemplazar coma por punto para estandarizar
            var normalizedInput = input.Trim().Replace(',', '.');

            // Usar InvariantCulture para que siempre interprete el punto como decimal
            return double.TryParse(normalizedInput, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Parses a string to double, accepting both dot (.) and comma (,) as decimal separators.
        /// Returns default value if parsing fails.
        /// </summary>
        /// <param name="input">The input string to parse</param>
        /// <param name="defaultValue">Default value to return if parsing fails (default: 0)</param>
        /// <returns>The parsed value or default value</returns>
        public static double ParseDoubleOrDefault(string? input, double defaultValue = 0)
        {
            return TryParseDouble(input, out double result) ? result : defaultValue;
        }
    }
}
