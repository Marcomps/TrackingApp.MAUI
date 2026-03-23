using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

public class PerfilesViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public PerfilesViewModel()
    {
        AgregarCommand   = new Command(async () => await NavigateToFormAsync(null));
        EditarCommand    = new Command<Perfil>(async p => await NavigateToFormAsync(p));
        EliminarCommand  = new Command<Perfil>(async p => await EliminarAsync(p));
        RefrescarCommand = new Command(Cargar);

        Cargar();
    }

    // ── Colecciones ───────────────────────────────────────────────────────────

    public ObservableCollection<Perfil> Perfiles { get; } = new();

    // ── Properties ───────────────────────────────────────────────────────────

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoEsBusy)); }
    }
    public bool NoEsBusy => !_isBusy;

    private bool _hayPerfiles;
    public bool HayPerfiles
    {
        get => _hayPerfiles;
        private set { _hayPerfiles = value; OnPropertyChanged(); OnPropertyChanged(nameof(NoHayPerfiles)); }
    }
    public bool NoHayPerfiles => !_hayPerfiles;

    /// <summary>
    /// Perfil seleccionado por el usuario. Cuando cambia, actualiza el perfil activo
    /// en DataService y lo persiste como preferencia.
    /// </summary>
    private Perfil? _perfilSeleccionado;
    public Perfil? PerfilSeleccionado
    {
        get => _perfilSeleccionado;
        set
        {
            if (value == null || value == _perfilSeleccionado) return;
            _perfilSeleccionado = value;
            AppServices.DataService.SetPerfilActivo(value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(NombreActivoDisplay));
            OnPropertyChanged(nameof(TipoActivoDisplay));
        }
    }

    public string NombreActivoDisplay => _perfilSeleccionado?.DisplayName ?? "Sin perfil activo";
    public string TipoActivoDisplay   => _perfilSeleccionado?.TipoPerfilDisplay ?? string.Empty;

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
            var fuente = AppServices.DataService.Perfiles;
            Perfiles.Clear();
            foreach (var p in fuente)
                Perfiles.Add(p);

            HayPerfiles = Perfiles.Count > 0;

            // Sincronizar selección con el perfil activo en DataService
            var activo = AppServices.DataService.PerfilActivo;
            _perfilSeleccionado = activo != null
                ? Perfiles.FirstOrDefault(p => p.Id == activo.Id)
                : Perfiles.FirstOrDefault();

            OnPropertyChanged(nameof(PerfilSeleccionado));
            OnPropertyChanged(nameof(NombreActivoDisplay));
            OnPropertyChanged(nameof(TipoActivoDisplay));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateToFormAsync(Perfil? perfil)
    {
        if (perfil != null)
            await Shell.Current.GoToAsync($"{nameof(PerfilFormPage)}?id={perfil.Id}");
        else
            await Shell.Current.GoToAsync(nameof(PerfilFormPage));
    }

    private async Task EliminarAsync(Perfil perfil)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar perfil",
            $"¿Eliminar el perfil \"{perfil.Nombre}\"?\n\nLos registros del perfil NO se eliminarán.",
            "Eliminar", "Cancelar");

        if (!confirmar) return;

        try
        {
            await AppServices.DataService.DeletePerfilAsync(perfil);
            Cargar();
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlert("No se puede eliminar", ex.Message, "OK");
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
