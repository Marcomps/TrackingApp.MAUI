using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.Tests.Mocks
{
    public class MockDatabaseService : IDatabaseService
    {
        public List<FoodEntry> FoodEntries { get; } = new();
        public List<Medication> Medications { get; } = new();
        public List<MedicationDose> Doses { get; } = new();
        public List<MedicationHistory> History { get; } = new();
        public List<MedicalAppointment> Appointments { get; } = new();
        public List<Perfil> Perfiles { get; } = new();
        public List<RegistroCrecimiento> RegistrosCrecimiento { get; } = new();

        public Task<List<FoodEntry>> GetAllFoodEntriesAsync() => Task.FromResult(FoodEntries);
        public Task<int> SaveFoodEntryAsync(FoodEntry entry)
        {
            if (entry.Id == 0) entry.Id = FoodEntries.Count + 1;
            if (!FoodEntries.Contains(entry)) FoodEntries.Add(entry);
            return Task.FromResult(1);
        }
        public Task<int> DeleteFoodEntryAsync(FoodEntry entry)
        {
            FoodEntries.Remove(entry);
            return Task.FromResult(1);
        }

        public Task<List<Medication>> GetAllMedicationsAsync() => Task.FromResult(Medications);
        public Task<Medication?> GetMedicationAsync(int id) => Task.FromResult(Medications.FirstOrDefault(m => m.Id == id));
        public Task<int> SaveMedicationAsync(Medication medication)
        {
            if (medication.Id == 0) medication.Id = Medications.Count + 1;
            if (!Medications.Contains(medication)) Medications.Add(medication);
            return Task.FromResult(1);
        }
        public Task<int> DeleteMedicationAsync(Medication medication)
        {
            Medications.Remove(medication);
            return Task.FromResult(1);
        }

        public Task<List<MedicationDose>> GetAllDosesAsync() => Task.FromResult(Doses);
        public Task<List<MedicationDose>> GetDosesByMedicationAsync(int medicationId) => Task.FromResult(Doses.Where(d => d.MedicationId == medicationId).ToList());
        public Task<int> SaveDoseAsync(MedicationDose dose)
        {
            if (dose.Id == 0) dose.Id = Doses.Count + 1;
            if (!Doses.Contains(dose)) Doses.Add(dose);
            return Task.FromResult(1);
        }
        public Task<int> SaveDosesAsync(IEnumerable<MedicationDose> doses)
        {
            foreach (var dose in doses)
            {
                if (dose.Id == 0) dose.Id = Doses.Count + 1;
                if (!Doses.Contains(dose)) Doses.Add(dose);
            }
            return Task.FromResult(1);
        }
        public Task<int> DeleteDoseAsync(MedicationDose dose)
        {
            Doses.Remove(dose);
            return Task.FromResult(1);
        }
        public Task<int> DeleteDosesByMedicationAsync(int medicationId)
        {
            Doses.RemoveAll(d => d.MedicationId == medicationId);
            return Task.FromResult(1);
        }

        public Task<List<MedicationHistory>> GetAllMedicationHistoryAsync() => Task.FromResult(History);
        public Task<List<MedicationHistory>> GetMedicationHistoryByIdAsync(int medicationId) => Task.FromResult(History.Where(h => h.MedicationId == medicationId).ToList());
        public Task<int> SaveMedicationHistoryAsync(MedicationHistory history)
        {
            if (history.Id == 0) history.Id = History.Count + 1;
            if (!History.Contains(history)) History.Add(history);
            return Task.FromResult(1);
        }
        public Task<int> DeleteMedicationHistoryAsync(MedicationHistory history)
        {
            History.Remove(history);
            return Task.FromResult(1);
        }

        public Task<List<MedicalAppointment>> GetAllAppointmentsAsync() => Task.FromResult(Appointments);
        public Task<int> SaveAppointmentAsync(MedicalAppointment appointment)
        {
            if (appointment.Id == 0) appointment.Id = Appointments.Count + 1;
            if (!Appointments.Contains(appointment)) Appointments.Add(appointment);
            return Task.FromResult(1);
        }
        public Task<int> DeleteAppointmentAsync(MedicalAppointment appointment)
        {
            Appointments.Remove(appointment);
            return Task.FromResult(1);
        }

        public Task<int> ClearAllDataAsync()
        {
            FoodEntries.Clear();
            Medications.Clear();
            Doses.Clear();
            History.Clear();
            return Task.FromResult(0);
        }
        public Task<int> DeleteAllDataAsync() => ClearAllDataAsync();
        public Task<string> GetDatabasePathAsync() => Task.FromResult("mock.db");
        public Task<long> GetDatabaseSizeAsync() => Task.FromResult(0L);

        // ========== RF-001: PERFILES ==========

        public Task<List<Perfil>> GetAllPerfilesAsync() => Task.FromResult(Perfiles);

        public Task<Perfil?> GetPerfilAsync(int id) =>
            Task.FromResult(Perfiles.FirstOrDefault(p => p.Id == id));

        public Task<int> SavePerfilAsync(Perfil perfil)
        {
            if (perfil.Id == 0)
            {
                perfil.Id = Perfiles.Count + 1;
                Perfiles.Add(perfil);
            }
            return Task.FromResult(1);
        }

        public Task<int> DeletePerfilAsync(Perfil perfil)
        {
            Perfiles.Remove(perfil);
            return Task.FromResult(1);
        }

        // ========== RF-002: REGISTROS DE CRECIMIENTO ==========

        public Task<List<RegistroCrecimiento>> GetAllRegistrosCrecimientoAsync() =>
            Task.FromResult(RegistrosCrecimiento);

        public Task<List<RegistroCrecimiento>> GetRegistrosCrecimientoByPerfilAsync(int perfilId) =>
            Task.FromResult(RegistrosCrecimiento.Where(r => r.PerfilId == perfilId).ToList());

        public Task<int> SaveRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            if (registro.Id == 0)
            {
                registro.Id = RegistrosCrecimiento.Count + 1;
                RegistrosCrecimiento.Add(registro);
            }
            return Task.FromResult(1);
        }

        public Task<int> DeleteRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            RegistrosCrecimiento.Remove(registro);
            return Task.FromResult(1);
        }
    }
}
