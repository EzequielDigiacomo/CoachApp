using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>
    /// Una anotacion del entrenador. Los vinculos son opcionales: puede ser una
    /// nota general, del atleta, de la sesion o de un trabajo puntual.
    /// </summary>
    public class AnotacionDto
    {
        public int Id { get; set; }

        public string? Titulo { get; set; }

        public string Texto { get; set; } = string.Empty;

        /// <summary>Manual o generada a partir de un audio.</summary>
        public OrigenAnotacion Origen { get; set; }

        /// <summary>true cuando la transcripcion es dudosa y conviene revisarla.</summary>
        public bool RequiereRevision { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int? AtletaId { get; set; }
        public string? AtletaNombre { get; set; }
        public string? AtletaApellido { get; set; }

        public int? EntrenamientoId { get; set; }
        public DateOnly? EntrenamientoFecha { get; set; }
        public Turno? EntrenamientoTurno { get; set; }
        public int? EntrenamientoSesion { get; set; }

        public int? TrabajoId { get; set; }
        public TipoTrabajo? TrabajoTipo { get; set; }

        /// <summary>Hora del trabajo, ya normalizada para mostrar ("09:30").</summary>
        public string? TrabajoHoraInicio { get; set; }

        public int? UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
    }
}
