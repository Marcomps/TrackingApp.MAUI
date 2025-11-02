namespace TrackingApp.Models
{
    public enum Unit
    {
        // Unidad por defecto
        Ounce,      // oz - Onza (por defecto)
        
        // Weight units
        Gram,       // g - Gramo
        Kilogram,   // kilo - Kilogramo
        Pound,      // libra - Libra
        
        // Volume unit
        Milliliter, // ml - Mililitro
        
        // Generic unit
        Unit        // unidad - Unidad genérica (ej: 3 unidades de capsulas)
    }

    public static class UnitExtensions
    {
        public static string GetDisplayText(this Unit unit)
        {
            return unit switch
            {
                Unit.Ounce => "oz",
                Unit.Gram => "g",
                Unit.Kilogram => "kilo",
                Unit.Pound => "libra",
                Unit.Milliliter => "ml",
                Unit.Unit => "unidad",
                _ => unit.ToString()
            };
        }

        /// <summary>
        /// Obtiene todas las unidades disponibles en la aplicación
        /// </summary>
        public static Unit[] GetAllUnits() => new[] 
        { 
            Unit.Ounce,     // Por defecto
            Unit.Gram, 
            Unit.Milliliter,
            Unit.Kilogram, 
            Unit.Pound, 
            Unit.Unit 
        };
    }
}