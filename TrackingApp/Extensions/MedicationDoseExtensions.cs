namespace TrackingApp.Models
{
    /// <summary>
    /// Extension methods for MedicationDose to add MAUI-specific functionality.
    /// </summary>
    public static class MedicationDoseExtensions
    {
        /// <summary>
        /// Gets the status color for a medication dose (MAUI-specific).
        /// </summary>
        public static Color GetStatusColor(this MedicationDose dose)
        {
            return dose.Status switch
            {
                "Confirmado" => Colors.LightGreen,
                "Atrasado" => Color.FromRgb(255, 230, 230),
                "Próximo" => Color.FromRgb(255, 249, 230),
                _ => Color.FromRgb(240, 243, 250)
            };
        }
    }
}
