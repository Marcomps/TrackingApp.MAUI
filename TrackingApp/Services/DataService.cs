using System.Collections.ObjectModel;
using TrackingApp.Models;

namespace TrackingApp.Services
{
    public class DataService
    {
        private static DataService? _instance;
        public static DataService Instance => _instance ??= new DataService();

        private readonly DatabaseService _databaseService;

        public ObservableCollection<FoodEntry> FoodEntries { get; } = new();
        public ObservableCollection<Medication> Medications { get; } = new();
        public ObservableCollection<MedicationDose> MedicationDoses { get; } = new();
        public ObservableCollection<MedicationEvent> CombinedMedicationEvents { get; } = new();
        public ObservableCollection<MedicalAppointment> Appointments { get; } = new();

        public string CurrentUserType { get; set; } = "Bebé";

        private DataService()
        {
            _databaseService = DatabaseService.Instance;
            _ = LoadDataFromDatabaseAsync();
        }

        private async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                // Cargar alimentos
                var foods = await _databaseService.GetAllFoodEntriesAsync();
                FoodEntries.Clear();
                foreach (var food in foods)
                {
                    FoodEntries.Add(food);
                }

                // Cargar medicamentos
                var medications = await _databaseService.GetAllMedicationsAsync();
                Medications.Clear();
                foreach (var med in medications)
                {
                    Medications.Add(med);
                }

                // Cargar dosis
                var doses = await _databaseService.GetAllDosesAsync();
                MedicationDoses.Clear();
                foreach (var dose in doses)
                {
                    // Obtener el medicamento asociado
                    dose.Medication = Medications.FirstOrDefault(m => m.Id == dose.MedicationId);
                    MedicationDoses.Add(dose);
                }

                // Cargar historial de medicamentos
                await LoadMedicationHistoryAsync();

                // Cargar citas médicas
                await LoadAppointmentsAsync();

                // Suscribirse a cambios para mantener la colección unificada
                MedicationDoses.CollectionChanged += (s, e) => RebuildCombinedEvents();
                MedicationHistory.CollectionChanged += (s, e) => RebuildCombinedEvents();

                // Construir la lista combinada inicial
                RebuildCombinedEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        public async Task AddFoodEntryAsync(FoodEntry entry)
        {
            entry.UserType = CurrentUserType;
            await _databaseService.SaveFoodEntryAsync(entry);
            FoodEntries.Insert(0, entry);
        }

        public async Task AddMedicationAsync(Medication medication, int days = 3)
        {
            medication.UserType = CurrentUserType;
            await _databaseService.SaveMedicationAsync(medication);
            Medications.Add(medication);
            await GenerateDosesForMedicationAsync(medication, days);
        }

        public async Task GenerateDosesForMedicationAsync(Medication medication, int numberOfDoses)
        {
            System.Diagnostics.Debug.WriteLine($"🔵 GenerateDosesForMedicationAsync: Medication={medication.Name}, NumberOfDoses={numberOfDoses}");
            
            // Limpiar dosis anteriores de este medicamento
            await _databaseService.DeleteDosesByMedicationAsync(medication.Id);
            
            var oldDoses = MedicationDoses.Where(d => d.MedicationId == medication.Id).ToList();
            foreach (var dose in oldDoses)
            {
                MedicationDoses.Remove(dose);
            }
            System.Diagnostics.Debug.WriteLine($"🔵 Cleared old doses. Creating {numberOfDoses} new doses...");

            var now = DateTime.Now;
            var firstDose = medication.FirstDoseTime;

            // Si la primera dosis ya pasó, usar la hora de hoy
            if (firstDose < now)
            {
                firstDose = DateTime.Today.Add(medication.FirstDoseTime.TimeOfDay);
                if (firstDose < now)
                {
                    // Si ya pasó la hora hoy, empezar mañana
                    firstDose = firstDose.AddDays(1);
                }
            }

            // Generar EXACTAMENTE numberOfDoses dosis (una por día)
            for (int i = 0; i < numberOfDoses; i++)
            {
                var doseTime = firstDose.AddDays(i);
                
                var newDose = new MedicationDose
                {
                    MedicationId = medication.Id,
                    Medication = medication,
                    ScheduledTime = doseTime,
                    IsConfirmed = false,
                    IsEdited = false
                };

                await _databaseService.SaveDoseAsync(newDose);
                MedicationDoses.Add(newDose);
                System.Diagnostics.Debug.WriteLine($"  ✅ Dose {i+1}: {doseTime:yyyy-MM-dd HH:mm}");
            }
            
            System.Diagnostics.Debug.WriteLine($"🔵 Total doses created: {numberOfDoses}");
        }

