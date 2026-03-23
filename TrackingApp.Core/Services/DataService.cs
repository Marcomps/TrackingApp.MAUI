using System.Collections.ObjectModel;
using TrackingApp.Models;

namespace TrackingApp.Services
{
    public class DataService
    {
        private readonly IDatabaseService _databaseService;
        private bool _suppressRebuild = false;

        public ObservableCollection<FoodEntry> FoodEntries { get; } = new();
        public ObservableCollection<Medication> Medications { get; } = new();
        public ObservableCollection<MedicationDose> MedicationDoses { get; } = new();
        public ObservableCollection<MedicationEvent> CombinedMedicationEvents { get; } = new();
        public ObservableCollection<MedicalAppointment> Appointments { get; } = new();
        public ObservableCollection<MedicationHistory> MedicationHistory { get; } = new();

        // RF-001 / RF-002
        public ObservableCollection<Perfil> Perfiles { get; } = new();
        public ObservableCollection<RegistroCrecimiento> RegistrosCrecimiento { get; } = new();

        /// <summary>
        /// Perfil activo seleccionado por el usuario. Null hasta que el usuario lo asigne.
        /// </summary>
        public Perfil? PerfilActivo { get; private set; }

        public string CurrentUserType { get; set; } = "Bebé";

        public DataService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            // Suscribir a cambios UNA SOLA VEZ en el constructor
            MedicationDoses.CollectionChanged += (s, e) => { if (!_suppressRebuild) RebuildCombinedEvents(); };
            MedicationHistory.CollectionChanged += (s, e) => { if (!_suppressRebuild) RebuildCombinedEvents(); };
            _ = LoadDataFromDatabaseAsync();
        }

        private async Task LoadDataFromDatabaseAsync()
        {
            _suppressRebuild = true;
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

                // RF-001 / RF-002: Cargar perfiles y registros de crecimiento
                await LoadPerfilesAsync();
                await LoadRegistrosCrecimientoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
            finally
            {
                _suppressRebuild = false;
                // Construir la lista combinada inicial
                RebuildCombinedEvents();
            }
        }

        /// <summary>
        /// Recarga todos los datos desde la base de datos. Llamar desde OnAppearing
        /// para asegurar que la UI refleje el estado actual de la DB.
        /// </summary>
        public async Task ReloadAllDataAsync()
        {
            _suppressRebuild = true;
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

                // Cargar dosis (solo no confirmadas)
                var doses = await _databaseService.GetAllDosesAsync();
                MedicationDoses.Clear();
                foreach (var dose in doses)
                {
                    if (!dose.IsConfirmed)
                    {
                        dose.Medication = Medications.FirstOrDefault(m => m.Id == dose.MedicationId);
                        MedicationDoses.Add(dose);
                    }
                }

                // Cargar historial de medicamentos
                await LoadMedicationHistoryAsync();

                // Cargar citas médicas
                await LoadAppointmentsAsync();

                // RF-001 / RF-002: Cargar perfiles y registros de crecimiento
                await LoadPerfilesAsync();
                await LoadRegistrosCrecimientoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reloading data: {ex.Message}");
            }
            finally
            {
                _suppressRebuild = false;
                // Reconstruir la lista combinada una sola vez al final
                RebuildCombinedEvents();
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

        public async Task GenerateDosesForMedicationAsync(Medication medication, int days)
        {
            _suppressRebuild = true;
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔵 GenerateDosesForMedicationAsync: Medication={medication.Name}, Days={days}, Frequency={medication.TotalFrequencyInMinutes}min");
                
                // Limpiar dosis anteriores de este medicamento
                await _databaseService.DeleteDosesByMedicationAsync(medication.Id);
                
                var oldDoses = MedicationDoses.Where(d => d.MedicationId == medication.Id).ToList();
                foreach (var dose in oldDoses)
                {
                    MedicationDoses.Remove(dose);
                }
                System.Diagnostics.Debug.WriteLine($"🔵 Cleared old doses. Generating doses for {days} days...");

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
            }
            finally
            {
                _suppressRebuild = false;
                RebuildCombinedEvents();
            }
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
            if (dose.IsConfirmed)
            {
                // Siempre guardar la hora actual cuando se confirma
                // (puede haber retrasos/adelantos en la toma)
                dose.ActualTime = DateTime.Now;
            }
            await _databaseService.SaveDoseAsync(dose);
        }

