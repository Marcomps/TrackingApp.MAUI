using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class AlimentoGraficasViewModel : INotifyPropertyChanged
{
    private static readonly System.Globalization.CultureInfo _culturaEs = new("es-ES");

    private int _dias = 7;
    private bool _rangoPersonalizado;
    private DateTime _fechaDesde = DateTime.Today.AddDays(-6);
    private DateTime _fechaHasta = DateTime.Today;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AlimentoGraficasViewModel()
    {
        SeleccionarPeriodoCommand = new Command<string>(param =>
        {
            if (int.TryParse(param, out int dias))
            {
                _dias = dias;
                _rangoPersonalizado = false;
                // Sync pickers to match the selected preset
                _fechaHasta = DateTime.Today;
                _fechaDesde = dias > 0 ? DateTime.Today.AddDays(-(dias - 1)) : DateTime.Today;
                RefreshPeriodButtonColors();
                OnPropertyChanged(nameof(FechaDesde));
                OnPropertyChanged(nameof(FechaHasta));
                CargarGrafica();
            }
        });
        RefrescarCommand = new Command(CargarGrafica);
        CargarGrafica();
    }

    // ── Propiedades de período ────────────────────────────────────────────────

    public int PeriodoSeleccionado => _dias;

    private void RefreshPeriodButtonColors()
    {
        OnPropertyChanged(nameof(PeriodoSeleccionado));
        OnPropertyChanged(nameof(BotonHoyBg));
        OnPropertyChanged(nameof(Boton7DiasBg));
        OnPropertyChanged(nameof(Boton30DiasBg));
        OnPropertyChanged(nameof(Boton90DiasBg));
        OnPropertyChanged(nameof(BotonTodoBg));
        OnPropertyChanged(nameof(BotonPersonalizadoBg));
        OnPropertyChanged(nameof(TextoHoy));
        OnPropertyChanged(nameof(Texto7Dias));
        OnPropertyChanged(nameof(Texto30Dias));
        OnPropertyChanged(nameof(Texto90Dias));
        OnPropertyChanged(nameof(TextoTodo));
        OnPropertyChanged(nameof(TextoPersonalizado));
    }

    public Color BotonHoyBg          => !_rangoPersonalizado && _dias == 1  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton7DiasBg        => !_rangoPersonalizado && _dias == 7  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton30DiasBg       => !_rangoPersonalizado && _dias == 30 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton90DiasBg       => !_rangoPersonalizado && _dias == 90 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonTodoBg         => !_rangoPersonalizado && _dias == 0  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonPersonalizadoBg => _rangoPersonalizado               ? Color.FromArgb("#1565C0")  : Color.FromArgb("#e0e0e0");

    public Color TextoHoy           => !_rangoPersonalizado && _dias == 1  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto7Dias         => !_rangoPersonalizado && _dias == 7  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto30Dias        => !_rangoPersonalizado && _dias == 30 ? Colors.White : Color.FromArgb("#555555");
    public Color Texto90Dias        => !_rangoPersonalizado && _dias == 90 ? Colors.White : Color.FromArgb("#555555");
    public Color TextoTodo          => !_rangoPersonalizado && _dias == 0  ? Colors.White : Color.FromArgb("#555555");
    public Color TextoPersonalizado => _rangoPersonalizado                 ? Colors.White : Color.FromArgb("#555555");

    // ── Rango personalizado ───────────────────────────────────────────────────

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
            CargarGrafica();
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
            CargarGrafica();
        }
    }

    // ── Propiedades de gráfica ────────────────────────────────────────────────

    private ISeries[] _series = [];
    public ISeries[] Series
    {
        get => _series;
        private set { _series = value; OnPropertyChanged(); }
    }

    private Axis[] _xAxes = [];
    public Axis[] XAxes
    {
        get => _xAxes;
        private set { _xAxes = value; OnPropertyChanged(); }
    }

    private Axis[] _yAxes =
    [
        new Axis
        {
            MinLimit    = 0,
            TextSize    = 11,
            LabelsPaint = new SolidColorPaint(new SKColor(100, 100, 100)),
        }
    ];
    public Axis[] YAxes
    {
        get => _yAxes;
        private set { _yAxes = value; OnPropertyChanged(); }
    }

    // ── Resúmenes estadísticos ────────────────────────────────────────────────

    private string _resumenFormula   = "Sin datos";
    private string _resumenLactancia = "Sin datos";
    private string _resumenSolido    = "Sin datos";
    private bool   _hayDatos;

    public string ResumenFormula
    {
        get => _resumenFormula;
        private set { _resumenFormula = value; OnPropertyChanged(); }
    }
    public string ResumenLactancia
    {
        get => _resumenLactancia;
        private set { _resumenLactancia = value; OnPropertyChanged(); }
    }
    public string ResumenSolido
    {
        get => _resumenSolido;
        private set { _resumenSolido = value; OnPropertyChanged(); }
    }
    public bool HayDatos
    {
        get => _hayDatos;
        private set { _hayDatos = value; OnPropertyChanged(); }
    }
    public bool NoHayDatos => !_hayDatos;

    // ── Comandos ──────────────────────────────────────────────────────────────

    public Command<string> SeleccionarPeriodoCommand { get; }
    public Command RefrescarCommand { get; }

    // ── Lógica de datos ───────────────────────────────────────────────────────

    public void CargarGrafica()
    {
        var entries = AppServices.DataService.FoodEntries;
        var hoy     = DateTime.Today;

        DateTime desde, hasta;
        if (_rangoPersonalizado)
        {
            desde = _fechaDesde.Date;
            hasta = _fechaHasta.Date;
            if (desde > hasta) (desde, hasta) = (hasta, desde);
        }
        else if (_dias == 0) // Todo
        {
            desde = entries.Any() ? entries.Min(e => e.Time.Date) : hoy.AddDays(-6);
            hasta = hoy;
        }
        else
        {
            hasta = hoy;
            desde = hoy.AddDays(-(_dias - 1));
        }

        int totalDias = Math.Max(1, (int)(hasta - desde).TotalDays + 1);

        // --- Determine bucket size ---
        // ≤ 60 days → daily  |  ≤ 365 days → weekly  |  > 365 days → monthly
        List<(DateTime Inicio, string Label)> buckets;
        int bucketDays;

        if (totalDias <= 60)
        {
            bucketDays = 1;
            buckets = Enumerable.Range(0, totalDias)
                .Select(i =>
                {
                    var d   = desde.AddDays(i);
                    var lbl = totalDias <= 7 ? d.ToString("ddd", _culturaEs) : d.ToString("dd/MM");
                    return (d, lbl);
                })
                .ToList();
        }
        else if (totalDias <= 365)
        {
            bucketDays = 7;
            buckets = new List<(DateTime, string)>();
            int dow = (int)desde.DayOfWeek;
            var ini = desde.AddDays(dow == 0 ? -6 : 1 - dow);
            if (ini > desde) ini = ini.AddDays(-7);
            for (var d = ini; d <= hasta; d = d.AddDays(7))
                buckets.Add((d, d.ToString("dd/MM")));
        }
        else
        {
            bucketDays = -1; // monthly
            buckets = new List<(DateTime, string)>();
            for (var m = new DateTime(desde.Year, desde.Month, 1); m <= hasta; m = m.AddMonths(1))
                buckets.Add((m, m.ToString("MMM yy", _culturaEs)));
        }

        int n            = buckets.Count;
        var formulaMl    = new double[n];
        var lactanciaMin = new double[n];
        var solidoGr     = new double[n];

        foreach (var entry in entries)
        {
            var fecha = entry.Time.Date;
            if (fecha < desde || fecha > hasta) continue;

            int idx;
            if (bucketDays == -1) // monthly
            {
                var mesF = new DateTime(fecha.Year, fecha.Month, 1);
                idx = buckets.FindIndex(b => b.Inicio == mesF);
            }
            else
            {
                // Last bucket whose start is ≤ the entry's date
                idx = buckets.FindLastIndex(b => b.Inicio <= fecha);
            }

            if (idx < 0 || idx >= n) continue;

            switch (entry.TipoAlimentacion)
            {
                case TipoAlimentacion.Lactancia:
                    lactanciaMin[idx] += entry.DuracionMinutos ?? 0;
                    break;
                case TipoAlimentacion.Solido:
                    solidoGr[idx] += entry.CantidadGramos.HasValue
                        ? (double)entry.CantidadGramos.Value
                        : entry.Amount;
                    break;
                default: // Formula y legado
                    formulaMl[idx] += entry.CantidadMl.HasValue
                        ? (double)entry.CantidadMl.Value
                        : entry.Amount;
                    break;
            }
        }

        bool tieneFormula   = formulaMl.Any(v => v > 0);
        bool tieneLactancia = lactanciaMin.Any(v => v > 0);
        bool tieneSolido    = solidoGr.Any(v => v > 0);

        var seriesList = new List<ISeries>();

        if (tieneFormula)
            seriesList.Add(new LineSeries<double>
            {
                Values         = formulaMl,
                Name           = "Fórmula (ml)",
                Stroke         = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                Fill           = new SolidColorPaint(new SKColor(33, 150, 243, 40)),
                GeometryStroke = new SolidColorPaint(new SKColor(33, 150, 243), 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });

        if (tieneLactancia)
            seriesList.Add(new LineSeries<double>
            {
                Values         = lactanciaMin,
                Name           = "Lactancia (min)",
                Stroke         = new SolidColorPaint(new SKColor(156, 39, 176), 3),
                Fill           = new SolidColorPaint(new SKColor(156, 39, 176, 40)),
                GeometryStroke = new SolidColorPaint(new SKColor(156, 39, 176), 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });

        if (tieneSolido)
            seriesList.Add(new LineSeries<double>
            {
                Values         = solidoGr,
                Name           = "Sólidos (g)",
                Stroke         = new SolidColorPaint(new SKColor(255, 152, 0), 3),
                Fill           = new SolidColorPaint(new SKColor(255, 152, 0, 40)),
                GeometryStroke = new SolidColorPaint(new SKColor(255, 152, 0), 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });

        if (seriesList.Count == 0)
            seriesList.Add(new LineSeries<double>
            {
                Values       = new double[n],
                Name         = "Sin datos",
                Stroke       = new SolidColorPaint(new SKColor(200, 200, 200), 1),
                Fill         = null,
                GeometrySize = 0,
            });

        Series = [.. seriesList];

        XAxes =
        [
            new Axis
            {
                Labels             = buckets.Select(b => b.Label).ToArray(),
                LabelsRotation     = n > 14 ? -45 : 0,
                TextSize           = 11,
                LabelsPaint        = new SolidColorPaint(new SKColor(80, 80, 80)),
                SeparatorsAtCenter = false,
                TicksAtCenter      = true,
            }
        ];

        string unidadTiempo  = bucketDays == 1 ? "día" : bucketDays == 7 ? "semana" : "mes";
        double totalF = formulaMl.Sum();
        double totalL = lactanciaMin.Sum();
        double totalS = solidoGr.Sum();

        ResumenFormula   = totalF > 0 ? $"{totalF:F0} ml total · {(n > 0 ? totalF / n : 0):F0} ml/{unidadTiempo} promedio"   : "Sin registros de fórmula";
        ResumenLactancia = totalL > 0 ? $"{totalL:F0} min total · {(n > 0 ? totalL / n : 0):F0} min/{unidadTiempo} promedio" : "Sin registros de lactancia";
        ResumenSolido    = totalS > 0 ? $"{totalS:F0} g total · {(n > 0 ? totalS / n : 0):F0} g/{unidadTiempo} promedio"     : "Sin registros de sólidos";

        HayDatos = totalF > 0 || totalL > 0 || totalS > 0;
        OnPropertyChanged(nameof(NoHayDatos));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
