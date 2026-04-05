using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class AlimentoListViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public AlimentoListViewModel()
    {
        AgregarCommand       = new Command(async () => await NavigateToFormAsync(null));
        EditarCommand        = new Command<FoodEntry>(async e => await NavigateToFormAsync(e));
        EliminarCommand      = new Command<FoodEntry>(async e => await EliminarAsync(e));
        RefrescarCommand     = new Command(Cargar);
        DiaPrevioCommand     = new Command(() => FechaFiltro = FechaFiltro.AddDays(-1));
        DiaSiguienteCommand  = new Command(() => FechaFiltro = FechaFiltro.AddDays(1));
        IrHoyCommand         = new Command(() => FechaFiltro = DateTime.Today);

        Cargar();
    }

    // ── Colecciones ───────────────────────────────────────────────────────────

    public ObservableCollection<FoodEntry> Registros { get; } = new();

    // ── Properties ───────────────────────────────────────────────────────────

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    private bool _hayRegistros;
    public bool HayRegistros
    {
        get => _hayRegistros;
        private set { _hayRegistros = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoHayRegistros)); }
    }
    public bool NoHayRegistros => !_hayRegistros;

    // Filtro de fecha
    private DateTime _fechaFiltro = DateTime.Today;
    public DateTime FechaFiltro
    {
        get => _fechaFiltro;
        set
        {
            if (_fechaFiltro == value.Date) return;
            _fechaFiltro = value.Date;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FechaFiltroDisplay));
            OnPropertyChanged(nameof(EsHoy));
            Cargar();
        }
    }
    public string FechaFiltroDisplay => _fechaFiltro.Date == DateTime.Today
        ? "Hoy"
        : _fechaFiltro.ToString("ddd d MMM", System.Globalization.CultureInfo.CurrentCulture);
    public bool EsHoy => _fechaFiltro.Date == DateTime.Today;

    // Bloquea selección de fechas futuras en el DatePicker
    public DateTime FechaMaxima => DateTime.Today;

    // Último registro (resumen en header)
    private FoodEntry? _ultimo;
    public FoodEntry? UltimoRegistro
    {
        get => _ultimo;
        private set
        {
            _ultimo = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UltimoTipoDisplay));
            OnPropertyChanged(nameof(UltimaCantidadDisplay));
            OnPropertyChanged(nameof(UltimaHoraDisplay));
            OnPropertyChanged(nameof(UltimaFechaDisplay));
        }
    }

    public string UltimoTipoDisplay    => _ultimo?.TipoAlimentacionDisplay ?? "—";
    public string UltimaCantidadDisplay => _ultimo != null ? BuildCantidadDisplay(_ultimo) : "—";
    public string UltimaHoraDisplay    => _ultimo?.FormattedTime ?? "—";
    public string UltimaFechaDisplay   => _ultimo?.FormattedDate ?? "—";

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand AgregarCommand       { get; }
    public ICommand EditarCommand        { get; }
    public ICommand EliminarCommand      { get; }
    public ICommand RefrescarCommand     { get; }
    public ICommand DiaPrevioCommand     { get; }
    public ICommand DiaSiguienteCommand  { get; }
    public ICommand IrHoyCommand         { get; }

    // ── Lógica ────────────────────────────────────────────────────────────────

    public void Cargar()
    {
        IsBusy = true;
        try
        {
            var fuente = AppServices.DataService.FoodEntries;
            var filtro = _fechaFiltro.Date;

            Registros.Clear();
            foreach (var e in fuente
                .Where(e => e.Time.Date == filtro)
                .OrderByDescending(e => e.Time))
                Registros.Add(e);

            UltimoRegistro = AppServices.DataService.FoodEntries
                .OrderByDescending(e => e.Time).FirstOrDefault();
            HayRegistros   = Registros.Count > 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task NavigateToFormAsync(FoodEntry? entrada)
    {
        var param = entrada != null ? $"?id={entrada.Id}" : string.Empty;
        await Shell.Current.GoToAsync($"{nameof(AlimentoFormPage)}{param}");
    }

    private async Task EliminarAsync(FoodEntry entrada)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar registro",
            $"¿Eliminar el registro de {entrada.TipoAlimentacionDisplay} del {entrada.FormattedDate}?",
            "Eliminar", "Cancelar");

        if (!confirmar) return;

        await AppServices.DataService.DeleteFoodEntryAsync(entrada);
        Cargar();
    }

    private static string BuildCantidadDisplay(FoodEntry e)
    {
        if (e.TipoAlimentacion == TipoAlimentacion.Lactancia)
            return e.DuracionMinutos.HasValue ? $"{e.DuracionMinutos} min" : "—";
        if (e.CantidadMl.HasValue)
            return $"{e.CantidadMl:F0} ml";
        if (e.CantidadGramos.HasValue)
            return $"{e.CantidadGramos:F0} g";
        return e.Amount > 0 ? $"{e.Amount} {e.Unit}" : "—";
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
