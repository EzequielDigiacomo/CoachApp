using AccesoDatos.Repositorios;
using Controladores.Contratos;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Mapeos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Alta y administracion de cuentas. Solo accesible para el superadmin.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(RolUsuario.SuperAdmin))]
    public class UsuariosController : ControllerBase, IUsuariosController
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly PasswordServicio _passwordServicio;

        public UsuariosController(
            IUsuarioRepositorio usuarioRepositorio,
            PasswordServicio passwordServicio)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _passwordServicio = passwordServicio;
        }

        /// <summary>Lista todas las cuentas.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> Listar()
        {
            var usuarios = await _usuarioRepositorio.ObtenerTodosAsync();
            return Ok(usuarios.Select(u => u.ToDto()));
        }

        /// <summary>Devuelve una cuenta por id.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(id);
            if (usuario is null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado." });
            }

            return Ok(usuario.ToDto());
        }

        /// <summary>Crea una nueva cuenta (entrenador o superadmin).</summary>
        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> Crear([FromBody] CrearUsuarioRequest request)
        {
            if (await _usuarioRepositorio.ExisteNombreUsuarioAsync(request.NombreUsuario))
            {
                return Conflict(new { mensaje = "El nombre de usuario ya existe." });
            }

            if (await _usuarioRepositorio.ExisteEmailAsync(request.Email))
            {
                return Conflict(new { mensaje = "El email ya esta registrado." });
            }

            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Rol = request.Rol,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            usuario.PasswordHash = _passwordServicio.Hashear(usuario, request.Password);

            await _usuarioRepositorio.CrearAsync(usuario);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, usuario.ToDto());
        }

        /// <summary>Activa o desactiva una cuenta.</summary>
        [HttpPatch("{id:int}/estado")]
        public async Task<ActionResult<UsuarioDto>> CambiarEstado(int id, [FromBody] ActualizarEstadoUsuarioRequest request)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(id);
            if (usuario is null)
            {
                return NotFound(new { mensaje = "Usuario no encontrado." });
            }

            usuario.Activo = request.Activo;
            await _usuarioRepositorio.ActualizarAsync(usuario);

            return Ok(usuario.ToDto());
        }
    }
}
