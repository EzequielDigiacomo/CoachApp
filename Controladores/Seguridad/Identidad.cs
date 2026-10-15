using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Entidades.Enums;

namespace Controladores.Seguridad
{
    /// <summary>Lee del token quien esta llamando y si es atleta o staff.</summary>
    public static class Identidad
    {
        public const string ClaimAtletaId = "atletaId";

        public const string RolesStaff =
            nameof(RolUsuario.SuperAdmin) + "," + nameof(RolUsuario.Entrenador);

        public static int? UsuarioId(ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? usuario.FindFirstValue("nameid");

            return int.TryParse(valor, out var id) ? id : null;
        }

        public static bool EsAtleta(ClaimsPrincipal usuario) =>
            usuario.IsInRole(nameof(RolUsuario.Atleta));

        public static bool EsStaff(ClaimsPrincipal usuario) =>
            usuario.IsInRole(nameof(RolUsuario.SuperAdmin))
            || usuario.IsInRole(nameof(RolUsuario.Entrenador));

        public static int? AtletaId(ClaimsPrincipal usuario)
        {
            var valor = usuario.FindFirstValue(ClaimAtletaId);
            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
