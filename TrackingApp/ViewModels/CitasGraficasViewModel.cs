using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class CitasGraficasViewModel : INotifyPropertyChanged
{
    private static readonly System.Globalization.CultureInfo _culturaEs = new("es-ES");

    private int _dias = 0; // 0 = Todo por defecto en citas
    private bool _rangoPersonalizado;
    private DateTime _fechaDesde = DateTime.Today.AddMonths(-6);
    private DateTime _fechaHasta = DateTime.Today;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CitasGraficasViewModel()
    {
        SeleccionarPeriodoCommand = new Command<string>(param =>
        {
            if (int.TryParse(param, out int dias))
            {
                _dias = dias;
                _rangoPersonalizado = false;
                _fechaHasta = DateTime.Today;
                _fechaDesde = dias > 0 ? DateTime.Today.AddDays(-(dias - 1)) : DateTime.MinValue;
                RefreshPeriodButtonColors();
                OnPropertyChanged(nameof(FechaDesde));
                OnPropertyChanged(nameof(FechaHasta));
                _ = CargarGraficasAsync();
            }
        });
        RefrescarCommand = new Command(() => _ = CargarGraficasAsync());
        ActivarRangoPersonalizadoCommand = new Command(() =>
        {
            _rangoPersonalizado = true;
            RefreshPeriodButtonColors();
            _ = CargarGraficasAsync();
        });
    }

    // ── Período ───────────────────────────────────────────────────────────────

    public int PeriodoSeleccionado => _dias;

    private void RefreshPeriodButtonColors()
    {
        OnPropertyChanged(nameof(PeriodoSeleccionado));
        OnPropertyChanged(nameof(Boton3MesesBg));
        OnPropertyChanged(nameof(Boton6MesesBg));
        OnPropertyChanged(nameof(Boton1AnoBg));
        OnPropertyChanged(nameof(BotonTodoBg));
        OnPropertyChanged(nameof(BotonPersonalizadoBg));
        OnPropertyChanged(nameof(Texto3Meses));
        OnPropertyChanged(nameof(Texto6Meses));
        OnPropertyChanged(nameof(Texto1Ano));
        OnPropertyChanged(nameof(TextoTodo));
        OnPropertyChanged(nameof(TextoPersonalizado));
    }

    public Color Boton3MesesBg        => !_rangoPersonalizado && _dias == 90  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton6MesesBg        => !_rangoPersonalizado && _dias == 180 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton1AnoBg          => !_rangoPersonalizado && _dias == 365 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonTodoBg          => !_rangoPersonalizado && _dias == 0   ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonPersonalizadoBg => _rangoPersonalizado                  ? Color.FromArgb("#1565C0") : Color.FromArgb("#e0e0e0");

    public Color Texto3Meses         => !_rangoPersonalizado && _dias == 90  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto6Meses         => !_rangoPersonalizado && _dias == 180 ? Colors.White : Color.FromArgb("#555555");
    public Color Texto1Ano           => !_rangoPersonalizado && _dias == 365 ? Colors.White : Color.FromArgb("#555555");
    public Color TextoTodo           => !_rangoPersonalizado && _dias == 0   ? Colors.White : Color.FromArgb("#555555");
    public Color TextoPersonalizado  => _rangoPersonalizado                  ? Colors.White : Color.FromArgb("#555555");

    public DateTime FechaDesde
    {
        get => _fechaDesde;
        set
        {
            if (_fechaDesde == value) return;
            _fechaDesde = value;
            _rangoPersonalizado = true;
            RefreshPeriodButtonColors();
            OnPropertyChanged();
            _ = CargarGraficasAsync();
        }
    }

    public DateTime FechaHasta
    {
        get => _fechaHasta;
        set
        {
            if (_fechaHasta == value) return;
            _fechaHasta = value;
            _rangoPersonalizado = true;
            RefreshPeriodButtonColors();
            OnPropertyChanged();
            _ = CargarGraficasAsync();
        }
    }

    // ── Series por mes ────────────────────────────────────────────────────────

    private ISeries[] _seriesPorMes = [];
    public ISeries[] SeriesPorMes
    {
        get => _seriesPorMes;
        private set { _seriesPorMes = value; OnPropertyChanged(); }
    }

    // ── Series por categoría ──────────────────────────────────────────────────

    private ISeries[] _seriesPorCategoria = [];
    public ISeries[] SeriesPorCategoria
    {
        get => _seriesPorCategoria;
        private set { _seriesPorCategoria = value; OnPropertyChanged(); }
    }

    private Axis[] _ejeXMes = [];
    public Axis[] EjeXMes
    {
        get => _ejeXMes;
        private set { _ejeXMes = value; OnPropertyChanged(); }
    }

    private Axis[] _ejeXCategoria = [];
    public Axis[] EjeXCategoria
    {
        get => _ejeXCategoria;
        private set { _ejeXCategoria = value; OnPropertyChanged(); }
    }

    public Axis[] EjeY =>
    [
        new Axis
        {
            MinLimit    = 0,
            TextSize    = 11,
            LabelsPaint = new SolidColorPaint(new SKColor(100, 100, 100)),
        }
    ];

    // ── Resumen ───────────────────────────────────────────────────────────────

    private string _resumen = "Sin datos";
    public string Resumen
    {
        get => _resumen;
        private set { _resumen = value; OnPropertyChanged(); }
    }

    private bool _hayDatos;
    public bool HayDatos
    {
        get => _hayDatos;
        private set { _hayDatos = value; OnPropertyChanged(); }
    }
    public bool NoHayDatos => !_hayDatos;

    // ── Comandos ──────────────────────────────────────────────────────────────

    public Command<string> SeleccionarPeriodoCommand { get; }
    public Command RefrescarCommand { get; }
    public Command ActivarRangoPersonalizadoCommand { get; }

    // ── Carga de datos ────────────────────────────────────────────────────────

    private bool _loading;

    private record CitasResult(
        ISeries[] SeriesPorMes,
        ISeries[] SeriesPorCategoria,
        Axis[]    EjeXMes,
        Axis[]    EjeXCategoria,
        string    Resumen,
        bool      HayDatos
    );

    public void CargarGraficas() => _ = CargarGraficasAsync();

    public async Task CargarGraficasAsync()
    {
        if (_loading) return;
        _loading = true;
        try
        {
            var perfilActivoId = AppServices.DataService.PerfilActivo?.Id ?? 0;
            var snapshot = AppServices.DataService.Appointments.Where(a => a.PerfilId == perfilActivoId).ToList();
            var r = await Task.Run(() => ComputeGraficas(snapshot));
            // Back on UI thread: apply all results
            SeriesPorMes       = r.SeriesPorMes;
            SeriesPorCategoria = r.SeriesPorCategoria;
            EjeXMes            = r.EjeXMes;
            EjeXCategoria      = r.EjeXCategoria;
            Resumen            = r.Resumen;
            HayDatos           = r.HayDatos;
            OnPropertyChanged(nameof(NoHayDatos));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading appointments chart: {ex.Message}");
        }
        finally { _loading = false; }
    }

    private CitasResult ComputeGraficas(List<MedicalAppointment> citas)
    {
        var hoy = DateTime.Today;

        DateTime desde, hasta;
        if (_rangoPersonalizado)
        {
            desde = _fechaDesde.Date;
            hasta = _fechaHasta.Date;
            if (desde > hasta) (desde, hasta) = (hasta, desde);
        }
        else if (_dias == 0) // Todo
        {
            desde = citas.Any() ? citas.Min(c => c.AppointmentDate.Date) : hoy.AddMonths(-6);
            hasta = citas.Any() ? citas.Max(c => c.AppointmentDate.Date) : hoy;
        }
        else
        {
            hasta = hoy;
            desde = hoy.AddDays(-(_dias - 1));
        }

        var filtradas = citas
            .Where(c => c.AppointmentDate.Date >= desde && c.AppointmentDate.Date <= hasta)
            .ToList();

        if (filtradas.Count == 0)
        {
            var ejeVacio = new Axis { Labels = [], TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)) };
            return new CitasResult(
                [new ColumnSeries<int> { Values = Array.Empty<int>(), Name = "Sin datos" }],
                [new ColumnSeries<int> { Values = Array.Empty<int>(), Name = "Sin datos" }],
                [ejeVacio], [ejeVacio],
                "Sin citas en este período.", false);
        }

        // ── Por mes ───────────────────────────────────────────────────────────
        var meses = new List<(DateTime Inicio, string Label)>();
        for (var m = new DateTime(desde.Year, desde.Month, 1); m <= hasta; m = m.AddMonths(1))
            meses.Add((m, m.ToString("MMM yy", _culturaEs)));

        var countsPorMes = meses
            .Select(m => filtradas.Count(c =>
                c.AppointmentDate.Year == m.Inicio.Year &&
                c.AppointmentDate.Month == m.Inicio.Month))
            .ToArray();

        ISeries[] seriesPorMes =
        [
            new ColumnSeries<int>
            {
                Values      = countsPorMes,
                Name        = "Citas",
                Fill        = new SolidColorPaint(new SKColor(42, 61, 102)),
                Stroke      = null,
                MaxBarWidth = 40,
            }
        ];

        Axis[] ejeXMes =
        [
            new Axis
            {
                Labels         = meses.Select(m => m.Label).ToArray(),
                LabelsRotation = meses.Count > 6 ? -45 : 0,
                TextSize       = 11,
                LabelsPaint    = new SolidColorPaint(new SKColor(80, 80, 80)),
                TicksAtCenter      = true,
                SeparatorsAtCenter = false,
            }
        ];

        // ── Por categoría ─────────────────────────────────────────────────────
        var categorias = Enum.GetValues<CategoriaCita>();
        var labels     = categorias
            .Select(cat => cat switch
            {
                CategoriaCita.ControlCrecimientoPeso => "Control",
                CategoriaCita.Vacunacion             => "Vacuna",
                CategoriaCita.Pediatria              => "Pediatría",
                CategoriaCita.Oftalmologia           => "Oftalmol.",
                CategoriaCita.Odontologia            => "Odontol.",
                CategoriaCita.Laboratorio            => "Laborat.",
                CategoriaCita.Otra                   => "Otra",
                _                                    => cat.ToString()
            })
            .ToArray();

        var colores = new[]
        {
            new SKColor(42, 61, 102), new SKColor(33, 150, 243), new SKColor(76, 175, 80),
            new SKColor(255, 152, 0), new SKColor(156, 39, 176), new SKColor(233, 30, 99),
            new SKColor(0, 150, 136),
        };

        var sersCat = categorias
            .Select((cat, i) => (ISeries)new ColumnSeries<int>
            {
                Values      = new[] { filtradas.Count(c => c.Categoria == cat) },
                Name        = labels[i],
                Fill        = new SolidColorPaint(colores[i % colores.Length]),
                Stroke      = null,
                MaxBarWidth = 40,
            })
            .Where(s => ((ColumnSeries<int>)s).Values?.Cast<int>().Sum() > 0)
            .ToArray();

        ISeries[] seriesPorCategoria = sersCat.Length > 0
            ? sersCat
            : [new ColumnSeries<int> { Values = Array.Empty<int>(), Name = "Sin datos" }];

        var labelsConDatos = categorias
            .Where((cat, i) => filtradas.Any(c => c.Categoria == cat))
            .Select(cat => labels[(int)cat])
            .ToArray();

        Axis[] ejeXCategoria =
        [
            new Axis
            {
                Labels      = labelsConDatos,
                TextSize    = 11,
                LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)),
                TicksAtCenter      = true,
                SeparatorsAtCenter = false,
            }
        ];

        int total      = filtradas.Count;
        int pendientes = filtradas.Count(c => c.Estado == EstadoCita.Pendiente);
        int completadas = filtradas.Count(c => c.Estado == EstadoCita.Completada);
        string resumen = $"Total: {total}  •  Pendientes: {pendientes}  •  Completadas: {completadas}";

        return new CitasResult(seriesPorMes, seriesPorCategoria, ejeXMes, ejeXCategoria, resumen, true);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
