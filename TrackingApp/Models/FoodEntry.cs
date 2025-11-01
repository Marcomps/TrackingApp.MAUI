using SQLite;

namespace TrackingApp.Models
{
    public class FoodEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string FoodType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public Unit Unit { get; set; } = Unit.Ounce; // Onza por defecto
        public DateTime Time { get; set; }
        public string UserType { get; set; } = string.Empty; // "Bebé", "Adulto", "Animal"
        
        // Para alimentación con duración (como lactancia)
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Ignore]
        public string DisplayText
        {
            get
            {
                var baseText = $"{DisplayAmount} de {FoodType}";
                if (StartTime.HasValue && EndTime.HasValue)
                {
                    return $"{baseText} ({DurationText})";
                }
                return $"{baseText} a las {FormattedTime}";
            }
        }
        
        [Ignore]
        public string DisplayAmount => $"{Amount} {Unit.GetDisplayText()}";
        
        [Ignore]
        public string FormattedTime => Time.ToString("hh:mm tt");
        
        [Ignore]
        public string FormattedDate => Time.ToString("dd/MM/yyyy");
        
        [Ignore]
        public string DurationText
        {
            get
            {
                if (!StartTime.HasValue || !EndTime.HasValue)
                    return string.Empty;
                
                var duration = EndTime.Value - StartTime.Value;
                var minutes = (int)duration.TotalMinutes;
                return $"{minutes} min";
            }
        }
        
        [Ignore]
        public string TimeRangeText
        {
            get
            {
                if (!StartTime.HasValue || !EndTime.HasValue)
                    return FormattedTime;
                
                return $"{StartTime.Value:hh:mm tt} - {EndTime.Value:hh:mm tt}";
            }
        }
    }
}
