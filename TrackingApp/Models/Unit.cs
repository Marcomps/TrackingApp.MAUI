namespace TrackingApp.Models
{
    public enum Unit
    {
        // Weight units
        Gram,
        Kilogram,
        Pound,
        Ounce,

        // Volume units
        Milliliter,
        Liter,
        Teaspoon,
        Tablespoon,
        Cup,

        // Length units
        Millimeter,
        Centimeter,
        Meter,
        Inch,
        Foot,

        // Other units
        Piece,
        Slice,
        Tablet,
        Capsule
    }

    public static class UnitExtensions
    {
        public static string GetDisplayName(this Unit unit)
        {
            return unit switch
            {
                Unit.Gram => "g",
                Unit.Kilogram => "kg",
                Unit.Pound => "lb",
                Unit.Ounce => "oz",
                Unit.Milliliter => "ml",
                Unit.Liter => "L",
                Unit.Teaspoon => "tsp",
                Unit.Tablespoon => "tbsp",
                Unit.Cup => "cup",
                Unit.Millimeter => "mm",
                Unit.Centimeter => "cm",
                Unit.Meter => "m",
                Unit.Inch => "in",
                Unit.Foot => "ft",
                Unit.Piece => "pieza",
                Unit.Slice => "rebanada",
                Unit.Tablet => "tableta",
                Unit.Capsule => "cápsula",
                _ => unit.ToString()
            };
        }

        public static Unit[] GetWeightUnits() => new[] { Unit.Gram, Unit.Kilogram, Unit.Pound, Unit.Ounce };
        public static Unit[] GetVolumeUnits() => new[] { Unit.Milliliter, Unit.Liter, Unit.Teaspoon, Unit.Tablespoon, Unit.Cup };
        public static Unit[] GetLengthUnits() => new[] { Unit.Millimeter, Unit.Centimeter, Unit.Meter, Unit.Inch, Unit.Foot };
        public static Unit[] GetCommonUnits() => new[] { Unit.Gram, Unit.Kilogram, Unit.Milliliter, Unit.Liter, Unit.Piece };
    }
}