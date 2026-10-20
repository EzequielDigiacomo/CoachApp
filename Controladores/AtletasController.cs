using AccesoDatos.Repositorios;
using Controladores.Contratos;
using Entidades;
using Controladores.Seguridad;
using Entidades.DTOs;
using Entidades.Mapeos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Alta, consulta y administracion de atletas.
    /// Accesible para cualquier usuario autenticado (entrenador o superadmin).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AtletasController : ControllerBase, IAtletasController
    {
        private readonly IAtletaRepositorio _atletaRepositorio;

        public AtletasController(IAtletaRepositorio atletaRepositorio)
        {
            _atletaRepositorio = atletaRepositorio;
        }

        /// <summary>
        /// Lista atletas. Se puede buscar por nombre, apellido o DNI,
        /// e incluir los dados de baja con "incluirInactivos".
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtletaDto>>> Listar(
            [FromQuery] string? busqueda,
            [FromQuery] bool incluirInactivos = false)
        {
            if (Identidad.EsAtleta(User))
            {
                var propio = Identidad.AtletaId(User);
                if (propio is null)
                {
                    return Ok(Array.Empty<AtletaDto>());
                }

                var propioAtleta = await _atletaRepositorio.ObtenerPorIdAsync(propio.Value);
                return Ok(propioAtleta is null ? Array.Empty<AtletaDto>() : new[] { propioAtleta.ToDto() });
            }

            var atletas = await _atletaRepositorio.ObtenerTodosAsync(busqueda, incluirInactivos);
            return Ok(atletas.Select(a => a.ToDto()));
        }

        /// <summary>Devuelve un atleta por id.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AtletaDto>> ObtenerPorId(int id)
        {
            if (Ajeno(id) is { } rechazo)
            {
                return rechazo;
            }

            var atleta = await _atletaRepositorio.ObtenerPorIdAsync(id);
            if (atleta is null)
            {
                return NotFound(new { mensaje = "Atleta no encontrado." });
            }

            return Ok(atleta.ToDto());
        }

        /// <summary>Busca un atleta por DNI.</summary>
        [HttpGet("dni/{dni}")]
        public async Task<ActionResult<AtletaDto>> ObtenerPorDni(string dni)
        {
            var atleta = await _atletaRepositorio.ObtenerPorDniAsync(NormalizarDni(dni));
            if (atleta is null || Ajeno(atleta.Id) is not null)
            {
                return NotFound(new { mensaje = "Atleta no encontrado." });
            }

            return Ok(atleta.ToDto());
        }

        /// <summary>Da de alta un atleta.</summary>
        [HttpPost]
        public async Task<ActionResult<AtletaDto>> Crear([FromBody] CrearAtletaRequest request)
        {
            if (SoloStaff() is { } rechazo)
            {
                return rechazo;
            }

            var dni = NormalizarDni(request.Dni);

            if (await _atletaRepositorio.ExisteDniAsync(dni))
            {
                return Conflict(new { mensaje = "Ya existe un atleta con ese DNI." });
            }

            var atleta = new Atleta
            {
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                FechaNacimiento = request.FechaNacimiento,
                Club = Limpiar(request.Club),
                Email = Limpiar(request.Email)?.ToLowerInvariant(),
                Dni = dni,
                Telefono = Limpiar(request.Telefono),
                Activo = true,
                FechaAlta = DateTime.UtcNow
            };

            await _atletaRepositorio.CrearAsync(atleta);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = atleta.Id }, atleta.ToDto());
        }

        /// <summary>Modifica los datos de un atleta.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AtletaDto>> Actualizar(int id, [FromBody] ActualizarAtletaRequest request)
        {
            if (SoloStaff() is { } rechazo)
            {
                return rechazo;
            }

            var atleta = await _atletaRepositorio.ObtenerPorIdAsync(id);
            if (atleta is null)
            {
                return NotFound(new { mensaje = "Atleta no encontrado." });
            }

            var dni = NormalizarDni(request.Dni);

            if (await _atletaRepositorio.ExisteDniAsync(dni, excluirId: id))
            {
                return Conflict(new { mensaje = "Ya existe otro atleta con ese DNI." });
            }

            atleta.Nombre = request.Nombre.Trim();
            atleta.Apellido = request.Apellido.Trim();
            atleta.FechaNacimiento = request.FechaNacimiento;
            atleta.Club = Limpiar(request.Club);
            atleta.Email = Limpiar(request.Email)?.ToLowerInvariant();
            atleta.Dni = dni;
            atleta.Telefono = Limpiar(request.Telefono);

            await _atletaRepositorio.ActualizarAsync(atleta);

            return Ok(atleta.ToDto());
        }

        /// <summary>
        /// Da de baja un atleta (baja logica). El registro se conserva
        /// para no perder su historial de entrenamientos.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            if (SoloStaff() is { } rechazo)
            {
                return rechazo;
            }

            var atleta = await _atletaRepositorio.ObtenerPorIdAsync(id);
            if (atleta is null)
            {
                return NotFound(new { mensaje = "Atleta no encontrado." });
            }

            atleta.Activo = false;
            await _atletaRepositorio.ActualizarAsync(atleta);

            return NoContent();
        }

        /// <summary>Reactiva un atleta dado de baja.</summary>
        [HttpPatch("{id:int}/estado")]
        public async Task<ActionResult<AtletaDto>> CambiarEstado(int id, [FromBody] ActualizarEstadoAtletaRequest request)
        {
            if (SoloStaff() is { } rechazo)
            {
                return rechazo;
            }

            var atleta = await _atletaRepositorio.ObtenerPorIdAsync(id);
            if (atleta is null)
            {
                return NotFound(new { mensaje = "Atleta no encontrado." });
            }

            atleta.Activo = request.Activo;
            await _atletaRepositorio.ActualizarAsync(atleta);

            return Ok(atleta.ToDto());
        }

        /// <summary>Guarda los colores de identificacion de varios atletas.</summary>
        [HttpPut("colores")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<IEnumerable<AtletaDto>>> GuardarColores(
            [FromBody] GuardarColoresAtletasRequest request)
        {
            var ids = request.Colores.Select(c => c.AtletaId).Distinct().ToList();
            var atletas = await _atletaRepositorio.ObtenerPorIdsAsync(ids);

            if (atletas.Count != ids.Count)
            {
                return BadRequest(new { mensaje = "Alguno de los atletas indicados no existe." });
            }

            var porId = atletas.ToDictionary(a => a.Id);
            foreach (var item in request.Colores)
            {
                porId[item.AtletaId].Color = GuardarColoresAtletasRequest.Normalizar(item.Color);
            }

            await _atletaRepositorio.GuardarCambiosAsync();

            return Ok(atletas
                .OrderBy(a => a.Apellido)
                .ThenBy(a => a.Nombre)
                .Select(a => a.ToDto()));
        }

        /// <summary>Quita puntos y espacios del DNI para guardarlo siempre igual.</summary>
        private static string NormalizarDni(string dni) =>
            new(dni.Where(c => char.IsDigit(c)).ToArray());

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private ActionResult? SoloStaff() =>
            Identidad.EsStaff(User) ? null : Forbid();

        /// <summary>El atleta solo puede ver su propia ficha.</summary>
        private ActionResult? Ajeno(int atletaId)
        {
            if (!Identidad.EsAtleta(User))
            {
                return null;
            }

            return Identidad.AtletaId(User) == atletaId
                ? null
                : NotFound(new { mensaje = "Atleta no encontrado." });
        }
    }
}
