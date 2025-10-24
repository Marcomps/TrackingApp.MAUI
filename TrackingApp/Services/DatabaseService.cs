using SQLite;
using TrackingApp.Models;

namespace TrackingApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
        private static DatabaseService? _instance;
        
        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private DatabaseService()
        {
        }

        private async Task InitializeAsync()
        {
            if (_database != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tracking.db3");
            _database = new SQLiteAsyncConnection(dbPath);

            // Crear tablas si no existen
            await _database.CreateTableAsync<FoodEntry>();
            await _database.CreateTableAsync<Medication>();
            await _database.CreateTableAsync<MedicationDose>();
            await _database.CreateTableAsync<MedicationHistory>();
            await _database.CreateTableAsync<MedicalAppointment>();
        }

        // ========== FOOD ENTRIES ==========
        
        public async Task<List<FoodEntry>> GetAllFoodEntriesAsync()
        {
            await InitializeAsync();
            return await _database!.Table<FoodEntry>()
                .OrderByDescending(f => f.Time)
                .ToListAsync();
        }

        public async Task<int> SaveFoodEntryAsync(FoodEntry entry)
        {
            await InitializeAsync();
            if (entry.Id != 0)
                return await _database!.UpdateAsync(entry);
            else
                return await _database!.InsertAsync(entry);
        }

        public async Task<int> DeleteFoodEntryAsync(FoodEntry entry)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(entry);
        }

        // ========== MEDICATIONS ==========
        
        public async Task<List<Medication>> GetAllMedicationsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<Medication>().ToListAsync();
        }

        public async Task<Medication?> GetMedicationAsync(int id)
        {
            await InitializeAsync();
            return await _database!.Table<Medication>()
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveMedicationAsync(Medication medication)
        {
            await InitializeAsync();
            if (medication.Id != 0)
                return await _database!.UpdateAsync(medication);
            else
                return await _database!.InsertAsync(medication);
        }

        public async Task<int> DeleteMedicationAsync(Medication medication)
        {
            await InitializeAsync();
            // Eliminar también las dosis asociadas
            await _database!.ExecuteAsync(
                "DELETE FROM MedicationDose WHERE MedicationId = ?", 
                medication.Id);
            return await _database!.DeleteAsync(medication);
        }

        // ========== MEDICATION DOSES ==========
        
        public async Task<List<MedicationDose>> GetAllDosesAsync()
        {
            await InitializeAsync();
            return await _database!.Table<MedicationDose>()
                .OrderBy(d => d.ScheduledTime)
                .ToListAsync();
        }

        public async Task<List<MedicationDose>> GetDosesByMedicationAsync(int medicationId)
        {
            await InitializeAsync();
            return await _database!.Table<MedicationDose>()
                .Where(d => d.MedicationId == medicationId)
                .OrderBy(d => d.ScheduledTime)
                .ToListAsync();
        }

        public async Task<int> SaveDoseAsync(MedicationDose dose)
        {
            await InitializeAsync();
            if (dose.Id != 0)
                return await _database!.UpdateAsync(dose);
            else
                return await _database!.InsertAsync(dose);
        }

        public async Task<int> DeleteDoseAsync(MedicationDose dose)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(dose);
        }

        public async Task<int> DeleteDosesByMedicationAsync(int medicationId)
        {
            await InitializeAsync();
            return await _database!.ExecuteAsync(
                "DELETE FROM MedicationDose WHERE MedicationId = ?", 
                medicationId);
        }

        // ========== UTILIDADES ==========
        
        public async Task<int> ClearAllDataAsync()
        {
            await InitializeAsync();
            await _database!.DeleteAllAsync<MedicationDose>();
            await _database!.DeleteAllAsync<Medication>();
            await _database!.DeleteAllAsync<FoodEntry>();
            await _database!.DeleteAllAsync<MedicationHistory>();
            return 0;
        }

        public async Task<int> DeleteAllDataAsync()
        {
            return await ClearAllDataAsync();
        }

        // ========== MEDICATION HISTORY ==========
        
        public async Task<List<MedicationHistory>> GetAllMedicationHistoryAsync()
        {
            await InitializeAsync();
            return await _database!.Table<MedicationHistory>()
                .OrderByDescending(h => h.AdministeredTime)
                .ToListAsync();
        }

        public async Task<List<MedicationHistory>> GetMedicationHistoryByIdAsync(int medicationId)
        {
            await InitializeAsync();
            return await _database!.Table<MedicationHistory>()
                .Where(h => h.MedicationId == medicationId)
                .OrderByDescending(h => h.AdministeredTime)
                .ToListAsync();
        }

        public async Task<int> SaveMedicationHistoryAsync(MedicationHistory history)
        {
            await InitializeAsync();
            if (history.Id != 0)
                return await _database!.UpdateAsync(history);
            else
                return await _database!.InsertAsync(history);
        }

        public async Task<int> DeleteMedicationHistoryAsync(MedicationHistory history)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(history);
        }

        public async Task<int> DeleteMedicationHistoryByMedicationAsync(int medicationId)
        {
            await InitializeAsync();
            return await _database!.ExecuteAsync(
                "DELETE FROM MedicationHistory WHERE MedicationId = ?", 
                medicationId);
        }

        // ========== MEDICAL APPOINTMENTS ==========
        
        public async Task<List<MedicalAppointment>> GetAllAppointmentsAsync()
        {
            await InitializeAsync();
            return await _database!.Table<MedicalAppointment>()
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<int> SaveAppointmentAsync(MedicalAppointment appointment)
        {
            await InitializeAsync();
            if (appointment.Id != 0)
                return await _database!.UpdateAsync(appointment);
            else
                return await _database!.InsertAsync(appointment);
        }

        public async Task<int> DeleteAppointmentAsync(MedicalAppointment appointment)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(appointment);
        }

        // ========== DATABASE INFO ==========

        public async Task<string> GetDatabasePathAsync()
        {
            await InitializeAsync();
            return Path.Combine(FileSystem.AppDataDirectory, "tracking.db3");
        }

        public async Task<long> GetDatabaseSizeAsync()
        {
            await InitializeAsync();
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "tracking.db3");
            if (File.Exists(dbPath))
            {
                var fileInfo = new FileInfo(dbPath);
                return fileInfo.Length;
            }
            return 0;
        }
    }
}
