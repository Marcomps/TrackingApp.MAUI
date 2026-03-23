using TrackingApp.Models;

namespace TrackingApp.Services
{
    public interface IDatabaseService
    {
        // Food Entries
        Task<List<FoodEntry>> GetAllFoodEntriesAsync();
        Task<int> SaveFoodEntryAsync(FoodEntry entry);
        Task<int> DeleteFoodEntryAsync(FoodEntry entry);

        // Medications
        Task<List<Medication>> GetAllMedicationsAsync();
        Task<Medication?> GetMedicationAsync(int id);
        Task<int> SaveMedicationAsync(Medication medication);
        Task<int> DeleteMedicationAsync(Medication medication);

        // Medication Doses
        Task<List<MedicationDose>> GetAllDosesAsync();
        Task<List<MedicationDose>> GetDosesByMedicationAsync(int medicationId);
        Task<int> SaveDoseAsync(MedicationDose dose);
        Task<int> DeleteDoseAsync(MedicationDose dose);
        Task<int> DeleteDosesByMedicationAsync(int medicationId);

        // Medication History
        Task<List<MedicationHistory>> GetAllMedicationHistoryAsync();
        Task<List<MedicationHistory>> GetMedicationHistoryByIdAsync(int medicationId);
        Task<int> SaveMedicationHistoryAsync(MedicationHistory history);
        Task<int> DeleteMedicationHistoryAsync(MedicationHistory history);

        // Medical Appointments
        Task<List<MedicalAppointment>> GetAllAppointmentsAsync();
        Task<int> SaveAppointmentAsync(MedicalAppointment appointment);
        Task<int> DeleteAppointmentAsync(MedicalAppointment appointment);

        // Utilities
        Task<int> ClearAllDataAsync();
        Task<int> DeleteAllDataAsync();
        Task<string> GetDatabasePathAsync();
        Task<long> GetDatabaseSizeAsync();
    }
}
