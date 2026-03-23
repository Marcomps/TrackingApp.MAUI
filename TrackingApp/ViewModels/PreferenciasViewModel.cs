using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

/// <summary>
/// Configuración de unidades de medida.
/// Lee/escribe SistemaUnidades en el perfil activo (si existe) y en Preferences
/// como fallback para cuando no hay perfiles creados.
/// </summary>
public class PreferenciasViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private const string PrefKey = "SistemaUnidades";

    private static readonly UnitService _unitSvc = new();

    public IReadOnlyList<string> OpcionesSistema { get; } = new[]
    {
        "Métrico  (kg, cm, ml)",
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
            OnPropertyChanged(nameof(EjemploPeso));
            OnPropertyChanged(nameof(EjemploTalla));
        }
    }

    public SistemaUnidades SistemaActual
        => _sistemaIdx == 1 ? SistemaUnidades.Imperial : SistemaUnidades.Metrico;

    // ── Ejemplos dinámicos ────────────────────────────────────────────────────

    public string EjemploPeso  => SistemaActual == SistemaUnidades.Imperial
        ? "Peso     → lb (libras)"
        : "Peso     → kg (kilogramos)";

    public string EjemploTalla => SistemaActual == SistemaUnidades.Imperial
        ? "Talla    → in (pulgadas)"
        : "Talla    → cm (centímetros)";

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand GuardarCommand  { get; }
    public ICommand CancelarCommand { get; }

    public PreferenciasViewModel()
    {
        GuardarCommand  = new Command(async () => await GuardarAsync());
        CancelarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        Cargar();
    }

    // ── Lógica ────────────────────────────────────────────────────────────────

    private void Cargar()
    {
        // Prioridad: perfil activo → Preferences → Métrico por defecto
        SistemaUnidades sistema;

        var perfil = AppServices.DataService.PerfilActivo;
        if (perfil != null)
        {
            sistema = perfil.SistemaUnidades;
        }
        else
        {
            var saved = Microsoft.Maui.Storage.Preferences.Get(PrefKey, SistemaUnidades.Metrico.ToString());
            sistema = Enum.TryParse<SistemaUnidades>(saved, out var parsed) ? parsed : SistemaUnidades.Metrico;
        }

        SistemaIndex = sistema == SistemaUnidades.Imperial ? 1 : 0;
    }

    private async Task GuardarAsync()
    {
        var sistema = SistemaActual;

        // Persistir en Preferences (sin datos sensibles)
        Microsoft.Maui.Storage.Preferences.Set(PrefKey, sistema.ToString());

        // Actualizar el perfil activo si existe
        var perfil = AppServices.DataService.PerfilActivo;
        if (perfil != null && perfil.SistemaUnidades != sistema)
        {
            perfil.SistemaUnidades = sistema;
            await AppServices.DataService.UpdatePerfilAsync(perfil);
        }

        await Shell.Current.DisplayAlert(
            "Guardado",
            $"Sistema {(sistema == SistemaUnidades.Imperial ? "Imperial" : "Métrico")} establecido como predeterminado.",
            "OK");

        await Shell.Current.GoToAsync("..");
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
