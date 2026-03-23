using SQLite;

namespace TrackingApp.Models
{
    /// <summary>
    /// RF-002 — Registro de métricas físicas (peso, talla, perímetro cefálico).
    /// Almacenamiento interno siempre en unidades métricas base: gramos y centímetros.
    /// La conversión para presentación se delega a IUnitService.
    /// </summary>
    public class RegistroCrecimiento
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>FK a Perfil.Id.</summary>
        public int PerfilId { get; set; }

        public DateTime Fecha { get; set; }

        /// <summary>Peso almacenado siempre en gramos.</summary>
        public decimal PesoGramos { get; set; }

        /// <summary>Talla almacenada siempre en centímetros.</summary>
        public decimal TallaCm { get; set; }

        /// <summary>Perímetro cefálico en cm. Solo aplica a perfil Bebé.</summary>
        public decimal? PerimCefCm { get; set; }

        /// <summary>
        /// IMC calculado: PesoKg / (TallaM²).
        /// Solo se muestra para perfiles con más de 2 años.
        /// Se recalcula en el servicio antes de persistir.
        /// </summary>
        public decimal IMC { get; set; }

        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // ── Propiedades calculadas (no persistidas) ──────────────────────────

        [Ignore]
        public Perfil? Perfil { get; set; }

        [Ignore]
        public decimal PesoKg => PesoGramos / 1000m;

        [Ignore]
        public decimal TallaM => TallaCm / 100m;

        [Ignore]
        public string FormattedDate => Fecha.ToString("dd/MM/yyyy");

        [Ignore]
        public string DisplayText =>
            $"{Fecha:dd/MM/yyyy} — {PesoKg:F2} kg / {TallaCm:F1} cm";

        /// <summary>
        /// Calcula el IMC a partir de los valores actuales.
        /// Debe llamarse antes de persistir el registro.
        /// </summary>
        public void CalcularIMC()
        {
            if (TallaCm <= 0) return;
            decimal tallaM = TallaCm / 100m;
            decimal pesoKg = PesoGramos / 1000m;
            IMC = pesoKg / (tallaM * tallaM);
        }
    }
}