        public async Task ConfirmDoseAndRecalculateAsync(MedicationDose dose, int days)
        {
            _suppressRebuild = true;
            try
            {
                // 1. Registrar la hora real de confirmación
                var confirmedTime = DateTime.Now;
                dose.IsConfirmed = true;
                dose.ActualTime = confirmedTime;

                // 2. Crear registro en historial
                var history = new MedicationHistory
                {
                    MedicationId = dose.MedicationId,
                    MedicationName = dose.Medication?.Name ?? "Desconocido",
                    Dose = dose.Medication?.Dose ?? "",
                    AdministeredTime = confirmedTime,
                    UserType = CurrentUserType
                };

                await SaveMedicationHistoryAsync(history);
                MedicationHistory.Insert(0, history);

                // 3. Eliminar la dosis confirmada de la colección de dosis activas
                //    para que desaparezca inmediatamente de "Próximas Dosis"
                await _databaseService.DeleteDoseAsync(dose);
                MedicationDoses.Remove(dose);

                // 4. Recalcular siguientes dosis usando la hora de confirmación directamente
                //    (no buscar en MedicationDoses porque la dosis ya fue eliminada)
                await RecalculateNextDosesFromConfirmedTimeAsync(dose.MedicationId, confirmedTime, days);
            }
            finally
            {
                _suppressRebuild = false;
                RebuildCombinedEvents();
            }
        }

        /// <summary>
        /// Recalcula las dosis pendientes usando la hora de confirmación proporcionada directamente.
        /// Usar este método después de confirmar una dosis (cuando la dosis ya fue eliminada de MedicationDoses).
        /// </summary>
        public async Task RecalculateNextDosesFromConfirmedTimeAsync(int medicationId, DateTime confirmedTime, int days)
        {
            var medication = Medications.FirstOrDefault(m => m.Id == medicationId);
            if (medication == null) return;

            var wasSuppressed = _suppressRebuild;
            _suppressRebuild = true;
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 RecalculateFromConfirmedTime: Medication={medication.Name}, ConfirmedAt={confirmedTime:HH:mm}");

                // La siguiente dosis es: hora de confirmación + frecuencia
                DateTime nextDoseTime = confirmedTime.AddMinutes(medication.TotalFrequencyInMinutes);
                System.Diagnostics.Debug.WriteLine($"  ➡️ Siguiente dosis: {nextDoseTime:HH:mm}");

                // Eliminar todas las dosis PENDIENTES existentes de este medicamento
                var pendingDoses = MedicationDoses
                    .Where(d => d.MedicationId == medicationId && !d.IsConfirmed)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"  🗑️ Eliminando {pendingDoses.Count} dosis pendientes...");
                
                foreach (var dose in pendingDoses)
                {
                    await _databaseService.DeleteDoseAsync(dose);
                    MedicationDoses.Remove(dose);
                }

                // Regenerar dosis desde nextDoseTime hasta días de cobertura
                var endDate = DateTime.Now.AddDays(days);
                var currentDose = nextDoseTime;
                int count = 0;

                System.Diagnostics.Debug.WriteLine($"  ➕ Generando nuevas dosis hasta {endDate:yyyy-MM-dd HH:mm}...");

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
                    
