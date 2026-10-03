using System.ComponentModel.DataAnnotations;
using Entidades.Enums;

namespace Entidades
{
    /// <summary>
    /// Usuario del sistema. Las cuentas se crean desde el panel de superadmin.
    /// </summary>
    public class Usuario
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Hash de la contrasena (nunca se guarda en texto plano).</summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        public RolUsuario Rol { get; set; } = RolUsuario.Entrenador;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoAcceso { get; set; }
    }
}
