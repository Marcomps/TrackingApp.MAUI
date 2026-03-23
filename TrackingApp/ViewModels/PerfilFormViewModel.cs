using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

[QueryProperty(nameof(PerfilId), "id")]
public class PerfilFormViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _perfilId;
    private Perfil? _perfilExistente;

    public PerfilFormViewModel()
    {
        GuardarCommand  = new Command(async () => await GuardarAsync(), () => EsValido);
        CancelarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    // ── QueryProperty ─────────────────────────────────────────────────────────

    public int PerfilId
    {
        set
        {
            _perfilId = value;
            if (value > 0) CargarPerfilExistente(value);
        }
    }

    // ── Título dinámico ───────────────────────────────────────────────────────

    public string Titulo => _perfilId > 0 ? "Editar perfil" : "Nuevo perfil";

    // ── Opciones de Pickers ───────────────────────────────────────────────────

    public IReadOnlyList<string> OpcionesTipo    { get; } = new[] { "Bebé / Recién nacido", "Adulto general" };
    public IReadOnlyList<string> OpcionesSexo    { get; } = new[] { "No especificado", "Masculino", "Femenino" };
    public IReadOnlyList<string> OpcionesSistema { get; } = new[] { "Métrico (kg, cm, ml)", "Imperial (lb, in, fl oz)" };

    // ── Campos ────────────────────────────────────────────────────────────────

    private string _nombre = string.Empty;
    public string Nombre
    {
        get => _nombre;
        set { _nombre = value; OnPropertyChanged(); ValidarYNotificar(); }
    }

    private int _tipoIndex;
    public int TipoIndex
    {
        get => _tipoIndex;
        set
        {
            _tipoIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EsBebe));
        }
    }

    private int _sexoIndex;
    public int SexoIndex
    {
        get => _sexoIndex;
        set { _sexoIndex = value; OnPropertyChanged(); }
    }

    private int _sistemaIndex;
    public int SistemaIndex
    {
        get => _sistemaIndex;
        set { _sistemaIndex = value; OnPropertyChanged(); }
    }

    private bool _tieneFecha;
    public bool TieneFecha
    {
        get => _tieneFecha;
        set { _tieneFecha = value; OnPropertyChanged(); }
    }

    private DateTime _fechaNacimiento = DateTime.Today;
    public DateTime FechaNacimiento
    {
        get => _fechaNacimiento;
        set { _fechaNacimiento = value; OnPropertyChanged(); }
    }

    private string _semanasTexto = string.Empty;
    public string SemanasTexto
    {
        get => _semanasTexto;
        set { _semanasTexto = value; OnPropertyChanged(); }
    }

    private string _notas = string.Empty;
    public string Notas
    {
        get => _notas;
        set { _notas = value; OnPropertyChanged(); }
    }

    // ── Computed ──────────────────────────────────────────────────────────────

    public bool EsBebe => _tipoIndex == 0;

    // ── Validación ────────────────────────────────────────────────────────────

    private string _errorNombre = string.Empty;
    public string ErrorNombre
    {
        get => _errorNombre;
        private set { _errorNombre = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneErrorNombre)); }
    }
    public bool TieneErrorNombre => !string.IsNullOrEmpty(_errorNombre);

    private bool EsValido => !string.IsNullOrWhiteSpace(_nombre) && _nombre.Length <= 60;

    private void ValidarYNotificar()
    {
        ErrorNombre = string.IsNullOrWhiteSpace(_nombre) ? "El nombre es obligatorio." :
                      _nombre.Length > 60               ? "Máximo 60 caracteres." : string.Empty;
        ((Command)GuardarCommand).ChangeCanExecute();
    }

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand GuardarCommand  { get; }
    public ICommand CancelarCommand { get; }

    // ── Carga (modo edición) ──────────────────────────────────────────────────

    private void CargarPerfilExistente(int id)
    {
        _perfilExistente = AppServices.DataService.Perfiles.FirstOrDefault(p => p.Id == id);
        if (_perfilExistente == null) return;

        _nombre      = _perfilExistente.Nombre;
        _tipoIndex   = _perfilExistente.TipoPerfil == TipoPerfil.Bebe ? 0 : 1;
        _sexoIndex   = _perfilExistente.Sexo switch
        {
            Sexo.Masculino => 1,
            Sexo.Femenino  => 2,
            _              => 0
        };
        _sistemaIndex       = _perfilExistente.SistemaUnidades == SistemaUnidades.Imperial ? 1 : 0;
        _tieneFecha         = _perfilExistente.FechaNacimiento.HasValue;
        _fechaNacimiento    = _perfilExistente.FechaNacimiento ?? DateTime.Today;
        _semanasTexto       = _perfilExistente.SemanasGestacion?.ToString() ?? string.Empty;
        _notas              = _perfilExistente.Notas ?? string.Empty;

        OnPropertyChanged(nameof(Titulo));
        OnPropertyChanged(nameof(Nombre));
        OnPropertyChanged(nameof(TipoIndex));
        OnPropertyChanged(nameof(EsBebe));
        OnPropertyChanged(nameof(SexoIndex));
        OnPropertyChanged(nameof(SistemaIndex));
        OnPropertyChanged(nameof(TieneFecha));
        OnPropertyChanged(nameof(FechaNacimiento));
        OnPropertyChanged(nameof(SemanasTexto));
        OnPropertyChanged(nameof(Notas));
    }

    // ── Guardar ───────────────────────────────────────────────────────────────

    private async Task GuardarAsync()
    {
        if (!EsValido) return;

        var perfil = _perfilExistente ?? new Perfil();
        perfil.Nombre          = _nombre.Trim();
        perfil.TipoPerfil      = _tipoIndex == 0 ? TipoPerfil.Bebe : TipoPerfil.AdultoGeneral;
        perfil.Sexo            = _sexoIndex switch { 1 => Sexo.Masculino, 2 => Sexo.Femenino, _ => Sexo.NoEspecificado };
        perfil.SistemaUnidades = _sistemaIndex == 1 ? SistemaUnidades.Imperial : SistemaUnidades.Metrico;
        perfil.FechaNacimiento = _tieneFecha ? _fechaNacimiento : null;
        perfil.SemanasGestacion = EsBebe && int.TryParse(_semanasTexto, out int sem) ? sem : null;
        perfil.Notas            = string.IsNullOrWhiteSpace(_notas) ? null : _notas.Trim();

        if (_perfilExistente != null)
            await AppServices.DataService.UpdatePerfilAsync(perfil);
        else
            await AppServices.DataService.AddPerfilAsync(perfil);

        await Shell.Current.GoToAsync("..");
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
