namespace TrackingApp.Models
{
    // ─── RF-001 Perfiles ───────────────────────────────────────────────────────

    public enum TipoPerfil
    {
        Bebe,
        AdultoGeneral
    }

    public enum Sexo
    {
        Masculino,
        Femenino,
        NoEspecificado
    }

    public enum SistemaUnidades
    {
        Metrico,
        Imperial
    }

    // ─── RF-003 Alimentación ───────────────────────────────────────────────────

    public enum TipoAlimentacion
    {
        Lactancia,
        Formula,
        Solido
    }

    public enum PechoLactancia
    {
        Izquierdo,
        Derecho,
        Ambos
    }

    // ─── RF-004 Citas ─────────────────────────────────────────────────────────

    public enum CategoriaCita
    {
        ControlCrecimientoPeso,
        Vacunacion,
        Pediatria,
        Oftalmologia,
        Odontologia,
        Laboratorio,
        Otra
    }

    public enum EstadoCita
    {
        Pendiente,
        Completada,
        Cancelada
    }

    // ─── RF-005 Unidades ──────────────────────────────────────────────────────

    public enum UnitType
    {
        // Peso (base: gramos)
        Gramos,
        Kilogramos,
        Libras,
        Onzas,

        // Longitud (base: centímetros)
        Centimetros,
        Metros,
        Pulgadas,
        Pies,

        // Volumen (base: mililitros)
        Mililitros,
        Litros,
        OnzasLiquidas,

        // Tiempo
        Minutos
    }

    public enum MeasureCategory
    {
        Peso,
        Longitud,
        Volumen,
        Tiempo
    }
}
