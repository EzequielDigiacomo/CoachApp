using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades.DTOs
{
    /// <summary>Datos necesarios para que el superadmin cree una cuenta.</summary>
    public class CrearUsuarioRequest
    {
        [Required, MaxLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; } = RolUsuario.Entrenador;
    }
}
