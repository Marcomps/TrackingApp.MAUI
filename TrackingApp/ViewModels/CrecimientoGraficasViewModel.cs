using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TrackingApp.Models;
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
        OnPropertyChanged(nameof(TextoBotonRango));
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

    public string TextoBotonRango => _rangoPersonalizado ? $"📅 {_fechaDesde:dd/MM} - {_fechaHasta:dd/MM}" : "📅 Rango";

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
    public Command ActivarRangoPersonalizadoCommand { get; }

    // ── Lógica de datos ───────────────────────────────────────────────────────

    private bool _loading;

    private record CrecimientoResult(
        ISeries[] SeriesPeso,
        ISeries[] SeriesTalla,
        ISeries[] SeriesIMC,
        Axis[]    EjeX,
        Axis[]    EjeXTalla,
        Axis[]    EjeXIMC,
        string    ResumenPeso,
        string    ResumenTalla,
        string    ResumenIMC,
        bool      HayDatos
    );

    public void CargarGraficas() => _ = CargarGraficasAsync();

    public async Task CargarGraficasAsync()
    {
        if (_loading) return;
        _loading = true;
        try
        {
            var snapshot = AppServices.DataService.RegistrosCrecimiento.ToList();
            var r = await Task.Run(() => ComputeGraficas(snapshot));
            // Back on UI thread: apply all results
            SeriesPeso  = r.SeriesPeso;
            SeriesTalla = r.SeriesTalla;
            SeriesIMC   = r.SeriesIMC;
            EjeX        = r.EjeX;
            EjeXTalla   = r.EjeXTalla;
            EjeXIMC     = r.EjeXIMC;
            ResumenPeso  = r.ResumenPeso;
            ResumenTalla = r.ResumenTalla;
            ResumenIMC   = r.ResumenIMC;
            HayDatos     = r.HayDatos;
            OnPropertyChanged(nameof(NoHayDatos));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading growth chart: {ex.Message}");
        }
        finally { _loading = false; }
    }

    private CrecimientoResult ComputeGraficas(List<RegistroCrecimiento> registros)
    {
        var hoy = DateTime.Today;

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
            var emptyLine = new LineSeries<double> { Values = Array.Empty<double>(), Name = "Sin datos", Stroke = new SolidColorPaint(new SKColor(200, 200, 200), 1), Fill = null, GeometrySize = 0 };
            var emptyAxis = new Axis[] { new Axis { Labels = [], TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)) } };
            return new CrecimientoResult(
                [emptyLine], [emptyLine], [emptyLine],
                emptyAxis, emptyAxis, emptyAxis,
                "Sin registros de crecimiento", "Sin registros de crecimiento", "Sin registros de crecimiento",
                false);
        }

        // Etiquetas del eje X
        bool muchos = filtrados.Count > 14;
        string[] etiquetas = filtrados
            .Select(r => r.Fecha.ToString("dd/MM"))
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

        var ejeX      = BuildXAxis(etiquetas, muchos);
        var ejeXTalla = BuildXAxis(etiquetas, muchos);
        var ejeXIMC   = BuildXAxis(etiquetas, muchos);

        ISeries[] seriesPeso =
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

        ISeries[] seriesTalla =
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

        ISeries[] seriesIMC =
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

        string resumenPeso  = $"Último: {pesoUlt:F2} kg · Rango: {pesoMin:F2} – {pesoMax:F2} kg";
        string resumenTalla = $"Último: {tallaUlt:F1} cm · {filtrados.Count} medición(es)";
        string resumenIMC   = $"Último IMC: {imcUlt:F1} kg/m²";

        return new CrecimientoResult(seriesPeso, seriesTalla, seriesIMC, ejeX, ejeXTalla, ejeXIMC, resumenPeso, resumenTalla, resumenIMC, true);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
