using SQLite;

namespace TrackingApp.Models
{
    public class MedicalAppointment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Doctor { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;

        // Propiedad calculada para el display
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
    }
}
