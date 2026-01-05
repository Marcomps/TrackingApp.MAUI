using SQLite;

namespace TrackingApp.Models
{
    public class FoodEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        public string FoodType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string UserType { get; set; } = string.Empty; // "Bebé", "Adulto", "Animal"

        [Ignore]
        public string DisplayText => $"{Amount} {Unit} de {FoodType} a las {Time:hh:mm tt}";
        
        [Ignore]
        public string DisplayAmount => $"{Amount} {Unit}";
        
        [Ignore]
        public string FormattedTime => Time.ToString("hh:mm tt");
        
        [Ignore]
        public string FormattedDate => Time.ToString("dd/MM/yyyy");
    }
}
