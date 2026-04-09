using TrackingApp.Models;

namespace TrackingApp.Helpers;

/// <summary>
/// Pure, MAUI-free helper that computes daily food summaries.
/// Kept in Core so it can be unit-tested without a MAUI runtime.
/// </summary>
public static class ResumenCalculator
{
    public record ResumenItem(string Icono, string TipoDisplay, string Total, string Tomas);

    /// <summary>
    /// Given a flat list of entries (already filtered to one day),
    /// returns one summary row per food group, preserving decimal precision.
    /// </summary>
    public static IReadOnlyList<ResumenItem> Compute(IEnumerable<FoodEntry> entries)
    {
        var list = entries.ToList();
        var result = new List<ResumenItem>();

        // ── Standard types: one row each ─────────────────────────────────────

        foreach (var grupo in list
            .Where(e => e.TipoAlimentacion != TipoAlimentacion.Personalizado)
            .GroupBy(e => e.TipoAlimentacion)
            .OrderBy(g => (int)g.Key))
        {
            int tomas = grupo.Count();
            string tomasLabel = FormatTomas(tomas);
            string icono;
            string total;

            switch (grupo.Key)
            {
                case TipoAlimentacion.Lactancia:
                    int totalMin = grupo.Sum(e => e.DuracionMinutos ?? 0);
                    total = totalMin > 0 ? $"{totalMin} min" : tomasLabel;
                    icono = "🤱";
                    break;

                case TipoAlimentacion.Formula:
                    // Sum as decimal to keep full precision, fallback to Amount (double→decimal)
                    decimal sumMl = grupo.Sum(e =>
                        e.CantidadMl.HasValue ? e.CantidadMl.Value : (decimal)e.Amount);
                    total = sumMl > 0 ? $"{sumMl:0.##} ml" : tomasLabel;
                    icono = "🍼";
                    break;

                case TipoAlimentacion.Solido:
                    decimal sumG = grupo.Sum(e =>
                        e.CantidadGramos.HasValue ? e.CantidadGramos.Value : (decimal)e.Amount);
                    total = sumG > 0 ? $"{sumG:0.##} g" : tomasLabel;
                    icono = "🥣";
                    break;

                default:
                    total = tomasLabel;
                    icono = "🍴";
                    break;
            }

            result.Add(new ResumenItem(icono, grupo.First().TipoAlimentacionDisplay, total, tomasLabel));
        }

        // ── Personalizado: one row per distinct FoodType name ─────────────────

        foreach (var grupo in list
            .Where(e => e.TipoAlimentacion == TipoAlimentacion.Personalizado)
            .GroupBy(e => string.IsNullOrWhiteSpace(e.FoodType) ? "Personalizado" : e.FoodType)
            .OrderBy(g => g.Key))
        {
            var entries2 = grupo.ToList();
            int tomas = entries2.Count;
            string tomasLabel = FormatTomas(tomas);
            string total;

            if (entries2.All(e => e.CantidadMl.HasValue))
            {
                decimal sum = entries2.Sum(e => e.CantidadMl!.Value);
                total = sum > 0 ? $"{sum:0.##} ml" : tomasLabel;
            }
            else if (entries2.All(e => e.CantidadGramos.HasValue))
            {
                decimal sum = entries2.Sum(e => e.CantidadGramos!.Value);
                total = sum > 0 ? $"{sum:0.##} g" : tomasLabel;
            }
            else
            {
                var units = entries2
                    .Where(e => !string.IsNullOrWhiteSpace(e.Unit))
                    .Select(e => e.Unit!)
                    .Distinct()
                    .ToList();

                if (units.Count == 1)
                {
                    // Amount is double — sum as double, then round to 4 dp before formatting
                    decimal sum = entries2.Sum(e => Math.Round((decimal)e.Amount, 4));
                    total = sum > 0 ? $"{sum:0.##} {units[0]}" : tomasLabel;
                }
                else
                {
                    // Mixed or no units — just show count
                    total = tomasLabel;
                }
            }

            result.Add(new ResumenItem("🍴", grupo.Key, total, tomasLabel));
        }

        return result;
    }

    private static string FormatTomas(int n) => n == 1 ? "1 toma" : $"{n} tomas";
}
