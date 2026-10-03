using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Una muestra de paladas por minuto que viene del formulario.</summary>
    public class GuardarPaladaRequest
    {
        /// <summary>Momento de la toma, contado desde el inicio del trabajo: "15" o "1:40".</summary>
        public string Tiempo { get; set; } = string.Empty;

        [Range(1, 400, ErrorMessage = "Las paladas por minuto tienen que estar entre 1 y 400.")]
        public int Ppm { get; set; }
    }
}