        public async Task RegenerateDosesAsync(int days)
        {
            foreach (var medication in Medications)
            {
                await GenerateDosesForMedicationAsync(medication, days);
            }
        }

        public async Task ConfirmDoseAsync(MedicationDose dose)
        {
            dose.IsConfirmed = !dose.IsConfirmed;
            if (dose.IsConfirmed && !dose.ActualTime.HasValue)
            {
                dose.ActualTime = DateTime.Now;
            }
            await _databaseService.SaveDoseAsync(dose);
        }

        public async Task EditDoseTimeAsync(MedicationDose dose, DateTime newTime)
        {
            dose.ActualTime = newTime;
            dose.ScheduledTime = newTime;
            dose.IsEdited = true;
            dose.IsConfirmed = false;
            await _databaseService.SaveDoseAsync(dose);
        }

        public IEnumerable<IGrouping<DateTime, MedicationDose>> GetDosesGroupedByDay(int? medicationId = null)
        {
            var doses = medicationId.HasValue
                ? MedicationDoses.Where(d => d.MedicationId == medicationId.Value)
                : MedicationDoses;

            return doses
                .OrderBy(d => d.ScheduledTime)
                .GroupBy(d => d.ScheduledTime.Date);
        }

        public async Task DeleteFoodEntryAsync(FoodEntry food)
        {
            await _databaseService.DeleteFoodEntryAsync(food);
            FoodEntries.Remove(food);
        }

        public async Task UpdateFoodEntryAsync(FoodEntry food)
        {
            await _databaseService.SaveFoodEntryAsync(food);
            // La UI se actualiza automáticamente gracias a ObservableCollection
        }

        public async Task DeleteMedicationAsync(Medication medication)
        {
            // Eliminar todas las dosis asociadas
            await _databaseService.DeleteDosesByMedicationAsync(medication.Id);
            
            var dosesToRemove = MedicationDoses.Where(d => d.MedicationId == medication.Id).ToList();
            foreach (var dose in dosesToRemove)
            {
                MedicationDoses.Remove(dose);
            }

            // Eliminar el medicamento
            await _databaseService.DeleteMedicationAsync(medication);
            Medications.Remove(medication);
        }

        public async Task UpdateMedicationAsync(Medication medication)
        {
            await _databaseService.SaveMedicationAsync(medication);
            // Actualizar las dosis asociadas para reflejar cambios en sus referencias de navegación
            var associatedDoses = MedicationDoses.Where(d => d.MedicationId == medication.Id).ToList();
            foreach (var dose in associatedDoses)
            {
                dose.Medication = medication;
                await _databaseService.SaveDoseAsync(dose);
            }
            RebuildCombinedEvents();
        }

        public async Task DeleteDoseAsync(MedicationDose dose)
        {
            await _databaseService.DeleteDoseAsync(dose);
            MedicationDoses.Remove(dose);
            RebuildCombinedEvents();
        }

        public async Task ResetAllDataAsync()
        {
            // Eliminar todo de la base de datos
            await _databaseService.DeleteAllDataAsync();

            // Limpiar colecciones
            FoodEntries.Clear();
            Medications.Clear();
            MedicationDoses.Clear();
            MedicationHistory.Clear();
            Appointments.Clear();
        }

        // ========== MEDICATION HISTORY ==========
        
