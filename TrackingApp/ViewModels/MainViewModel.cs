using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private string _selectedUserType = "Bebé";
        private int _selectedDays = 3;
        private int? _selectedMedicationId;
        private Medication? _selectedMedication;
        private string _selectedHistoryRange = "Hoy";

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
            ResetAllDataCommand = new Command(ResetAllData);
            ConfirmDoseCommand = new Command<MedicationDose>(ConfirmDose);
            EditDoseCommand = new Command<MedicationDose>(EditDose);
            DeleteDoseCommand = new Command<MedicationDose>(DeleteDose);
            RefreshCalendarCommand = new Command(RefreshCalendar);
            AddMedicationHistoryCommand = new Command(AddMedicationHistoryEntry);
            DeleteMedicationHistoryCommand = new Command<MedicationHistory>(DeleteMedicationHistory);
            ConfirmEventCommand = new Command<MedicationEvent>(ConfirmEvent);
            DeleteEventCommand = new Command<MedicationEvent>(DeleteEvent);
            AddAppointmentCommand = new Command(AddAppointment);
            EditAppointmentCommand = new Command<MedicalAppointment>(EditAppointment);
            DeleteAppointmentCommand = new Command<MedicalAppointment>(DeleteAppointment);

            // Subscribe to collection changes
            _dataService.Medications.CollectionChanged += (s, e) => UpdateSelectedMedication();
            _dataService.FoodEntries.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredFoodEntries));
            _dataService.MedicationHistory.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(FilteredMedicationHistory));
                OnPropertyChanged(nameof(FilteredCombinedEvents));
            };
            _dataService.CombinedMedicationEvents.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredCombinedEvents));
            _dataService.Appointments.CollectionChanged += (s, e) => OnPropertyChanged(nameof(FilteredAppointments));
            
            // Set first medication as default
            UpdateSelectedMedication();
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
        public List<string> Units => new() { "oz", "ml", "g", "taza", "cucharada", "minutos", "horas" };
        public List<int> DaysOptions => new() { 1, 2, 3, 5, 7 };

        public int SelectedDays
        {
            get => _selectedDays;
            set
            {
                _selectedDays = value;
                _ = _dataService.RegenerateDosesAsync(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(GroupedDoses));
            }
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
                OnPropertyChanged(nameof(SelectedMedicationFilterLabel));
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
                var (startDate, endDate) = GetDateRange();
                var filtered = Medications.Where(m => m.FirstDoseTime >= startDate && m.FirstDoseTime <= endDate).ToList();
                return new ObservableCollection<Medication>(filtered);
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

        private string _foodUnit = "oz";
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
        public ICommand ResetAllDataCommand { get; }
        public ICommand ConfirmDoseCommand { get; }
        public ICommand EditDoseCommand { get; }
        public ICommand DeleteDoseCommand { get; }
        public ICommand RefreshCalendarCommand { get; }
        public ICommand AddMedicationHistoryCommand { get; }
        public ICommand DeleteMedicationHistoryCommand { get; }
        public ICommand ConfirmEventCommand { get; }
        public ICommand DeleteEventCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }

        private async void AddFood()
        {
            if (string.IsNullOrWhiteSpace(FoodType) || string.IsNullOrWhiteSpace(FoodAmount))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Por favor complete todos los campos", "OK")!;
                return;
            }

            if (!double.TryParse(FoodAmount, out double amount))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "La cantidad debe ser un número", "OK")!;
                return;
            }

            var entry = new FoodEntry
            {
                FoodType = FoodType,
                Amount = amount,
                Unit = FoodUnit,
                Time = DateTime.Today.Add(FoodTime)
            };

            await _dataService.AddFoodEntryAsync(entry);

            // Clear fields
            FoodType = string.Empty;
            FoodAmount = string.Empty;
            FoodTime = DateTime.Now.TimeOfDay;

            OnPropertyChanged(nameof(FilteredFoodEntries));
            await Application.Current?.MainPage?.DisplayAlert("Éxito", "Alimento agregado", "OK")!;
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

            var medication = new Medication
            {
                Name = MedicationName,
                Dose = MedicationDose,
                FrequencyHours = hours,
                FrequencyMinutes = minutes,
                FirstDoseTime = DateTime.Today.Add(MedicationTime)
            };

            await _dataService.AddMedicationAsync(medication);

            // Clear fields
            MedicationName = string.Empty;
            MedicationDose = string.Empty;
            MedicationFrequencyHours = string.Empty;
            MedicationFrequencyMinutes = string.Empty;
            MedicationTime = DateTime.Now.TimeOfDay;

            OnPropertyChanged(nameof(GroupedDoses));
            OnPropertyChanged(nameof(FilteredMedications));
            UpdateSelectedMedication();
            await Application.Current?.MainPage?.DisplayAlert("Éxito", "Medicamento agregado", "OK")!;
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

            if (double.TryParse(newAmountStr, out double newAmount))
            {
                food.FoodType = newType;
                food.Amount = newAmount;
                await _dataService.UpdateFoodEntryAsync(food);
                OnPropertyChanged(nameof(FilteredFoodEntries));
                await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Alimento actualizado", "OK")!;
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
            // Confirmar la dosis y registrar en el historial
            await _dataService.ConfirmDoseAsync(dose);
            
            // Agregar al historial de medicamentos
            if (dose.Medication != null && dose.IsConfirmed)
            {
                var history = new MedicationHistory
                {
                    MedicationId = dose.MedicationId,
                    MedicationName = dose.Medication.Name,
                    Dose = dose.Medication.Dose,
                    AdministeredTime = dose.ActualTime ?? DateTime.Now,
                    UserType = _dataService.CurrentUserType
                };
                await _dataService.SaveMedicationHistoryAsync(history);
                MedicationHistory.Insert(0, history);
                OnPropertyChanged(nameof(FilteredMedicationHistory));
            }
            
            OnPropertyChanged(nameof(GroupedDoses));
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
                var (startDate, endDate) = GetDateRange();
                var filtered = MedicationHistory
                    .Where(h => h.AdministeredTime >= startDate && h.AdministeredTime <= endDate)
                    .OrderByDescending(h => h.AdministeredTime)
                    .ToList();
                
                return new ObservableCollection<MedicationHistory>(filtered);
            }
        }

        private async void AddMedicationHistoryEntry()
        {
            if (SelectedMedication == null)
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Por favor seleccione un medicamento primero", "OK")!;
                return;
            }

            // Permitir al usuario elegir la hora
            var timeStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Registrar Dosis",
                "¿A qué hora se administró? (HH:mm)\nDejar vacío para usar hora actual",
                placeholder: DateTime.Now.ToString("HH:mm"))!;

            DateTime administeredTime = DateTime.Now;
            
            if (!string.IsNullOrWhiteSpace(timeStr) && TimeSpan.TryParse(timeStr, out TimeSpan enteredTime))
            {
                administeredTime = DateTime.Today.Add(enteredTime);
            }

            var history = new MedicationHistory
            {
                MedicationId = SelectedMedication.Id,
                MedicationName = SelectedMedication.Name,
                Dose = SelectedMedication.Dose,
                AdministeredTime = administeredTime,
                UserType = _dataService.CurrentUserType
            };

            await _dataService.SaveMedicationHistoryAsync(history);
            MedicationHistory.Insert(0, history);
            _dataService.RebuildCombinedEvents();
            OnPropertyChanged(nameof(FilteredMedicationHistory));
            OnPropertyChanged(nameof(FilteredCombinedEvents));
            await Application.Current?.MainPage?.DisplayAlert("✅ Registrado", $"Dosis de {SelectedMedication.Name} registrada a las {administeredTime:HH:mm}", "OK")!;
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
                await _dataService.DeleteMedicationHistoryAsync(history);
                _dataService.RebuildCombinedEvents();
                OnPropertyChanged(nameof(FilteredMedicationHistory));
                OnPropertyChanged(nameof(FilteredCombinedEvents));
            }
        }

        public ObservableCollection<MedicationEvent> FilteredCombinedEvents
        {
            get
            {
                var (startDate, endDate) = GetDateRange();
                var filtered = CombinedMedicationEvents
                    .Where(e => e.EventTime >= startDate && e.EventTime <= endDate)
                    .OrderByDescending(e => e.EventTime)
                    .ToList();

                return new ObservableCollection<MedicationEvent>(filtered);
            }
        }

        private async void ConfirmEvent(MedicationEvent ev)
        {
            if (ev == null) return;

            if (!ev.IsHistory)
            {
                var dose = _dataService.MedicationDoses.FirstOrDefault(d => d.Id == ev.SourceId);
                if (dose != null)
                {
                    await _dataService.ConfirmDoseAsync(dose);

                    var history = new MedicationHistory
                    {
                        MedicationId = dose.MedicationId,
                        MedicationName = dose.Medication?.Name ?? string.Empty,
                        Dose = dose.Medication?.Dose ?? string.Empty,
                        AdministeredTime = dose.ActualTime ?? DateTime.Now,
                        UserType = _dataService.CurrentUserType
                    };

                    await _dataService.SaveMedicationHistoryAsync(history);
                    _dataService.MedicationHistory.Insert(0, history);
                    _dataService.RebuildCombinedEvents();
                    OnPropertyChanged(nameof(FilteredCombinedEvents));
                    OnPropertyChanged(nameof(GroupedDoses));
                }
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
                var filtered = Appointments
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                    .OrderBy(a => a.AppointmentDate)
                    .ToList();
                
                return new ObservableCollection<MedicalAppointment>(filtered);
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
            await Application.Current?.MainPage?.DisplayAlert("✅ Cita agregada", "La cita médica ha sido registrada", "OK")!;
        }

        private async void EditAppointment(MedicalAppointment appointment)
        {
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

            appointment.Title = newTitle;
            appointment.Doctor = newDoctor ?? string.Empty;
            appointment.Location = newLocation ?? string.Empty;

            await _dataService.UpdateAppointmentAsync(appointment);
            OnPropertyChanged(nameof(FilteredAppointments));
            await Application.Current?.MainPage?.DisplayAlert("✅ Actualizada", "Cita actualizada correctamente", "OK")!;
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
                await Application.Current?.MainPage?.DisplayAlert("Eliminada", "Cita eliminada", "OK")!;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
