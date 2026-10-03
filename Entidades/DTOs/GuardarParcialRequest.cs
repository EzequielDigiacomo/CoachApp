using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Un parcial que viene del formulario.</summary>
    public class GuardarParcialRequest
    {
        [Range(1, 100000, ErrorMessage = "La distancia del parcial tiene que estar entre 1 y 100000 metros.")]
        public int DistanciaMetros { get; set; }

        /// <summary>Tiempo escrito a mano: "2:12" o "1:02:03".</summary>
        public string Tiempo { get; set; } = string.Empty;
    }
}
