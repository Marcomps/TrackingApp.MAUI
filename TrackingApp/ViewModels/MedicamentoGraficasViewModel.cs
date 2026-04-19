using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class MedicamentoGraficasViewModel : INotifyPropertyChanged
{
    private static readonly System.Globalization.CultureInfo _culturaEs = new("es-ES");

    private int _dias = 30;
    private bool _rangoPersonalizado;
    private DateTime _fechaDesde = DateTime.Today.AddDays(-29);
    private DateTime _fechaHasta = DateTime.Today;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MedicamentoGraficasViewModel()
    {
        SeleccionarPeriodoCommand = new Command<string>(param =>
        {
            if (int.TryParse(param, out int dias))
            {
                _dias = dias;
                _rangoPersonalizado = false;
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

    // ── Período ───────────────────────────────────────────────────────────────

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

    public Color Boton7DiasBg         => !_rangoPersonalizado && _dias == 7  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton30DiasBg        => !_rangoPersonalizado && _dias == 30 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color Boton3MesesBg        => !_rangoPersonalizado && _dias == 90 ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonTodoBg          => !_rangoPersonalizado && _dias == 0  ? Color.FromArgb("#2a3d66") : Color.FromArgb("#e0e0e0");
    public Color BotonPersonalizadoBg => _rangoPersonalizado                 ? Color.FromArgb("#1565C0") : Color.FromArgb("#e0e0e0");

    public Color Texto7Dias          => !_rangoPersonalizado && _dias == 7  ? Colors.White : Color.FromArgb("#555555");
    public Color Texto30Dias         => !_rangoPersonalizado && _dias == 30 ? Colors.White : Color.FromArgb("#555555");
    public Color Texto3Meses         => !_rangoPersonalizado && _dias == 90 ? Colors.White : Color.FromArgb("#555555");
    public Color TextoTodo           => !_rangoPersonalizado && _dias == 0  ? Colors.White : Color.FromArgb("#555555");
    public Color TextoPersonalizado  => _rangoPersonalizado                 ? Colors.White : Color.FromArgb("#555555");

    public string TextoBotonRango => _rangoPersonalizado ? $"📅 {_fechaDesde:dd/MM} - {_fechaHasta:dd/MM}" : "📅 Rango";

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

    // ── Series de dosis por día ───────────────────────────────────────────────

    private ISeries[] _seriesDosis = [];
    public ISeries[] SeriesDosis
    {
        get => _seriesDosis;
        private set { _seriesDosis = value; OnPropertyChanged(); }
    }

    // ── Series por medicamento ────────────────────────────────────────────────

    private ISeries[] _seriesPorMed = [];
    public ISeries[] SeriesPorMed
    {
        get => _seriesPorMed;
        private set { _seriesPorMed = value; OnPropertyChanged(); }
    }

    private Axis[] _ejeXDosis = [];
    public Axis[] EjeXDosis
    {
        get => _ejeXDosis;
        private set { _ejeXDosis = value; OnPropertyChanged(); }
    }

    private Axis[] _ejeXPorMed = [];
    public Axis[] EjeXPorMed
    {
        get => _ejeXPorMed;
        private set { _ejeXPorMed = value; OnPropertyChanged(); }
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

    private record MedResult(
        ISeries[] SeriesDosis, ISeries[] SeriesPorMed,
        Axis[]    EjeXDosis,   Axis[]    EjeXPorMed,
        string    Resumen,     bool      HayDatos);

    public void CargarGraficas() => _ = CargarGraficasAsync();

    public async Task CargarGraficasAsync()
    {
        if (_loading) return;
        _loading = true;
        try
        {
            var perfilActivoId = AppServices.DataService.PerfilActivo?.Id ?? 0;
            var medicationsIds = AppServices.DataService.Medications.Where(m => m.PerfilId == perfilActivoId).Select(m => m.Id);
            var snapshot = AppServices.DataService.MedicationHistory.Where(h => medicationsIds.Contains(h.MedicationId)).ToList();
            var r = await Task.Run(() => ComputeGraficas(snapshot));
            SeriesDosis  = r.SeriesDosis;
            SeriesPorMed = r.SeriesPorMed;
            EjeXDosis    = r.EjeXDosis;
            EjeXPorMed   = r.EjeXPorMed;
            Resumen      = r.Resumen;
            HayDatos     = r.HayDatos;
            OnPropertyChanged(nameof(NoHayDatos));
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error med chart: {ex.Message}"); }
        finally { _loading = false; }
    }

    private MedResult ComputeGraficas(List<TrackingApp.Models.MedicationHistory> historial)
    {
        var hoy = DateTime.Today;
        DateTime desde, hasta;
        if (_rangoPersonalizado)
        {
            desde = _fechaDesde.Date; hasta = _fechaHasta.Date;
            if (desde > hasta) (desde, hasta) = (hasta, desde);
        }
        else if (_dias == 0)
        {
            desde = historial.Any() ? historial.Min(h => h.AdministeredTime.Date) : hoy.AddDays(-29);
            hasta = hoy;
        }
        else { hasta = hoy; desde = hoy.AddDays(-(_dias - 1)); }

        var filtrados = historial.Where(h => h.AdministeredTime.Date >= desde && h.AdministeredTime.Date <= hasta).ToList();

        if (filtrados.Count == 0)
        {
            var ejeVacio = new Axis { Labels = [], TextSize = 11, LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)) };
            return new MedResult(
                [new ColumnSeries<int> { Values = Array.Empty<int>(), Name = "Sin datos" }],
                [new ColumnSeries<int> { Values = Array.Empty<int>(), Name = "Sin datos" }],
                [ejeVacio], [ejeVacio],
                "Sin registros de medicamentos en este período.", false);
        }

        int totalDias = Math.Max(1, (int)(hasta - desde).TotalDays + 1);
        bool muchos   = totalDias > 14;

        var diasLabel = Enumerable.Range(0, totalDias)
            .Select(i => (Fecha: desde.AddDays(i), Label: muchos
                ? desde.AddDays(i).ToString("dd/MM")
                : desde.AddDays(i).ToString("ddd", _culturaEs)))
            .ToList();

        var countsPorDia = diasLabel
            .Select(d => filtrados.Count(h => h.AdministeredTime.Date == d.Fecha))
            .ToArray();

        ISeries[] seriesDosis =
        [
            new ColumnSeries<int>
            {
                Values = countsPorDia, Name = "Dosis administradas",
                Fill = new SolidColorPaint(new SKColor(42, 61, 102)), Stroke = null, MaxBarWidth = 30,
            }
        ];

        Axis[] ejeXDosis =
        [
            new Axis
            {
                Labels         = diasLabel.Select(d => d.Label).ToArray(), LabelsRotation = muchos ? -45 : 0,
                TextSize       = 11, LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)),
                SeparatorsAtCenter = false, TicksAtCenter = true,
            }
        ];

        var grupos = filtrados
            .GroupBy(h => h.MedicationName)
            .Select(g => (Nombre: g.Key, Count: g.Count()))
            .OrderByDescending(g => g.Count).Take(8).ToList();

        var colores = new[] {
            new SKColor(42,61,102), new SKColor(33,150,243), new SKColor(76,175,80),  new SKColor(255,152,0),
            new SKColor(156,39,176),new SKColor(233,30,99),  new SKColor(0,150,136),  new SKColor(255,87,34),
        };

        ISeries[] seriesPorMed = grupos
            .Select((g, i) => (ISeries)new ColumnSeries<int>
            {
                Values = new[] { g.Count }, Name = g.Nombre,
                Fill = new SolidColorPaint(colores[i % colores.Length]), Stroke = null, MaxBarWidth = 40,
            }).ToArray();

        Axis[] ejeXPorMed =
        [
            new Axis
            {
                Labels      = grupos.Select(g => g.Nombre.Length > 12 ? g.Nombre[..12] + "…" : g.Nombre).ToArray(),
                TextSize    = 11, LabelsPaint = new SolidColorPaint(new SKColor(80, 80, 80)),
                TicksAtCenter = true, SeparatorsAtCenter = false,
            }
        ];

        string top    = grupos.FirstOrDefault().Nombre ?? "—";
        int    topCnt = grupos.FirstOrDefault().Count;
        return new MedResult(seriesDosis, seriesPorMed, ejeXDosis, ejeXPorMed,
            $"Total: {filtrados.Count} dosis  •  Más frecuente: {top} ({topCnt})", true);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
