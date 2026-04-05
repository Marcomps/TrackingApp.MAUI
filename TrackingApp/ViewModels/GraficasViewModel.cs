using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TrackingApp.ViewModels;

public class GraficasViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ── Sub-ViewModels ────────────────────────────────────────────────────────

    public AlimentoGraficasViewModel    AlimentoVm    { get; } = new();
    public MedicamentoGraficasViewModel MedicamentoVm { get; } = new();
    public CrecimientoGraficasViewModel CrecimientoVm { get; } = new();
    public CitasGraficasViewModel       CitasVm       { get; } = new();

    // ── Tab activo (0=Alimento, 1=Medicamento, 2=Crecimiento, 3=Citas) ────────

    private int _tabActivo = 0;

    public bool EsTabAlimento    => _tabActivo == 0;
    public bool EsTabMedicamento => _tabActivo == 1;
    public bool EsTabCrecimiento => _tabActivo == 2;
    public bool EsTabCitas       => _tabActivo == 3;

    // ── Colors for active tab indicator ──────────────────────────────────────

    private static Color ActiveBg  => Color.FromArgb("#2a3d66");
    private static Color InactiveBg => Colors.White;
    private static Color ActiveText  => Color.FromArgb("#2a3d66");
    private static Color InactiveText => Color.FromArgb("#999999");

    public Color BgAlimento    => _tabActivo == 0 ? ActiveBg   : InactiveBg;
    public Color BgMedicamento => _tabActivo == 1 ? ActiveBg   : InactiveBg;
    public Color BgCrecimiento => _tabActivo == 2 ? ActiveBg   : InactiveBg;
    public Color BgCitas       => _tabActivo == 3 ? ActiveBg   : InactiveBg;

    public Color TextAlimento    => _tabActivo == 0 ? ActiveText : InactiveText;
    public Color TextMedicamento => _tabActivo == 1 ? ActiveText : InactiveText;
    public Color TextCrecimiento => _tabActivo == 2 ? ActiveText : InactiveText;
    public Color TextCitas       => _tabActivo == 3 ? ActiveText : InactiveText;

    public FontAttributes FontAlimento    => _tabActivo == 0 ? FontAttributes.Bold : FontAttributes.None;
    public FontAttributes FontMedicamento => _tabActivo == 1 ? FontAttributes.Bold : FontAttributes.None;
    public FontAttributes FontCrecimiento => _tabActivo == 2 ? FontAttributes.Bold : FontAttributes.None;
    public FontAttributes FontCitas       => _tabActivo == 3 ? FontAttributes.Bold : FontAttributes.None;

    // ── Commands ──────────────────────────────────────────────────────────────

    public Command<int> CambiarTabCommand { get; }

    public GraficasViewModel()
    {
        CambiarTabCommand = new Command<int>(tab =>
        {
            if (_tabActivo == tab) return;
            _tabActivo = tab;
            NotifyTabChange();
            RefrescarTabActivo();
        });
    }

    private void NotifyTabChange()
    {
        OnPropertyChanged(nameof(EsTabAlimento));
        OnPropertyChanged(nameof(EsTabMedicamento));
        OnPropertyChanged(nameof(EsTabCrecimiento));
        OnPropertyChanged(nameof(EsTabCitas));
        OnPropertyChanged(nameof(BgAlimento));
        OnPropertyChanged(nameof(BgMedicamento));
        OnPropertyChanged(nameof(BgCrecimiento));
        OnPropertyChanged(nameof(BgCitas));
        OnPropertyChanged(nameof(TextAlimento));
        OnPropertyChanged(nameof(TextMedicamento));
        OnPropertyChanged(nameof(TextCrecimiento));
        OnPropertyChanged(nameof(TextCitas));
        OnPropertyChanged(nameof(FontAlimento));
        OnPropertyChanged(nameof(FontMedicamento));
        OnPropertyChanged(nameof(FontCrecimiento));
        OnPropertyChanged(nameof(FontCitas));
    }

    /// <summary>Refreshes the currently active tab's chart data.</summary>
    public void RefrescarTabActivo()
    {
        switch (_tabActivo)
        {
            case 0: AlimentoVm.CargarGrafica();    break;
            case 1: MedicamentoVm.CargarGraficas(); break;
            case 2: CrecimientoVm.CargarGraficas(); break;
            case 3: CitasVm.CargarGraficas();        break;
        }
    }

    /// <summary>Refreshes ALL tabs (call after add/edit/delete).</summary>
    public void RefrescarTodos()
    {
        AlimentoVm.CargarGrafica();
        MedicamentoVm.CargarGraficas();
        CrecimientoVm.CargarGraficas();
        CitasVm.CargarGraficas();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
