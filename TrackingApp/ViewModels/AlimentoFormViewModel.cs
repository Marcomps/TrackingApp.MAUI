using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

[QueryProperty(nameof(EntradaId), "id")]
public class AlimentoFormViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _entradaId;
    private FoodEntry? _entradaExistente;

    // ── Opciones de Picker ────────────────────────────────────────────────────

    public IReadOnlyList<string> TiposAlimentacion { get; } =
        new[] { "Fórmula / Biberón", "Lactancia materna", "Alimentación sólida", "Personalizado" };

    public IReadOnlyList<string> OpcionesPecho { get; } =
        new[] { "Izquierdo", "Derecho", "Ambos" };

    private List<int> _perfilesIds = new();
    public IReadOnlyList<string> OpcionesPerfiles { get; private set; } = new List<string>();

    private int _perfilIndex;
    public int PerfilIndex
    {
        get => _perfilIndex;
        set { _perfilIndex = value; OnPropertyChanged(); }
    }

    public AlimentoFormViewModel()
    {
        GuardarCommand  = new Command(async () => await GuardarAsync());
        CancelarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        _fecha = DateTime.Today;
        _hora  = DateTime.Now.TimeOfDay;

        // Cargar perfiles para el picker
        var perfiles = AppServices.DataService.Perfiles.ToList();
        _perfilesIds = new List<int> { 0 };
        var nombres  = new List<string> { "(Sin perfil)" };
        foreach (var p in perfiles)
        {
            _perfilesIds.Add(p.Id);
            nombres.Add(p.Nombre);
        }
        OpcionesPerfiles = nombres;

        var activeId  = AppServices.DataService.PerfilActivo?.Id ?? 0;
        var activeIdx = _perfilesIds.IndexOf(activeId);
        _perfilIndex  = activeIdx >= 0 ? activeIdx : 0;
    }

    // ── QueryProperty ─────────────────────────────────────────────────────────

    public int EntradaId
    {
        set
        {
            _entradaId = value;
            if (value > 0) CargarEntradaExistente(value);
        }
    }

    public string Titulo => _entradaId > 0 ? "Editar registro" : "Nuevo registro";

    // ── Tipo de alimentación ──────────────────────────────────────────────────

    private int _tipoIndex;
    public int TipoIndex
    {
        get => _tipoIndex;
        set
        {
            _tipoIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EsLactancia));
            OnPropertyChanged(nameof(EsFormula));
            OnPropertyChanged(nameof(EsSolido));
            OnPropertyChanged(nameof(EsPersonalizado));
        }
    }

    public bool EsLactancia    => _tipoIndex == 1;
    public bool EsFormula      => _tipoIndex == 0;
    public bool EsSolido       => _tipoIndex == 2;
    public bool EsPersonalizado => _tipoIndex == 3;

    private string _nombrePersonalizado = string.Empty;
    public string NombrePersonalizado
    {
        get => _nombrePersonalizado;
        set { _nombrePersonalizado = value; OnPropertyChanged(); }
    }

    // ── Campos comunes ────────────────────────────────────────────────────────

    private DateTime _fecha;
    public DateTime Fecha
    {
        get => _fecha;
        set { _fecha = value; OnPropertyChanged(); }
    }

    private TimeSpan _hora;
    public TimeSpan Hora
    {
        get => _hora;
        set { _hora = value; OnPropertyChanged(); }
    }

    private string _notas = string.Empty;
    public string Notas
    {
        get => _notas;
        set { _notas = value; OnPropertyChanged(); }
    }

    // ── Campos Lactancia ──────────────────────────────────────────────────────

    private int _pechoIndex;
    public int PechoIndex
    {
        get => _pechoIndex;
        set { _pechoIndex = value; OnPropertyChanged(); }
    }

    private string _duracionTexto = string.Empty;
    public string DuracionTexto
    {
        get => _duracionTexto;
        set { _duracionTexto = value; OnPropertyChanged(); }
    }

    // ── Campos Fórmula ────────────────────────────────────────────────────────

    private string _cantidadMlTexto = string.Empty;
    public string CantidadMlTexto
    {
        get => _cantidadMlTexto;
        set { _cantidadMlTexto = value; OnPropertyChanged(); }
    }

    public List<string> UnidadesFormula { get; } = new() { "ml", "oz", "cc" };

    private string _unidadFormula = "ml";
    public string UnidadFormula
    {
        get => _unidadFormula;
        set { _unidadFormula = value ?? "ml"; OnPropertyChanged(); }
    }

    // ── Campos Sólido ─────────────────────────────────────────────────────────

    private string _cantidadGramosTexto = string.Empty;
    public string CantidadGramosTexto
    {
        get => _cantidadGramosTexto;
        set { _cantidadGramosTexto = value; OnPropertyChanged(); }
    }

    public List<string> UnidadesSolido { get; } = new() { "g", "oz", "cucharadas", "tazas", "porción" };

    private string _unidadSolido = "g";
    public string UnidadSolido
    {
        get => _unidadSolido;
        set { _unidadSolido = value ?? "g"; OnPropertyChanged(); }
    }

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand GuardarCommand  { get; }
    public ICommand CancelarCommand { get; }

    // ── Lógica ─────────────────────────────────────────────────────────────────

    private void CargarEntradaExistente(int id)
    {
        var e = AppServices.DataService.FoodEntries.FirstOrDefault(x => x.Id == id);
        if (e == null) return;

        _entradaExistente = e;
        Fecha = e.Time.Date;
        Hora  = e.Time.TimeOfDay;
        Notas = e.Notas ?? string.Empty;

        TipoIndex = e.TipoAlimentacion switch
        {
            TipoAlimentacion.Formula       => 0,
            TipoAlimentacion.Lactancia     => 1,
            TipoAlimentacion.Solido        => 2,
            TipoAlimentacion.Personalizado => 3,
            _                              => 0
        };

        if (e.TipoAlimentacion == TipoAlimentacion.Personalizado)
            NombrePersonalizado = e.FoodType ?? string.Empty;

        PechoIndex = e.Pecho switch
        {
            PechoLactancia.Izquierdo => 0,
            PechoLactancia.Derecho   => 1,
            PechoLactancia.Ambos     => 2,
            _                        => 0
        };

        DuracionTexto       = e.DuracionMinutos?.ToString() ?? string.Empty;
        CantidadMlTexto     = e.CantidadMl?.ToString("F0") ?? string.Empty;
        CantidadGramosTexto = e.CantidadGramos?.ToString("F0") ?? string.Empty;

        // Cargar unidad guardada
        if (e.TipoAlimentacion == TipoAlimentacion.Formula && !string.IsNullOrWhiteSpace(e.Unit) && e.Unit != "min")
            UnidadFormula = e.Unit;
        if (e.TipoAlimentacion == TipoAlimentacion.Solido && !string.IsNullOrWhiteSpace(e.Unit) && e.Unit != "min")
            UnidadSolido = e.Unit;

        var perfilIdx = _perfilesIds.IndexOf(e.PerfilId);
        PerfilIndex   = perfilIdx >= 0 ? perfilIdx : 0;

        OnPropertyChanged(nameof(Titulo));
    }

    private async Task GuardarAsync()
    {
        var tipo = _tipoIndex switch
        {
            0 => TipoAlimentacion.Formula,
            1 => TipoAlimentacion.Lactancia,
            2 => TipoAlimentacion.Solido,
            3 => TipoAlimentacion.Personalizado,
            _ => TipoAlimentacion.Formula
        };

        int? duracion   = null;
        decimal? canMl  = null;
        decimal? canGrs = null;

        if (tipo == TipoAlimentacion.Lactancia)
        {
            if (int.TryParse(_duracionTexto, out int d) && d > 0)
                duracion = d;
        }
        else if (tipo == TipoAlimentacion.Formula)
        {
            if (decimal.TryParse(_cantidadMlTexto,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal ml) && ml > 0)
                canMl = ml;
        }
        else
        {
            if (decimal.TryParse(_cantidadGramosTexto,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal g) && g > 0)
                canGrs = g;
        }

        var perfilId = _perfilIndex >= 0 && _perfilIndex < _perfilesIds.Count
            ? _perfilesIds[_perfilIndex] : 0;
        var fechaHora = _fecha.Date.Add(_hora);

        var pecho = (PechoLactancia)_pechoIndex;

        if (_entradaExistente != null)
        {
            _entradaExistente.TipoAlimentacion = tipo;
            _entradaExistente.Pecho            = tipo == TipoAlimentacion.Lactancia ? pecho : null;
            _entradaExistente.DuracionMinutos  = duracion;
            _entradaExistente.CantidadMl       = canMl;
            _entradaExistente.CantidadGramos   = canGrs;
            _entradaExistente.Time             = fechaHora;
            _entradaExistente.Notas            = string.IsNullOrWhiteSpace(_notas) ? null : _notas;
            _entradaExistente.PerfilId         = perfilId;

            // Keep legacy fields in sync
            _entradaExistente.FoodType = tipo switch
            {
                TipoAlimentacion.Lactancia     => "Lactancia",
                TipoAlimentacion.Formula       => "Fórmula",
                TipoAlimentacion.Solido        => "Sólido",
                TipoAlimentacion.Personalizado => string.IsNullOrWhiteSpace(_nombrePersonalizado) ? "Personalizado" : _nombrePersonalizado,
                _                              => "Alimento"
            };
            _entradaExistente.Amount = (double)(canMl ?? canGrs ?? (duracion.HasValue ? (decimal)duracion.Value : 0m));
            _entradaExistente.Unit   = tipo == TipoAlimentacion.Lactancia ? "min" :
                                       tipo == TipoAlimentacion.Formula   ? _unidadFormula :
                                       tipo == TipoAlimentacion.Solido    ? _unidadSolido : string.Empty;

            await AppServices.DataService.UpdateFoodEntryAsync(_entradaExistente);
        }
        else
        {
            var nuevaEntrada = new FoodEntry
            {
                TipoAlimentacion = tipo,
                Pecho            = tipo == TipoAlimentacion.Lactancia ? pecho : null,
                DuracionMinutos  = duracion,
                CantidadMl       = canMl,
                CantidadGramos   = canGrs,
                Time             = fechaHora,
                Notas            = string.IsNullOrWhiteSpace(_notas) ? null : _notas,
                PerfilId         = perfilId,

                // Legacy fields
                FoodType = tipo switch
                {
                    TipoAlimentacion.Lactancia     => "Lactancia",
                    TipoAlimentacion.Formula       => "Fórmula",
                    TipoAlimentacion.Solido        => "Sólido",
                    TipoAlimentacion.Personalizado => string.IsNullOrWhiteSpace(_nombrePersonalizado) ? "Personalizado" : _nombrePersonalizado,
                    _                              => "Alimento"
                },
                Amount = (double)(canMl ?? canGrs ?? (duracion.HasValue ? (decimal)duracion.Value : 0m)),
                Unit   = tipo == TipoAlimentacion.Lactancia ? "min" :
                         tipo == TipoAlimentacion.Formula   ? _unidadFormula :
                         tipo == TipoAlimentacion.Solido    ? _unidadSolido : string.Empty
            };
            await AppServices.DataService.AddFoodEntryAsync(nuevaEntrada);
        }

        await Shell.Current.GoToAsync("..");
    }

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
