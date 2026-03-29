using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

/// <summary>
/// Configuracion de unidades de medida.
/// Lee/escribe SistemaUnidades en el perfil activo (si existe) y en Preferences
/// como fallback. Tambien expone selectores individuales por tipo de magnitud.
/// </summary>
public class PreferenciasViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string PrefKeySistema  = "SistemaUnidades";
    private const string PrefKeyPeso     = "UnidadPeso";
    private const string PrefKeyTalla    = "UnidadTalla";
    private const string PrefKeyLiquido  = "UnidadLiquido";

    private static readonly UnitService _unitSvc = new();

    // ── Sistema general (Metrico / Imperial) ─────────────────────────────────

    public IReadOnlyList<string> OpcionesSistema { get; } = new[]
    {
        "Metrico  (kg, cm, ml)",
        "Imperial (lb, in, fl oz)"
    };

    private int _sistemaIdx;
    public int SistemaIndex
    {
        get => _sistemaIdx;
        set
        {
            if (_sistemaIdx == value) return;
            _sistemaIdx = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SistemaActual));
            // Sync individual pickers when sistema changes
            if (value == 0) { UnidadPesoIndex = 0; UnidadTallaIndex = 0; UnidadLiquidoIndex = 0; }
            else            { UnidadPesoIndex = 2; UnidadTallaIndex = 2; UnidadLiquidoIndex = 2; }
        }
    }

    public SistemaUnidades SistemaActual
        => _sistemaIdx == 1 ? SistemaUnidades.Imperial : SistemaUnidades.Metrico;

    // ── Unidad de Peso ────────────────────────────────────────────────────────

    public IReadOnlyList<string> OpcionesPeso { get; } = new[]
        { "kg  (kilogramos)", "g  (gramos)", "lb  (libras)", "oz  (onzas)" };

    private int _unidadPesoIdx;
    public int UnidadPesoIndex
    {
        get => _unidadPesoIdx;
        set
        {
            if (_unidadPesoIdx == value) return;
            _unidadPesoIdx = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EjemploPeso));
        }
    }

    // ── Unidad de Talla ───────────────────────────────────────────────────────

    public IReadOnlyList<string> OpcionesTalla { get; } = new[]
        { "cm  (centimetros)", "m  (metros)", "in  (pulgadas)", "ft  (pies)", "yd  (yardas)" };

    private int _unidadTallaIdx;
    public int UnidadTallaIndex
    {
        get => _unidadTallaIdx;
        set
        {
            if (_unidadTallaIdx == value) return;
            _unidadTallaIdx = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EjemploTalla));
        }
    }

    // ── Unidad de Liquido / Volumen ───────────────────────────────────────────

    public IReadOnlyList<string> OpcionesLiquido { get; } = new[]
        { "ml  (mililitros)", "l  (litros)", "fl oz  (onzas liquidas)", "tsp  (cucharadita)", "tbsp  (cucharada)" };

    private int _unidadLiquidoIdx;
    public int UnidadLiquidoIndex
    {
        get => _unidadLiquidoIdx;
        set
        {
            if (_unidadLiquidoIdx == value) return;
            _unidadLiquidoIdx = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EjemploLiquido));
        }
    }

    // ── Ejemplos dinamicos ────────────────────────────────────────────────────

    public string EjemploPeso    => $"Peso    -> {OpcionesPeso[_unidadPesoIdx]}";
    public string EjemploTalla   => $"Talla   -> {OpcionesTalla[_unidadTallaIdx]}";
    public string EjemploLiquido => $"Volumen -> {OpcionesLiquido[_unidadLiquidoIdx]}";

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand GuardarCommand     { get; }
    public ICommand RestablecerCommand { get; }

    public PreferenciasViewModel()
    {
        GuardarCommand     = new Command(async () => await GuardarAsync());
        RestablecerCommand = new Command(Cargar);

        Cargar();
    }

    // ── Logica ────────────────────────────────────────────────────────────────

    private void Cargar()
    {
        // Sistema general
        SistemaUnidades sistema;
        var perfil = AppServices.DataService.PerfilActivo;
        if (perfil != null)
            sistema = perfil.SistemaUnidades;
        else
        {
            var saved = Microsoft.Maui.Storage.Preferences.Get(PrefKeySistema, SistemaUnidades.Metrico.ToString());
            sistema = Enum.TryParse<SistemaUnidades>(saved, out var parsed) ? parsed : SistemaUnidades.Metrico;
        }
        _sistemaIdx = sistema == SistemaUnidades.Imperial ? 1 : 0;
        OnPropertyChanged(nameof(SistemaIndex));

        // Unidades individuales (desde Preferences; default coherente con sistema)
        int pesoDefault    = _sistemaIdx == 0 ? 0 : 2;  // kg : lb
        int tallaDefault   = _sistemaIdx == 0 ? 0 : 2;  // cm : in
        int liquidoDefault = _sistemaIdx == 0 ? 0 : 2;  // ml : fl oz

        _unidadPesoIdx    = Microsoft.Maui.Storage.Preferences.Get(PrefKeyPeso,    pesoDefault);
        _unidadTallaIdx   = Microsoft.Maui.Storage.Preferences.Get(PrefKeyTalla,   tallaDefault);
        _unidadLiquidoIdx = Microsoft.Maui.Storage.Preferences.Get(PrefKeyLiquido, liquidoDefault);

        // Clamp to valid range
        _unidadPesoIdx    = Math.Clamp(_unidadPesoIdx,    0, OpcionesPeso.Count - 1);
        _unidadTallaIdx   = Math.Clamp(_unidadTallaIdx,   0, OpcionesTalla.Count - 1);
        _unidadLiquidoIdx = Math.Clamp(_unidadLiquidoIdx, 0, OpcionesLiquido.Count - 1);

        OnPropertyChanged(nameof(UnidadPesoIndex));
        OnPropertyChanged(nameof(UnidadTallaIndex));
        OnPropertyChanged(nameof(UnidadLiquidoIndex));
        OnPropertyChanged(nameof(EjemploPeso));
        OnPropertyChanged(nameof(EjemploTalla));
        OnPropertyChanged(nameof(EjemploLiquido));
    }

    private async Task GuardarAsync()
    {
        var sistema = SistemaActual;

        // Guardar sistema
        Microsoft.Maui.Storage.Preferences.Set(PrefKeySistema, sistema.ToString());

        // Guardar unidades individuales
        Microsoft.Maui.Storage.Preferences.Set(PrefKeyPeso,    _unidadPesoIdx);
        Microsoft.Maui.Storage.Preferences.Set(PrefKeyTalla,   _unidadTallaIdx);
        Microsoft.Maui.Storage.Preferences.Set(PrefKeyLiquido, _unidadLiquidoIdx);

        // Actualizar perfil activo si existe
        var perfil = AppServices.DataService.PerfilActivo;
        if (perfil != null && perfil.SistemaUnidades != sistema)
        {
            perfil.SistemaUnidades = sistema;
            await AppServices.DataService.UpdatePerfilAsync(perfil);
        }

        await Shell.Current.DisplayAlert(
            "Guardado",
            "Preferencias de unidades guardadas correctamente.",
            "OK");
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}