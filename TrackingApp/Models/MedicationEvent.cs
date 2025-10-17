using SQLite;

namespace TrackingApp.Models
{
    /// <summary>
    /// Evento unificado de medicamento que puede representar una dosis programada o un registro histórico
    /// </summary>
    public class MedicationEvent
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int MedicationId { get; set; }

        [Ignore]
        public Medication? Medication { get; set; }

        // Texto a mostrar (nombre + dosis)
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;

        // Momento del evento
        public DateTime EventTime { get; set; }

        // Indica si proviene del historial (confirmado/registro pasado) o del calendario (programado)
        public bool IsHistory { get; set; }

        // Si aplica, referencia al Id original (doseId o historyId)
        public int SourceId { get; set; }

        // Si el evento fue confirmado (para dosis programadas)
        public bool IsConfirmed { get; set; }

        [Ignore]
        public string DisplayTime => EventTime.ToString("hh:mm tt"); // Formato 12 horas con AM/PM

        [Ignore]
        public string DisplayDate => EventTime.ToString("dd/MM/yyyy");

        [Ignore]
        public string DisplayText => IsHistory
            ? $"{DisplayTime} — {MedicationName} ({Dose})"
            : $"{DisplayTime} — {MedicationName} ({Dose}) [Programado]";
    }
}