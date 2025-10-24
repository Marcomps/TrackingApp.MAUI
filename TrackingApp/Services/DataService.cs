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
                for (int i = 0; i < foods.Count; i++)
                {
                    FoodEntries.Add(foods[i]);
                }

                // Cargar medicamentos
                var medications = await _databaseService.GetAllMedicationsAsync();
                Medications.Clear();
                for (int i = 0; i < medications.Count; i++)
                {
                    Medications.Add(medications[i]);
                }

                // Cargar dosis
                var doses = await _databaseService.GetAllDosesAsync();
                MedicationDoses.Clear();
                for (int i = 0; i < doses.Count; i++)
                {
                    // Obtener el medicamento asociado
                    doses[i].Medication = Medications.FirstOrDefault(m => m.Id == doses[i].MedicationId);
                    MedicationDoses.Add(doses[i]);
                }

                // Cargar historial de medicamentos
                await LoadMedicationHistoryAsync();

                // Cargar citas médicas
                await LoadAppointmentsAsync();

                // ✅ CRÍTICO: Sincronizar dosis con historial al cargar la app
                // Esto asegura que las dosis pendientes estén calculadas desde la última dosis confirmada
                System.Diagnostics.Debug.WriteLine($"🔄 Sincronizando dosis con historial...");
                await SyncDosesWithHistoryAsync();

                // Suscribirse a cambios para mantener la colección unificada
                MedicationDoses.CollectionChanged += (s, e) => RebuildCombinedEvents();
                MedicationHistory.CollectionChanged += (s, e) => RebuildCombinedEvents();

                // Construir la lista combinada inicial
                RebuildCombinedEvents();
                
                System.Diagnostics.Debug.WriteLine($"✅ Data loaded: {Medications.Count} meds, {MedicationDoses.Count} doses, {MedicationHistory.Count} history");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        /// <summary>
        /// Sincroniza las dosis pendientes con el historial confirmado al cargar la aplicación.
        /// Verifica si hay historial confirmado y recalcula las dosis pendientes desde ahí.
        /// </summary>
        private async Task SyncDosesWithHistoryAsync()
        {
            bool needsRefresh = false;
            
            for (int i = 0; i < Medications.Count; i++)
            {
                var medication = Medications[i];
                var hasHistory = MedicationHistory.Any(h => h.MedicationId == medication.Id);
                
                if (hasHistory)
                {
                    System.Diagnostics.Debug.WriteLine($"  🔄 {medication.Name}: Sincronizando con historial...");
                    await RecalculateNextDosesFromLastConfirmedAsync(medication.Id, 3); // Usar 3 días por defecto
                    needsRefresh = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  ✓ {medication.Name}: Sin historial, dosis OK");
                }
            }
            
            // ✅ CRÍTICO: Recargar dosis desde la base de datos después de sincronizar
            // Esto asegura que la UI muestre las dosis recalculadas correctamente
            if (needsRefresh)
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Refrescando dosis en UI...");
                var doses = await _databaseService.GetAllDosesAsync();
                MedicationDoses.Clear();
                for (int i = 0; i < doses.Count; i++)
                {
                    doses[i].Medication = Medications.FirstOrDefault(m => m.Id == doses[i].MedicationId);
                    MedicationDoses.Add(doses[i]);
                }
                System.Diagnostics.Debug.WriteLine($"✅ UI actualizada con {MedicationDoses.Count} dosis");
            }
        }

        public async Task AddFoodEntryAsync(FoodEntry entry)
        {
            entry.UserType = CurrentUserType;
            await _databaseService.SaveFoodEntryAsync(entry);
            FoodEntries.Insert(0, entry);
        }

        public async Task AddMedicationAsync(Medication medication, int days = 3, bool firstDoseAlreadyTaken = false)
        {
            medication.UserType = CurrentUserType;
            await _databaseService.SaveMedicationAsync(medication);
            Medications.Add(medication);
            await GenerateDosesForMedicationAsync(medication, days, firstDoseAlreadyTaken);
        }

        public async Task GenerateDosesForMedicationAsync(Medication medication, int days, bool firstDoseAlreadyTaken = false)
        {
            System.Diagnostics.Debug.WriteLine($"🔵 GenerateDosesForMedicationAsync: Medication={medication.Name}, Days={days}, Frequency={medication.TotalFrequencyInMinutes}min, FirstDoseAlreadyTaken={firstDoseAlreadyTaken}");
            
            // ✅ CRÍTICO: Solo eliminar dosis NO confirmadas (pendientes) de este medicamento
            var pendingDoses = MedicationDoses.Where(d => d.MedicationId == medication.Id && !d.IsConfirmed).ToList();
            
            System.Diagnostics.Debug.WriteLine($"🗑️ Eliminando {pendingDoses.Count} dosis pendientes (NO confirmadas)...");
            
            for (int i = 0; i < pendingDoses.Count; i++)
            {
                await _databaseService.DeleteDoseAsync(pendingDoses[i]);
                MedicationDoses.Remove(pendingDoses[i]);
            }

            System.Diagnostics.Debug.WriteLine($"🔵 Cleared pending doses. Generating doses for {days} days...");

            var now = DateTime.Now;
            var firstDose = medication.FirstDoseTime;

            // 🆕 Si el usuario confirmó que ya tomó la primera dosis, agregarla al historial
            if (firstDoseAlreadyTaken)
            {
                var history = new MedicationHistory
                {
                    MedicationId = medication.Id,
                    MedicationName = medication.Name,
                    Dose = medication.Dose,
                    AdministeredTime = firstDose,
                    UserType = CurrentUserType,
                    TreatmentStartDate = medication.TreatmentStartDate,
                    TreatmentEndDate = medication.TreatmentEndDate
                };
                await SaveMedicationHistoryAsync(history);
                MedicationHistory.Insert(0, history);
                System.Diagnostics.Debug.WriteLine($"✅ Primera dosis agregada al historial: {firstDose:HH:mm}");
                
                // La siguiente dosis empieza desde firstDose + frecuencia
                firstDose = firstDose.AddMinutes(medication.TotalFrequencyInMinutes);
            }
            else
            {
                // Si la primera dosis ya pasó, usar la hora de hoy o mañana
                if (firstDose < now)
                {
                    firstDose = DateTime.Today.Add(medication.FirstDoseTime.TimeOfDay);
                    if (firstDose < now)
                    {
                        // Si ya pasó la hora hoy, empezar mañana
                        firstDose = firstDose.AddDays(1);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"🔵 First dose: {firstDose:yyyy-MM-dd HH:mm}");

            // Generar dosis según frecuencia durante N días
            var endDate = firstDose.AddDays(days);
            var currentDose = firstDose;
            int doseCount = 0;

            while (currentDose < endDate)
            {
                var newDose = new MedicationDose
                {
                    MedicationId = medication.Id,
                    Medication = medication,
                    ScheduledTime = currentDose,
                    IsConfirmed = false,
                    IsEdited = false
                };

                await _databaseService.SaveDoseAsync(newDose);
                MedicationDoses.Add(newDose);
                doseCount++;
                System.Diagnostics.Debug.WriteLine($"  ✅ Dose {doseCount}: {currentDose:yyyy-MM-dd HH:mm}");
                
                // Siguiente dosis según la frecuencia
                currentDose = currentDose.AddMinutes(medication.TotalFrequencyInMinutes);
            }
            
            System.Diagnostics.Debug.WriteLine($"🔵 Total doses created: {doseCount} for {days} days");
            RebuildCombinedEvents();
        }

        public async Task RegenerateDosesAsync(int days)
        {
            System.Diagnostics.Debug.WriteLine($"🔄 RegenerateDosesAsync: Regenerando para {Medications.Count} medicamentos con {days} días de cobertura");
            
            for (int i = 0; i < Medications.Count; i++)
            {
                var medication = Medications[i];
                
                // ✅ CRÍTICO: Verificar si hay historial confirmado
                var hasConfirmedHistory = MedicationHistory.Any(h => h.MedicationId == medication.Id);
                
                if (hasConfirmedHistory)
                {
                    // Si hay historial, usar RecalculateNextDosesFromLastConfirmedAsync
                    System.Diagnostics.Debug.WriteLine($"  📊 {medication.Name}: Tiene historial confirmado, recalculando desde última dosis...");
                    await RecalculateNextDosesFromLastConfirmedAsync(medication.Id, days);
                }
                else
                {
                    // Si NO hay historial, usar GenerateDosesForMedicationAsync
                    System.Diagnostics.Debug.WriteLine($"  🆕 {medication.Name}: Sin historial, generando dosis desde FirstDoseTime...");
                    await GenerateDosesForMedicationAsync(medication, days);
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"✅ RegenerateDosesAsync completado");
        }

        public async Task ConfirmDoseAsync(MedicationDose dose, bool useScheduledTime = false)
        {
            if (dose.IsConfirmed)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Dosis ya confirmada: {dose.Medication?.Name} - {dose.ScheduledTime:HH:mm}");
                return; // Ya está confirmada, evitar duplicación
            }
            
            dose.IsConfirmed = true;
            // 🆕 Usar hora programada o actual según elección del usuario
            dose.ActualTime = useScheduledTime ? dose.ScheduledTime : DateTime.Now;
            
            System.Diagnostics.Debug.WriteLine($"✅ Confirmando dosis: {dose.Medication?.Name} - Scheduled: {dose.ScheduledTime:HH:mm}, Actual: {dose.ActualTime:HH:mm}");
            
            await _databaseService.SaveDoseAsync(dose);
        }

        /// <summary>
        /// CRÍTICO: Recalcula las dosis pendientes (no confirmadas) de un medicamento
        /// basándose en la última dosis confirmada + frecuencia.
        /// Esto permite que si hay retraso, las siguientes dosis se ajusten automáticamente.
        /// </summary>
        public async Task RecalculateNextDosesFromLastConfirmedAsync(int medicationId, int days)
        {
            var medication = Medications.FirstOrDefault(m => m.Id == medicationId);
            if (medication == null) return;

            System.Diagnostics.Debug.WriteLine($"🔄 RecalculateNextDoses: Medication={medication.Name}, Frequency={medication.TotalFrequencyInMinutes}min");

            // 1. Buscar la última entrada en el historial de este medicamento
            var lastHistory = MedicationHistory
                .Where(h => h.MedicationId == medicationId)
                .OrderByDescending(h => h.AdministeredTime)
                .FirstOrDefault();

            DateTime nextDoseTime;
            
            if (lastHistory != null)
            {
                // Si hay historial, la siguiente empieza desde ahí + frecuencia
                nextDoseTime = lastHistory.AdministeredTime.AddMinutes(medication.TotalFrequencyInMinutes);
                System.Diagnostics.Debug.WriteLine($"  ✅ Último historial encontrado: {lastHistory.AdministeredTime:yyyy-MM-dd HH:mm:ss}");
                System.Diagnostics.Debug.WriteLine($"  ➡️ Cálculo: {lastHistory.AdministeredTime:HH:mm} + {medication.TotalFrequencyInMinutes}min = {nextDoseTime:HH:mm}");
            }
            else
            {
                // ✅ MEJORADO: Si no hay historial, calcular la próxima dosis basándose en cuántas dosis han transcurrido
                // Esto coincide con la lógica de app.js para consistencia
                var now = DateTime.Now;
                nextDoseTime = medication.FirstDoseTime;
                
                if (nextDoseTime < now)
                {
                    // Calcular cuánto tiempo ha pasado desde la primera dosis
                    var elapsedMinutes = (now - nextDoseTime).TotalMinutes;
                    
                    // Calcular cuántas dosis han transcurrido (redondear hacia arriba)
                    var dosesElapsed = Math.Ceiling(elapsedMinutes / medication.TotalFrequencyInMinutes);
                    
                    // Calcular la próxima dosis sumando las dosis transcurridas * frecuencia
                    nextDoseTime = medication.FirstDoseTime.AddMinutes(dosesElapsed * medication.TotalFrequencyInMinutes);
                    
                    System.Diagnostics.Debug.WriteLine($"  ℹ️ No hay historial. FirstDoseTime: {medication.FirstDoseTime:yyyy-MM-dd HH:mm}");
                    System.Diagnostics.Debug.WriteLine($"  ⏱️ Tiempo transcurrido: {elapsedMinutes:F0} min, Dosis transcurridas: {dosesElapsed}");
                    System.Diagnostics.Debug.WriteLine($"  ➡️ Próxima dosis calculada: {nextDoseTime:yyyy-MM-dd HH:mm}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  ℹ️ No hay historial, FirstDoseTime está en el futuro: {nextDoseTime:yyyy-MM-dd HH:mm:ss}");
                }
            }

            // 2. Eliminar todas las dosis PENDIENTES (no confirmadas) de este medicamento
            var pendingDoses = MedicationDoses
                .Where(d => d.MedicationId == medicationId && !d.IsConfirmed)
                .ToList();
            
            System.Diagnostics.Debug.WriteLine($"  🗑️ Eliminando {pendingDoses.Count} dosis pendientes...");
            
            for (int i = 0; i < pendingDoses.Count; i++)
            {
                await _databaseService.DeleteDoseAsync(pendingDoses[i]);
                MedicationDoses.Remove(pendingDoses[i]);
            }

            // 3. Regenerar dosis desde nextDoseTime hasta días de cobertura
            var endDate = DateTime.Now.AddDays(days);
            var currentDose = nextDoseTime;
            int count = 0;

            System.Diagnostics.Debug.WriteLine($"  ➕ Generando nuevas dosis desde {nextDoseTime:yyyy-MM-dd HH:mm} hasta {endDate:yyyy-MM-dd HH:mm}...");

            while (currentDose < endDate)
            {
                var newDose = new MedicationDose
                {
                    MedicationId = medication.Id,
                    Medication = medication,
                    ScheduledTime = currentDose,
                    IsConfirmed = false,
                    IsEdited = false
                };

                await _databaseService.SaveDoseAsync(newDose);
                MedicationDoses.Add(newDose);
                count++;
                
                System.Diagnostics.Debug.WriteLine($"    📅 Nueva dosis #{count}: {currentDose:yyyy-MM-dd HH:mm}");
                currentDose = currentDose.AddMinutes(medication.TotalFrequencyInMinutes);
            }

            System.Diagnostics.Debug.WriteLine($"  ✅ Generadas {count} nuevas dosis. Primera: {nextDoseTime:HH:mm}, Última: {currentDose.AddMinutes(-medication.TotalFrequencyInMinutes):HH:mm}");
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
            // Eliminar el historial de este medicamento
            await _databaseService.DeleteMedicationHistoryByMedicationAsync(medication.Id);
            
            var historyToRemove = MedicationHistory.Where(h => h.MedicationId == medication.Id).ToList();
            for (int i = 0; i < historyToRemove.Count; i++)
            {
                MedicationHistory.Remove(historyToRemove[i]);
            }
            
            // Eliminar las dosis asociadas
            await _databaseService.DeleteDosesByMedicationAsync(medication.Id);
            
            var dosesToRemove = MedicationDoses.Where(d => d.MedicationId == medication.Id).ToList();
            for (int i = 0; i < dosesToRemove.Count; i++)
            {
                MedicationDoses.Remove(dosesToRemove[i]);
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
            for (int i = 0; i < associatedDoses.Count; i++)
            {
                associatedDoses[i].Medication = medication;
                await _databaseService.SaveDoseAsync(associatedDoses[i]);
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

        public async Task UpdateMedicationHistoryAsync(MedicationHistory history)
        {
            await _databaseService.SaveMedicationHistoryAsync(history);
            // La UI se actualiza automáticamente gracias a ObservableCollection
        }

        public async Task AddMedicationHistoryAsync(Medication medication)
        {
            var history = new MedicationHistory
            {
                MedicationId = medication.Id,
                MedicationName = medication.Name,
                Dose = medication.Dose,
                AdministeredTime = DateTime.Now,
                UserType = CurrentUserType,
                TreatmentStartDate = medication.TreatmentStartDate,
                TreatmentEndDate = medication.TreatmentEndDate
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
                for (int i = 0; i < history.Count; i++)
                {
                    MedicationHistory.Add(history[i]);
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
                for (int i = 0; i < MedicationHistory.Count; i++)
                {
                    var h = MedicationHistory[i];
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
                var pendingDoses = MedicationDoses.Where(dose => !dose.IsConfirmed).ToList();
                for (int i = 0; i < pendingDoses.Count; i++)
                {
                    var d = pendingDoses[i];
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
                for (int i = 0; i < ordered.Count; i++)
                {
                    CombinedMedicationEvents.Add(ordered[i]);
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
                for (int i = 0; i < appointments.Count; i++)
                {
                    Appointments.Add(appointments[i]);
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
        
        public async Task ConfirmAppointmentAsync(MedicalAppointment appointment)
        {
            appointment.IsConfirmed = true;
            appointment.ConfirmedDate = DateTime.Now;
            await _databaseService.SaveAppointmentAsync(appointment);
        }

        // Historial
        public async Task<List<MedicationHistory>> GetAllMedicationHistoryAsync()
        {
            return await _databaseService.GetAllMedicationHistoryAsync();
        }
    }
}
