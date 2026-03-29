using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class CrecimientoGraficasViewModel : INotifyPropertyChanged
{
    private static readonly System.Globalization.CultureInfo _culturaEs = new("es-ES");

    // 7 | 30 | 90 | 0 = Todo
    private int _dias = 30;
    private bool _rangoPersonalizado;
    private DateTime _fechaDesde = DateTime.Today.AddDays(-29);
    private DateTime _fechaHasta = DateTime.Today;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CrecimientoGraficasViewModel()
    {
        SeleccionarPeriodoCommand = new Command<string>(param =>
        {
            if (int.TryParse(param, out int dias))
            {
                _dias = dias;
                _rangoPersonalizado = false;
                // Sync pickers
                _fechaHasta = DateTime.Today;
                _fechaDesde = dias > 0 ? DateTime.Today.AddDays(-(dias - 1)) : DateTime.Today;
                RefreshPeriodButtonColors();
                OnPropertyChanged(nameof(FechaDesde));
                OnPropertyChanged(nameof(FechaHasta));
                CargarGraficas();
            }
        });
        RefrescarCommand = new Command(CargarGraficas);
        CargarGraficas();
    }

    // ── Propiedades de período ────────────────────────────────────────────────

    public int PeriodoSeleccionado => _dias;

    private void RefreshPeriodButtonColors()
    {
        OnPropertyChanged(nameof(PeriodoSeleccionado));
        OnPropertyChanged(nameof(Boton7DiasBg));
        OnPropertyChanged(nameof(Boton30DiasBg));
        OnPropertyChanged(nameof(Boton3MesesBg));
        OnPropertyChanged(nameof(BotonTodoBg));
        OnPropertyChanged(nameof(BotonPersonalizadoBg));
        OnPropertyChanged(nameof(Texto7Dias));
        OnPropertyChanged(nameof(Texto30Dias));
        OnPropertyChanged(nameof(Texto3Meses));
        OnPropertyChanged(nameof(TextoTodo));
        OnPropertyChanged(nameof(TextoPersonalizado));
    }

    public Color Boton7DiasBg        => !_rangoPersonalizado && _dias == 7  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton30DiasBg       => !_rangoPersonalizado && _dias == 30 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton3MesesBg       => !_rangoPersonalizado && _dias == 90 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonTodoBg         => !_rangoPersonalizado && _dias == 0  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonPersonalizadoBg => _rangoPersonalizado                ? Color.FromArgb("#1565C0")  : Color.FromArgb("#e0e0e0");

    public Color Texto7Dias          => !_rangoPersonalizado && _dias == 7  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto30Dias         => !_rangoPersonalizado && _dias == 30 ? Colors.White : Color.FromArgb("#555555");
    public Color Texto3Meses         => !_rangoPersonalizado && _dias == 90 ? Colors.White : Color.FromArgb("#555555");
    public Color TextoTodo           => !_rangoPersonalizado && _dias == 0  ? Colors.White : Color.FromArgb("#555555");
    public Color TextoPersonalizado  => _rangoPersonalizado                 ? Colors.White : Color.FromArgb("#555555");

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
            CargarGraficas();
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
            CargarGraficas();
        }
    }

    // ── Gráfica Peso ─────────────────────────────────────────────────────────

    private ISeries[] _seriesPeso = [];
    public ISeries[] SeriesPeso
    {
        get => _seriesPeso;
        private set { _seriesPeso = value; OnPropertyChanged(); }
    }

    // ── Gráfica Talla ─────────────────────────────────────────────────────────

    private ISeries[] _seriesTalla = [];
    public ISeries[] SeriesTalla
    {
        get => _seriesTalla;
        private set { _seriesTalla = value; OnPropertyChanged(); }
    }

    // ── Gráfica IMC ──────────────────────────────────────────────────────────

    private ISeries[] _seriesIMC = [];
    public ISeries[] SeriesIMC
    {
        get => _seriesIMC;
        private set { _seriesIMC = value; OnPropertyChanged(); }
    }

    // Ejes compartidos (fecha) para los 3 charts
    private Axis[] _ejeX = [];
    public Axis[] EjeX
    {
        get => _ejeX;
        private set { _ejeX = value; OnPropertyChanged(); }
    }

    private Axis[] _ejeXTalla = [];
    public Axis[] EjeXTalla
    {
        get => _ejeXTalla;
        private set { _ejeXTalla = value; OnPropertyChanged(); }
    }

    private Axis[] _ejeXIMC = [];
    public Axis[] EjeXIMC
    {
        get => _ejeXIMC;
        private set { _ejeXIMC = value; OnPropertyChanged(); }
    }

    private static Axis[] BuildYAxis(double? min = null) =>
    [
        new Axis
        {
            MinLimit    = min,
            TextSize    = 11,
            LabelsPaint = new SolidColorPaint(new SKColor(100, 100, 100)),
        }
    ];

    public Axis[] EjeYPeso  => BuildYAxis(0);
    public Axis[] EjeYTalla => BuildYAxis(0);
    public Axis[] EjeYIMC   => BuildYAxis(0);

    // ── Resúmenes ─────────────────────────────────────────────────────────────

    private string _resumenPeso  = "Sin datos";
    private string _resumenTalla = "Sin datos";
    private string _resumenIMC   = "Sin datos";
    private bool   _hayDatos;

    public string ResumenPeso
    {
        get => _resumenPeso;
        private set { _resumenPeso = value; OnPropertyChanged(); }
    }
    public string ResumenTalla
    {
        get => _resumenTalla;
        private set { _resumenTalla = value; OnPropertyChanged(); }
    }
    public string ResumenIMC
    {
        get => _resumenIMC;
        private set { _resumenIMC = value; OnPropertyChanged(); }
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

    public void CargarGraficas()
    {
        var registros = AppServices.DataService.RegistrosCrecimiento;
        var hoy       = DateTime.Today;

        DateTime desde, hasta;
        if (_rangoPersonalizado)
        {
            desde = _fechaDesde.Date;
            hasta = _fechaHasta.Date;
            if (desde > hasta) (desde, hasta) = (hasta, desde);
        }
        else if (_dias > 0)
        {
            hasta = hoy;
            desde = hoy.AddDays(-(_dias - 1));
        }
        else // Todo
        {
            desde = DateTime.MinValue;
            hasta = hoy;
        }

        // Filtrar por período
        var filtrados = registros
            .Where(r => r.Fecha.Date >= desde && r.Fecha.Date <= hasta)
            .OrderBy(r => r.Fecha)
            .ToList();

        if (filtrados.Count == 0)
        {
            SeriesPeso  = [new LineSeries<double> { Values = Array.Empty<double>(), Name = "Sin datos", Stroke = new SolidColorPaint(new SKColor(200, 200, 200), 1), Fill = null, GeometrySize = 0 }];
            SeriesTalla = [new LineSeries<double> { Values = Array.Empty<double>(), Name = "Sin datos", Stroke = new SolidColorPaint(new SKColor(200, 200, 200), 1), Fill = null, GeometrySize = 0 }];
            SeriesIMC   = [new LineSeries<double> { Values = Array.Empty<double>(), Name = "Sin datos", Stroke = new SolidColorPaint(new SKColor(200, 200, 200), 1), Fill = null, GeometrySize = 0 }];
            var emptyAxis = new Axis[] { new Axis { Labels = [], TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)) } };
            EjeX     = emptyAxis;
            EjeXTalla = emptyAxis;
            EjeXIMC  = emptyAxis;
            ResumenPeso  = "Sin registros de crecimiento";
            ResumenTalla = "Sin registros de crecimiento";
            ResumenIMC   = "Sin registros de crecimiento";
            HayDatos = false;
            OnPropertyChanged(nameof(NoHayDatos));
            return;
        }

        // Etiquetas del eje X
        bool muchos = filtrados.Count > 14;
        string[] etiquetas = filtrados
            .Select(r => muchos ? r.Fecha.ToString("dd/MM") : r.Fecha.ToString("dd/MM"))
            .ToArray();

        var pesoValues  = filtrados.Select(r => (double)r.PesoKg).ToArray();
        var tallaValues = filtrados.Select(r => (double)r.TallaCm).ToArray();
        var imcValues   = filtrados.Select(r => (double)r.IMC).ToArray();

        static Axis[] BuildXAxis(string[] labels, bool rotate) =>
        [
            new Axis
            {
                Labels             = labels,
                LabelsRotation     = rotate ? -45 : 0,
                TextSize           = 11,
                LabelsPaint        = new SolidColorPaint(new SKColor(80, 80, 80)),
                SeparatorsAtCenter = false,
                TicksAtCenter      = true,
            }
        ];

        EjeX      = BuildXAxis(etiquetas, muchos);
        EjeXTalla = BuildXAxis(etiquetas, muchos);
        EjeXIMC   = BuildXAxis(etiquetas, muchos);

        SeriesPeso =
        [
            new LineSeries<double>
            {
                Values         = pesoValues,
                Name           = "Peso (kg)",
                Stroke         = new SolidColorPaint(new SKColor(33, 150, 243), 3),
                Fill           = new SolidColorPaint(new SKColor(33, 150, 243, 40)),
                GeometryStroke = new SolidColorPaint(new SKColor(33, 150, 243), 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            }
        ];

        SeriesTalla =
        [
            new LineSeries<double>
            {
                Values         = tallaValues,
                Name           = "Talla (cm)",
                Stroke         = new SolidColorPaint(new SKColor(76, 175, 80), 3),
                Fill           = new SolidColorPaint(new SKColor(76, 175, 80, 40)),
                GeometryStroke = new SolidColorPaint(new SKColor(76, 175, 80), 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            }
        ];

        SeriesIMC =
        [
            new LineSeries<double>
            {
                Values         = imcValues,
                Name           = "IMC (kg/m²)",
                Stroke         = new SolidColorPaint(new SKColor(255, 152, 0), 3),
                Fill           = new SolidColorPaint(new SKColor(255, 152, 0, 40)),
                GeometryStroke = new SolidColorPaint(new SKColor(255, 152, 0), 4),
                GeometrySize   = 10,
                LineSmoothness = 0.4,
            }
        ];

        // Resúmenes
        double pesoMin  = pesoValues.Min();
        double pesoMax  = pesoValues.Max();
        double pesoUlt  = pesoValues[^1];
        double tallaUlt = tallaValues[^1];
        double imcUlt   = imcValues[^1];

        ResumenPeso  = $"Último: {pesoUlt:F2} kg · Rango: {pesoMin:F2} – {pesoMax:F2} kg";
        ResumenTalla = $"Último: {tallaUlt:F1} cm · {filtrados.Count} medición(es)";
        ResumenIMC   = $"Último IMC: {imcUlt:F1} kg/m²";
        HayDatos     = true;
        OnPropertyChanged(nameof(NoHayDatos));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
