namespace TrackingApp.Services
{
    using TrackingApp.Models;

    /// <summary>
    /// RF-005 — Contrato del servicio centralizado de conversión y formateo de unidades.
    /// Todos los módulos deben usar este servicio para garantizar consistencia.
    /// El almacenamiento interno siempre es en unidades métricas base:
    ///   Peso → gramos | Longitud → centímetros | Volumen → mililitros
    /// </summary>
    public interface IUnitService
    {
        /// <summary>
        /// Convierte un valor de una unidad a otra.
        /// Ambas unidades deben pertenecer a la misma <see cref="MeasureCategory"/>.
        /// </summary>
        decimal Convert(decimal value, UnitType from, UnitType to);

        /// <summary>
        /// Formatea un valor con su símbolo de unidad (ej: "3.5 kg", "120 ml").
        /// </summary>
        string Format(decimal value, UnitType unit, int decimals = 1);

        /// <summary>
        /// Devuelve las unidades disponibles para una categoría de medida.
        /// </summary>
        IEnumerable<UnitType> GetUnitsForCategory(MeasureCategory category);

        /// <summary>
        /// Devuelve el símbolo de texto corto para una unidad (ej: "kg", "cm", "fl oz").
        /// </summary>
        string GetSymbol(UnitType unit);

        /// <summary>
        /// Devuelve la categoría de medida a la que pertenece la unidad.
        /// </summary>
        MeasureCategory GetCategory(UnitType unit);

        /// <summary>
        /// Convierte un valor a la unidad base de su categoría (gramos, cm, ml o minutos).
        /// </summary>
        decimal ToBaseUnit(decimal value, UnitType from);

        /// <summary>
        /// Convierte desde la unidad base de una categoría a la unidad destino.
        /// </summary>
        decimal FromBaseUnit(decimal baseValue, UnitType to);
    }
}
