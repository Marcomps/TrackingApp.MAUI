using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class CrecimientoViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public CrecimientoViewModel()
    {
        AgregarCommand    = new Command(async () => await NavigateToFormAsync(null));
        EditarCommand     = new Command<RegistroCrecimiento>(async r => await NavigateToFormAsync(r));
        EliminarCommand   = new Command<RegistroCrecimiento>(async r => await EliminarAsync(r));
        RefrescarCommand  = new Command(Cargar);

        Cargar();
    }

    // ── Colecciones ───────────────────────────────────────────────────────────

    public ObservableCollection<RegistroCrecimiento> Registros { get; } = new();

    // ── Properties ───────────────────────────────────────────────────────────

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoEsIsBusy)); }
    }
    public bool NoEsIsBusy => !_isBusy;

    private bool _hayRegistros;
    public bool HayRegistros
    {
        get => _hayRegistros;
        private set { _hayRegistros = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoHayRegistros)); }
    }
    public bool NoHayRegistros => !_hayRegistros;

    // Último registro (resumen en header)
    private RegistroCrecimiento? _ultimo;
    public RegistroCrecimiento? UltimoRegistro
    {
        get => _ultimo;
        private set
        {
            _ultimo = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UltimoPesoDisplay));
            OnPropertyChanged(nameof(UltimoTallaDisplay));
            OnPropertyChanged(nameof(UltimoIMCDisplay));
            OnPropertyChanged(nameof(UltimaFechaDisplay));
        }
    }
    public string UltimoPesoDisplay   => _ultimo != null ? $"{_ultimo.PesoKg:F2} kg" : "—";
    public string UltimoTallaDisplay  => _ultimo != null ? $"{_ultimo.TallaCm:F1} cm" : "—";
    public string UltimoIMCDisplay    => _ultimo != null ? $"{_ultimo.IMC:F1} kg/m²" : "—";
    public string UltimaFechaDisplay  => _ultimo?.Fecha.ToString("dd/MM/yyyy") ?? "—";

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand AgregarCommand   { get; }
    public ICommand EditarCommand    { get; }
    public ICommand EliminarCommand  { get; }
    public ICommand RefrescarCommand { get; }

    // ── Lógica ────────────────────────────────────────────────────────────────

    public void Cargar()
    {
        IsBusy = true;
        try
        {
            var fuente = AppServices.DataService.RegistrosCrecimiento;

            Registros.Clear();
            foreach (var r in fuente.OrderByDescending(r => r.Fecha))
                Registros.Add(r);

            UltimoRegistro = Registros.FirstOrDefault();
            HayRegistros   = Registros.Count > 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static async Task NavigateToFormAsync(RegistroCrecimiento? registro)
    {
        var param = registro != null ? $"?id={registro.Id}" : string.Empty;
        await Shell.Current.GoToAsync($"{nameof(CrecimientoFormPage)}{param}");
    }

    private async Task EliminarAsync(RegistroCrecimiento registro)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar registro",
            $"¿Eliminar el registro del {registro.FormattedDate}?",
            "Eliminar", "Cancelar");

        if (!confirmar) return;

        await AppServices.DataService.DeleteRegistroCrecimientoAsync(registro);
        Cargar();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
