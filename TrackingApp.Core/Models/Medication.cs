using SQLite;

namespace TrackingApp.Models
{
    public class Medication
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public int FrequencyHours { get; set; }
        public int FrequencyMinutes { get; set; }
        public DateTime FirstDoseTime { get; set; }

        /// <summary>Mantenido por compatibilidad. Usar PerfilId para nuevos registros.</summary>
        public string UserType { get; set; } = string.Empty;

        /// <summary>FK a Perfil.Id. 0 = sin perfil asignado (registros legacy).</summary>
        public int PerfilId { get; set; }

        /// <summary>Si true, se programará una notificación local para cada dosis de este medicamento.</summary>
        public bool ReminderEnabled { get; set; }

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