                    currentDose = currentDose.AddMinutes(medication.TotalFrequencyInMinutes);
                }

                System.Diagnostics.Debug.WriteLine($"  ✅ Generadas {count} nuevas dosis");
            }
            finally
            {
                _suppressRebuild = wasSuppressed;
                if (!_suppressRebuild) RebuildCombinedEvents();
            }
        }

        /// <summary>
        /// CRÍTICO: Recalcula las dosis pendientes (no confirmadas) de un medicamento
        /// basándose en la última dosis del historial + frecuencia.
        /// Usado cuando se elimina un registro del historial.
        /// </summary>
        public async Task RecalculateNextDosesFromLastConfirmedAsync(int medicationId, int days)
        {
            var medication = Medications.FirstOrDefault(m => m.Id == medicationId);
            if (medication == null) return;

            _suppressRebuild = true;
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 RecalculateNextDoses: Medication={medication.Name}");

                // Buscar la última dosis en el HISTORIAL (MedicationHistory), no en MedicationDoses
                var lastHistory = MedicationHistory
                    .Where(h => h.MedicationId == medicationId)
                    .OrderByDescending(h => h.AdministeredTime)
                    .FirstOrDefault();

                DateTime nextDoseTime;
                
                if (lastHistory != null)
                {
                    nextDoseTime = lastHistory.AdministeredTime.AddMinutes(medication.TotalFrequencyInMinutes);
                    System.Diagnostics.Debug.WriteLine($"  ✅ Última en historial: {lastHistory.AdministeredTime:HH:mm}");
                    System.Diagnostics.Debug.WriteLine($"  ➡️ Siguiente dosis: {nextDoseTime:HH:mm}");
                }
                else
                {
                    // Si no hay historial, usar la primera dosis programada original
                    nextDoseTime = medication.FirstDoseTime;
                    if (nextDoseTime < DateTime.Now)
                    {
                        nextDoseTime = DateTime.Now.Date.Add(medication.FirstDoseTime.TimeOfDay);
                        if (nextDoseTime < DateTime.Now)
                        {
                            nextDoseTime = nextDoseTime.AddDays(1);
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"  ℹ️ No hay historial, usar FirstDoseTime: {nextDoseTime:HH:mm}");
                }

                // 2. Eliminar todas las dosis PENDIENTES (no confirmadas) de este medicamento
                var pendingDoses = MedicationDoses
                    .Where(d => d.MedicationId == medicationId && !d.IsConfirmed)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"  🗑️ Eliminando {pendingDoses.Count} dosis pendientes...");
                
                foreach (var dose in pendingDoses)
                {
                    await _databaseService.DeleteDoseAsync(dose);
                    MedicationDoses.Remove(dose);
                }

                // 3. Regenerar dosis desde nextDoseTime hasta días de cobertura
                var endDate = DateTime.Now.AddDays(days);
                var currentDose = nextDoseTime;
                int count = 0;

                System.Diagnostics.Debug.WriteLine($"  ➕ Generando nuevas dosis hasta {endDate:yyyy-MM-dd HH:mm}...");

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
                    
                    currentDose = currentDose.AddMinutes(medication.TotalFrequencyInMinutes);
                }

                System.Diagnostics.Debug.WriteLine($"  ✅ Generadas {count} nuevas dosis");
            }
            finally
            {
                _suppressRebuild = false;
                RebuildCombinedEvents();
            }
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

        // ========== RF-001: PERFILES ==========

        public async Task LoadPerfilesAsync()
        {
            try
            {
                var perfiles = await _databaseService.GetAllPerfilesAsync();
                Perfiles.Clear();
                foreach (var perfil in perfiles)
                    Perfiles.Add(perfil);

                // Seleccionar el primero como activo si no hay ninguno seleccionado
                if (PerfilActivo == null && Perfiles.Count > 0)
                    PerfilActivo = Perfiles[0];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading perfiles: {ex.Message}");
            }
        }

        public async Task AddPerfilAsync(Perfil perfil)
        {
            perfil.FechaCreacion = DateTime.Now;
            await _databaseService.SavePerfilAsync(perfil);
            Perfiles.Add(perfil);

            if (PerfilActivo == null)
                PerfilActivo = perfil;
        }

        public async Task UpdatePerfilAsync(Perfil perfil)
        {
            await _databaseService.SavePerfilAsync(perfil);
        }

        /// <summary>
        /// Elimina el perfil. Si era el activo, selecciona otro perfil automáticamente.
        /// Lanza InvalidOperationException si es el único perfil existente.
        /// </summary>
        public async Task DeletePerfilAsync(Perfil perfil)
        {
            if (Perfiles.Count <= 1)
                throw new InvalidOperationException("Debe existir al menos un perfil activo.");

            await _databaseService.DeletePerfilAsync(perfil);
            Perfiles.Remove(perfil);

            if (PerfilActivo?.Id == perfil.Id)
                PerfilActivo = Perfiles.FirstOrDefault();
        }

        public void SetPerfilActivo(Perfil perfil)
        {
            PerfilActivo = perfil;
        }

        // ========== RF-002: REGISTROS DE CRECIMIENTO ==========

        public async Task LoadRegistrosCrecimientoAsync()
        {
            try
            {
                var registros = PerfilActivo != null
                    ? await _databaseService.GetRegistrosCrecimientoByPerfilAsync(PerfilActivo.Id)
                    : await _databaseService.GetAllRegistrosCrecimientoAsync();

                RegistrosCrecimiento.Clear();
                foreach (var r in registros)
                {
                    r.Perfil = Perfiles.FirstOrDefault(p => p.Id == r.PerfilId);
                    RegistrosCrecimiento.Add(r);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading registros crecimiento: {ex.Message}");
            }
        }

        public async Task AddRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            if (PerfilActivo != null)
                registro.PerfilId = PerfilActivo.Id;

            registro.CalcularIMC();
            registro.FechaCreacion = DateTime.Now;
            await _databaseService.SaveRegistroCrecimientoAsync(registro);

            registro.Perfil = Perfiles.FirstOrDefault(p => p.Id == registro.PerfilId);
            RegistrosCrecimiento.Insert(0, registro);
        }

        public async Task UpdateRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            registro.CalcularIMC();
            await _databaseService.SaveRegistroCrecimientoAsync(registro);
        }

        public async Task DeleteRegistroCrecimientoAsync(RegistroCrecimiento registro)
        {
            await _databaseService.DeleteRegistroCrecimientoAsync(registro);
            RegistrosCrecimiento.Remove(registro);
        }
    }
}
