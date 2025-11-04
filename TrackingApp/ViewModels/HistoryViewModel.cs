using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels
{
    public class HistoryViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<MedicationHistory> _allMedicationHistory;
        private ObservableCollection<FoodEntry> _allFoodHistory;
        private List<MedicalAppointment> _allAppointments;
        
        private string _selectedMedicationFilter = "Todos";
        private string _selectedFoodTypeFilter = "Todos";
        private string _selectedUnitFilter = "Todos";
        private string _selectedProfileFilter = "Todos";
        private string _selectedDateRangeFilter = "Seleccionar período...";
        private DateTime? _customStartDate;
        private DateTime? _customEndDate;

        public ObservableCollection<MedicationHistory> FilteredMedicationHistory { get; set; }
        public ObservableCollection<FoodEntry> FilteredFoodHistory { get; set; }
        public ObservableCollection<MedicalAppointment> FilteredAppointments { get; set; }
        public ObservableCollection<string> MedicationNames { get; set; }
        public ObservableCollection<string> FoodTypes { get; set; }
        public ObservableCollection<string> Units { get; set; }
        public ObservableCollection<string> Profiles { get; set; }
        public ObservableCollection<string> DateRangeOptions { get; set; }

        public int TotalConfirmedDoses => FilteredMedicationHistory?.Count ?? 0;
        public int TotalConfirmedAppointments => FilteredAppointments?.Count ?? 0;
        public int TotalFoodEntries => FilteredFoodHistory?.Count ?? 0;
        public double TotalFoodAmount => FilteredFoodHistory?.Sum(f => f.Amount) ?? 0;
        public string MostFrequentFoodType 
        {
            get
            {
                if (FilteredFoodHistory == null || !FilteredFoodHistory.Any())
                    return "N/A";
                
                return FilteredFoodHistory
                    .GroupBy(f => f.FoodType)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A";
            }
        }

        public string SelectedMedicationFilter
        {
            get => _selectedMedicationFilter;
            set
            {
                _selectedMedicationFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string SelectedFoodTypeFilter
        {
            get => _selectedFoodTypeFilter;
            set
            {
                _selectedFoodTypeFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string SelectedUnitFilter
        {
            get => _selectedUnitFilter;
            set
            {
                _selectedUnitFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string SelectedProfileFilter
        {
            get => _selectedProfileFilter;
            set
            {
                _selectedProfileFilter = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string SelectedDateRangeFilter
        {
            get => _selectedDateRangeFilter;
            set
            {
                _selectedDateRangeFilter = value;
                OnPropertyChanged();
                if (value == "Rango personalizado")
                {
                    _ = ShowCustomDateRangePicker();
                }
                else
                {
                    ApplyFilters();
                }
            }
        }

        public ICommand DeleteMedicationHistoryCommand { get; }
        public ICommand DeleteFoodHistoryCommand { get; }
        public ICommand EditMedicationHistoryCommand { get; }
        public ICommand EditFoodCommand { get; }
        public ICommand DeleteAppointmentHistoryCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand RefreshCommand { get; }

        public HistoryViewModel()
        {
            _dataService = DataService.Instance;
            
            _allMedicationHistory = new ObservableCollection<MedicationHistory>();
            _allFoodHistory = new ObservableCollection<FoodEntry>();
            _allAppointments = new List<MedicalAppointment>();
            FilteredMedicationHistory = new ObservableCollection<MedicationHistory>();
            FilteredFoodHistory = new ObservableCollection<FoodEntry>();
            FilteredAppointments = new ObservableCollection<MedicalAppointment>();
            
            MedicationNames = new ObservableCollection<string> { "Todos" };
            FoodTypes = new ObservableCollection<string> { "Todos" };
            Units = new ObservableCollection<string> { "Todos", "g", "ml", "unidades", "cucharadas", "tazas" };
            Profiles = new ObservableCollection<string> { "Todos", "Adulto", "Niño" };
            DateRangeOptions = new ObservableCollection<string> 
            { 
                "Seleccionar período...",  // Opción por defecto (no carga nada)
                "Todo el historial",
                "Hoy", 
                "Últimos 7 días", 
                "Últimos 30 días",
                "Este mes",
                "Mes anterior",
                "Rango personalizado"
            };

            DeleteMedicationHistoryCommand = new Command<MedicationHistory>(DeleteMedicationHistory);
            DeleteFoodHistoryCommand = new Command<FoodEntry>(DeleteFoodHistory);
            EditMedicationHistoryCommand = new Command<MedicationHistory>(EditMedicationHistory);
            EditFoodCommand = new Command<FoodEntry>(EditFood);
            DeleteAppointmentHistoryCommand = new Command<MedicalAppointment>(DeleteAppointmentHistory);
            EditAppointmentCommand = new Command<MedicalAppointment>(EditAppointment);
            RefreshCommand = new Command(async () => await LoadHistoryAsync());

            LoadHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            // Cargar historial de medicamentos
            var medHistory = await _dataService.GetAllMedicationHistoryAsync();
            _allMedicationHistory.Clear();
            foreach (var item in medHistory.OrderByDescending(h => h.AdministeredTime))
            {
                _allMedicationHistory.Add(item);
            }

            // Cargar historial de alimentos (todos los registros)
            var foodHistory = _dataService.FoodEntries.OrderByDescending(f => f.Time).ToList();
            _allFoodHistory.Clear();
            foreach (var item in foodHistory)
            {
                _allFoodHistory.Add(item);
            }

            // Cargar citas médicas confirmadas desde la base de datos (solo en memoria, no mostrar)
            var allAppointments = await _dataService.GetAllAppointmentsAsync();
            _allAppointments = allAppointments.Where(a => a.IsConfirmed).ToList();
            
            // NO cargar las citas en FilteredAppointments al inicio - solo cuando se seleccione un filtro
            FilteredAppointments.Clear();

            // Actualizar filtros disponibles
            UpdateAvailableFilters();

            // Aplicar filtros
            ApplyFilters();
        }

        private void UpdateAvailableFilters()
        {
            // Actualizar nombres de medicamentos
            var medNames = _allMedicationHistory.Select(h => h.MedicationName).Distinct().OrderBy(n => n).ToList();
            MedicationNames.Clear();
            MedicationNames.Add("Todos");
            foreach (var name in medNames)
            {
                MedicationNames.Add(name);
            }

            // Actualizar tipos de alimentos
            var foodTypes = _allFoodHistory.Select(f => f.FoodType).Distinct().OrderBy(t => t).ToList();
            FoodTypes.Clear();
            FoodTypes.Add("Todos");
            foreach (var type in foodTypes)
            {
                FoodTypes.Add(type);
            }

            OnPropertyChanged(nameof(MedicationNames));
            OnPropertyChanged(nameof(FoodTypes));
        }

        private void ApplyFilters()
        {
            // Filtrar medicamentos
            var filteredMeds = _allMedicationHistory.AsEnumerable();

            // Filtro por medicamento
            if (SelectedMedicationFilter != "Todos")
            {
                filteredMeds = filteredMeds.Where(h => h.MedicationName == SelectedMedicationFilter);
            }

            // Filtro por perfil
            if (SelectedProfileFilter != "Todos")
            {
                filteredMeds = filteredMeds.Where(h => h.UserType == SelectedProfileFilter);
            }

            // Filtro por rango de fechas
            var (startDate, endDate) = GetDateRange();
            filteredMeds = filteredMeds.Where(h => h.AdministeredTime >= startDate && h.AdministeredTime <= endDate);

            FilteredMedicationHistory.Clear();
            foreach (var item in filteredMeds.OrderByDescending(h => h.AdministeredTime))
            {
                FilteredMedicationHistory.Add(item);
            }

            // Filtrar alimentos
            var filteredFoods = _allFoodHistory.AsEnumerable();

            // Filtro por tipo de alimento
            if (SelectedFoodTypeFilter != "Todos")
            {
                filteredFoods = filteredFoods.Where(f => f.FoodType == SelectedFoodTypeFilter);
            }

            // Filtro por unidad
            if (SelectedUnitFilter != "Todos")
            {
                filteredFoods = filteredFoods.Where(f => f.Unit.GetDisplayText() == SelectedUnitFilter);
            }

            // Filtro por perfil
            if (SelectedProfileFilter != "Todos")
            {
                filteredFoods = filteredFoods.Where(f => f.UserType == SelectedProfileFilter);
            }

            // Filtro por rango de fechas
            filteredFoods = filteredFoods.Where(f => f.Time >= startDate && f.Time <= endDate);

            FilteredFoodHistory.Clear();
            foreach (var item in filteredFoods.OrderByDescending(f => f.Time))
            {
                FilteredFoodHistory.Add(item);
            }

            // Filtrar citas médicas confirmadas (desde _allAppointments que tiene las citas de la BD)
            FilteredAppointments.Clear();
            
            // Si se seleccionó "Seleccionar período...", no mostrar nada
            if (SelectedDateRangeFilter == "Seleccionar período...")
            {
                // Dejar la lista vacía
            }
            else
            {
                var filteredAppointments = _allAppointments.AsEnumerable();

                // Filtro por perfil
                if (SelectedProfileFilter != "Todos")
                {
                    filteredAppointments = filteredAppointments.Where(a => a.UserType == SelectedProfileFilter);
                }

                // Filtro por rango de fechas - usar AppointmentDate (fecha de la cita)
                filteredAppointments = filteredAppointments.Where(a => 
                {
                    return a.AppointmentDate >= startDate && a.AppointmentDate <= endDate;
                });

                foreach (var appointment in filteredAppointments.OrderByDescending(a => a.AppointmentDate))
                {
                    FilteredAppointments.Add(appointment);
                }
            }

            // Actualizar estadísticas
            OnPropertyChanged(nameof(TotalConfirmedDoses));
            OnPropertyChanged(nameof(TotalConfirmedAppointments));
            OnPropertyChanged(nameof(TotalFoodEntries));
            OnPropertyChanged(nameof(TotalFoodAmount));
            OnPropertyChanged(nameof(MostFrequentFoodType));
            OnPropertyChanged(nameof(TotalFoodAmount));
            OnPropertyChanged(nameof(MostFrequentFoodType));
        }

        private (DateTime startDate, DateTime endDate) GetDateRange()
        {
            var now = DateTime.Now;
            var endDate = now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            return SelectedDateRangeFilter switch
            {
                "Hoy" => (now.Date, endDate),
                "Últimos 7 días" => (now.Date.AddDays(-7), endDate),
                "Últimos 30 días" => (now.Date.AddDays(-30), endDate),
                "Este mes" => (new DateTime(now.Year, now.Month, 1), endDate),
                "Mes anterior" => (new DateTime(now.Year, now.Month, 1).AddMonths(-1), new DateTime(now.Year, now.Month, 1).AddDays(-1).AddHours(23).AddMinutes(59)),
                "Rango personalizado" when _customStartDate.HasValue && _customEndDate.HasValue => (_customStartDate.Value, _customEndDate.Value.AddHours(23).AddMinutes(59)),
                _ => (DateTime.MinValue, DateTime.MaxValue) // Todo el historial
            };
        }

        private async Task ShowCustomDateRangePicker()
        {
            // Solicitar fecha de inicio
            var startDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Fecha de inicio",
                "Ingrese fecha de inicio (dd/MM/yyyy):",
                placeholder: DateTime.Today.AddDays(-7).ToString("dd/MM/yyyy"))!;

            if (string.IsNullOrEmpty(startDateStr))
            {
                // Usuario canceló - volver a la opción anterior
                _selectedDateRangeFilter = "Todo el historial";
                OnPropertyChanged(nameof(SelectedDateRangeFilter));
                return;
            }

            // Solicitar fecha de fin
            var endDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Fecha de fin",
                "Ingrese fecha de fin (dd/MM/yyyy):",
                placeholder: DateTime.Today.ToString("dd/MM/yyyy"))!;

            if (string.IsNullOrEmpty(endDateStr))
            {
                // Usuario canceló - volver a la opción anterior
                _selectedDateRangeFilter = "Todo el historial";
                OnPropertyChanged(nameof(SelectedDateRangeFilter));
                return;
            }

            // Validar y parsear las fechas
            if (DateTime.TryParseExact(startDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime start) &&
                DateTime.TryParseExact(endDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime end))
            {
                if (end < start)
                {
                    await Application.Current?.MainPage?.DisplayAlert("Error", "La fecha de fin no puede ser anterior a la fecha de inicio", "OK")!;
                    _selectedDateRangeFilter = "Todo el historial";
                    OnPropertyChanged(nameof(SelectedDateRangeFilter));
                    return;
                }

                _customStartDate = start;
                _customEndDate = end;
                ApplyFilters();
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Formato de fecha inválido. Use dd/MM/yyyy", "OK")!;
                _selectedDateRangeFilter = "Todo el historial";
                OnPropertyChanged(nameof(SelectedDateRangeFilter));
            }
        }

        private async void DeleteMedicationHistory(MedicationHistory history)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {history.MedicationName} administrado el {history.FormattedDate}?",
                "Sí", "No")!;

            if (confirm)
            {
                // Guardar el medicationId antes de borrar
                int medicationId = history.MedicationId;
                
                await _dataService.DeleteMedicationHistoryAsync(history);
                _allMedicationHistory.Remove(history);
                
                // 🔄 Recalcular las dosis pendientes después de borrar del historial
                System.Diagnostics.Debug.WriteLine($"🔄 Recalculando dosis pendientes tras borrar historial de medicamento {medicationId}...");
                await _dataService.RecalculateNextDosesFromLastConfirmedAsync(medicationId, 7); // 7 días por defecto
                _dataService.RebuildCombinedEvents();
                
                ApplyFilters();
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Registro eliminado del historial y dosis recalculadas", "OK")!;
            }
        }

        private async void DeleteFoodHistory(FoodEntry food)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {food.FoodType} del historial?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteFoodEntryAsync(food);
                _allFoodHistory.Remove(food);
                UpdateAvailableFilters();
                ApplyFilters();
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Registro de alimento eliminado", "OK")!;
            }
        }

        private async void EditMedicationHistory(MedicationHistory medicationHistory)
        {
            // Solo permitir editar la hora de administración
            var newTimeStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Hora",
                $"Hora de administración de {medicationHistory.MedicationName} (formato 12h, ej: 09:30 AM o 02:45 PM):",
                initialValue: medicationHistory.AdministeredTime.ToString("hh:mm tt"))!;

            if (string.IsNullOrWhiteSpace(newTimeStr)) return;

            // Intentar parsear la hora
            DateTime newTime;
            if (DateTime.TryParse(newTimeStr, out var parsedTime))
            {
                newTime = medicationHistory.AdministeredTime.Date + parsedTime.TimeOfDay;
            }
            else
            {
                await Application.Current?.MainPage?.DisplayAlert("❌ Error", "Formato de hora inválido. Use formato 12h con AM/PM", "OK")!;
                return;
            }

            medicationHistory.AdministeredTime = newTime;
            await _dataService.UpdateMedicationHistoryAsync(medicationHistory);
            
            // 🔄 CRÍTICO: Recalcular las siguientes dosis desde esta dosis editada
            // Esto asegura que si cambias la hora de una dosis pasada, las futuras se ajusten
            System.Diagnostics.Debug.WriteLine($"🔄 Recalculando dosis futuras después de editar {medicationHistory.MedicationName}...");
            await _dataService.RecalculateNextDosesFromLastConfirmedAsync(medicationHistory.MedicationId, 3); // Usar 3 días por defecto
            
            // Actualizar la lista local y los filtros
            UpdateAvailableFilters();
            ApplyFilters();
            
            await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Hora actualizada y dosis futuras recalculadas", "OK")!;
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

            if (double.TryParse(newAmountStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double newAmount))
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
                
                // Actualizar la lista local y los filtros
                UpdateAvailableFilters();
                ApplyFilters();
                
                await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Alimento actualizado (incluye nueva hora)", "OK")!;
            }
        }

        private async void DeleteAppointmentHistory(MedicalAppointment appointment)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar la cita '{appointment.Title}' del historial?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteAppointmentAsync(appointment);
                _allAppointments.Remove(appointment);
                ApplyFilters();
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Cita eliminada del historial", "OK")!;
            }
        }

        private async void EditAppointment(MedicalAppointment appointment)
        {
            // Prompt para editar título
            var newTitle = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Título",
                "Título de la cita:",
                initialValue: appointment.Title)!;

            if (string.IsNullOrWhiteSpace(newTitle)) return;

            // Prompt para editar notas
            var newNotes = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Notas",
                "Notas adicionales:",
                initialValue: appointment.Notes ?? "")!;

            // Prompt para editar fecha de la cita
            var newDateStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Fecha de Cita",
                "Fecha de la cita (formato dd/MM/yyyy, ej: 25/12/2024):",
                initialValue: appointment.AppointmentDate.ToString("dd/MM/yyyy"))!;

            if (string.IsNullOrWhiteSpace(newDateStr)) return;

            // Prompt para editar hora de la cita
            var newTimeStr = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Editar Hora de Cita",
                "Hora de la cita (formato 12h, ej: 09:30 AM o 02:45 PM):",
                initialValue: appointment.AppointmentDate.ToString("hh:mm tt"))!;

            if (string.IsNullOrWhiteSpace(newTimeStr)) return;

            // Parsear fecha
            if (!DateTime.TryParseExact(newDateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newDate))
            {
                await Application.Current?.MainPage?.DisplayAlert("❌ Error", "Formato de fecha inválido. Use dd/MM/yyyy", "OK")!;
                return;
            }

            // Parsear hora
            if (!DateTime.TryParse(newTimeStr, out var parsedTime))
            {
                await Application.Current?.MainPage?.DisplayAlert("❌ Error", "Formato de hora inválido. Use formato 12h con AM/PM", "OK")!;
                return;
            }

            // Combinar fecha y hora
            DateTime newAppointmentDate = newDate.Date + parsedTime.TimeOfDay;

            appointment.Title = newTitle;
            appointment.Notes = newNotes;
            appointment.AppointmentDate = newAppointmentDate;
            
            await _dataService.UpdateAppointmentAsync(appointment);
            
            // Recargar la lista desde la base de datos para reflejar cambios
            var allAppointments = await _dataService.GetAllAppointmentsAsync();
            _allAppointments = allAppointments.Where(a => a.IsConfirmed).ToList();
            
            ApplyFilters();
            
            await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Cita médica actualizada correctamente", "OK")!;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
