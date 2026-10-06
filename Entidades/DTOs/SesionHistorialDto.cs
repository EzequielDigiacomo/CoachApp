using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>
    /// Una sesion del historial del atleta. Entra cuando la asistencia esta
    /// marcada, haya o no trabajos cargados.
    /// </summary>
    public class SesionHistorialDto
    {
        public int EntrenamientoId { get; set; }

        public DateOnly Fecha { get; set; }

        public Turno Turno { get; set; }

        public int Sesion { get; set; }

        public bool Asistio { get; set; }
    }
}
