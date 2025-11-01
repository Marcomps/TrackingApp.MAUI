namespace TrackingApp.Models
{
    public enum Unit
    {
        // Unidad por defecto
        Ounce,      // oz - Onza (por defecto)
        
        // Weight units
        Gram,       // g - Gramo
        Kilogram,   // kg - Kilogramo
        Pound,      // lb - Libra
        
        // Length unit
        Meter,      // m - Metro
        
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
                Unit.Kilogram => "kg",
                Unit.Pound => "lb",
                Unit.Meter => "m",
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
            Unit.Kilogram, 
            Unit.Pound, 
            Unit.Meter, 
            Unit.Unit 
        };
    }
}