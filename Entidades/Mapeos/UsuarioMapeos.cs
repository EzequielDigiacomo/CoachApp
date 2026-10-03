using Entidades.DTOs;

namespace Entidades.Mapeos
{
    /// <summary>Conversion de la entidad Usuario a su DTO publico.</summary>
    public static class UsuarioMapeos
    {
        /// <summary>
        /// Arma el DTO del usuario. No incluye el hash de la contrasena.
        /// </summary>
        public static UsuarioDto ToDto(this Usuario usuario) => new()
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Rol = usuario.Rol,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            UltimoAcceso = usuario.UltimoAcceso
        };
    }
}
