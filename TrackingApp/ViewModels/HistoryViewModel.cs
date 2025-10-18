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

        public ObservableCollection<MedicationHistory> MedicationHistory { get; set; }

        public int TotalConfirmedDoses => MedicationHistory?.Count ?? 0;

        public ICommand DeleteHistoryCommand { get; }

        public HistoryViewModel()
        {
            _dataService = DataService.Instance;
            MedicationHistory = new ObservableCollection<MedicationHistory>();

            DeleteHistoryCommand = new Command<MedicationHistory>(DeleteHistory);

            LoadHistoryAsync();
        }

        private async void LoadHistoryAsync()
        {
            var history = await _dataService.GetAllMedicationHistoryAsync();
            
            MedicationHistory.Clear();
            foreach (var item in history.OrderByDescending(h => h.AdministeredTime))
            {
                MedicationHistory.Add(item);
            }

            OnPropertyChanged(nameof(TotalConfirmedDoses));
        }

        private async void DeleteHistory(MedicationHistory history)
        {
            bool confirm = await Application.Current?.MainPage?.DisplayAlert(
                "Confirmar",
                $"¿Eliminar registro de {history.MedicationName} administrado el {history.FormattedDate}?",
                "Sí", "No")!;

            if (confirm)
            {
                await _dataService.DeleteMedicationHistoryAsync(history);
                MedicationHistory.Remove(history);
                OnPropertyChanged(nameof(TotalConfirmedDoses));
                await Application.Current?.MainPage?.DisplayAlert("Eliminado", "Registro eliminado del historial", "OK")!;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
