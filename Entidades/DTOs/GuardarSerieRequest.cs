using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Una serie que viene del formulario de gimnasio.</summary>
    public class GuardarSerieRequest
    {
        [Range(1, 1000, ErrorMessage = "Las repeticiones de cada serie tienen que estar entre 1 y 1000.")]
        public int Repeticiones { get; set; }

        [Range(1, 100, ErrorMessage = "El porcentaje tiene que estar entre 1 y 100.")]
        public int? Porcentaje { get; set; }

        [Range(0, 1000, ErrorMessage = "El peso tiene que estar entre 0 y 1000 kg.")]
        public decimal? PesoKg { get; set; }
    }
}
