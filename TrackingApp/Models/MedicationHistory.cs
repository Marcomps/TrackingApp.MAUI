using SQLite;

namespace TrackingApp.Models
{
    /// <summary>
    /// Registro histórico de cada dosis de medicamento administrada
    /// </summary>
    public class MedicationHistory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int MedicationId { get; set; }
        
        [Ignore]
        public Medication? Medication { get; set; }
        
        public string MedicationName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public DateTime AdministeredTime { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty; // Notas opcionales

        [Ignore]
        public string DisplayText => $"{MedicationName} - {Dose} - {AdministeredTime:dd/MM/yyyy hh:mm tt}";
        
        [Ignore]
        public string FormattedTime => AdministeredTime.ToString("hh:mm tt"); // Formato 12 horas con AM/PM
        
        [Ignore]
        public string FormattedDate => AdministeredTime.ToString("dd/MM/yyyy");
    }
}
