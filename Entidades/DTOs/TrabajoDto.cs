using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>
    /// Un trabajo o control que hizo el atleta en la sesion. No lleva distancia
    /// ni tiempo propios: eso vive en cada parcial.
    /// </summary>
    public class TrabajoDto
    {
        public int Id { get; set; }

        public int EntrenamientoId { get; set; }

        public int AtletaId { get; set; }

        /// <summary>Lugar donde se hizo el trabajo: gimnasio, tierra o agua.</summary>
        public TipoTrabajo Tipo { get; set; }

        /// <summary>Hora del dia en que se hizo el trabajo ("09:30").</summary>
        public string? HoraInicio { get; set; }

        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; }

        /// <summary>Marcas de los trabajos de tierra y agua.</summary>
        public List<ParcialDto> Parciales { get; set; } = [];

        /// <summary>Ejercicios de los trabajos de gimnasio.</summary>
        public List<EjercicioDto> Ejercicios { get; set; } = [];

        /// <summary>
        /// Muestras de paladas por minuto de los trabajos de agua, en orden de toma.
        /// Cada parcial lleva ademas las que le corresponden por tiempo.
        /// </summary>
        public List<PaladaDto> Paladas { get; set; } = [];

        /// <summary>
        /// Datos de la sesion a la que pertenece el trabajo. Se completan
        /// siempre que el entrenamiento venga cargado (por ejemplo, en el
        /// historial del atleta), y si no quedan en null.
        /// </summary>
        public DateOnly? EntrenamientoFecha { get; set; }

        public Turno? EntrenamientoTurno { get; set; }

        public int? EntrenamientoSesion { get; set; }
    }
}
