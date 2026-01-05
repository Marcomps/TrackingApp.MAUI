using SQLite;

namespace TrackingApp.Models
{
    public partial class MedicationDose
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public int MedicationId { get; set; }
        
        [Ignore]
        public Medication? Medication { get; set; }
        
        public DateTime ScheduledTime { get; set; }
        public DateTime? ActualTime { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsEdited { get; set; }

        [Ignore]
        public string Status
        {
            get
            {
                if (IsConfirmed) return "Confirmado";
                
                var now = DateTime.Now;
                var diffMinutes = (ScheduledTime - now).TotalMinutes;
                
                if (diffMinutes < -30) return "Atrasado";
                if (diffMinutes < 30) return "Próximo";
                return "Programado";
            }
        }

        [Ignore]
        public string DisplayTime => (ActualTime ?? ScheduledTime).ToString("hh:mm tt");
        
        [Ignore]
        public string DisplayText => $"{DisplayTime} - {Medication?.Name} ({Medication?.Dose})";
    }
}
