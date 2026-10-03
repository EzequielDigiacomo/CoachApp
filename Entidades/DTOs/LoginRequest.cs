using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    public class LoginRequest
    {
        [Required, MaxLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
