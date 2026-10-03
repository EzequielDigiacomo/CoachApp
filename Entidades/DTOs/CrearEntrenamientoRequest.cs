using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>Datos para crear una sesion de entrenamiento.</summary>
    public class CrearEntrenamientoRequest
    {
        [Required]
        public DateOnly Fecha { get; set; }

        [Required]
        public Turno Turno { get; set; }

        /// <summary>Numero de sesion dentro del turno: 1 a 5.</summary>
        [Range(1, 5, ErrorMessage = "La sesion tiene que ser un numero del 1 al 5.")]
        public int Sesion { get; set; }

        [MaxLength(500, ErrorMessage = "La descripcion no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        [MaxLength(50, ErrorMessage = "El club no puede superar los 50 caracteres.")]
        public string? Club { get; set; }

        /// <summary>
        /// Atletas que se asignan al crear la sesion. Es opcional:
        /// tambien se pueden agregar despues desde el detalle.
        /// </summary>
        public List<int> AtletaIds { get; set; } = [];
    }
}
