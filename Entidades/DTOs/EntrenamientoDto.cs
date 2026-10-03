using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>Datos publicos de una sesion de entrenamiento.</summary>
    public class EntrenamientoDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public Turno Turno { get; set; }

        /// <summary>Numero de sesion dentro del turno: 1 a 5.</summary>
        public int Sesion { get; set; }

        public string? Descripcion { get; set; }
        public string? Club { get; set; }
        public DateTime FechaCreacion { get; set; }

        public int CantidadAtletas { get; set; }
        public int CantidadPresentes { get; set; }
        public int CantidadAusentes { get; set; }
        public int CantidadSinMarcar { get; set; }

        /// <summary>
        /// Atletas asignados. En el listado viene vacio para no cargar de mas;
        /// en el detalle viene completo.
        /// </summary>
        public List<AtletaEnEntrenamientoDto> Atletas { get; set; } = [];
    }
}
