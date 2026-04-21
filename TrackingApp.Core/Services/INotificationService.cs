using TrackingApp.Models;

namespace TrackingApp.Services
{
    /// <summary>
    /// Abstraction for scheduling and cancelling local dose-reminder notifications.
    /// The implementation lives in TrackingApp (platform layer).
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Requests OS-level notification permission from the user.</summary>
        Task<bool> RequestPermissionAsync();

        /// <summary>
        /// Schedules a local notification for a future dose.
        /// Only called when medication.ReminderEnabled == true and dose is in the future.
        /// </summary>
        Task ScheduleDoseNotificationAsync(MedicationDose dose, Medication medication);

        /// <summary>Cancels the notification for a single dose (by dose.Id).</summary>
        void CancelDoseNotification(int doseId);

        /// <summary>Cancels notifications for all provided dose IDs (e.g. when deleting a medication).</summary>
        void CancelDoseNotifications(IEnumerable<int> doseIds);

        /// <summary>
        /// Schedules a local notification for a future appointment.
        /// Only called when appointment.RecordatorioMinutos.HasValue and appointment is in the future.
        /// </summary>
        Task ScheduleAppointmentNotificationAsync(MedicalAppointment appointment);

        /// <summary>Cancels the notification for a single appointment (by appointment.Id).</summary>
        void CancelAppointmentNotification(int appointmentId);
    }
}
