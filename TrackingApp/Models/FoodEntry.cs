using SQLite;

namespace TrackingApp.Models
{
    public class FoodEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string FoodType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public Unit Unit { get; set; } = Unit.Gram;
        public DateTime Time { get; set; }
        
        // Nuevos campos para duración
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        public string UserType { get; set; } = string.Empty; // "Bebé", "Adulto", "Animal"

        [Ignore]
        public string DisplayText
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue)
                {
                    var duration = EndTime.Value - StartTime.Value;
                    return $"{Amount} {Unit.GetDisplayName()} de {FoodType}\n⏱️ {duration.TotalMinutes:F0} min ({StartTime:hh:mm tt} - {EndTime:hh:mm tt})";
                }
                return $"{Amount} {Unit.GetDisplayName()} de {FoodType}\n🕐 {Time:hh:mm tt}";
            }
        }
        
        [Ignore]
        public string DisplayAmount => $"{Amount} {Unit.GetDisplayName()}";
        
        [Ignore]
        public string FormattedTime => Time.ToString("hh:mm tt");
        
        [Ignore]
        public string FormattedDate => Time.ToString("dd/MM/yyyy");
        
        [Ignore]
        public string DurationText
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue)
                {
                    var duration = EndTime.Value - StartTime.Value;
                    return $"{duration.TotalMinutes:F0} min";
                }
                return string.Empty;
            }
        }
        
        [Ignore]
        public string TimeRangeText
        {
            get
            {
                if (StartTime.HasValue && EndTime.HasValue)
                {
                    return $"{StartTime:hh:mm tt} - {EndTime:hh:mm tt}";
                }
                return FormattedTime;
            }
        }
    }
}
