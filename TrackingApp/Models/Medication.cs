using SQLite;

namespace TrackingApp.Models
{
    public class Medication
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public int FrequencyHours { get; set; } // Horas (ej: 0, 1, 2)
        public int FrequencyMinutes { get; set; } // Minutos (ej: 0, 30, 45)
        public DateTime FirstDoseTime { get; set; }
        public string UserType { get; set; } = string.Empty; // "Bebé", "Adulto", "Animal"

        [Ignore]
        public string DisplayText 
        { 
            get
            {
                if (FrequencyHours > 0 && FrequencyMinutes > 0)
                    return $"{Name} ({Dose}) cada {FrequencyHours}h {FrequencyMinutes}min";
                else if (FrequencyHours > 0)
                    return $"{Name} ({Dose}) cada {FrequencyHours}h";
                else
                    return $"{Name} ({Dose}) cada {FrequencyMinutes}min";
            }
        }

        [Ignore]
        public int TotalFrequencyInMinutes 
        { 
            get
            {
                return (FrequencyHours * 60) + FrequencyMinutes;
            }
        }
    }
}
