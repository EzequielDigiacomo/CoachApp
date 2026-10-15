using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Logins de los atletas: alta, usuario, email y contraseña.
    /// Lo usa el entrenador y el superadmin.
    /// </summary>
    [ApiController]
    [Route("api/cuentas-atleta")]
    [Authorize(Roles = Identidad.RolesStaff)]
    public class CuentasAtletaController : ControllerBase
    {
        private readonly IAtletaRepositorio _atletas;
        private readonly IUsuarioRepositorio _usuarios;
        private readonly PasswordServicio _passwords;

        public CuentasAtletaController(
            IAtletaRepositorio atletas,
            IUsuarioRepositorio usuarios,
            PasswordServicio passwords)
        {
            _atletas = atletas;
            _usuarios = usuarios;
            _passwords = passwords;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaAtletaDto>>> Listar()
        {
            var atletas = await _atletas.ObtenerTodosAsync(busqueda: null, incluirInactivos: false);
            var cuentas = (await _usuarios.ObtenerTodosAsync())
                .Where(u => u.Rol == RolUsuario.Atleta && u.AtletaId is not null)
                .ToDictionary(u => u.AtletaId!.Value);

            var lista = atletas
                .OrderBy(a => a.Apellido)
                .ThenBy(a => a.Nombre)
                .Select(atleta =>
                {
                    cuentas.TryGetValue(atleta.Id, out var usuario);
                    return ADto(atleta, usuario);
                });

            return Ok(lista);
        }

        [HttpPost]
        public async Task<ActionResult<CuentaAtletaDto>> Crear([FromBody] CrearCuentaAtletaRequest request)
        {
            var atleta = await _atletas.ObtenerPorIdAsync(request.AtletaId);
            if (atleta is null || !atleta.Activo)
            {
                return NotFound(new { mensaje = "Atleta no encontrado." });
            }

            if (await _usuarios.ObtenerPorAtletaAsync(atleta.Id) is not null)
            {
                return Conflict(new { mensaje = "Ese atleta ya tiene un login." });
            }

            var nombreUsuario = request.NombreUsuario.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _usuarios.ExisteNombreUsuarioAsync(nombreUsuario))
            {
                return Conflict(new { mensaje = "El nombre de usuario ya existe." });
            }

            if (await _usuarios.ExisteEmailAsync(email))
            {
                return Conflict(new { mensaje = "El email ya está registrado." });
            }

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Email = email,
                Nombre = atleta.Nombre,
                Apellido = atleta.Apellido,
                Rol = RolUsuario.Atleta,
                AtletaId = atleta.Id,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            usuario.PasswordHash = _passwords.Hashear(usuario, request.Password);

            await _usuarios.CrearAsync(usuario);

            return Ok(ADto(atleta, usuario));
        }

        [HttpPut("{usuarioId:int}")]
        public async Task<ActionResult<CuentaAtletaDto>> Editar(int usuarioId, [FromBody] EditarCuentaAtletaRequest request)
        {
            var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);
            if (usuario is null || usuario.Rol != RolUsuario.Atleta || usuario.AtletaId is null)
            {
                return NotFound(new { mensaje = "Cuenta de atleta no encontrada." });
            }

            var nombreUsuario = request.NombreUsuario.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _usuarios.ExisteNombreUsuarioAsync(nombreUsuario, usuario.Id))
            {
                return Conflict(new { mensaje = "El nombre de usuario ya existe." });
            }

            if (await _usuarios.ExisteEmailAsync(email, usuario.Id))
            {
                return Conflict(new { mensaje = "El email ya está registrado." });
            }

            usuario.NombreUsuario = nombreUsuario;
            usuario.Email = email;
            usuario.Activo = request.Activo;
            await _usuarios.ActualizarAsync(usuario);

            var atleta = await _atletas.ObtenerPorIdAsync(usuario.AtletaId.Value);
            return Ok(ADto(atleta!, usuario));
        }

        [HttpPut("{usuarioId:int}/password")]
        public async Task<IActionResult> CambiarPassword(int usuarioId, [FromBody] CambiarPasswordCuentaRequest request)
        {
            var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);
            if (usuario is null || usuario.Rol != RolUsuario.Atleta)
            {
                return NotFound(new { mensaje = "Cuenta de atleta no encontrada." });
            }

            usuario.PasswordHash = _passwords.Hashear(usuario, request.Password);
            await _usuarios.ActualizarAsync(usuario);

            return NoContent();
        }

        private static CuentaAtletaDto ADto(Atleta atleta, Usuario? usuario) => new()
        {
            AtletaId = atleta.Id,
            Nombre = atleta.Nombre,
            Apellido = atleta.Apellido,
            EmailAtleta = atleta.Email,
            UsuarioId = usuario?.Id,
            NombreUsuario = usuario?.NombreUsuario,
            Email = usuario?.Email,
            Activo = usuario?.Activo
        };
    }
}
