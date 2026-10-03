using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>
    /// Un ejercicio dentro de un trabajo de gimnasio, con sus series.
    /// Por ejemplo, sentadillas con cuatro series.
    /// </summary>
    public class TrabajoEjercicio
    {
        public int Id { get; set; }

        public int TrabajoId { get; set; }
        public Trabajo Trabajo { get; set; } = null!;

        [MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Orden en que se cargaron los ejercicios dentro del trabajo.</summary>
        public int Orden { get; set; }

        public ICollection<TrabajoSerie> Series { get; set; } = new List<TrabajoSerie>();
    }
}
