using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;
using System.Globalization;

namespace TrackingApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private string _selectedUserType = "Bebé";
        private int _selectedDays = 2;
        private int? _selectedMedicationId;
        private Medication? _selectedMedication;
        private string _selectedHistoryRange = "Esta semana";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _dataService = DataService.Instance;

            // Commands
            AddFoodCommand = new Command(AddFood);
            AddMedicationCommand = new Command(AddMedication);
            DeleteFoodCommand = new Command<FoodEntry>(DeleteFood);
            EditFoodCommand = new Command<FoodEntry>(EditFood);
            DeleteMedicationCommand = new Command<Medication>(DeleteMedication);
            EditMedicationCommand = new Command<Medication>(EditMedication);
            ResetAllDataCommand = new Command(ResetAllData);
            ConfirmDoseCommand = new Command<MedicationDose>(ConfirmDose);
            EditDoseCommand = new Command<MedicationDose>(EditDose);
            DeleteDoseCommand = new Command<MedicationDose>(DeleteDose);
            RefreshCalendarCommand = new Command(RefreshCalendar);
            DeleteMedicationHistoryCommand = new Command<MedicationHistory>(DeleteMedicationHistory);
            ConfirmEventCommand = new Command<MedicationEvent>(ConfirmEvent);
            DeleteEventCommand = new Command<MedicationEvent>(DeleteEvent);
            AddAppointmentCommand = new Command(AddAppointment);
            EditAppointmentCommand = new Command<MedicalAppointment>(EditAppointment);
            ChangeAppointmentDateTimeCommand = new Command<MedicalAppointment>(ChangeAppointmentDateTime);
            DeleteAppointmentCommand = new Command<MedicalAppointment>(DeleteAppointment);
            ConfirmAppointmentCommand = new Command<MedicalAppointment>(ConfirmAppointment);

            // Subscribe to collection changes
            _dataService.Medications.CollectionChanged += (s, e) => 
            {
                UpdateSelectedMedication();
                OnPropertyChanged(nameof(FilteredMedications));
                System.Diagnostics.Debug.WriteLine($"🔔 Medications changed, count: {Medications.Count}");
            };
            _dataService.FoodEntries.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredFoodEntries));
            _dataService.MedicationHistory.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(FilteredMedicationHistory));
                OnPropertyChanged(nameof(FilteredCombinedEvents));
                OnPropertyChanged(nameof(PendingDoses));
                OnPropertyChanged(nameof(ConfirmedDoses));
            };
            _dataService.CombinedMedicationEvents.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(FilteredCombinedEvents));
                OnPropertyChanged(nameof(PendingDoses));
                OnPropertyChanged(nameof(ConfirmedDoses));
            };
            _dataService.Appointments.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredAppointments));
            
            // Set first medication as default
            UpdateSelectedMedication();
            
            // Forzar notificación inicial después de que se carguen los datos
            Task.Run(async () =>
            {
                await Task.Delay(500); // Esperar a que termine la carga inicial
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(PendingDoses));
                    OnPropertyChanged(nameof(ConfirmedDoses));
                    OnPropertyChanged(nameof(FilteredCombinedEvents));
                    OnPropertyChanged(nameof(FilteredMedications));
                    System.Diagnostics.Debug.WriteLine($"🔔 Notificación inicial - {Medications.Count} medicamentos");
                });
            });
        }

        // Properties
        public ObservableCollection<FoodEntry> FoodEntries => _dataService.FoodEntries;
        public ObservableCollection<Medication> Medications => _dataService.Medications;

        public string SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                _selectedUserType = value;
                _dataService.CurrentUserType = value;
                OnPropertyChanged();
            }
        }

        public List<string> UserTypes => new() { "Bebé", "Adulto", "Animal" };
        public List<string> Units => UnitExtensions.GetCommonUnits().Select(u => u.GetDisplayName()).ToList();
        public List<int> DaysOptions => new() { 1, 2, 3, 5, 7 };

        public int SelectedDays
        {
            get => _selectedDays;
            set
            {
                _selectedDays = value;
                OnPropertyChanged();
                // 🔄 Regenerar dosis automáticamente al cambiar días
                _ = UpdateDosesForSelectedDaysAsync();
            }
        }

        private async Task UpdateDosesForSelectedDaysAsync()
        {
            await _dataService.RegenerateDosesAsync(_selectedDays);
            _dataService.RebuildCombinedEvents();
            NotifyDosesChanged();
        }

        public Medication? SelectedMedication
        {
            get => _selectedMedication;
            set
            {
                _selectedMedication = value;
                _selectedMedicationId = value?.Id;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GroupedDoses));
                OnPropertyChanged(nameof(PendingDoses));
                OnPropertyChanged(nameof(SelectedMedicationFilterLabel));
                OnPropertyChanged(nameof(FilteredCombinedEvents));
                OnPropertyChanged(nameof(FilteredMedicationHistory));
            }
        }

        public int? SelectedMedicationId
        {
            get => _selectedMedicationId;
            set
            {
                _selectedMedicationId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GroupedDoses));
                OnPropertyChanged(nameof(FilteredCombinedEvents));
                OnPropertyChanged(nameof(FilteredMedicationHistory));
            }
        }

        public string SelectedMedicationFilterLabel
        {
            get
            {
                if (_selectedMedication == null)
                    return "Filtro: Todos los medicamentos";
                
                return $"Filtro: {_selectedMedication.Name}";
            }
        }

        // Filtro de historial por rango de fechas
        public List<string> HistoryRanges => new() { "Hoy", "Semana", "Mes", "Trimestre", "Semestre", "Año", "Personalizado" };

        public string SelectedHistoryRange
        {
            get => _selectedHistoryRange;
            set
            {
                _selectedHistoryRange = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredFoodEntries));
                OnPropertyChanged(nameof(FilteredMedications));
                
                if (value == "Personalizado")
                {
                    _ = ShowCustomDateRangePicker();
                }
            }
        }

        private DateTime _customStartDate = DateTime.Today;
        private DateTime _customEndDate = DateTime.Today;

        public ObservableCollection<FoodEntry> FilteredFoodEntries
        {
            get
            {
                var (startDate, endDate) = GetDateRange();
                var filtered = FoodEntries.Where(f => f.Time >= startDate && f.Time <= endDate).ToList();
                return new ObservableCollection<FoodEntry>(filtered);
            }
        }

        public ObservableCollection<Medication> FilteredMedications
        {
            get
            {
                // IMPORTANTE: NO filtrar medicamentos por fecha
                // Los medicamentos deben mostrarse SIEMPRE para poder editarlos/eliminarlos
                // independientemente de cuándo fue la primera dosis
                return new ObservableCollection<Medication>(Medications);
            }
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            return _selectedHistoryRange switch
            {
                "Hoy" => (today, today.AddDays(1).AddSeconds(-1)),
                "Semana" => (today.AddDays(-7), now),
                "Mes" => (today.AddMonths(-1), now),
                "Trimestre" => (today.AddMonths(-3), now),
                "Semestre" => (today.AddMonths(-6), now),
                "Año" => (today.AddYears(-1), now),
                "Personalizado" => (_customStartDate, _customEndDate.AddDays(1).AddSeconds(-1)),
                _ => (DateTime.MinValue, DateTime.MaxValue)
            };
        }

        public List<IGrouping<DateTime, MedicationDose>> GroupedDoses
        {
            get
            {
                return _dataService.GetDosesGroupedByDay(SelectedMedicationId).ToList();
            }
        }

        // Food Entry Properties
        private string _foodType = string.Empty;
        public string FoodType
        {
            get => _foodType;
            set
            {
                _foodType = value;
                OnPropertyChanged();
            }
        }

        private string _foodAmount = string.Empty;
        public string FoodAmount
        {
            get => _foodAmount;
            set
            {
                _foodAmount = value;
                OnPropertyChanged();
            }
        }

        private string _foodUnit = "g";
        public string FoodUnit
        {
            get => _foodUnit;
            set
            {
                _foodUnit = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _foodTime = DateTime.Now.TimeOfDay;
        public TimeSpan FoodTime
        {
            get => _foodTime;
            set
            {
                _foodTime = value;
                OnPropertyChanged();
            }
        }

        // Medication Properties
        private string _medicationName = string.Empty;
        public string MedicationName
        {
            get => _medicationName;
            set
            {
                _medicationName = value;
                OnPropertyChanged();
            }
        }

        private string _medicationDose = string.Empty;
        public string MedicationDose
        {
            get => _medicationDose;
            set
            {
                _medicationDose = value;
                OnPropertyChanged();
            }
        }

        private string _medicationFrequencyHours = string.Empty;
        public string MedicationFrequencyHours
        {
            get => _medicationFrequencyHours;
            set
            {
                _medicationFrequencyHours = value;
                OnPropertyChanged();
            }
        }

        private string _medicationFrequencyMinutes = string.Empty;
        public string MedicationFrequencyMinutes
        {
            get => _medicationFrequencyMinutes;
            set
            {
                _medicationFrequencyMinutes = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _medicationTime = DateTime.Now.TimeOfDay;
        public TimeSpan MedicationTime
        {
            get => _medicationTime;
            set
            {
                _medicationTime = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand AddFoodCommand { get; }
        public ICommand AddMedicationCommand { get; }
        public ICommand DeleteFoodCommand { get; }
        public ICommand EditFoodCommand { get; }
        public ICommand DeleteMedicationCommand { get; }
        public ICommand EditMedicationCommand { get; }
        public ICommand ResetAllDataCommand { get; }
        public ICommand ConfirmDoseCommand { get; }
        public ICommand EditDoseCommand { get; }
        public ICommand DeleteDoseCommand { get; }
        public ICommand RefreshCalendarCommand { get; }
        public ICommand DeleteMedicationHistoryCommand { get; }
        public ICommand ConfirmEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand ChangeAppointmentDateTimeCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand ConfirmAppointmentCommand { get; }

        private async void AddFood()
        {
            if (string.IsNullOrWhiteSpace(FoodType) || string.IsNullOrWhiteSpace(FoodAmount))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Por favor complete todos los campos", "OK")!;
                return;
            }

            if (!double.TryParse(FoodAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out double amount))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "La cantidad debe ser un número", "OK")!;
                return;
            }

            // Preguntar si quiere registrar con duración
            bool withDuration = await Application.Current?.MainPage?.DisplayAlert(
                "Duración",
                "¿Desea registrar con hora de inicio y fin?",
                "Sí", "No")!;

            var entry = new FoodEntry
            {
                FoodType = FoodType,
                Amount = amount,
                Unit = ConvertStringToUnit(FoodUnit),
                Time = DateTime.Today.Add(FoodTime)
            };

            if (withDuration)
            {
                // Pedir hora de inicio con TimePicker visual
                var startTimePicker = new TrackingApp.Views.TimePickerPopup(DateTime.Now.TimeOfDay);
                await Application.Current?.MainPage?.Navigation.PushModalAsync(startTimePicker)!;
                
                // Esperar a que se cierre el popup
                await Task.Run(async () =>
                {
                    while (Application.Current?.MainPage?.Navigation.ModalStack.Count > 0)
                    {
                        await Task.Delay(100);
                    }
                });

                if (!startTimePicker.SelectedTime.HasValue)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Cancelado", "Registro cancelado", "OK")!;
                    return;
                }

                // Pedir duración en minutos
                string? durationStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                    "Duración",
                    "Ingrese la duración en minutos (ej: 20):",
                    initialValue: "20",
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrWhiteSpace(durationStr) || !int.TryParse(durationStr, out int durationMinutes))
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Duración inválida", "OK")!;
                    return;
                }

                // Usar la hora seleccionada
                entry.StartTime = DateTime.Today.Add(startTimePicker.SelectedTime.Value);
                entry.EndTime = entry.StartTime.Value.AddMinutes(durationMinutes);
                entry.Time = entry.StartTime.Value; // Usar StartTime como Time principal
            }

            await _dataService.AddFoodEntryAsync(entry);

            // Clear fields
            FoodType = string.Empty;
            FoodAmount = string.Empty;
            FoodTime = DateTime.Now.TimeOfDay;

            OnPropertyChanged(nameof(FilteredFoodEntries));
            
            string successMsg = withDuration && entry.StartTime.HasValue && entry.EndTime.HasValue
                ? $"Alimento agregado\n{entry.DurationText} desde {entry.StartTime:hh:mm tt}"
                : "Alimento agregado";
            
            await Application.Current?.MainPage?.DisplayAlert("Éxito", successMsg, "OK")!;
        }

        private async void AddMedication()
        {
            if (string.IsNullOrWhiteSpace(MedicationName) || 
                string.IsNullOrWhiteSpace(MedicationDose))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Por favor complete nombre y dosis", "OK")!;
                return;
            }

            // Validar horas y minutos
            int hours = 0;
            int minutes = 0;

            if (!string.IsNullOrWhiteSpace(MedicationFrequencyHours))
            {
                if (!int.TryParse(MedicationFrequencyHours, out hours) || hours < 0)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Las horas deben ser un número válido", "OK")!;
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(MedicationFrequencyMinutes))
            {
                if (!int.TryParse(MedicationFrequencyMinutes, out minutes) || minutes < 0 || minutes >= 60)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Los minutos deben estar entre 0 y 59", "OK")!;
                    return;
                }
            }

            // Validar que al menos uno tenga valor
            if (hours == 0 && minutes == 0)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Debe ingresar al menos horas o minutos", "OK")!;
                return;
            }

            // Preguntar por fechas de tratamiento
            var startDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Fecha de Inicio",
                "Ingrese fecha de inicio del tratamiento (dd/MM/yyyy):",
                initialValue: DateTime.Today.ToString("dd/MM/yyyy"))!;

            DateTime treatmentStart = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(startDateStr) && 
                DateTime.TryParseExact(startDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedStart))
            {
                treatmentStart = parsedStart;
            }

            // Preguntar si tiene fecha de fin (tratamiento temporal)
            bool hasDuration = await Application.Current?.MainPage?.DisplayAlert(
                "Duración del Tratamiento",
                "¿Este tratamiento tiene fecha de finalización?",
                "Sí", "No (Continuo)")!;

            DateTime? treatmentEnd = null;
            if (hasDuration)
            {
                var endDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                    "Fecha de Fin",
                    "Ingrese fecha de fin del tratamiento (dd/MM/yyyy):",
                    initialValue: DateTime.Today.AddDays(30).ToString("dd/MM/yyyy"))!;

                if (!string.IsNullOrWhiteSpace(endDateStr) && 
                    DateTime.TryParseExact(endDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedEnd))
                {
                    treatmentEnd = parsedEnd;
                }
            }

            // 🆕 NUEVO ENFOQUE: La primera dosis SIEMPRE se registra en el historial
            // Esto simplifica la edición y hace que la primera dosis sea el punto de referencia
            var firstDoseTime = DateTime.Today.Add(MedicationTime);
            
            bool confirmFirstDose = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar Primera Dosis",
                $"¿Ya tomaste tu primera dosis de {MedicationName} a las {firstDoseTime:HH:mm}?\n\n" +
                $"Esta dosis se registrará en el historial y se usará como referencia para calcular las siguientes dosis.",
                "Sí, ya la tomé", "No, la tomaré después")!;

            if (!confirmFirstDose)
            {
                await Application.Current?.MainPage?.DisplayAlert(
                    "Información",
                    $"Cuando tomes tu primera dosis a las {firstDoseTime:HH:mm}, regístrala en esta app.\n\n" +
                    $"Por ahora, NO se creará el medicamento.",
                    "Entendido")!;
                return;
            }

            // Crear el medicamento
            var medication = new Medication
            {
                Name = MedicationName,
                Dose = MedicationDose,
                FrequencyHours = hours,
                FrequencyMinutes = minutes,
                FirstDoseTime = firstDoseTime,
                TreatmentStartDate = treatmentStart,
                TreatmentEndDate = treatmentEnd
            };

            // Agregar medicamento (sin generar primera dosis como pendiente)
            await _dataService.AddMedicationAsync(medication, SelectedDays, true); // ← true = primera dosis ya tomada
            
            // Notificar creación del historial
            OnPropertyChanged(nameof(FilteredMedicationHistory));

            // Clear fields
            MedicationName = string.Empty;
            MedicationDose = string.Empty;
            MedicationFrequencyHours = string.Empty;
            MedicationFrequencyMinutes = string.Empty;
            MedicationTime = DateTime.Now.TimeOfDay;

            OnPropertyChanged(nameof(GroupedDoses));
            OnPropertyChanged(nameof(FilteredMedications));
            UpdateSelectedMedication();
            
            string durationMsg = treatmentEnd.HasValue 
                ? $" (hasta {treatmentEnd.Value:dd/MM/yyyy})" 
                : " (tratamiento continuo)";
            await Application.Current?.MainPage?.DisplayAlert("Éxito", $"Medicamento agregado con dosis para {SelectedDays} días{durationMsg}", "OK")!;
        }

        private async void DeleteFood(FoodEntry food)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar '{food.FoodType}'?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteFoodEntryAsync(food);
                OnPropertyChanged(nameof(FilteredFoodEntries));
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Alimento eliminado", "OK")!;
            }
        }

        private async void EditFood(FoodEntry food)
        {
            // Prompt para editar tipo
            var newType = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Alimento",
                "Tipo de alimento:",
                initialValue: food.FoodType)!;

            if (string.IsNullOrWhiteSpace(newType)) return;

            // Prompt para editar cantidad
            var newAmountStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Cantidad",
                "Cantidad:",
                initialValue: food.Amount.ToString(),
                keyboard: Keyboard.Numeric)!;

            if (string.IsNullOrWhiteSpace(newAmountStr)) return;

            // Prompt para editar hora
            var newTimeStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Hora",
                "Hora (formato 12h, ej: 09:30 AM o 02:45 PM):",
                initialValue: food.Time.ToString("hh:mm tt"))!;

            if (string.IsNullOrWhiteSpace(newTimeStr)) return;

            if (double.TryParse(newAmountStr, out double newAmount))
            {
                // Intentar parsear la hora
                DateTime newTime;
                if (DateTime.TryParse(newTimeStr, out var parsedTime))
                {
                    newTime = food.Time.Date + parsedTime.TimeOfDay;
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("❌ Error", "Formato de hora inválido. Use formato 12h con AM/PM", "OK")!;
                    return;
                }

                food.FoodType = newType;
                food.Amount = newAmount;
                food.Time = newTime;
                await _dataService.UpdateFoodEntryAsync(food);
                OnPropertyChanged(nameof(FilteredFoodEntries));
                await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Alimento actualizado (incluye nueva hora)", "OK")!;
            }
        }

        private async void DeleteMedication(Medication medication)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar '{medication.Name}' y todas sus dosis?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteMedicationAsync(medication);
                OnPropertyChanged(nameof(GroupedDoses));
                OnPropertyChanged(nameof(FilteredMedications));
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Medicamento y dosis eliminados", "OK")!;
            }
        }

        private async void EditMedication(Medication medication)
        {
            var newName = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Nombre",
                "Nuevo nombre:",
                placeholder: medication.Name,
                initialValue: medication.Name)!;

            if (string.IsNullOrWhiteSpace(newName))
                return;

            var newDose = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Dosis",
                "Nueva dosis:",
                placeholder: medication.Dose,
                initialValue: medication.Dose)!;

            if (string.IsNullOrWhiteSpace(newDose))
                return;

            var newFreqHoursStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Frecuencia (Horas)",
                "Horas (0-24):",
                keyboard: Keyboard.Numeric,
                initialValue: medication.FrequencyHours.ToString())!;

            var newFreqMinutesStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Frecuencia (Minutos)",
                "Minutos (0-59):",
                keyboard: Keyboard.Numeric,
                initialValue: medication.FrequencyMinutes.ToString())!;

            int frequencyHours = 0;
            int frequencyMinutes = 0;

            if (!string.IsNullOrWhiteSpace(newFreqHoursStr) && !int.TryParse(newFreqHoursStr, out frequencyHours))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Las horas deben ser un número válido", "OK")!;
                return;
            }

            if (!string.IsNullOrWhiteSpace(newFreqMinutesStr))
            {
                if (!int.TryParse(newFreqMinutesStr, out frequencyMinutes) || frequencyMinutes < 0 || frequencyMinutes > 59)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "Los minutos deben estar entre 0 y 59", "OK")!;
                    return;
                }
            }

            if (frequencyHours == 0 && frequencyMinutes == 0)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Debe ingresar al menos horas o minutos", "OK")!;
                return;
            }

            // 🆕 Solo actualizar nombre, dosis y frecuencia
            // Las dosis se regeneran automáticamente desde el historial
            medication.Name = newName;
            medication.Dose = newDose;
            medication.FrequencyHours = frequencyHours;
            medication.FrequencyMinutes = frequencyMinutes;

            await _dataService.UpdateMedicationAsync(medication);
            
            // 🔄 Recalcular las dosis pendientes desde el último registro del historial
            System.Diagnostics.Debug.WriteLine($"🔄 Recalculando dosis del medicamento editado: {medication.Name}");
            await _dataService.RecalculateNextDosesFromLastConfirmedAsync(medication.Id, SelectedDays);
            
            _dataService.RebuildCombinedEvents();
            OnPropertyChanged(nameof(FilteredMedications));
            NotifyDosesChanged();
            OnPropertyChanged(nameof(GroupedDoses));
            await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Medicamento actualizado. Las dosis futuras se calculan desde tu última toma registrada en el historial.", "OK")!;
        }

        private async void ResetAllData()
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "⚠️ ADVERTENCIA",
                "Esto borrará TODOS los datos:\n• Todos los alimentos\n• Todos los medicamentos\n• Todos los horarios\n\n¿Estás seguro?",
                "Sí, borrar todo", "Cancelar")!;

            if (!confirm) return;

            // Segunda confirmación
            bool doubleConfirm = await Application.Current?.MainPage?.DisplayAlert(
                "⚠️ ÚLTIMA CONFIRMACIÓN",
                "Esta acción NO se puede deshacer.\n¿Continuar?",
                "Sí, estoy seguro", "No")!;

            if (doubleConfirm)
            {
                await _dataService.ResetAllDataAsync();
                OnPropertyChanged(nameof(GroupedDoses));
                OnPropertyChanged(nameof(FilteredFoodEntries));
                OnPropertyChanged(nameof(FilteredMedications));
                await Application.Current?.MainPage?.DisplayAlert("✅ Completado", "Todos los datos han sido eliminados", "OK")!;
            }
        }

        private void UpdateSelectedMedication()
        {
            // Auto-select first medication if available and none selected
            if (Medications.Any() && _selectedMedication == null)
            {
                SelectedMedication = Medications.First();
            }
            else if (!Medications.Any())
            {
                SelectedMedication = null;
            }
            
            OnPropertyChanged(nameof(SelectedMedicationFilterLabel));
        }

        private async Task ShowCustomDateRangePicker()
        {
            var startDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Fecha Inicial",
                "Ingresa la fecha de inicio (dd/MM/yyyy):",
                placeholder: DateTime.Today.AddDays(-30).ToString("dd/MM/yyyy"))!;

            if (string.IsNullOrWhiteSpace(startDateStr))
            {
                SelectedHistoryRange = "Hoy";
                return;
            }

            var endDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Fecha Final",
                "Ingresa la fecha de fin (dd/MM/yyyy):",
                placeholder: DateTime.Today.ToString("dd/MM/yyyy"))!;

            if (string.IsNullOrWhiteSpace(endDateStr))
            {
                SelectedHistoryRange = "Hoy";
                return;
            }

            if (DateTime.TryParseExact(startDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime start) &&
                DateTime.TryParseExact(endDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime end))
            {
                if (start > end)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "La fecha inicial no puede ser mayor que la fecha final", "OK")!;
                    SelectedHistoryRange = "Hoy";
                    return;
                }

                _customStartDate = start;
                _customEndDate = end;
                OnPropertyChanged(nameof(FilteredFoodEntries));
                OnPropertyChanged(nameof(FilteredMedications));
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Formato de fecha inválido. Usa dd/MM/yyyy", "OK")!;
                SelectedHistoryRange = "Hoy";
            }
        }

        private async void ConfirmDose(MedicationDose dose)
        {
            System.Diagnostics.Debug.WriteLine($"🔔 ConfirmDose: {dose.Medication?.Name} - Scheduled: {dose.ScheduledTime:HH:mm}");
            
            if (dose.IsConfirmed)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Dosis ya confirmada, ignorando...");
                return;
            }
            
            // 🆕 Preguntar si quiere usar hora actual o programada
            bool useScheduledTime = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar Hora de Dosis",
                $"Dosis programada: {dose.ScheduledTime:HH:mm}\n" +
                $"Hora actual: {DateTime.Now:HH:mm}\n\n" +
                $"¿Qué hora deseas registrar?",
                $"Programada ({dose.ScheduledTime:HH:mm})", 
                $"Actual ({DateTime.Now:HH:mm})")!;
            
            // Confirmar la dosis con la hora elegida
            await _dataService.ConfirmDoseAsync(dose, useScheduledTime);
            
            // Agregar al historial de medicamentos
            if (dose.Medication != null && dose.IsConfirmed)
            {
                // Verificar que no exista ya en el historial (evitar duplicados)
                var existingHistory = MedicationHistory.FirstOrDefault(h => 
                    h.MedicationId == dose.MedicationId && 
                    Math.Abs((h.AdministeredTime - (dose.ActualTime ?? DateTime.Now)).TotalMinutes) < 1);
                
                if (existingHistory != null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Ya existe historial para esta dosis, evitando duplicado");
                }
                else
                {
                    var history = new MedicationHistory
                    {
                        MedicationId = dose.MedicationId,
                        MedicationName = dose.Medication.Name,
                        Dose = dose.Medication.Dose,
                        AdministeredTime = dose.ActualTime ?? DateTime.Now,
                        UserType = _dataService.CurrentUserType,
                        TreatmentStartDate = dose.Medication.TreatmentStartDate,
                        TreatmentEndDate = dose.Medication.TreatmentEndDate
                    };
                    await _dataService.SaveMedicationHistoryAsync(history);
                    MedicationHistory.Insert(0, history);
                    System.Diagnostics.Debug.WriteLine($"💾 Historial creado: {history.MedicationName} at {history.AdministeredTime:HH:mm}");
                }
                
                OnPropertyChanged(nameof(FilteredMedicationHistory));
                
                // 🔄 CRÍTICO: Recalcular las siguientes dosis basadas en la hora real de administración
                System.Diagnostics.Debug.WriteLine($"🔄 Recalculando siguientes dosis desde {dose.ActualTime:HH:mm}...");
                await _dataService.RecalculateNextDosesFromLastConfirmedAsync(dose.MedicationId, SelectedDays);
            }
            
            _dataService.RebuildCombinedEvents();
            NotifyDosesChanged();
            OnPropertyChanged(nameof(GroupedDoses));
            OnPropertyChanged(nameof(PendingDoses));
        }

        private async void EditDose(MedicationDose dose)
        {
            var result = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Hora",
                "Ingrese la nueva hora (HH:mm):",
                initialValue: dose.ScheduledTime.ToString("HH:mm")
            )!;

            if (!string.IsNullOrWhiteSpace(result) && TimeSpan.TryParse(result, out TimeSpan newTime))
            {
                var newDateTime = dose.ScheduledTime.Date.Add(newTime);
                await _dataService.EditDoseTimeAsync(dose, newDateTime);
                OnPropertyChanged(nameof(GroupedDoses));
            }
        }

        private async void DeleteDose(MedicationDose dose)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar dosis de {dose.Medication?.Name} programada para {dose.ScheduledTime:HH:mm}?",
                "Sí",
                "No"
            )!;

            if (confirm)
            {
                await _dataService.DeleteDoseAsync(dose);
                OnPropertyChanged(nameof(GroupedDoses));
            }
        }

        private async void RefreshCalendar()
        {
            await _dataService.RegenerateDosesAsync(SelectedDays);
            OnPropertyChanged(nameof(GroupedDoses));
        }

        // ========== MEDICATION HISTORY ==========
        
        public ObservableCollection<MedicationHistory> MedicationHistory => _dataService.MedicationHistory;

        // Expose the combined events collection (calendar + history)
        public ObservableCollection<MedicationEvent> CombinedMedicationEvents => _dataService.CombinedMedicationEvents;

        public ObservableCollection<MedicationHistory> FilteredMedicationHistory
        {
            get
            {
                // El historial muestra eventos PASADOS, no futuros
                var now = DateTime.Now;
                var (startDate, _) = GetDateRange();
                
                // Para historial, queremos desde startDate hacia atrás hasta ahora
                var filtered = MedicationHistory
                    .Where(h => h.AdministeredTime <= now) // Solo eventos pasados
                    .OrderByDescending(h => h.AdministeredTime)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"📊 FilteredMedicationHistory: Total en MedicationHistory={MedicationHistory.Count}, Filtrados={filtered.Count}");
                
                return new ObservableCollection<MedicationHistory>(filtered);
            }
        }

        private async void DeleteMedicationHistory(MedicationHistory history)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {history.MedicationName}?",
                "Sí",
                "No"
            )!;

            if (confirm)
            {
                int medicationId = history.MedicationId;
                await _dataService.DeleteMedicationHistoryAsync(history);
                
                // 🔄 IMPORTANTE: Recalcular las siguientes dosis después de borrar del historial
                System.Diagnostics.Debug.WriteLine($"🔄 Recalculando dosis después de borrar historial de {history.MedicationName}...");
                await _dataService.RecalculateNextDosesFromLastConfirmedAsync(medicationId, SelectedDays);
                
                _dataService.RebuildCombinedEvents();
                NotifyDosesChanged();
                OnPropertyChanged(nameof(FilteredMedicationHistory));
                OnPropertyChanged(nameof(GroupedDoses));
            }
        }

        // NUEVA IMPLEMENTACIÓN: Dosis pendientes ordenadas con la siguiente dosis marcada
        public ObservableCollection<MedicationEvent> PendingDoses
        {
            get
            {
                var now = DateTime.Now;
                var (startDate, endDate) = GetDateRange();
                
                // Filtrar solo eventos pendientes (no confirmados, no históricos)
                var pending = CombinedMedicationEvents
                    .Where(e => !e.IsHistory && !e.IsConfirmed)
                    .Where(e => e.EventTime >= startDate && e.EventTime <= endDate);

                // Aplicar filtro de medicamento si hay uno seleccionado
                if (SelectedMedicationId.HasValue)
                {
                    pending = pending.Where(e => e.MedicationId == SelectedMedicationId.Value);
                }

                var list = pending.ToList();
                
                // Identificar la siguiente dosis (la más cercana en el futuro o presente)
                var nextDose = list
                    .Where(e => e.EventTime >= now)
                    .OrderBy(e => e.EventTime)
                    .FirstOrDefault();

                // Marcar la siguiente dosis
                foreach (var dose in list)
                {
                    dose.IsNextDose = (nextDose != null && dose.Id == nextDose.Id);
                }

                // Ordenar: siguiente dosis primero, luego por hora ascendente
                var ordered = list
                    .OrderByDescending(e => e.IsNextDose)
                    .ThenBy(e => e.EventTime)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"✅ Dosis pendientes: {ordered.Count}, Siguiente: {nextDose?.EventTime:HH:mm}");
                
                return new ObservableCollection<MedicationEvent>(ordered);
            }
        }

        // Historial de dosis confirmadas (ordenadas de más reciente a más antigua)
        public ObservableCollection<MedicationEvent> ConfirmedDoses
        {
            get
            {
                var (startDate, endDate) = GetDateRange();
                
                // Filtrar solo eventos confirmados/históricos
                var confirmed = CombinedMedicationEvents
                    .Where(e => e.IsHistory || e.IsConfirmed)
                    .Where(e => e.EventTime >= startDate && e.EventTime <= endDate);

                // Aplicar filtro de medicamento si hay uno seleccionado
                if (SelectedMedicationId.HasValue)
                {
                    confirmed = confirmed.Where(e => e.MedicationId == SelectedMedicationId.Value);
                }

                // Ordenar de más reciente a más antiguo
                var ordered = confirmed.OrderByDescending(e => e.EventTime).ToList();
                
                System.Diagnostics.Debug.WriteLine($"✅ Dosis confirmadas: {ordered.Count}");
                
                return new ObservableCollection<MedicationEvent>(ordered);
            }
        }

        // DEPRECADO: Mantener por compatibilidad pero redirigir a PendingDoses
        public ObservableCollection<MedicationEvent> FilteredCombinedEvents => PendingDoses;

        private async void ConfirmEvent(MedicationEvent ev)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== ConfirmEvent START ===");
                
                if (ev == null)
                {
                    System.Diagnostics.Debug.WriteLine("❌ ConfirmEvent: ev is null");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"📋 Event: Id={ev.Id}, MedId={ev.MedicationId}, SourceId={ev.SourceId}, IsHistory={ev.IsHistory}, Name={ev.MedicationName}");

                if (ev.IsHistory)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Event is already history");
                    await Application.Current?.MainPage?.DisplayAlert("Información", "Este medicamento ya fue administrado", "OK")!;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"🔍 Searching dose with Id={ev.SourceId} in {_dataService.MedicationDoses.Count} doses");
                System.Diagnostics.Debug.WriteLine($"📊 Available doses: {string.Join(", ", _dataService.MedicationDoses.Select(d => $"[{d.Id}]"))}");
                
                var dose = _dataService.MedicationDoses.FirstOrDefault(d => d.Id == ev.SourceId);
                if (dose == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Dose NOT found for SourceId={ev.SourceId}");
                    await Application.Current?.MainPage?.DisplayAlert("Error", "No se encontró la dosis programada", "OK")!;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Found dose {dose.Id}, confirming...");
                
                if (dose.IsConfirmed)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Dosis ya confirmada, ignorando...");
                    await Application.Current?.MainPage?.DisplayAlert("Información", "Esta dosis ya fue confirmada", "OK")!;
                    return;
                }
                
                await _dataService.ConfirmDoseAsync(dose);

                // Verificar que no exista ya en el historial (evitar duplicados)
                var existingHistory = _dataService.MedicationHistory.FirstOrDefault(h => 
                    h.MedicationId == dose.MedicationId && 
                    Math.Abs((h.AdministeredTime - (dose.ActualTime ?? DateTime.Now)).TotalMinutes) < 1);
                
                if (existingHistory != null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Ya existe historial para esta dosis, evitando duplicado");
                }
                else
                {
                    var history = new MedicationHistory
                    {
                        MedicationId = dose.MedicationId,
                        MedicationName = dose.Medication?.Name ?? string.Empty,
                        Dose = dose.Medication?.Dose ?? string.Empty,
                        AdministeredTime = dose.ActualTime ?? DateTime.Now,
                        UserType = _dataService.CurrentUserType
                    };

                    System.Diagnostics.Debug.WriteLine($"📝 Creating history: {history.MedicationName} at {history.AdministeredTime:HH:mm}");
                    await _dataService.SaveMedicationHistoryAsync(history);
                    _dataService.MedicationHistory.Insert(0, history);
                    System.Diagnostics.Debug.WriteLine($"💾 History saved. Total={_dataService.MedicationHistory.Count}");
                }
                
                // 🔄 IMPORTANTE: Recalcular las siguientes dosis desde la última confirmada
                System.Diagnostics.Debug.WriteLine($"🔄 Recalculando siguientes dosis para {ev.MedicationName}...");
                await _dataService.RecalculateNextDosesFromLastConfirmedAsync(dose.MedicationId, SelectedDays);
                
                _dataService.RebuildCombinedEvents();
                NotifyDosesChanged();
                OnPropertyChanged(nameof(FilteredMedicationHistory));
                OnPropertyChanged(nameof(GroupedDoses));
                
                System.Diagnostics.Debug.WriteLine("=== ConfirmEvent SUCCESS ===");
                await Application.Current?.MainPage?.DisplayAlert("✅", $"Dosis de {ev.MedicationName} confirmada", "OK")!;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                await Application.Current?.MainPage?.DisplayAlert("Error", $"Error al confirmar: {ex.Message}", "OK")!;
            }
        }

        private async void DeleteEvent(MedicationEvent ev)
        {
            if (ev == null) return;

            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                ev.IsHistory ? $"¿Eliminar registro de {ev.MedicationName}?" : $"¿Eliminar dosis programada de {ev.MedicationName} para {ev.EventTime:HH:mm}?",
                "Sí",
                "No")!;

            if (!confirm) return;

            if (ev.IsHistory)
            {
                var history = _dataService.MedicationHistory.FirstOrDefault(h => h.Id == ev.SourceId);
                if (history != null)
                {
                    await _dataService.DeleteMedicationHistoryAsync(history);
                }
            }
            else
            {
                var dose = _dataService.MedicationDoses.FirstOrDefault(d => d.Id == ev.SourceId);
                if (dose != null)
                {
                    await _dataService.DeleteDoseAsync(dose);
                }
            }

            _dataService.RebuildCombinedEvents();
            OnPropertyChanged(nameof(FilteredCombinedEvents));
            OnPropertyChanged(nameof(GroupedDoses));
        }

        // ========== MEDICAL APPOINTMENTS ==========
        
        public ObservableCollection<MedicalAppointment> Appointments => _dataService.Appointments;

        public ObservableCollection<MedicalAppointment> FilteredAppointments
        {
            get
            {
                var (startDate, endDate) = GetDateRange();
                System.Diagnostics.Debug.WriteLine($"🏥 FilteredAppointments: Rango de fechas {startDate:yyyy-MM-dd HH:mm} a {endDate:yyyy-MM-dd HH:mm}");
                System.Diagnostics.Debug.WriteLine($"📊 Total citas en Appointments: {Appointments.Count}");
                
                var filtered = Appointments
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();
                
                System.Diagnostics.Debug.WriteLine($"✅ Citas filtradas: {filtered.Count}");
                foreach (var app in filtered)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {app.Title} - {app.AppointmentDate:yyyy-MM-dd HH:mm}");
                }
                
                return new ObservableCollection<MedicalAppointment>(filtered);
            }
        }

        // Citas pendientes (no confirmadas)
        public ObservableCollection<MedicalAppointment> PendingAppointments
        {
            get
            {
                var (startDate, endDate) = GetDateRange();
                var pending = Appointments
                    .Where(a => !a.IsConfirmed)
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();
                
                return new ObservableCollection<MedicalAppointment>(pending);
            }
        }

        // Citas confirmadas (historial)
        public ObservableCollection<MedicalAppointment> ConfirmedAppointments
        {
            get
            {
                var (startDate, endDate) = GetDateRange();
                var confirmed = Appointments
                    .Where(a => a.IsConfirmed)
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                    .OrderByDescending(a => a.ConfirmedDate ?? a.AppointmentDate)
                    .ToList();
                
                return new ObservableCollection<MedicalAppointment>(confirmed);
            }
        }

        private string _appointmentTitle = string.Empty;
        public string AppointmentTitle
        {
            get => _appointmentTitle;
            set
            {
                _appointmentTitle = value;
                OnPropertyChanged();
            }
        }

        private string _appointmentDescription = string.Empty;
        public string AppointmentDescription
        {
            get => _appointmentDescription;
            set
            {
                _appointmentDescription = value;
                OnPropertyChanged();
            }
        }

        private DateTime _appointmentDate = DateTime.Today;
        public DateTime AppointmentDate
        {
            get => _appointmentDate;
            set
            {
                _appointmentDate = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _appointmentTime = DateTime.Now.TimeOfDay;
        public TimeSpan AppointmentTime
        {
            get => _appointmentTime;
            set
            {
                _appointmentTime = value;
                OnPropertyChanged();
            }
        }

        private string _appointmentLocation = string.Empty;
        public string AppointmentLocation
        {
            get => _appointmentLocation;
            set
            {
                _appointmentLocation = value;
                OnPropertyChanged();
            }
        }

        private string _appointmentDoctor = string.Empty;
        public string AppointmentDoctor
        {
            get => _appointmentDoctor;
            set
            {
                _appointmentDoctor = value;
                OnPropertyChanged();
            }
        }

        private async void AddAppointment()
        {
            if (string.IsNullOrWhiteSpace(AppointmentTitle))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Por favor ingrese un título para la cita", "OK")!;
                return;
            }

            var appointment = new MedicalAppointment
            {
                Title = AppointmentTitle,
                Description = AppointmentDescription,
                AppointmentDate = AppointmentDate.Add(AppointmentTime),
                Location = AppointmentLocation,
                Doctor = AppointmentDoctor
            };

            await _dataService.AddAppointmentAsync(appointment);

            // Clear fields
            AppointmentTitle = string.Empty;
            AppointmentDescription = string.Empty;
            AppointmentDate = DateTime.Today;
            AppointmentTime = DateTime.Now.TimeOfDay;
            AppointmentLocation = string.Empty;
            AppointmentDoctor = string.Empty;

            OnPropertyChanged(nameof(FilteredAppointments));
            OnPropertyChanged(nameof(PendingAppointments));
            OnPropertyChanged(nameof(ConfirmedAppointments));
            await Application.Current?.MainPage?.DisplayAlert("✅ Cita agregada", "La cita médica ha sido registrada", "OK")!;
        }

        private async void EditAppointment(MedicalAppointment appointment)
        {
            // 🆕 Simplificado: Solo editar título, doctor y ubicación
            // Para cambiar fecha/hora, el usuario debe eliminar y crear nueva cita
            
            var newTitle = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Cita",
                "Título:",
                initialValue: appointment.Title)!;

            if (string.IsNullOrWhiteSpace(newTitle)) return;

            var newDoctor = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Doctor",
                "Nombre del doctor:",
                initialValue: appointment.Doctor)!;

            var newLocation = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Ubicación",
                "Ubicación:",
                initialValue: appointment.Location)!;

            var newDescription = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Descripción",
                "Descripción (opcional):",
                initialValue: appointment.Description)!;

            appointment.Title = newTitle;
            appointment.Doctor = newDoctor ?? string.Empty;
            appointment.Location = newLocation ?? string.Empty;
            appointment.Description = newDescription ?? string.Empty;

            await _dataService.UpdateAppointmentAsync(appointment);
            OnPropertyChanged(nameof(FilteredAppointments));
            OnPropertyChanged(nameof(PendingAppointments));
            OnPropertyChanged(nameof(ConfirmedAppointments));
            
            await Application.Current?.MainPage?.DisplayAlert(
                "✅ Actualizada", 
                "Cita actualizada correctamente.", 
                "OK")!;
        }

        private async void ChangeAppointmentDateTime(MedicalAppointment appointment)
        {
            System.Diagnostics.Debug.WriteLine($"🔵 ChangeAppointmentDateTime ejecutado para: {appointment?.Title}");
            
            if (appointment == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Appointment es null!");
                return;
            }

            try
            {
                // 🆕 Modal visual con DatePicker y TimePicker para cambiar fecha/hora
                var modal = new ContentPage
                {
                    Title = "Cambiar Fecha y Hora",
                    BackgroundColor = Colors.White
                };

            // DatePicker con estilo más visual que muestra calendario nativo al tocar
            var datePicker = new DatePicker
            {
                Date = appointment.AppointmentDate.Date,
                MinimumDate = DateTime.Today,
                Format = "dddd dd MMMM yyyy", // Formato personalizado: Viernes 24 Octubre 2025
                FontSize = 16,
                TextColor = Color.FromArgb("#333"),
                BackgroundColor = Color.FromArgb("#f5f5f5"),
                Margin = new Thickness(20, 10),
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Fill
            };

            var timePicker = new TimePicker
            {
                Time = appointment.AppointmentDate.TimeOfDay,
                Format = "HH:mm",
                FontSize = 20,
                TextColor = Color.FromArgb("#333"),
                BackgroundColor = Color.FromArgb("#f5f5f5"),
                Margin = new Thickness(20, 10),
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Center
            };

            var saveButton = new Button
            {
                Text = "Guardar Cambios",
                BackgroundColor = Color.FromArgb("#4CAF50"),
                TextColor = Colors.White,
                Margin = new Thickness(20, 20),
                CornerRadius = 10,
                HeightRequest = 50
            };

            var cancelButton = new Button
            {
                Text = "Cancelar",
                BackgroundColor = Color.FromArgb("#f44336"),
                TextColor = Colors.White,
                Margin = new Thickness(20, 0),
                CornerRadius = 10,
                HeightRequest = 50
            };

            saveButton.Clicked += async (s, e) =>
            {
                try
                {
                    var newDateTime = datePicker.Date.Add(timePicker.Time);
                    appointment.AppointmentDate = newDateTime;
                    
                    await _dataService.UpdateAppointmentAsync(appointment);
                    
                    // Cerrar modal primero
                    if (Application.Current?.MainPage?.Navigation != null)
                    {
                        await Application.Current.MainPage.Navigation.PopModalAsync();
                    }
                    
                    // Actualizar UI
                    OnPropertyChanged(nameof(FilteredAppointments));
                    OnPropertyChanged(nameof(PendingAppointments));
                    OnPropertyChanged(nameof(ConfirmedAppointments));
                    
                    // Mostrar confirmación
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "✅ Fecha Actualizada",
                            $"La cita se ha movido a:\n{newDateTime:dd/MM/yyyy HH:mm}",
                            "OK");
                    }
                }
                catch (Exception ex)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    }
                }
            };

            cancelButton.Clicked += async (s, e) =>
            {
                if (Application.Current?.MainPage?.Navigation != null)
                {
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                }
            };

                modal.Content = new ScrollView
                {
                    Content = new StackLayout
                    {
                        Padding = new Thickness(20),
                        Spacing = 15,
                        Children =
                        {
                            new Frame
                            {
                                BackgroundColor = Color.FromArgb("#E3F2FD"),
                                BorderColor = Color.FromArgb("#2196F3"),
                                CornerRadius = 10,
                                Padding = 15,
                                Margin = new Thickness(0, 10, 0, 20),
                                Content = new StackLayout
                                {
                                    Children =
                                    {
                                        new Label
                                        {
                                            Text = $"📅 {appointment.Title}",
                                            FontSize = 20,
                                            FontAttributes = FontAttributes.Bold,
                                            HorizontalOptions = LayoutOptions.Center,
                                            TextColor = Color.FromArgb("#1976D2")
                                        },
                                        new Label
                                        {
                                            Text = $"Fecha actual: {appointment.AppointmentDate:dd/MM/yyyy HH:mm}",
                                            FontSize = 14,
                                            HorizontalOptions = LayoutOptions.Center,
                                            TextColor = Color.FromArgb("#555"),
                                            Margin = new Thickness(0, 5, 0, 0)
                                        }
                                    }
                                }
                            },
                            new Label
                            {
                                Text = "📆 Toca para abrir el calendario:",
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#333"),
                                Margin = new Thickness(0, 10, 0, 5)
                            },
                            datePicker,
                            new Label
                            {
                                Text = "🕐 Selecciona la hora:",
                                FontSize = 16,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#333"),
                                Margin = new Thickness(0, 20, 0, 5)
                            },
                            timePicker,
                            saveButton,
                            cancelButton
                        }
                    }
                };

                // Envolver en NavigationPage para que se muestre correctamente
                var navigationPage = new NavigationPage(modal);
                
                System.Diagnostics.Debug.WriteLine("🔵 Intentando mostrar modal...");
                
                if (Application.Current?.MainPage?.Navigation != null)
                {
                    await Application.Current.MainPage.Navigation.PushModalAsync(navigationPage);
                    System.Diagnostics.Debug.WriteLine("✅ Modal mostrado correctamente");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Navigation es null!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en ChangeAppointmentDateTime: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");
                
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        $"No se pudo abrir el editor de fecha: {ex.Message}", "OK");
                }
            }
        }

        private async void DeleteAppointment(MedicalAppointment appointment)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar cita '{appointment.Title}'?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteAppointmentAsync(appointment);
                OnPropertyChanged(nameof(FilteredAppointments));
                OnPropertyChanged(nameof(PendingAppointments));
                OnPropertyChanged(nameof(ConfirmedAppointments));
                await Application.Current?.MainPage?.DisplayAlert("Eliminada", "Cita eliminada", "OK")!;
            }
        }

        private async void ConfirmAppointment(MedicalAppointment appointment)
        {
            if (appointment.IsConfirmed)
            {
                await Application.Current?.MainPage?.DisplayAlert("Información", "Esta cita ya fue confirmada", "OK")!;
                return;
            }

            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar Cita",
                $"¿Confirmar la cita '{appointment.Title}'?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.ConfirmAppointmentAsync(appointment);
                OnPropertyChanged(nameof(FilteredAppointments));
                OnPropertyChanged(nameof(PendingAppointments));
                OnPropertyChanged(nameof(ConfirmedAppointments));
                await Application.Current?.MainPage?.DisplayAlert("✅ Confirmada", $"Cita '{appointment.Title}' confirmada", "OK")!;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper para notificar cambios en las listas de dosis
        private void NotifyDosesChanged()
        {
            OnPropertyChanged(nameof(FilteredCombinedEvents));
            OnPropertyChanged(nameof(PendingDoses));
            OnPropertyChanged(nameof(ConfirmedDoses));
        }

        private Unit ConvertStringToUnit(string unitString)
        {
            return unitString switch
            {
                "g" => Unit.Gram,
                "kg" => Unit.Kilogram,
                "lb" => Unit.Pound,
                "oz" => Unit.Ounce,
                "ml" => Unit.Milliliter,
                "L" => Unit.Liter,
                "pieza" => Unit.Piece,
                _ => Unit.Gram
            };
        }
    }
}
