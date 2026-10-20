using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Sensaciones al remar. El atleta las escribe (máximo 1000 caracteres)
    /// y el staff las lee.
    /// </summary>
    [ApiController]
    [Route("api/sensaciones")]
    [Authorize]
    public class SensacionesController : ControllerBase
    {
        private readonly ISensacionRepositorio _sensaciones;

        public SensacionesController(ISensacionRepositorio sensaciones)
        {
            _sensaciones = sensaciones;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensacionDto>>> Listar()
        {
            if (Identidad.EsAtleta(User))
            {
                var propio = Identidad.AtletaId(User);
                if (propio is null)
                {
                    return Ok(Array.Empty<SensacionDto>());
                }

                var propias = await _sensaciones.ListarAsync(propio);
                return Ok(propias.Select(ADto));
            }

            if (!Identidad.EsStaff(User))
            {
                return Forbid();
            }

            var sensaciones = await _sensaciones.ListarAsync(atletaId: null);
            return Ok(sensaciones.Select(ADto));
        }

        [HttpPost]
        public async Task<ActionResult<SensacionDto>> Crear([FromBody] GuardarSensacionRequest request)
        {
            var propio = Identidad.EsAtleta(User) ? Identidad.AtletaId(User) : null;
            if (propio is null)
            {
                return Forbid();
            }

            var sensacion = new Sensacion
            {
                AtletaId = propio.Value,
                Texto = request.Texto.Trim(),
                FechaCreacion = DateTime.UtcNow
            };

            await _sensaciones.CrearAsync(sensacion);
            sensacion = (await _sensaciones.ObtenerAsync(sensacion.Id))!;
            return Ok(ADto(sensacion));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SensacionDto>> Actualizar(int id, [FromBody] GuardarSensacionRequest request)
        {
            var sensacion = await _sensaciones.ObtenerAsync(id);
            if (sensacion is null)
            {
                return NotFound(new { mensaje = "Sensación no encontrada." });
            }

            if (!Identidad.EsAtleta(User) || Identidad.AtletaId(User) != sensacion.AtletaId)
            {
                return Forbid();
            }

            sensacion.Texto = request.Texto.Trim();
            await _sensaciones.ActualizarAsync(sensacion);
            return Ok(ADto(sensacion));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var sensacion = await _sensaciones.ObtenerAsync(id);
            if (sensacion is null)
            {
                return NotFound(new { mensaje = "Sensación no encontrada." });
            }

            if (!Identidad.EsAtleta(User) || Identidad.AtletaId(User) != sensacion.AtletaId)
            {
                return Forbid();
            }

            await _sensaciones.EliminarAsync(sensacion);
            return NoContent();
        }

        private static SensacionDto ADto(Sensacion sensacion) => new()
        {
            Id = sensacion.Id,
            AtletaId = sensacion.AtletaId,
            Nombre = sensacion.Atleta?.Nombre ?? string.Empty,
            Apellido = sensacion.Atleta?.Apellido ?? string.Empty,
            Color = sensacion.Atleta?.Color,
            Texto = sensacion.Texto,
            FechaCreacion = sensacion.FechaCreacion
        };
    }
}
