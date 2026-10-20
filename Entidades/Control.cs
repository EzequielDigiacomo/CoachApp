using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Control de fuerza (u otro tipo) independiente de una sesion de entrenamiento.
    /// El detalle de gimnasio vive en ejercicios y series.
    /// </summary>
    public class Control
    {
        public int Id { get; set; }

        public int AtletaId { get; set; }
        public Atleta Atleta { get; set; } = null!;

        /// <summary>Gimnasio, tierra o agua.</summary>
        public TipoTrabajo Tipo { get; set; }

        public DateOnly Fecha { get; set; }

        [MaxLength(300)]
        public string? Observaciones { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public ICollection<ControlEjercicio> Ejercicios { get; set; } = new List<ControlEjercicio>();
    }
}
