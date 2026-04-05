using TrackingApp.Models;

namespace TrackingApp.Services
{
    public interface IDatabaseService
    {
        // ── Food Entries ──────────────────────────────────────────────────────
        Task<List<FoodEntry>> GetAllFoodEntriesAsync();
        Task<int> SaveFoodEntryAsync(FoodEntry entry);
        Task<int> DeleteFoodEntryAsync(FoodEntry entry);

        // ── Medications ───────────────────────────────────────────────────────
        Task<List<Medication>> GetAllMedicationsAsync();
        Task<Medication?> GetMedicationAsync(int id);
        Task<int> SaveMedicationAsync(Medication medication);
        Task<int> DeleteMedicationAsync(Medication medication);

        // ── Medication Doses ──────────────────────────────────────────────────
        Task<List<MedicationDose>> GetAllDosesAsync();
        Task<List<MedicationDose>> GetDosesByMedicationAsync(int medicationId);
        Task<int> SaveDoseAsync(MedicationDose dose);
        Task<int> SaveDosesAsync(IEnumerable<MedicationDose> doses);
        Task<int> DeleteDoseAsync(MedicationDose dose);
        Task<int> DeleteDosesByMedicationAsync(int medicationId);

        // ── Medication History ────────────────────────────────────────────────
        Task<List<MedicationHistory>> GetAllMedicationHistoryAsync();
        Task<List<MedicationHistory>> GetMedicationHistoryByIdAsync(int medicationId);
        Task<int> SaveMedicationHistoryAsync(MedicationHistory history);
        Task<int> DeleteMedicationHistoryAsync(MedicationHistory history);

        // ── Medical Appointments ──────────────────────────────────────────────
        Task<List<MedicalAppointment>> GetAllAppointmentsAsync();
        Task<int> SaveAppointmentAsync(MedicalAppointment appointment);
        Task<int> DeleteAppointmentAsync(MedicalAppointment appointment);

        // ── RF-001: Perfiles ──────────────────────────────────────────────────
        Task<List<Perfil>> GetAllPerfilesAsync();
        Task<Perfil?> GetPerfilAsync(int id);
        Task<int> SavePerfilAsync(Perfil perfil);
        Task<int> DeletePerfilAsync(Perfil perfil);

        // ── RF-002: Registros de Crecimiento ─────────────────────────────────
        Task<List<RegistroCrecimiento>> GetAllRegistrosCrecimientoAsync();
        Task<List<RegistroCrecimiento>> GetRegistrosCrecimientoByPerfilAsync(int perfilId);
        Task<int> SaveRegistroCrecimientoAsync(RegistroCrecimiento registro);
        Task<int> DeleteRegistroCrecimientoAsync(RegistroCrecimiento registro);

        // ── Utilities ────────────────────────────────────────────────────────
        Task<int> ClearAllDataAsync();
        Task<int> DeleteAllDataAsync();
        Task<string> GetDatabasePathAsync();
        Task<long> GetDatabaseSizeAsync();
    }
}
