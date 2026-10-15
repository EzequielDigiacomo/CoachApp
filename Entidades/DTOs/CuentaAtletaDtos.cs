using System.ComponentModel.DataAnnotations;

namespace Entidades.DTOs
{
    /// <summary>Atleta y, si existe, el login asociado.</summary>
    public class CuentaAtletaDto
    {
        public int AtletaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string? EmailAtleta { get; set; }
        public int? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Email { get; set; }
        public bool? Activo { get; set; }
    }

    public class CrearCuentaAtletaRequest
    {
        public int AtletaId { get; set; }

        [Required, MaxLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class EditarCuentaAtletaRequest
    {
        [Required, MaxLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }

    public class CambiarPasswordCuentaRequest
    {
        [Required, MinLength(6), MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
