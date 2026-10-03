using System.Security.Claims;
using AccesoDatos.Repositorios;
using Controladores.Contratos;
using Controladores.Seguridad;
using Entidades.DTOs;
using Entidades.Mapeos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase, IAuthController
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly PasswordServicio _passwordServicio;
        private readonly ITokenServicio _tokenServicio;

        public AuthController(
            IUsuarioRepositorio usuarioRepositorio,
            PasswordServicio passwordServicio,
            ITokenServicio tokenServicio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _passwordServicio = passwordServicio;
            _tokenServicio = tokenServicio;
        }

        /// <summary>Inicia sesion y devuelve el token JWT.</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorNombreUsuarioAsync(request.NombreUsuario);

            if (usuario is null || !usuario.Activo || !_passwordServicio.Verificar(usuario, request.Password))
            {
                return Unauthorized(new { mensaje = "Usuario o contraseña inválidos." });
            }

            var token = _tokenServicio.GenerarToken(usuario);

            usuario.UltimoAcceso = DateTime.UtcNow;
            await _usuarioRepositorio.ActualizarAsync(usuario);

            return Ok(new LoginResponse
            {
                Token = token.Token,
                Expira = token.Expira,
                Usuario = usuario.ToDto()
            });
        }

        /// <summary>Devuelve los datos del usuario autenticado.</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UsuarioDto>> Me()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim, out var id))
            {
                return Unauthorized();
            }

            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(id);
            if (usuario is null)
            {
                return Unauthorized();
            }

            return Ok(usuario.ToDto());
        }
    }
}
