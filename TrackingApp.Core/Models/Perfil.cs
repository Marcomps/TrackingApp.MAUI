using SQLite;

namespace TrackingApp.Models
{
    /// <summary>
    /// RF-001 — Perfil de usuario. Todos los registros (alimentos, crecimiento, citas, medicamentos)
    /// quedan asociados a un perfil mediante PerfilId.
    /// El almacenamiento de unidades siempre es en unidades métricas base (g, cm, ml);
    /// SistemaUnidades solo afecta la presentación.
    /// </summary>
    public class Perfil
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>Nombre visible del perfil. Obligatorio, máx. 60 caracteres.</summary>
        public string Nombre { get; set; } = string.Empty;

        public TipoPerfil TipoPerfil { get; set; } = TipoPerfil.AdultoGeneral;

        /// <summary>Nullable para adultos sin fecha conocida.</summary>
        public DateTime? FechaNacimiento { get; set; }

        public Sexo Sexo { get; set; } = Sexo.NoEspecificado;

        /// <summary>Ruta local al archivo de imagen. Almacenado en FileSystem.AppDataDirectory.</summary>
        public string? FotoPath { get; set; }

        public SistemaUnidades SistemaUnidades { get; set; } = SistemaUnidades.Metrico;

        /// <summary>Solo relevante para TipoPerfil.Bebe.</summary>
        public int? SemanasGestacion { get; set; }

        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // ── Propiedades calculadas (no persistidas) ──────────────────────────

        [Ignore]
        public string DisplayName => string.IsNullOrWhiteSpace(Nombre) ? "Perfil sin nombre" : Nombre;

        [Ignore]
        public bool EsBebe => TipoPerfil == TipoPerfil.Bebe;

        /// <summary>Edad en meses calculada desde FechaNacimiento. Null si no hay fecha.</summary>
        [Ignore]
        public int? EdadEnMeses
        {
            get
            {
                if (FechaNacimiento == null) return null;
                var now = DateTime.Now;
                return ((now.Year - FechaNacimiento.Value.Year) * 12)
                       + now.Month - FechaNacimiento.Value.Month;
            }
        }

        /// <summary>Edad en años completos. Null si no hay fecha.</summary>
        [Ignore]
        public int? EdadEnAnios
        {
            get
            {
                if (FechaNacimiento == null) return null;
                var now = DateTime.Now;
                int years = now.Year - FechaNacimiento.Value.Year;
                if (now < FechaNacimiento.Value.AddYears(years)) years--;
                return years;
            }
        }

        [Ignore]
        public string TipoPerfilDisplay => TipoPerfil switch
        {
            TipoPerfil.Bebe => "Bebé / Recién nacido",
            TipoPerfil.AdultoGeneral => "Persona",
            TipoPerfil.Mascota => "Mascota",
            _ => TipoPerfil.ToString()
        };

        [Ignore]
        public string TipoPerfilEmoji => TipoPerfil switch
        {
            TipoPerfil.Bebe => "👶",
            TipoPerfil.Mascota => "🐾",
            _ => "👤"
        };

        [Ignore]
        public string SexoDisplay => Sexo switch
        {
            Sexo.Masculino => "Masculino",
            Sexo.Femenino => "Femenino",
            Sexo.NoEspecificado => "No especificado",
            _ => Sexo.ToString()
        };
    }
}
