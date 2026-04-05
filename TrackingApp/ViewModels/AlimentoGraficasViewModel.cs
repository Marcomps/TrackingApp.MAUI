using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

/// <summary>One row in the Personalizado summary section.</summary>
public class PersonalizadoResumenItem
{
    public string Nombre  { get; init; } = "";
    public string Resumen { get; init; } = "";
}

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

    private string _resumenFormula      = "Sin datos";
    private string _resumenLactancia   = "Sin datos";
    private string _resumenSolido      = "Sin datos";
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

    /// <summary>One item per distinct Personalizado food name.</summary>
    public ObservableCollection<PersonalizadoResumenItem> PersonalizadoResumenes { get; } = new();

    public bool HayPersonalizado => PersonalizadoResumenes.Count > 0;

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

        int n = buckets.Count;

        // ── Per-unit groupings ────────────────────────────────────────────────
        // Each key is the unit string (e.g. "ml", "oz"). Value is a bucket array.
        var formulaGroups      = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);
        var solidoGroups       = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);
        var personalizadoGroups = new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase);
        var lactanciaMin       = new double[n];

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
                idx = buckets.FindLastIndex(b => b.Inicio <= fecha);
            }

            if (idx < 0 || idx >= n) continue;

            switch (entry.TipoAlimentacion)
            {
                case TipoAlimentacion.Lactancia:
                    lactanciaMin[idx] += entry.DuracionMinutos ?? 0;
                    break;

                case TipoAlimentacion.Solido:
                {
                    var unit = string.IsNullOrWhiteSpace(entry.Unit) ? "g" : entry.Unit;
                    if (!solidoGroups.ContainsKey(unit)) solidoGroups[unit] = new double[n];
                    solidoGroups[unit][idx] += entry.CantidadGramos.HasValue
                        ? (double)entry.CantidadGramos.Value
                        : entry.Amount;
                    break;
                }

                case TipoAlimentacion.Personalizado:
                {
                    var name = string.IsNullOrWhiteSpace(entry.FoodType) ? "Personalizado" : entry.FoodType;
                    if (!personalizadoGroups.ContainsKey(name)) personalizadoGroups[name] = new double[n];
                    double pVal = entry.CantidadGramos.HasValue && entry.CantidadGramos.Value > 0
                        ? (double)entry.CantidadGramos.Value
                        : entry.Amount > 0 ? entry.Amount : 1.0;
                    personalizadoGroups[name][idx] += pVal;
                    break;
                }

                default: // Formula y legado
                {
                    var unit = string.IsNullOrWhiteSpace(entry.Unit) ? "ml" : entry.Unit;
                    if (!formulaGroups.ContainsKey(unit)) formulaGroups[unit] = new double[n];
                    formulaGroups[unit][idx] += entry.CantidadMl.HasValue
                        ? (double)entry.CantidadMl.Value
                        : entry.Amount;
                    break;
                }
            }
        }

        // ── Color palettes ───────────────────────────────────────────────────
        SKColor[] formulaColors      = [new SKColor(33,150,243), new SKColor(3,169,244), new SKColor(0,188,212)];
        SKColor[] solidoColors       = [new SKColor(255,152,0),  new SKColor(255,87,34),  new SKColor(255,193,7)];
        SKColor[] personalizadoColors = [new SKColor(76,175,80), new SKColor(139,195,74), new SKColor(56,142,60)];
        var lactanciaColor = new SKColor(156, 39, 176);

        var seriesList = new List<ISeries>();
        int ci;

        // Formula series – one per unit
        ci = 0;
        foreach (var kv in formulaGroups.OrderBy(k => k.Key))
        {
            if (!kv.Value.Any(v => v > 0)) continue;
            var c = formulaColors[ci % formulaColors.Length]; ci++;
            seriesList.Add(new LineSeries<double>
            {
                Values         = kv.Value,
                Name           = $"Fórmula ({kv.Key})",
                Stroke         = new SolidColorPaint(c, 3),
                Fill           = new SolidColorPaint(new SKColor(c.Red, c.Green, c.Blue, 40)),
                GeometryStroke = new SolidColorPaint(c, 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });
        }

        // Lactancia – single series (always minutes)
        if (lactanciaMin.Any(v => v > 0))
            seriesList.Add(new LineSeries<double>
            {
                Values         = lactanciaMin,
                Name           = "Lactancia (min)",
                Stroke         = new SolidColorPaint(lactanciaColor, 3),
                Fill           = new SolidColorPaint(new SKColor(lactanciaColor.Red, lactanciaColor.Green, lactanciaColor.Blue, 40)),
                GeometryStroke = new SolidColorPaint(lactanciaColor, 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });

        // Sólido series – one per unit
        ci = 0;
        foreach (var kv in solidoGroups.OrderBy(k => k.Key))
        {
            if (!kv.Value.Any(v => v > 0)) continue;
            var c = solidoColors[ci % solidoColors.Length]; ci++;
            seriesList.Add(new LineSeries<double>
            {
                Values         = kv.Value,
                Name           = $"Sólidos ({kv.Key})",
                Stroke         = new SolidColorPaint(c, 3),
                Fill           = new SolidColorPaint(new SKColor(c.Red, c.Green, c.Blue, 40)),
                GeometryStroke = new SolidColorPaint(c, 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });
        }

        // Personalizado series – one per food name
        ci = 0;
        foreach (var kv in personalizadoGroups.OrderBy(k => k.Key))
        {
            if (!kv.Value.Any(v => v > 0)) continue;
            var c = personalizadoColors[ci % personalizadoColors.Length]; ci++;
            seriesList.Add(new LineSeries<double>
            {
                Values         = kv.Value,
                Name           = kv.Key,
                Stroke         = new SolidColorPaint(c, 3),
                Fill           = new SolidColorPaint(new SKColor(c.Red, c.Green, c.Blue, 40)),
                GeometryStroke = new SolidColorPaint(c, 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            });
        }

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

        // ── Resúmenes por unidad ─────────────────────────────────────────────
        string unidadTiempo = bucketDays == 1 ? "día" : bucketDays == 7 ? "semana" : "mes";

        var formulaResumenParts = formulaGroups
            .Where(kv => kv.Value.Sum() > 0)
            .OrderByDescending(kv => kv.Value.Sum())
            .Select(kv => $"{kv.Value.Sum():F0} {kv.Key}");
        ResumenFormula = formulaResumenParts.Any()
            ? string.Join(" · ", formulaResumenParts)
            : "Sin registros de fórmula";

        double totalL = lactanciaMin.Sum();
        ResumenLactancia = totalL > 0
            ? $"{totalL:F0} min total · {(n > 0 ? totalL / n : 0):F0} min/{unidadTiempo} prom."
            : "Sin registros de lactancia";

        var solidoResumenParts = solidoGroups
            .Where(kv => kv.Value.Sum() > 0)
            .OrderByDescending(kv => kv.Value.Sum())
            .Select(kv => $"{kv.Value.Sum():F0} {kv.Key}");
        ResumenSolido = solidoResumenParts.Any()
            ? string.Join(" · ", solidoResumenParts)
            : "Sin registros de sólidos";

        // Build one resumen item per distinct Personalizado food name
        PersonalizadoResumenes.Clear();
        foreach (var kv in personalizadoGroups.OrderBy(k => k.Key))
        {
            if (!kv.Value.Any(v => v > 0)) continue;
            double total = kv.Value.Sum();
            // Find the unit used by this food type
            var matchingEntries = entries.Where(e =>
                e.TipoAlimentacion == TipoAlimentacion.Personalizado &&
                (string.IsNullOrWhiteSpace(e.FoodType) ? "Personalizado" : e.FoodType) == kv.Key &&
                e.Time.Date >= desde && e.Time.Date <= hasta).ToList();
            int cnt = matchingEntries.Count;
            var unit = matchingEntries.Select(e => e.Unit).FirstOrDefault(u => !string.IsNullOrWhiteSpace(u)) ?? "";
            var resumen = total > 1.0 * cnt   // has real quantity data
                ? $"{total:0.##}{(string.IsNullOrWhiteSpace(unit) ? "" : " " + unit)} total · {cnt} registro{(cnt != 1 ? "s" : "")}"
                : $"{cnt} registro{(cnt != 1 ? "s" : "")}";
            PersonalizadoResumenes.Add(new PersonalizadoResumenItem { Nombre = kv.Key, Resumen = resumen });
        }
        OnPropertyChanged(nameof(HayPersonalizado));

        bool anyFormula      = formulaGroups.Any(kv => kv.Value.Any(v => v > 0));
        bool anyLactancia    = lactanciaMin.Any(v => v > 0);
        bool anySolido       = solidoGroups.Any(kv => kv.Value.Any(v => v > 0));
        bool anyPersonalizado = personalizadoGroups.Any(kv => kv.Value.Any(v => v > 0));
        HayDatos = anyFormula || anyLactancia || anySolido || anyPersonalizado;
        OnPropertyChanged(nameof(NoHayDatos));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
