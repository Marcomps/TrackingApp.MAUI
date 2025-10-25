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

        public ObservableCollection<MedicationHistory> AllMedicationHistory
        {
            get => _allMedicationHistory;
            set
            {
                _allMedicationHistory = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FoodEntry> AllFoodHistory
        {
            get => _allFoodHistory;
            set
            {
                _allFoodHistory = value;
                OnPropertyChanged();
            }
        }

        public string SelectedMedicationFilter
        {
            get => _selectedMedicationFilter;
            set
            {
                _selectedMedicationFilter = value;
                OnPropertyChanged();
                FilterMedicationHistory();
            }
        }

        public string SelectedFoodTypeFilter
        {
            get => _selectedFoodTypeFilter;
            set
            {
                _selectedFoodTypeFilter = value;
                OnPropertyChanged();
                FilterFoodHistory();
            }
        }

        public ObservableCollection<MedicationHistory> FilteredMedicationHistory { get; set; }
        public ObservableCollection<FoodEntry> FilteredFoodHistory { get; set; }

        public List<string> MedicationFilterOptions { get; set; }
        public List<string> FoodTypeFilterOptions { get; set; }

        public ICommand DeleteMedicationHistoryCommand { get; set; }
        public ICommand DeleteFoodHistoryCommand { get; set; }
        public ICommand EditMedicationHistoryCommand { get; set; }
        public ICommand EditFoodCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public string FormattedTime { get; set; }
        public string MedicationName { get; set; }
        public string Dose { get; set; }
        public string FormattedDate { get; set; }
        public string UserType { get; set; }
        public string FoodType { get; set; }
        public string DisplayAmount { get; set; }

        public HistoryViewModel()
        {
            _dataService = DataService.Instance;

            AllMedicationHistory = _dataService.MedicationHistory;
            AllFoodHistory = _dataService.FoodEntries;
            FilteredMedicationHistory = new ObservableCollection<MedicationHistory>();
            FilteredFoodHistory = new ObservableCollection<FoodEntry>();

            MedicationFilterOptions = new List<string>
            {
                "Todos",
                "Últimos 7 días",
                "Últimos 30 días",
                "Este mes",
                "Mes anterior"
            };

            FoodTypeFilterOptions = new List<string>
            {
                "Todos",
                "Desayuno",
                "Almuerzo",
                "Cena",
                "Merienda"
            };

            DeleteMedicationHistoryCommand = new Command<MedicationHistory>(async (history) => await DeleteMedicationHistory(history));
            DeleteFoodHistoryCommand = new Command<FoodEntry>(async (food) => await DeleteFoodHistory(food));
            EditMedicationHistoryCommand = new Command<MedicationHistory>(EditMedicationHistory);
            EditFoodCommand = new Command<FoodEntry>(EditFood);
            RefreshCommand = new Command(async () => await LoadHistoryAsync());

            FormattedTime = string.Empty;
            MedicationName = string.Empty;
            Dose = string.Empty;
            FormattedDate = string.Empty;
            UserType = string.Empty;
            FoodType = string.Empty;
            DisplayAmount = string.Empty;

            _ = LoadHistoryAsync(); // Ensure the call is awaited
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                FilterMedicationHistory();
                FilterFoodHistory();
            }
            catch (Exception ex)
            {
                await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayAlert("Error", $"Error loading history: {ex.Message}", "OK");
            }
        }

        private void FilterMedicationHistory()
        {
            var filtered = AllMedicationHistory.AsEnumerable();

            if (SelectedMedicationFilter != "Todos")
            {
                filtered = SelectedMedicationFilter switch
                {
                    "Últimos 7 días" => filtered.Where(m => m.AdministeredTime >= DateTime.Now.AddDays(-7)),
                    "Últimos 30 días" => filtered.Where(m => m.AdministeredTime >= DateTime.Now.AddDays(-30)),
                    "Este mes" => filtered.Where(m => m.AdministeredTime.Month == DateTime.Now.Month && m.AdministeredTime.Year == DateTime.Now.Year),
                    "Mes anterior" => filtered.Where(m => m.AdministeredTime.Month == DateTime.Now.AddMonths(-1).Month && m.AdministeredTime.Year == DateTime.Now.AddMonths(-1).Year),
                    _ => filtered
                };
            }

            FilteredMedicationHistory.Clear();
            foreach (var item in filtered.OrderByDescending(m => m.AdministeredTime))
            {
                FilteredMedicationHistory.Add(item);
            }
        }

        private void FilterFoodHistory()
        {
            var filtered = AllFoodHistory.AsEnumerable();

            if (SelectedFoodTypeFilter != "Todos")
            {
                filtered = filtered.Where(f => f.FoodType == SelectedFoodTypeFilter);
            }

            FilteredFoodHistory.Clear();
            foreach (var item in filtered.OrderByDescending(f => f.Time))
            {
                FilteredFoodHistory.Add(item);
            }
        }

        private async Task DeleteMedicationHistory(MedicationHistory history)
        {
            bool confirm = await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {history.MedicationName} del historial?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteMedicationHistoryAsync(history);
                await LoadHistoryAsync();
            }
        }

        private async Task DeleteFoodHistory(FoodEntry food)
        {
            bool confirm = await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {food.FoodType} del historial?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteFoodEntryAsync(food);
                await LoadHistoryAsync();
            }
        }

        private async void EditMedicationHistory(MedicationHistory medicationHistory)
        {
            // Usar TimePicker visual como en AddFood
            var timePicker = new TrackingApp.Views.TimePickerPopup(medicationHistory.AdministeredTime.TimeOfDay);
            await Application.Current?.Windows?.FirstOrDefault()?.Page?.Navigation.PushModalAsync(timePicker)!;
            
            // Esperar a que se cierre el popup
            await Task.Run(async () =>
            {
                while (Application.Current?.Windows?.FirstOrDefault()?.Page?.Navigation.ModalStack.Count > 0)
                {
                    await Task.Delay(100);
                }
            });

            // Actualizar la hora
            var newTime = timePicker.SelectedTime;
            if (newTime.HasValue)
            {
                medicationHistory.AdministeredTime = medicationHistory.AdministeredTime.Date.Add(newTime.Value);
                await _dataService.UpdateMedicationHistoryAsync(medicationHistory);

                // Recalcular próximas dosis
                await _dataService.RecalculateNextDosesFromLastConfirmedAsync(medicationHistory.MedicationId, 3);

                await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayAlert("✅ Actualizado", $"Hora actualizada a {newTime:hh:mm tt} y dosis futuras recalculadas", "OK")!;
                await LoadHistoryAsync();
            }
        }

        private async void EditFood(FoodEntry foodEntry)
        {
            // Editar tipo de comida
            var newType = await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayPromptAsync(
                "Editar Tipo de Comida",
                "Nuevo tipo:",
                initialValue: foodEntry.FoodType,
                keyboard: Keyboard.Text)!;

            if (!string.IsNullOrWhiteSpace(newType))
            {
                foodEntry.FoodType = newType;
            }

            // Editar cantidad
            var newAmountStr = await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayPromptAsync(
                "Editar Cantidad",
                "Nueva cantidad:",
                initialValue: foodEntry.Amount.ToString(),
                keyboard: Keyboard.Numeric)!;

            if (!string.IsNullOrWhiteSpace(newAmountStr) && double.TryParse(newAmountStr, out double newAmount))
            {
                foodEntry.Amount = newAmount;
            }

            // Editar hora
            var newTimeStr = await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayPromptAsync(
                "Editar Hora",
                "Nueva hora (formato 12h con AM/PM, ej: 02:30 PM):",
                initialValue: foodEntry.Time.ToString("hh:mm tt"),
                keyboard: Keyboard.Text)!;

            if (!string.IsNullOrWhiteSpace(newTimeStr))
            {
                if (DateTime.TryParse(newTimeStr, out DateTime newTime))
                {
                    foodEntry.Time = newTime;
                }
                else
                {
                    await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayAlert("❌ Error", "Formato de hora inválido. Use formato 12h con AM/PM", "OK")!;
                    return;
                }
            }

            await _dataService.UpdateFoodEntryAsync(foodEntry);
            await Application.Current?.Windows?.FirstOrDefault()?.Page?.DisplayAlert("✅ Actualizado", "Alimento actualizado (incluye nueva hora)", "OK")!;
            await LoadHistoryAsync();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
