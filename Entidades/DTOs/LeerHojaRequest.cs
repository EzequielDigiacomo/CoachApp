using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>La planilla de Google Sheets que se quiere leer.</summary>
    public class LeerHojaRequest
    {
        [Required(ErrorMessage = "Pegá el link de la planilla.")]
        [MaxLength(1000, ErrorMessage = "El link es demasiado largo.")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de la pestaña a leer. Lo elige el entrenador de la lista que
        /// devuelve el listado de pestañas. No hace falta para listarlas.
        /// </summary>
        [MaxLength(200)]
        public string? Pestana { get; set; }

        /// <summary>
        /// Celdas a leer, del estilo "B4:E49". Si no viene, se lee la pestaña
        /// entera. Sirve para mostrar solo la parte que interesa.
        /// </summary>
        [MaxLength(50, ErrorMessage = "Las celdas se escriben como B4:E49.")]
        public string? Rango { get; set; }
    }
}
