using System.ComponentModel.DataAnnotations;

namespace Entidades
{
    /// <summary>
    /// Sesion de Garmin Connect del entrenador. La contrasena no se guarda:
    /// solo los tokens cifrados con los que se leen las actividades de sus amigos.
    /// </summary>
    public class CuentaGarmin
    {
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? NombreCompleto { get; set; }

        /// <summary>Token de acceso cifrado.</summary>
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>Token de refresco cifrado.</summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string ClientId { get; set; } = string.Empty;

        public DateTime ExpiraUtc { get; set; }
    }
}
