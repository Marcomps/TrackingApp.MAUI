using System.Collections.ObjectModel;
using System.ComponentModel;
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
        
        private string _selectedMedicationFilter = "Todos";
        private string _selectedFoodTypeFilter = "Todos";
        private string _selectedUnitFilter = "Todos";
        private string _selectedProfileFilter = "Todos";
        private string _selectedDateRangeFilter = "Todo el historial";
        private DateTime? _customStartDate;
        private DateTime? _customEndDate;

        public ObservableCollection<MedicationHistory> FilteredMedicationHistory { get; set; }
        public ObservableCollection<FoodEntry> FilteredFoodHistory { get; set; }
        public ObservableCollection<string> MedicationNames { get; set; }
        public ObservableCollection<string> FoodTypes { get; set; }
        public ObservableCollection<string> Units { get; set; }
        public ObservableCollection<string> Profiles { get; set; }
        public ObservableCollection<string> DateRangeOptions { get; set; }

        public int TotalConfirmedDoses => FilteredMedicationHistory?.Count ?? 0;
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
                ApplyFilters();
            }
        }

        public ICommand DeleteMedicationHistoryCommand { get; }
        public ICommand DeleteFoodHistoryCommand { get; }
        public ICommand EditFoodCommand { get; }
        public ICommand RefreshCommand { get; }

        public HistoryViewModel()
        {
            _dataService = DataService.Instance;
            
            _allMedicationHistory = new ObservableCollection<MedicationHistory>();
            _allFoodHistory = new ObservableCollection<FoodEntry>();
            FilteredMedicationHistory = new ObservableCollection<MedicationHistory>();
            FilteredFoodHistory = new ObservableCollection<FoodEntry>();
            
            MedicationNames = new ObservableCollection<string> { "Todos" };
            FoodTypes = new ObservableCollection<string> { "Todos" };
            Units = new ObservableCollection<string> { "Todos", "g", "ml", "unidades", "cucharadas", "tazas" };
            Profiles = new ObservableCollection<string> { "Todos", "Adulto", "Niño" };
            DateRangeOptions = new ObservableCollection<string> 
            { 
                "Todo el historial",
                "Hoy", 
                "Últimos 7 días", 
                "Últimos 30 días",
                "Este mes",
                "Mes anterior"
            };

            DeleteMedicationHistoryCommand = new Command<MedicationHistory>(DeleteMedicationHistory);
            DeleteFoodHistoryCommand = new Command<FoodEntry>(DeleteFoodHistory);
            EditFoodCommand = new Command<FoodEntry>(EditFood);
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
                filteredFoods = filteredFoods.Where(f => f.Unit == SelectedUnitFilter);
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

            // Actualizar estadísticas
            OnPropertyChanged(nameof(TotalConfirmedDoses));
            OnPropertyChanged(nameof(TotalFoodEntries));
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

        private async void DeleteMedicationHistory(MedicationHistory history)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {history.MedicationName} administrado el {history.FormattedDate}?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteMedicationHistoryAsync(history);
                _allMedicationHistory.Remove(history);
                ApplyFilters();
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Registro eliminado del historial", "OK")!;
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
                
                // Actualizar la lista local y los filtros
                UpdateAvailableFilters();
                ApplyFilters();
                
                await Application.Current?.MainPage?.DisplayAlert("✅ Actualizado", "Alimento actualizado (incluye nueva hora)", "OK")!;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
