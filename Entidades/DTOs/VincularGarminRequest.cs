using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Credenciales de Garmin Connect. La contrasena no se persiste.</summary>
    public class VincularGarminRequest
    {
        [Required, MaxLength(200)]
        public string Usuario { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Password { get; set; } = string.Empty;
    }
}
