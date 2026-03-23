using TrackingApp.Models;

namespace TrackingApp.Services
{
    /// <summary>
    /// RF-005 — Implementación del servicio centralizado de conversión de unidades.
    /// Unidades base internas: gramos (peso), centímetros (longitud), mililitros (volumen), minutos (tiempo).
    /// Conversión siempre pasa por la unidad base: valor → base → destino.
    /// </summary>
    public class UnitService : IUnitService
    {
        public decimal Convert(decimal value, UnitType from, UnitType to)
        {
            if (from == to) return value;
            decimal baseValue = ToBaseUnit(value, from);
            return FromBaseUnit(baseValue, to);
        }

        public string Format(decimal value, UnitType unit, int decimals = 1)
        {
            string symbol = GetSymbol(unit);
            return $"{value.ToString($"F{decimals}")} {symbol}";
        }

        public IEnumerable<UnitType> GetUnitsForCategory(MeasureCategory category) => category switch
        {
            MeasureCategory.Peso     => new[] { UnitType.Gramos, UnitType.Kilogramos, UnitType.Libras, UnitType.Onzas },
            MeasureCategory.Longitud => new[] { UnitType.Centimetros, UnitType.Metros, UnitType.Pulgadas, UnitType.Pies },
            MeasureCategory.Volumen  => new[] { UnitType.Mililitros, UnitType.Litros, UnitType.OnzasLiquidas },
            MeasureCategory.Tiempo   => new[] { UnitType.Minutos },
            _ => Array.Empty<UnitType>()
        };

        public string GetSymbol(UnitType unit) => unit switch
        {
            UnitType.Gramos        => "g",
            UnitType.Kilogramos    => "kg",
            UnitType.Libras        => "lb",
            UnitType.Onzas         => "oz",
            UnitType.Centimetros   => "cm",
            UnitType.Metros        => "m",
            UnitType.Pulgadas      => "in",
            UnitType.Pies          => "ft",
            UnitType.Mililitros    => "ml",
            UnitType.Litros        => "L",
            UnitType.OnzasLiquidas => "fl oz",
            UnitType.Minutos       => "min",
            _                      => string.Empty
        };

        public MeasureCategory GetCategory(UnitType unit) => unit switch
        {
            UnitType.Gramos or UnitType.Kilogramos or UnitType.Libras or UnitType.Onzas
                => MeasureCategory.Peso,
            UnitType.Centimetros or UnitType.Metros or UnitType.Pulgadas or UnitType.Pies
                => MeasureCategory.Longitud,
            UnitType.Mililitros or UnitType.Litros or UnitType.OnzasLiquidas
                => MeasureCategory.Volumen,
            UnitType.Minutos
                => MeasureCategory.Tiempo,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Unidad no reconocida.")
        };

        /// <summary>
        /// Convierte a la unidad base de la categoría:
        ///   Peso → gramos | Longitud → centímetros | Volumen → mililitros | Tiempo → minutos
        /// </summary>
        public decimal ToBaseUnit(decimal value, UnitType from) => from switch
        {
            // Peso → gramos
            UnitType.Gramos     => value,
            UnitType.Kilogramos => value * 1_000m,
            UnitType.Libras     => value * 453.59237m,
            UnitType.Onzas      => value * 28.3495231m,

            // Longitud → centímetros
            UnitType.Centimetros => value,
            UnitType.Metros      => value * 100m,
            UnitType.Pulgadas    => value * 2.54m,
            UnitType.Pies        => value * 30.48m,

            // Volumen → mililitros
            UnitType.Mililitros    => value,
            UnitType.Litros        => value * 1_000m,
            UnitType.OnzasLiquidas => value * 29.5735296m,

            // Tiempo → minutos
            UnitType.Minutos => value,

            _ => value
        };

        /// <summary>
        /// Convierte desde la unidad base de la categoría a la unidad destino.
        /// </summary>
        public decimal FromBaseUnit(decimal baseValue, UnitType to) => to switch
        {
            // Base gramos → peso
            UnitType.Gramos     => baseValue,
            UnitType.Kilogramos => baseValue / 1_000m,
            UnitType.Libras     => baseValue / 453.59237m,
            UnitType.Onzas      => baseValue / 28.3495231m,

            // Base centímetros → longitud
            UnitType.Centimetros => baseValue,
            UnitType.Metros      => baseValue / 100m,
            UnitType.Pulgadas    => baseValue / 2.54m,
            UnitType.Pies        => baseValue / 30.48m,

            // Base mililitros → volumen
            UnitType.Mililitros    => baseValue,
            UnitType.Litros        => baseValue / 1_000m,
            UnitType.OnzasLiquidas => baseValue / 29.5735296m,

            // Base minutos → tiempo
            UnitType.Minutos => baseValue,

            _ => baseValue
        };
    }
}
