using System.Security.Claims;
using AccesoDatos.Repositorios;
using Controladores.Contratos;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Mapeos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Anotaciones del entrenador: notas sueltas, de un atleta, de una sesion
    /// o de un trabajo puntual. Cualquier usuario autenticado puede usarlas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnotacionesController : ControllerBase, IAnotacionesController
    {
        private readonly IAnotacionRepositorio _anotacionRepositorio;
        private readonly IAtletaRepositorio _atletaRepositorio;
        private readonly IEntrenamientoRepositorio _entrenamientoRepositorio;
        private readonly ITrabajoRepositorio _trabajoRepositorio;

        public AnotacionesController(
            IAnotacionRepositorio anotacionRepositorio,
            IAtletaRepositorio atletaRepositorio,
            IEntrenamientoRepositorio entrenamientoRepositorio,
            ITrabajoRepositorio trabajoRepositorio)
        {
            _anotacionRepositorio = anotacionRepositorio;
            _atletaRepositorio = atletaRepositorio;
            _entrenamientoRepositorio = entrenamientoRepositorio;
            _trabajoRepositorio = trabajoRepositorio;
        }

        /// <summary>
        /// Lista anotaciones, de la mas nueva a la mas vieja. Se puede filtrar
        /// por atleta, sesion o trabajo, y buscar texto en el titulo o el cuerpo.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnotacionDto>>> Listar(
            [FromQuery] int? atletaId,
            [FromQuery] int? entrenamientoId,
            [FromQuery] int? trabajoId,
            [FromQuery] string? busqueda)
        {
            var anotaciones = await _anotacionRepositorio.ObtenerAsync(
                atletaId, entrenamientoId, trabajoId, busqueda);

            return Ok(anotaciones.Select(a => a.ToDto()));
        }

        /// <summary>Devuelve una anotacion por id.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AnotacionDto>> ObtenerPorId(int id)
        {
            var anotacion = await _anotacionRepositorio.ObtenerPorIdAsync(id);
            if (anotacion is null)
            {
                return NotFound(new { mensaje = "Anotación no encontrada." });
            }

            return Ok(anotacion.ToDto());
        }

        /// <summary>Crea una anotacion.</summary>
        [HttpPost]
        public async Task<ActionResult<AnotacionDto>> Crear([FromBody] GuardarAnotacionRequest request)
        {
            var contexto = await ResolverContextoAsync(request);
            if (contexto.Error is not null)
            {
                return BadRequest(new { mensaje = contexto.Error });
            }

            var anotacion = new Anotacion
            {
                Titulo = Limpiar(request.Titulo),
                Texto = request.Texto.Trim(),
                Origen = OrigenAnotacion.Manual,
                FechaCreacion = DateTime.UtcNow,
                AtletaId = contexto.AtletaId,
                EntrenamientoId = contexto.EntrenamientoId,
                TrabajoId = request.TrabajoId,
                UsuarioId = UsuarioActualId()
            };

            await _anotacionRepositorio.CrearAsync(anotacion);

            // Se relee para devolver los vinculos ya resueltos.
            var creada = await _anotacionRepositorio.ObtenerPorIdAsync(anotacion.Id);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = anotacion.Id }, creada!.ToDto());
        }

        /// <summary>Modifica el texto y los vinculos de una anotacion.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<AnotacionDto>> Actualizar(int id, [FromBody] GuardarAnotacionRequest request)
        {
            var anotacion = await _anotacionRepositorio.ObtenerPorIdAsync(id);
            if (anotacion is null)
            {
                return NotFound(new { mensaje = "Anotación no encontrada." });
            }

            var contexto = await ResolverContextoAsync(request);
            if (contexto.Error is not null)
            {
                return BadRequest(new { mensaje = contexto.Error });
            }

            anotacion.Titulo = Limpiar(request.Titulo);
            anotacion.Texto = request.Texto.Trim();
            anotacion.AtletaId = contexto.AtletaId;
            anotacion.EntrenamientoId = contexto.EntrenamientoId;
            anotacion.TrabajoId = request.TrabajoId;

            await _anotacionRepositorio.ActualizarAsync(anotacion);

            var actualizada = await _anotacionRepositorio.ObtenerPorIdAsync(anotacion.Id);
            return Ok(actualizada!.ToDto());
        }

        /// <summary>Elimina una anotacion.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var anotacion = await _anotacionRepositorio.ObtenerPorIdAsync(id);
            if (anotacion is null)
            {
                return NotFound(new { mensaje = "Anotación no encontrada." });
            }

            await _anotacionRepositorio.EliminarAsync(anotacion);
            return NoContent();
        }

        /// <summary>
        /// Decide a que atleta y sesion queda atada la anotacion. Si viene un
        /// trabajo, manda el trabajo: el atleta y la sesion salen de ahi.
        /// </summary>
        private async Task<(int? AtletaId, int? EntrenamientoId, string? Error)> ResolverContextoAsync(
            GuardarAnotacionRequest request)
        {
            if (request.TrabajoId is not null)
            {
                var trabajo = await _trabajoRepositorio.ObtenerPorIdAsync(request.TrabajoId.Value);
                if (trabajo is null)
                {
                    return (null, null, "El trabajo indicado no existe.");
                }

                return (trabajo.AtletaId, trabajo.EntrenamientoId, null);
            }

            if (request.AtletaId is not null)
            {
                var atleta = await _atletaRepositorio.ObtenerPorIdAsync(request.AtletaId.Value);
                if (atleta is null)
                {
                    return (null, null, "El atleta indicado no existe.");
                }
            }

            if (request.EntrenamientoId is not null)
            {
                var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(request.EntrenamientoId.Value);
                if (entrenamiento is null)
                {
                    return (null, null, "La sesión indicada no existe.");
                }
            }

            return (request.AtletaId, request.EntrenamientoId, null);
        }

        private int? UsuarioActualId() =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
