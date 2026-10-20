using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>Ejercicio dentro de un control de gimnasio, con sus series.</summary>
    public class ControlEjercicio
    {
        public int Id { get; set; }

        public int ControlId { get; set; }
        public Control Control { get; set; } = null!;

        [MaxLength(120)]
        public string Nombre { get; set; } = string.Empty;

        public int Orden { get; set; }

        public ICollection<ControlSerie> Series { get; set; } = new List<ControlSerie>();
    }
}
