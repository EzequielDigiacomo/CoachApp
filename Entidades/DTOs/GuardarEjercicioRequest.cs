using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Un ejercicio que viene del formulario de gimnasio.</summary>
    public class GuardarEjercicioRequest
    {
        [Required(ErrorMessage = "Escribí el nombre del ejercicio.")]
        [MaxLength(120, ErrorMessage = "El nombre del ejercicio no puede superar los 120 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Series del ejercicio, en el orden en que se cargaron.</summary>
        public List<GuardarSerieRequest> Series { get; set; } = [];
    }
}
