using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

[QueryProperty(nameof(RegistroId), "id")]
public class CrecimientoFormViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly UnitService _unitSvc = new();

    private int _registroId;
    private RegistroCrecimiento? _registroExistente;

    // ── Opciones de unidad para los Pickers ───────────────────────────────────
    public IReadOnlyList<UnitType> UnidadesPeso   { get; } = new[] { UnitType.Kilogramos, UnitType.Gramos, UnitType.Libras, UnitType.Onzas };
    public IReadOnlyList<UnitType> UnidadesTalla  { get; } = new[] { UnitType.Centimetros, UnitType.Pulgadas, UnitType.Pies, UnitType.Metros };
    public IReadOnlyList<string>   NombresPeso    { get; } = new[] { "kg", "g", "lb", "oz" };
    public IReadOnlyList<string>   NombresTalla   { get; } = new[] { "cm", "in", "ft", "m" };

    public CrecimientoFormViewModel()
    {
        GuardarCommand  = new Command(async () => await GuardarAsync(), () => EsValido);
        CancelarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        _fecha    = DateTime.Today;
        _hora     = DateTime.Now.TimeOfDay;
        _pesoTxt  = string.Empty;
        _tallaTxt = string.Empty;
        _perimTxt = string.Empty;
        _notas    = string.Empty;

        // Defecto según el SistemaUnidades del perfil activo
        var sistema = AppServices.DataService.PerfilActivo?.SistemaUnidades
                      ?? SistemaUnidades.Metrico;
        _unidadPesoIdx  = sistema == SistemaUnidades.Imperial ? 2 : 0; // lb o kg
        _unidadTallaIdx = sistema == SistemaUnidades.Imperial ? 1 : 0; // in o cm
    }

    // ── QueryProperty ─────────────────────────────────────────────────────────

    public int RegistroId
    {
        set
        {
            _registroId = value;
            if (value > 0) CargarRegistroExistente(value);
        }
    }

    // ── Título dinámico ───────────────────────────────────────────────────────

    public string Titulo => _registroId > 0 ? "Editar registro" : "Nuevo registro";

    // ── Campos del formulario ─────────────────────────────────────────────────

    private DateTime _fecha;
    public DateTime Fecha
    {
        get => _fecha;
        set { _fecha = value; OnPropertyChanged(); RecalcularIMC(); }
    }

    private TimeSpan _hora;
    public TimeSpan Hora
    {
        get => _hora;
        set { _hora = value; OnPropertyChanged(); }
    }

    private string _pesoTxt;
    public string PesoTexto
    {
        get => _pesoTxt;
        set { _pesoTxt = value; OnPropertyChanged(); RecalcularIMC(); ValidarYNotificar(); }
    }

    private string _tallaTxt;
    public string TallaTexto
    {
        get => _tallaTxt;
        set { _tallaTxt = value; OnPropertyChanged(); RecalcularIMC(); ValidarYNotificar(); }
    }

    // ── Selectores de unidad ──────────────────────────────────────────────────

    private int _unidadPesoIdx;
    public int UnidadPesoIndex
    {
        get => _unidadPesoIdx;
        set
        {
            _unidadPesoIdx = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimboloPeso));
            RecalcularIMC();
        }
    }

    private int _unidadTallaIdx;
    public int UnidadTallaIndex
    {
        get => _unidadTallaIdx;
        set
        {
            _unidadTallaIdx = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SimboloTalla));
            RecalcularIMC();
        }
    }

    private UnitType UnidadPesoActual  => UnidadesPeso[Math.Clamp(_unidadPesoIdx,  0, UnidadesPeso.Count  - 1)];
    private UnitType UnidadTallaActual => UnidadesTalla[Math.Clamp(_unidadTallaIdx, 0, UnidadesTalla.Count - 1)];

    public string SimboloPeso  => _unitSvc.GetSymbol(UnidadPesoActual);
    public string SimboloTalla => _unitSvc.GetSymbol(UnidadTallaActual);

    private string _perimTxt;
    public string PerimCefTexto
    {
        get => _perimTxt;
        set { _perimTxt = value; OnPropertyChanged(); }
    }

    private string _notas;
    public string Notas
    {
        get => _notas;
        set { _notas = value; OnPropertyChanged(); }
    }

    // ── IMC calculado en tiempo real ──────────────────────────────────────────

    private string _imcDisplay = "—";
    public string IMCDisplay
    {
        get => _imcDisplay;
        private set { _imcDisplay = value; OnPropertyChanged(); }
    }

    // ── Validación ────────────────────────────────────────────────────────────

    private string _errorPeso  = string.Empty;
    private string _errorTalla = string.Empty;

    public string ErrorPeso
    {
        get => _errorPeso;
        private set { _errorPeso = value; OnPropertyChanged(); OnPropertyChanged(nameof(TienePesoError)); }
    }
    public string ErrorTalla
    {
        get => _errorTalla;
        private set { _errorTalla = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneTallaError)); }
    }

    public bool TienePesoError  => !string.IsNullOrEmpty(_errorPeso);
    public bool TieneTallaError => !string.IsNullOrEmpty(_errorTalla);

    public bool EsValido =>
        TryParseDecimal(_pesoTxt,  out var p) && p > 0 &&
        TryParseDecimal(_tallaTxt, out var t) && t > 0;

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand GuardarCommand  { get; }
    public ICommand CancelarCommand { get; }

    // ── Lógica ────────────────────────────────────────────────────────────────

    private void CargarRegistroExistente(int id)
    {
        var r = AppServices.DataService.RegistrosCrecimiento.FirstOrDefault(x => x.Id == id);
        if (r == null) return;

        _registroExistente = r;
        Fecha       = r.Fecha.Date;
        Hora        = r.Fecha.TimeOfDay;

        // Convertir desde unidad base (gramos → unidad seleccionada)
        if (r.PesoGramos > 0)
            PesoTexto  = _unitSvc.FromBaseUnit(r.PesoGramos, UnidadPesoActual).ToString("F3");
        TallaTexto = _unitSvc.FromBaseUnit(r.TallaCm, UnidadTallaActual).ToString("F2");

        PerimCefTexto = r.PerimCefCm.HasValue ? r.PerimCefCm.Value.ToString("F1") : string.Empty;
        Notas       = r.Notas ?? string.Empty;

        OnPropertyChanged(nameof(Titulo));
    }

    private void RecalcularIMC()
    {
        if (TryParseDecimal(_pesoTxt, out var pesoEnUnidad) && pesoEnUnidad > 0 &&
            TryParseDecimal(_tallaTxt, out var tallaEnUnidad) && tallaEnUnidad > 0)
        {
            // Convertir a unidades base para calcular IMC (kg y metros)
            decimal pesoGramos = _unitSvc.ToBaseUnit(pesoEnUnidad, UnidadPesoActual);
            decimal tallaCm    = _unitSvc.ToBaseUnit(tallaEnUnidad, UnidadTallaActual);
            decimal pesoKg     = pesoGramos / 1000m;
            decimal tallaM     = tallaCm / 100m;
            if (tallaM > 0)
                IMCDisplay = $"{pesoKg / (tallaM * tallaM):F1} kg/m²";
        }
        else
        {
            IMCDisplay = "—";
        }
    }

    private void ValidarYNotificar()
    {
        if (!string.IsNullOrWhiteSpace(_pesoTxt))
        {
            bool ok = TryParseDecimal(_pesoTxt, out var v) && v > 0;
            ErrorPeso = ok ? string.Empty : "Ingresa un peso válido mayor a 0";
        }
        else
        {
            ErrorPeso = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(_tallaTxt))
        {
            bool ok = TryParseDecimal(_tallaTxt, out var v) && v > 0;
            ErrorTalla = ok ? string.Empty : "Ingresa una talla válida mayor a 0";
        }
        else
        {
            ErrorTalla = string.Empty;
        }
    }

    private async Task GuardarAsync()
    {
        if (!TryParseDecimal(_pesoTxt, out var pesoEnUnidad) || pesoEnUnidad <= 0)
        {
            ErrorPeso = "El peso es obligatorio y debe ser mayor a 0";
            return;
        }
        if (!TryParseDecimal(_tallaTxt, out var tallaEnUnidad) || tallaEnUnidad <= 0)
        {
            ErrorTalla = "La talla es obligatoria y debe ser mayor a 0";
            return;
        }

        // Convertir a unidades base para almacenamiento
        decimal pesoGramos = _unitSvc.ToBaseUnit(pesoEnUnidad, UnidadPesoActual);
        decimal tallaCm    = _unitSvc.ToBaseUnit(tallaEnUnidad, UnidadTallaActual);

        decimal? perimCef = null;
        if (!string.IsNullOrWhiteSpace(_perimTxt) && TryParseDecimal(_perimTxt, out var pc) && pc > 0)
            perimCef = pc; // perímetro cefálico siempre en cm

        var registro = _registroExistente ?? new RegistroCrecimiento();
        registro.Fecha        = Fecha.Date + Hora;
        registro.PesoGramos   = pesoGramos;
        registro.TallaCm      = tallaCm;
        registro.PerimCefCm   = perimCef;
        registro.Notas        = string.IsNullOrWhiteSpace(Notas) ? null : Notas.Trim();

        if (_registroExistente != null)
            await AppServices.DataService.UpdateRegistroCrecimientoAsync(registro);
        else
            await AppServices.DataService.AddRegistroCrecimientoAsync(registro);

        await Shell.Current.GoToAsync("..");
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var normalized = text.Replace(',', '.');
        return decimal.TryParse(normalized,
            System.Globalization.NumberStyles.AllowDecimalPoint,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
