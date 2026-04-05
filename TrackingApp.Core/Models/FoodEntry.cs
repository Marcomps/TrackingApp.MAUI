using SQLite;

namespace TrackingApp.Models
{
    public class FoodEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ── Campos existentes ────────────────────────────────────────────────

        public string FoodType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Time { get; set; }

        /// <summary>Mantenido por compatibilidad. Usar PerfilId para nuevos registros.</summary>
        public string UserType { get; set; } = string.Empty;

        // ── RF-003: Campos nuevos v2.0 ───────────────────────────────────────

        /// <summary>FK a Perfil.Id. 0 = sin perfil asignado (registros legacy).</summary>
        public int PerfilId { get; set; }

        public TipoAlimentacion TipoAlimentacion { get; set; } = TipoAlimentacion.Formula;

        /// <summary>Solo se usa cuando TipoAlimentacion == Lactancia.</summary>
        public PechoLactancia? Pecho { get; set; }

        /// <summary>Duración en minutos. Solo se usa cuando TipoAlimentacion == Lactancia.</summary>
        public int? DuracionMinutos { get; set; }

        /// <summary>
        /// Cantidad en mililitros (fórmula/biberón). Almacenado en ml.
        /// Corresponde al campo Amount legado para registros de tipo líquido.
        /// </summary>
        public decimal? CantidadMl { get; set; }

        /// <summary>Cantidad en gramos (alimentación sólida). Almacenado en g.</summary>
        public decimal? CantidadGramos { get; set; }

        public string? Notas { get; set; }

        // ── Propiedades calculadas (no persistidas) ──────────────────────────

        [Ignore]
        public string DisplayText => $"{Amount} {Unit} de {FoodType} a las {Time:hh:mm tt}";

        [Ignore]
        public string DisplayAmount
        {
            get
            {
                if (TipoAlimentacion == TipoAlimentacion.Lactancia)
                    return DuracionMinutos.HasValue ? $"{DuracionMinutos} min" : "—";
                if (Amount > 0 && !string.IsNullOrWhiteSpace(Unit))
                    return $"{Amount:0.##} {Unit}";
                if (Amount > 0)
                    return $"{Amount:0.##}";
                return "—";
            }
        }

        [Ignore]
        public string FormattedTime => Time.ToString("hh:mm tt");

        [Ignore]
        public string FormattedDate => Time.ToString("dd/MM/yyyy");

        [Ignore]
        public string TipoAlimentacionDisplay => TipoAlimentacion switch
        {
            TipoAlimentacion.Lactancia     => "Lactancia materna",
            TipoAlimentacion.Formula       => "Fórmula / Biberón",
            TipoAlimentacion.Solido        => "Alimentación sólida",
            TipoAlimentacion.Personalizado => string.IsNullOrWhiteSpace(FoodType) ? "Personalizado" : FoodType,
            _                              => TipoAlimentacion.ToString()
        };
    }
}
