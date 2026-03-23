using SQLite;

namespace TrackingApp.Models
{
    public class MedicalAppointment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ── Campos existentes ────────────────────────────────────────────────

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Doctor { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        /// <summary>Mantenido por compatibilidad. Usar PerfilId para nuevos registros.</summary>
        public string UserType { get; set; } = string.Empty;

        public bool IsConfirmed { get; set; }
        public DateTime? ConfirmedDate { get; set; }

        // ── RF-004: Campos nuevos v2.0 ───────────────────────────────────────

        /// <summary>FK a Perfil.Id. 0 = sin perfil asignado (registros legacy).</summary>
        public int PerfilId { get; set; }

        public CategoriaCita Categoria { get; set; } = CategoriaCita.Pediatria;

        /// <summary>Lugar y/o nombre del médico (campo unificado para simplicidad).</summary>
        public string? LugarYDoctor { get; set; }

        /// <summary>Observaciones registradas tras la cita.</summary>
        public string? NotasPostCita { get; set; }

        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        /// <summary>Anticipación del recordatorio en minutos (ej: 60 = 1 hora antes, 1440 = 1 día antes).</summary>
        public int? RecordatorioMinutos { get; set; }

        // ── Propiedades calculadas (no persistidas) ──────────────────────────

        [Ignore]
        public string DisplayText => $"{AppointmentDate:dd/MM/yyyy HH:mm} - {Title}{(string.IsNullOrWhiteSpace(Doctor) ? "" : $" (Dr. {Doctor})")}";

        [Ignore]
        public string FormattedDate => AppointmentDate.ToString("dd/MM/yyyy");

        [Ignore]
        public string FormattedTime => AppointmentDate.ToString("HH:mm");

        [Ignore]
        public string FormattedDateTime => AppointmentDate.ToString("dd/MM/yyyy HH:mm");

        [Ignore]
        public bool IsPast => AppointmentDate < DateTime.Now;

        [Ignore]
        public bool IsToday => AppointmentDate.Date == DateTime.Today;

        [Ignore]
        public bool IsFuture => AppointmentDate > DateTime.Now;

        [Ignore]
        public string CategoriaDisplay => Categoria switch
        {
            CategoriaCita.ControlCrecimientoPeso => "Control de crecimiento / peso",
            CategoriaCita.Vacunacion             => "Vacunación",
            CategoriaCita.Pediatria              => "Pediatría / Medicina general",
            CategoriaCita.Oftalmologia           => "Oftalmología",
            CategoriaCita.Odontologia            => "Odontología",
            CategoriaCita.Laboratorio            => "Laboratorio / Exámenes",
            CategoriaCita.Otra                   => "Otra",
            _                                    => Categoria.ToString()
        };

        [Ignore]
        public string EstadoDisplay => Estado switch
        {
            EstadoCita.Pendiente   => "Pendiente",
            EstadoCita.Completada  => "Completada",
            EstadoCita.Cancelada   => "Cancelada",
            _                      => Estado.ToString()
        };
    }
}
