using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Sesion de entrenamiento identificada por fecha, turno y numero de sesion.
    /// </summary>
    public class Entrenamiento
    {
        public int Id { get; set; }

        public DateOnly Fecha { get; set; }

        public Turno Turno { get; set; }

        /// <summary>Numero de sesion dentro del turno: 1 a 5.</summary>
        public int Sesion { get; set; }

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? Club { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>Atletas que participan de la sesion (relacion N:N).</summary>
        public ICollection<EntrenamientoAtleta> Atletas { get; set; } = new List<EntrenamientoAtleta>();

        /// <summary>Anotaciones asociadas a la sesion de entrenamiento.</summary>
        public ICollection<Anotacion> Anotaciones { get; set; } = new List<Anotacion>();
    }
}
