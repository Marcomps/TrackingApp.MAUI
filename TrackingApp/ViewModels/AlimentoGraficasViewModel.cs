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
    private int _dias = 7;

    public event PropertyChangedEventHandler? PropertyChanged;

    public AlimentoGraficasViewModel()
    {
        SeleccionarPeriodoCommand = new Command<string>(param =>
        {
            if (int.TryParse(param, out int dias))
            {
                _dias = dias;
                OnPropertyChanged(nameof(PeriodoSeleccionado));
                OnPropertyChanged(nameof(BotonHoyBg));
                OnPropertyChanged(nameof(Boton7DiasBg));
                OnPropertyChanged(nameof(Boton30DiasBg));
                CargarGrafica();
            }
        });
        RefrescarCommand = new Command(CargarGrafica);
        CargarGrafica();
    }

    // ── Propiedades de período ────────────────────────────────────────────────

    public int PeriodoSeleccionado => _dias;

    public Color BotonHoyBg    => _dias == 1  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton7DiasBg  => _dias == 7  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton30DiasBg => _dias == 30 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");

    public Color TextoHoy    => _dias == 1  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto7Dias  => _dias == 7  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto30Dias => _dias == 30 ? Colors.White : Color.FromArgb("#555555");

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
        var desde   = hoy.AddDays(-(_dias - 1));

        var diasList = Enumerable.Range(0, _dias)
                                 .Select(i => desde.AddDays(i))
                                 .ToList();

        string[] etiquetas = _dias <= 7
            ? diasList.Select(d => d.ToString("ddd", new System.Globalization.CultureInfo("es-ES"))).ToArray()
            : diasList.Select(d => d.ToString("dd/MM")).ToArray();

        var formulaMl    = new double[_dias];
        var lactanciaMin = new double[_dias];
        var solidoGr     = new double[_dias];

        foreach (var entry in entries)
        {
            var fecha = entry.Time.Date;
            int idx   = (int)(fecha - desde).TotalDays;
            if (idx < 0 || idx >= _dias) continue;

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

        bool tieneAlgoFormula   = formulaMl.Any(v => v > 0);
        bool tieneAlgoLactancia = lactanciaMin.Any(v => v > 0);
        bool tieneAlgoSolido    = solidoGr.Any(v => v > 0);

        var seriesList = new List<ISeries>();

        if (tieneAlgoFormula)
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

        if (tieneAlgoLactancia)
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

        if (tieneAlgoSolido)
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

        // Si no ha nada de datos, mostrar línea vacía para que el chart no quede en blanco
        if (seriesList.Count == 0)
            seriesList.Add(new LineSeries<double>
            {
                Values = new double[_dias],
                Name   = "Sin datos",
                Stroke = new SolidColorPaint(new SKColor(200, 200, 200), 1),
                Fill   = null,
                GeometrySize = 0,
            });

        Series = [.. seriesList];

        XAxes =
        [
            new Axis
            {
                Labels             = etiquetas,
                LabelsRotation     = _dias > 14 ? -45 : 0,
                TextSize           = 11,
                LabelsPaint        = new SolidColorPaint(new SKColor(80, 80, 80)),
                SeparatorsAtCenter = false,
                TicksAtCenter      = true,
            }
        ];

        // Resúmenes
        double totalF = formulaMl.Sum();
        double totalL = lactanciaMin.Sum();
        double totalS = solidoGr.Sum();

        ResumenFormula = totalF > 0
            ? $"{totalF:F0} ml total · {totalF / _dias:F0} ml/día promedio"
            : "Sin registros de fórmula";

        ResumenLactancia = totalL > 0
            ? $"{totalL:F0} min total · {totalL / _dias:F0} min/día promedio"
            : "Sin registros de lactancia";

        ResumenSolido = totalS > 0
            ? $"{totalS:F0} g total · {totalS / _dias:F0} g/día promedio"
            : "Sin registros de sólidos";

        HayDatos = totalF > 0 || totalL > 0 || totalS > 0;
        OnPropertyChanged(nameof(NoHayDatos));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
