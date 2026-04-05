using SQLite;
using TrackingApp.Models;

namespace TrackingApp.Services
{
    public class DatabaseService : IDatabaseService
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
            // RF-001 / RF-002
            await _database.CreateTableAsync<Perfil>();
            await _database.CreateTableAsync<RegistroCrecimiento>();

            // ── Índices para acelerar consultas de gráficas y filtros por perfil ─────

            // FoodEntry: consultas de gráficas por perfil+período y por tipo
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_foodentry_perfil_time ON FoodEntry(PerfilId, Time)");
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_foodentry_time ON FoodEntry(Time)");
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_foodentry_tipo ON FoodEntry(TipoAlimentacion)");

            // RegistroCrecimiento: gráficas de crecimiento por perfil+fecha
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_crecimiento_perfil_fecha ON RegistroCrecimiento(PerfilId, Fecha)");
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_crecimiento_fecha ON RegistroCrecimiento(Fecha)");

            // MedicationDose: calendario de dosis por medicamento y por tiempo
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_dose_medid_scheduled ON MedicationDose(MedicationId, ScheduledTime)");
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_dose_scheduled ON MedicationDose(ScheduledTime)");

            // MedicationHistory: historial por medicamento y tiempo de administración
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_medhistory_medid ON MedicationHistory(MedicationId)");
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_medhistory_time ON MedicationHistory(AdministeredTime)");

            // Medication: filtrado por perfil
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_medication_perfil ON Medication(PerfilId)");

            // MedicalAppointment: calendario por fecha y por perfil
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_appointment_perfil_date ON MedicalAppointment(PerfilId, AppointmentDate)");
            await _database.ExecuteAsync(
                "CREATE INDEX IF NOT EXISTS idx_appointment_date ON MedicalAppointment(AppointmentDate)");
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

        public async Task<int> SaveDosesAsync(IEnumerable<MedicationDose> doses)
        {
            await InitializeAsync();
            return await _database!.InsertAllAsync(doses);
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

        // ========== RF-001: PERFILES ==========

        public async Task<List<Perfil>> GetAllPerfilesAsync()
        {
            await InitializeAsync();
            return await _database!.Table<Perfil>()
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Perfil?> GetPerfilAsync(int id)
        {
            await InitializeAsync();
            return await _database!.Table<Perfil>()
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SavePerfilAsync(Perfil perfil)
        {
            await InitializeAsync();
            if (perfil.Id != 0)
                return await _database!.UpdateAsync(perfil);
            else
                return await _database!.InsertAsync(perfil);
        }

        public async Task<int> DeletePerfilAsync(Perfil perfil)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(perfil);
        }

        // ========== RF-002: REGISTROS DE CRECIMIENTO ==========

        public async Task<List<RegistroCrecimiento>> GetAllRegistrosCrecimientoAsync()
        {
            await InitializeAsync();
            return await _database!.Table<RegistroCrecimiento>()
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }

        public async Task<List<RegistroCrecimiento>> GetRegistrosCrecimientoByPerfilAsync(int perfilId)
        {
            await InitializeAsync();
            return await _database!.Table<RegistroCrecimiento>()
                .Where(r => r.PerfilId == perfilId)
                .OrderByDescending(r => r.Fecha)
                .ToListAsync();
        }

        public async Task<int> SaveRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            await InitializeAsync();
            if (registro.Id != 0)
                return await _database!.UpdateAsync(registro);
            else
                return await _database!.InsertAsync(registro);
        }

        public async Task<int> DeleteRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(registro);
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
