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
        ToggleCalendarCommand = new Command(() => MostrarCalendario = !MostrarCalendario);
        MesAnteriorCommand   = new Command(IrMesAnterior);
        MesSiguienteCommand  = new Command(IrMesSiguiente);
        SelectDayCommand     = new Command<CalendarDayItem>(SeleccionarDia);

        _calendarMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        BuildCalendarDays();
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
            // Clamp: never allow future dates
            var target = value.Date > DateTime.Today ? DateTime.Today : value.Date;
            if (_fechaFiltro == target) return;
            _fechaFiltro = target;
            // Sync calendar month view to the selected date
            _calendarMonth = new DateTime(_fechaFiltro.Year, _fechaFiltro.Month, 1);
            OnPropertyChanged();
            OnPropertyChanged(nameof(FechaFiltroDisplay));
            OnPropertyChanged(nameof(EsHoy));
            OnPropertyChanged(nameof(CalendarMonthDisplay));
            OnPropertyChanged(nameof(PuedeMesSiguiente));
            BuildCalendarDays();
            Cargar();
        }
    }
    public string FechaFiltroDisplay => _fechaFiltro.Date == DateTime.Today
        ? "Hoy"
        : _fechaFiltro.ToString("ddd d MMM", System.Globalization.CultureInfo.CurrentCulture);
    public bool EsHoy => _fechaFiltro.Date == DateTime.Today;

    // ── Calendario inline ─────────────────────────────────────────────────────

    private DateTime _calendarMonth;

    private bool _mostrarCalendario;
    public bool MostrarCalendario
    {
        get => _mostrarCalendario;
        set { _mostrarCalendario = value; OnPropertyChanged(); }
    }

    public string CalendarMonthDisplay =>
        _calendarMonth.ToString("MMMM yyyy", System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>False when the calendar is already showing the current month (can't go further into the future).</summary>
    public bool PuedeMesSiguiente =>
        _calendarMonth < new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    public ObservableCollection<CalendarDayItem> CalendarDays { get; } = new();

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

    public ICommand AgregarCommand        { get; }
    public ICommand EditarCommand         { get; }
    public ICommand EliminarCommand       { get; }
    public ICommand RefrescarCommand      { get; }
    public ICommand DiaPrevioCommand      { get; }
    public ICommand DiaSiguienteCommand   { get; }
    public ICommand IrHoyCommand          { get; }
    public ICommand ToggleCalendarCommand { get; }
    public ICommand MesAnteriorCommand    { get; }
    public ICommand MesSiguienteCommand   { get; }
    public ICommand SelectDayCommand      { get; }

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

    // ── Calendar helpers ──────────────────────────────────────────────────────

    private void IrMesAnterior()
    {
        _calendarMonth = _calendarMonth.AddMonths(-1);
        OnPropertyChanged(nameof(CalendarMonthDisplay));
        OnPropertyChanged(nameof(PuedeMesSiguiente));
        BuildCalendarDays();
    }

    private void IrMesSiguiente()
    {
        if (!PuedeMesSiguiente) return;
        _calendarMonth = _calendarMonth.AddMonths(1);
        OnPropertyChanged(nameof(CalendarMonthDisplay));
        OnPropertyChanged(nameof(PuedeMesSiguiente));
        BuildCalendarDays();
    }

    private void SeleccionarDia(CalendarDayItem item)
    {
        if (item.IsEmpty || item.IsFuture || !item.Date.HasValue) return;
        FechaFiltro = item.Date.Value;
        MostrarCalendario = false;
    }

    private void BuildCalendarDays()
    {
        CalendarDays.Clear();

        var firstDay = _calendarMonth;
        var lastDay  = _calendarMonth.AddMonths(1).AddDays(-1);

        // Monday-first offset: Mon=0 … Sun=6
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7;
        for (int i = 0; i < startOffset; i++)
            CalendarDays.Add(new CalendarDayItem { Label = "" });

        for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
            CalendarDays.Add(new CalendarDayItem
            {
                Date           = d,
                Label          = d.Day.ToString(),
                IsCurrentMonth = true,
                IsSelected     = d.Date == _fechaFiltro.Date,
            });

        // Pad to complete last row (multiple of 7)
        int remaining = (7 - CalendarDays.Count % 7) % 7;
        for (int i = 0; i < remaining; i++)
            CalendarDays.Add(new CalendarDayItem { Label = "" });
    }
}
