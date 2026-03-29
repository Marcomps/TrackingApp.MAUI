using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class CitasListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public CitasListViewModel()
    {
        AgregarCommand   = new Command(async () => await NavigateToFormAsync(null));
        EditarCommand    = new Command<MedicalAppointment>(async c => await NavigateToFormAsync(c));
        EliminarCommand  = new Command<MedicalAppointment>(async c => await EliminarAsync(c));
        ConfirmarCommand = new Command<MedicalAppointment>(async c => await ConfirmarAsync(c));
        RefrescarCommand = new Command(Cargar);
        FiltrarCommand   = new Command<string>(SetFiltro);

        Cargar();
    }

    // ── Colecciones ───────────────────────────────────────────────────────────

    private readonly List<MedicalAppointment> _todasLasCitas = new();
    public ObservableCollection<MedicalAppointment> Citas { get; } = new();

    // ── Filtro ────────────────────────────────────────────────────────────────

    private string _filtroEstado = "Todas";
    public string FiltroEstado
    {
        get => _filtroEstado;
        private set
        {
            _filtroEstado = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FiltroTodas));
            OnPropertyChanged(nameof(FiltroPendientes));
            OnPropertyChanged(nameof(FiltroCompletadas));
            AplicarFiltro();
        }
    }

    public bool FiltroTodas       => _filtroEstado == "Todas";
    public bool FiltroPendientes  => _filtroEstado == "Pendientes";
    public bool FiltroCompletadas => _filtroEstado == "Completadas";

    // ── Properties ───────────────────────────────────────────────────────────

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    private bool _hayCitas;
    public bool HayCitas
    {
        get => _hayCitas;
        private set { _hayCitas = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoHayCitas)); }
    }
    public bool NoHayCitas => !_hayCitas;

    // Resumen
    public int CitasPendientesCount  => _todasLasCitas.Count(c => c.Estado == EstadoCita.Pendiente);
    public int CitasHoyCount         => _todasLasCitas.Count(c => c.IsToday);

    private MedicalAppointment? _proximaCita;
    public MedicalAppointment? ProximaCita
    {
        get => _proximaCita;
        private set { _proximaCita = value; OnPropertyChanged(); OnPropertyChanged(nameof(HayProximaCita)); }
    }
    public bool HayProximaCita => _proximaCita != null;

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand AgregarCommand   { get; }
    public ICommand EditarCommand    { get; }
    public ICommand EliminarCommand  { get; }
    public ICommand ConfirmarCommand { get; }
    public ICommand RefrescarCommand { get; }
    public ICommand FiltrarCommand   { get; }

    // ── Lógica ────────────────────────────────────────────────────────────────

    public void Cargar()
    {
        IsBusy = true;
        try
        {
            var fuente = AppServices.DataService.Appointments;

            _todasLasCitas.Clear();
            foreach (var c in fuente.OrderBy(c => c.AppointmentDate))
                _todasLasCitas.Add(c);

            ProximaCita = _todasLasCitas
                .FirstOrDefault(c => c.Estado == EstadoCita.Pendiente && c.AppointmentDate >= DateTime.Now);

            OnPropertyChanged(nameof(CitasPendientesCount));
            OnPropertyChanged(nameof(CitasHoyCount));

            AplicarFiltro();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetFiltro(string filtro)
    {
        FiltroEstado = filtro;
    }

    private void AplicarFiltro()
    {
        Citas.Clear();
        var lista = _filtroEstado switch
        {
            "Pendientes"  => _todasLasCitas.Where(c => c.Estado == EstadoCita.Pendiente),
            "Completadas" => _todasLasCitas.Where(c => c.Estado == EstadoCita.Completada),
            _             => _todasLasCitas.AsEnumerable()
        };

        foreach (var c in lista.OrderBy(c => c.AppointmentDate))
            Citas.Add(c);

        HayCitas = Citas.Count > 0;
    }

    private static async Task NavigateToFormAsync(MedicalAppointment? cita)
    {
        var param = cita != null ? $"?id={cita.Id}" : string.Empty;
        await Shell.Current.GoToAsync($"{nameof(CitaFormPage)}{param}");
    }

    private async Task EliminarAsync(MedicalAppointment cita)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar cita",
            $"¿Eliminar '{cita.Title}'?",
            "Eliminar", "Cancelar");

        if (!confirmar) return;

        await AppServices.DataService.DeleteAppointmentAsync(cita);
        Cargar();
    }

    private async Task ConfirmarAsync(MedicalAppointment cita)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Confirmar cita",
            $"¿Marcar '{cita.Title}' como completada?",
            "Sí", "No");

        if (!confirmar) return;

        await AppServices.DataService.ConfirmAppointmentAsync(cita);
        Cargar();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
