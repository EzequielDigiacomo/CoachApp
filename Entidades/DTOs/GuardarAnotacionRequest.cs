using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>
    /// Datos para crear o modificar una anotacion. Se usa el mismo contrato en
    /// los dos casos: el modal guarda la nota completa.
    /// </summary>
    public class GuardarAnotacionRequest
    {
        [MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string? Titulo { get; set; }

        [Required(ErrorMessage = "Escribí el texto de la anotación.")]
        [MaxLength(4000, ErrorMessage = "La anotación no puede superar los 4000 caracteres.")]
        public string Texto { get; set; } = string.Empty;

        public int? AtletaId { get; set; }

        public int? EntrenamientoId { get; set; }

        /// <summary>
        /// Si viene, el atleta y la sesion se toman del trabajo, para que la
        /// anotacion no quede apuntando a un atleta distinto del trabajo.
        /// </summary>
        public int? TrabajoId { get; set; }
    }
}
