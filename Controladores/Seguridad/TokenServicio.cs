using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Entidades;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Controladores.Seguridad
{
    public class TokenServicio : ITokenServicio
    {
        private readonly IConfiguration _configuracion;

        public TokenServicio(IConfiguration configuracion)
        {
            _configuracion = configuracion;
        }

        public TokenGenerado GenerarToken(Usuario usuario)
        {
            var key = _configuracion["Jwt:Key"]
                ?? throw new InvalidOperationException("Falta configurar 'Jwt:Key'.");
            var issuer = _configuracion["Jwt:Issuer"];
            var audience = _configuracion["Jwt:Audience"];
            var minutos = int.TryParse(_configuracion["Jwt:ExpiraMinutos"], out var m) ? m : 120;

            var expira = DateTime.UtcNow.AddMinutes(minutos);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name, usuario.NombreUsuario),
                new(ClaimTypes.Role, usuario.Rol.ToString()),
                new(JwtRegisteredClaimNames.Email, usuario.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expira,
                signingCredentials: credenciales);

            return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expira);
        }
    }
}
