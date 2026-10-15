using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Notas que escribe el atleta. El staff las lee todas.
    /// </summary>
    [ApiController]
    [Route("api/anotaciones-atleta")]
    [Authorize]
    public class AnotacionesAtletaController : ControllerBase
    {
        private readonly IAnotacionAtletaRepositorio _notas;

        public AnotacionesAtletaController(IAnotacionAtletaRepositorio notas)
        {
            _notas = notas;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnotacionAtletaDto>>> Listar()
        {
            if (Identidad.EsAtleta(User))
            {
                var propio = Identidad.AtletaId(User);
                if (propio is null)
                {
                    return Ok(Array.Empty<AnotacionAtletaDto>());
                }

                var propias = await _notas.ListarAsync(propio);
                return Ok(propias.Select(ADto));
            }

            if (!Identidad.EsStaff(User))
            {
                return Forbid();
            }

            var notas = await _notas.ListarAsync(atletaId: null);
            return Ok(notas.Select(ADto));
        }

        [HttpPost]
        public async Task<ActionResult<AnotacionAtletaDto>> Crear([FromBody] GuardarAnotacionAtletaRequest request)
        {
            var propio = ExigirAtleta();
            if (propio is null)
            {
                return Forbid();
            }

            var nota = new AnotacionAtleta
            {
                AtletaId = propio.Value,
                Texto = request.Texto.Trim(),
                FechaCreacion = DateTime.UtcNow
            };

            await _notas.CrearAsync(nota);
            nota = (await _notas.ObtenerAsync(nota.Id))!;
            return Ok(ADto(nota));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AnotacionAtletaDto>> Actualizar(int id, [FromBody] GuardarAnotacionAtletaRequest request)
        {
            var nota = await _notas.ObtenerAsync(id);
            if (nota is null)
            {
                return NotFound(new { mensaje = "Anotación no encontrada." });
            }

            if (!PuedeEscribir(nota.AtletaId))
            {
                return Forbid();
            }

            nota.Texto = request.Texto.Trim();
            await _notas.ActualizarAsync(nota);
            return Ok(ADto(nota));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var nota = await _notas.ObtenerAsync(id);
            if (nota is null)
            {
                return NotFound(new { mensaje = "Anotación no encontrada." });
            }

            if (!PuedeEscribir(nota.AtletaId))
            {
                return Forbid();
            }

            await _notas.EliminarAsync(nota);
            return NoContent();
        }

        private int? ExigirAtleta() =>
            Identidad.EsAtleta(User) ? Identidad.AtletaId(User) : null;

        private bool PuedeEscribir(int atletaId) =>
            Identidad.EsAtleta(User) && Identidad.AtletaId(User) == atletaId;

        private static AnotacionAtletaDto ADto(AnotacionAtleta nota) => new()
        {
            Id = nota.Id,
            AtletaId = nota.AtletaId,
            Nombre = nota.Atleta?.Nombre ?? string.Empty,
            Apellido = nota.Atleta?.Apellido ?? string.Empty,
            Texto = nota.Texto,
            FechaCreacion = nota.FechaCreacion
        };
    }
}
