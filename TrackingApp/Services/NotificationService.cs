using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using TrackingApp.Models;

namespace TrackingApp.Services
{
    /// <summary>
    /// Cross-platform implementation of <see cref="INotificationService"/> using Plugin.LocalNotification.
    /// Schedules, updates, and cancels local reminder notifications for medication doses.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private static NotificationService? _instance;
        public static NotificationService Instance => _instance ??= new NotificationService();

        private const string ChannelId = "medication_reminders";

        private NotificationService() { }

        /// <inheritdoc/>
        public async Task<bool> RequestPermissionAsync()
        {
            try
            {
                return await LocalNotificationCenter.Current.RequestNotificationPermission();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notifications] Permission request failed: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task ScheduleDoseNotificationAsync(MedicationDose dose, Medication medication)
        {
            // Only schedule future doses (with a 5-second buffer)
            if (dose.ScheduledTime <= DateTime.Now.AddSeconds(5))
                return;

            try
            {
                var request = new NotificationRequest
                {
                    NotificationId = dose.Id,
                    Title = $"💊 {medication.Name}",
                    Description = $"Hora de tomar {medication.Dose}",
                    BadgeNumber = 1,
                    CategoryType = NotificationCategoryType.Reminder,
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = dose.ScheduledTime,
                        RepeatType = NotificationRepeat.No,
                    },
                    Android = new AndroidOptions
                    {
                        ChannelId = ChannelId,
                    },
                };

                await LocalNotificationCenter.Current.Show(request);
                System.Diagnostics.Debug.WriteLine($"[Notifications] Scheduled dose {dose.Id} for {dose.ScheduledTime:HH:mm} ({medication.Name})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notifications] Schedule failed for dose {dose.Id}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void CancelDoseNotification(int doseId)
        {
            try
            {
                LocalNotificationCenter.Current.Cancel(doseId);
                System.Diagnostics.Debug.WriteLine($"[Notifications] Cancelled dose notification {doseId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notifications] Cancel failed for {doseId}: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public void CancelDoseNotifications(IEnumerable<int> doseIds)
        {
            try
            {
                foreach (var id in doseIds)
                    LocalNotificationCenter.Current.Cancel(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notifications] Bulk cancel failed: {ex.Message}");
            }
        }
    }
}
