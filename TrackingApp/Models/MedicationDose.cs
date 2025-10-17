using SQLite;

namespace TrackingApp.Models
{
    public class MedicationDose
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
        public Color StatusColor
        {
            get
            {
                return Status switch
                {
                    "Confirmado" => Colors.LightGreen,
                    "Atrasado" => Color.FromRgb(255, 230, 230),
                    "Próximo" => Color.FromRgb(255, 249, 230),
                    _ => Color.FromRgb(240, 243, 250)
                };
            }
        }

        [Ignore]
        public string DisplayTime => (ActualTime ?? ScheduledTime).ToString("hh:mm tt");
        
        [Ignore]
        public string DisplayText => $"{DisplayTime} - {Medication?.Name} ({Medication?.Dose})";
    }
}