        public ObservableCollection<MedicationHistory> MedicationHistory { get; } = new();

        public async Task SaveMedicationHistoryAsync(MedicationHistory history)
        {
            await _databaseService.SaveMedicationHistoryAsync(history);
        }

        public async Task AddMedicationHistoryAsync(Medication medication)
        {
            var history = new MedicationHistory
            {
                MedicationId = medication.Id,
                MedicationName = medication.Name,
                Dose = medication.Dose,
                AdministeredTime = DateTime.Now,
                UserType = CurrentUserType
            };

            await _databaseService.SaveMedicationHistoryAsync(history);
            MedicationHistory.Insert(0, history);
        }

        public async Task LoadMedicationHistoryAsync()
        {
            try
            {
                var history = await _databaseService.GetAllMedicationHistoryAsync();
                MedicationHistory.Clear();
                foreach (var item in history)
                {
                    MedicationHistory.Add(item);
                }
                RebuildCombinedEvents();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading medication history: {ex.Message}");
            }
        }

        public async Task DeleteMedicationHistoryAsync(MedicationHistory history)
        {
            await _databaseService.DeleteMedicationHistoryAsync(history);
            MedicationHistory.Remove(history);
            RebuildCombinedEvents();
        }

        public void RebuildCombinedEvents()
        {
            try
            {
                var list = new List<MedicationEvent>();

                // Agregar historial (eventos reales)
                foreach (var h in MedicationHistory)
                {
                    list.Add(new MedicationEvent
                    {
                        Id = h.Id,
                        MedicationId = h.MedicationId,
                        MedicationName = h.MedicationName,
                        Dose = h.Dose,
                        EventTime = h.AdministeredTime,
                        IsHistory = true,
                        SourceId = h.Id,
                        IsConfirmed = true
                    });
                }

                // Agregar dosis programadas (SOLO las NO confirmadas)
                // Las confirmadas ya están en MedicationHistory
                foreach (var d in MedicationDoses.Where(dose => !dose.IsConfirmed))
                {
                    list.Add(new MedicationEvent
                    {
                        Id = d.Id,
                        MedicationId = d.MedicationId,
                        Medication = d.Medication,
                        MedicationName = d.Medication?.Name ?? string.Empty,
                        Dose = d.Medication?.Dose ?? string.Empty,
                        EventTime = d.ScheduledTime,
                        IsHistory = false,
                        SourceId = d.Id,
                        IsConfirmed = false  // Siempre false porque estamos filtrando solo las no confirmadas
                    });
                }
                
                System.Diagnostics.Debug.WriteLine($"🔵 RebuildCombinedEvents: History={MedicationHistory.Count}, PendingDoses={MedicationDoses.Count(d => !d.IsConfirmed)}, Total={list.Count}");

                var ordered = list.OrderByDescending(x => x.EventTime).ToList();

                CombinedMedicationEvents.Clear();
                foreach (var ev in ordered)
                {
                    CombinedMedicationEvents.Add(ev);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rebuilding combined events: {ex.Message}");
            }
        }

        // ========== MEDICAL APPOINTMENTS ==========
        
        public async Task LoadAppointmentsAsync()
        {
            try
            {
                var appointments = await _databaseService.GetAllAppointmentsAsync();
                Appointments.Clear();
                foreach (var appointment in appointments)
                {
                    Appointments.Add(appointment);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading appointments: {ex.Message}");
            }
        }

        public async Task AddAppointmentAsync(MedicalAppointment appointment)
        {
            appointment.UserType = CurrentUserType;
            await _databaseService.SaveAppointmentAsync(appointment);
            Appointments.Add(appointment);
        }

        public async Task UpdateAppointmentAsync(MedicalAppointment appointment)
        {
            await _databaseService.SaveAppointmentAsync(appointment);
        }

        public async Task DeleteAppointmentAsync(MedicalAppointment appointment)
        {
            await _databaseService.DeleteAppointmentAsync(appointment);
            Appointments.Remove(appointment);
        }
    }
}
