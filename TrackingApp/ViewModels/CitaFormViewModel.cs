using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackingApp.Models;
using TrackingApp.Services;

namespace TrackingApp.ViewModels;

[QueryProperty(nameof(CitaId), "id")]
public class CitaFormViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _citaId;
    private MedicalAppointment? _citaExistente;

    // ── Opciones de Picker ────────────────────────────────────────────────────

    public IReadOnlyList<string> Categorias { get; } = new[]
    {
        "Pediatría / Medicina general",
        "Vacunación",
        "Control de crecimiento / peso",
        "Oftalmología",
        "Odontología",
        "Laboratorio / Exámenes",
        "Otra"
    };

    public IReadOnlyList<string> Estados { get; } =
        new[] { "Pendiente", "Completada", "Cancelada" };

    public IReadOnlyList<string> OpcionesRecordatorio { get; } =
        new[] { "Sin recordatorio", "15 min antes", "30 min antes", "1 hora antes", "2 horas antes", "1 día antes", "2 días antes", "3 días antes", "1 semana antes" };

    private static readonly int?[] _recordatorioValores =
        { null, 15, 30, 60, 120, 1440, 2880, 4320, 10080, 2880, 4320, 10080 };

    public CitaFormViewModel()
    {
        GuardarCommand  = new Command(async () => await GuardarAsync());
        CancelarCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

        _fecha = DateTime.Today;
        _hora  = DateTime.Now.TimeOfDay;
    }

    // ── QueryProperty ─────────────────────────────────────────────────────────

    public int CitaId
    {
        set
        {
            _citaId = value;
            if (value > 0) CargarCitaExistente(value);
        }
    }

    public string Titulo => _citaId > 0 ? "Editar cita" : "Nueva cita";

    // ── Campos del formulario ─────────────────────────────────────────────────

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    private string _descripcion = string.Empty;
    public string Descripcion
    {
        get => _descripcion;
        set { _descripcion = value; OnPropertyChanged(); }
    }

    private int _categoriaIndex;
    public int CategoriaIndex
    {
        get => _categoriaIndex;
        set { _categoriaIndex = value; OnPropertyChanged(); }
    }

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

    private string _lugarYDoctor = string.Empty;
    public string LugarYDoctor
    {
        get => _lugarYDoctor;
        set { _lugarYDoctor = value; OnPropertyChanged(); }
    }

    private int _estadoIndex;
    public int EstadoIndex
    {
        get => _estadoIndex;
        set { _estadoIndex = value; OnPropertyChanged(); }
    }

    private int _recordatorioIndex;
    public int RecordatorioIndex
    {
        get => _recordatorioIndex;
        set { _recordatorioIndex = value; OnPropertyChanged(); }
    }

    private string _notasPostCita = string.Empty;
    public string NotasPostCita
    {
        get => _notasPostCita;
        set { _notasPostCita = value; OnPropertyChanged(); }
    }

    // ── Comandos ──────────────────────────────────────────────────────────────

    public ICommand GuardarCommand  { get; }
    public ICommand CancelarCommand { get; }

    // ── Lógica ────────────────────────────────────────────────────────────────

    private void CargarCitaExistente(int id)
    {
        var c = AppServices.DataService.Appointments.FirstOrDefault(x => x.Id == id);
        if (c == null) return;

        _citaExistente = c;
        Title          = c.Title;
        Descripcion    = c.Description;
        LugarYDoctor   = c.LugarYDoctor ?? (string.IsNullOrWhiteSpace(c.Doctor) ? c.Location : c.Doctor);
        Fecha          = c.AppointmentDate.Date;
        Hora           = c.AppointmentDate.TimeOfDay;
        NotasPostCita  = c.NotasPostCita ?? string.Empty;

        CategoriaIndex = c.Categoria switch
        {
            CategoriaCita.Pediatria               => 0,
            CategoriaCita.Vacunacion              => 1,
            CategoriaCita.ControlCrecimientoPeso  => 2,
            CategoriaCita.Oftalmologia            => 3,
            CategoriaCita.Odontologia             => 4,
            CategoriaCita.Laboratorio             => 5,
            CategoriaCita.Otra                    => 6,
            _                                     => 0
        };

        EstadoIndex = c.Estado switch
        {
            EstadoCita.Pendiente  => 0,
            EstadoCita.Completada => 1,
            EstadoCita.Cancelada  => 2,
            _                     => 0
        };

        RecordatorioIndex = c.RecordatorioMinutos.HasValue
            ? Array.FindIndex(_recordatorioValores, v => v == c.RecordatorioMinutos.Value)
              .Let(i => i < 0 ? 0 : i)
            : 0;

        OnPropertyChanged(nameof(Titulo));
    }

    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(_title))
        {
            await Shell.Current.DisplayAlert("Campo requerido", "El título de la cita es obligatorio.", "OK");
            return;
        }

        var categoria = _categoriaIndex switch
        {
            0 => CategoriaCita.Pediatria,
            1 => CategoriaCita.Vacunacion,
            2 => CategoriaCita.ControlCrecimientoPeso,
            3 => CategoriaCita.Oftalmologia,
            4 => CategoriaCita.Odontologia,
            5 => CategoriaCita.Laboratorio,
            6 => CategoriaCita.Otra,
            _ => CategoriaCita.Pediatria
        };

        var estado = _estadoIndex switch
        {
            0 => EstadoCita.Pendiente,
            1 => EstadoCita.Completada,
            2 => EstadoCita.Cancelada,
            _ => EstadoCita.Pendiente
        };

        var recordatorio = _recordatorioIndex >= 0 && _recordatorioIndex < _recordatorioValores.Length
            ? _recordatorioValores[_recordatorioIndex]
            : null;

        var perfilId  = AppServices.DataService.PerfilActivo?.Id ?? 0;
        var fechaHora = _fecha.Date.Add(_hora);
        MedicalAppointment? nuevaCita = null;

        if (_citaExistente != null)
        {
            _citaExistente.Title             = _title.Trim();
            _citaExistente.Description       = _descripcion.Trim();
            _citaExistente.Categoria         = categoria;
            _citaExistente.Estado            = estado;
            _citaExistente.AppointmentDate   = fechaHora;
            _citaExistente.LugarYDoctor      = string.IsNullOrWhiteSpace(_lugarYDoctor) ? null : _lugarYDoctor.Trim();
            _citaExistente.NotasPostCita     = string.IsNullOrWhiteSpace(_notasPostCita) ? null : _notasPostCita.Trim();
            _citaExistente.RecordatorioMinutos = recordatorio;
            _citaExistente.PerfilId          = perfilId;

            // Legacy fields
            _citaExistente.Doctor   = _lugarYDoctor.Trim();
            _citaExistente.Location = _lugarYDoctor.Trim();

            await AppServices.DataService.UpdateAppointmentAsync(_citaExistente);
        }
        else
        {
            nuevaCita = new MedicalAppointment
            {
                Title              = _title.Trim(),
                Description        = _descripcion.Trim(),
                Categoria          = categoria,
                Estado             = estado,
                AppointmentDate    = fechaHora,
                LugarYDoctor       = string.IsNullOrWhiteSpace(_lugarYDoctor) ? null : _lugarYDoctor.Trim(),
                NotasPostCita      = string.IsNullOrWhiteSpace(_notasPostCita) ? null : _notasPostCita.Trim(),
                RecordatorioMinutos = recordatorio,
                PerfilId           = perfilId,

                // Legacy fields
                Doctor   = _lugarYDoctor.Trim(),
                Location = _lugarYDoctor.Trim()
            };
            await AppServices.DataService.AddAppointmentAsync(nuevaCita);
        }

        // Programar notificación si aplica
        var citaGuardada = _citaExistente ?? nuevaCita;
        NotificationService.Instance.CancelAppointmentNotification(citaGuardada.Id);
        if (recordatorio.HasValue)
        {
            await NotificationService.Instance.ScheduleAppointmentNotificationAsync(citaGuardada);
        }

        await Shell.Current.GoToAsync("..");
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// ── Helper for array FindIndex chaining ──────────────────────────────────────
file static class IntExtensions
{
    internal static int Let(this int value, Func<int, int> fn) => fn(value);
}
