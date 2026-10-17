using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Mapeos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>Catalogo de botes K2 y K4.</summary>
    [ApiController]
    [Route("api/equipos")]
    [Authorize(Roles = Identidad.RolesStaff)]
    public class EquiposController : ControllerBase
    {
        private readonly IEquipoRepositorio _equipos;
        private readonly IAtletaRepositorio _atletas;

        public EquiposController(IEquipoRepositorio equipos, IAtletaRepositorio atletas)
        {
            _equipos = equipos;
            _atletas = atletas;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipoDto>>> Listar([FromQuery] bool incluirInactivos = false)
        {
            var lista = await _equipos.ListarAsync(incluirInactivos);
            return Ok(lista.Select(e => e.ToDto()));
        }

        [HttpPost]
        public async Task<ActionResult<EquipoDto>> Crear([FromBody] GuardarEquipoRequest request)
        {
            if (Validar(request) is { } error)
            {
                return error;
            }

            if (!await AtletasValidos(request.AtletaIds))
            {
                return BadRequest(new { mensaje = "Hay que elegir atletas activos y sin repetir." });
            }

            var creado = await _equipos.CrearAsync(request.Tipo, request.AtletaIds);
            return Ok(creado.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EquipoDto>> Actualizar(int id, [FromBody] GuardarEquipoRequest request)
        {
            if (Validar(request) is { } error)
            {
                return error;
            }

            if (!await AtletasValidos(request.AtletaIds))
            {
                return BadRequest(new { mensaje = "Hay que elegir atletas activos y sin repetir." });
            }

            var actualizado = await _equipos.ActualizarAsync(id, request.Tipo, request.AtletaIds);
            if (actualizado is null)
            {
                return NotFound(new { mensaje = "Bote no encontrado." });
            }

            return Ok(actualizado.ToDto());
        }

        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] ActualizarEstadoAtletaRequest request)
        {
            if (!await _equipos.CambiarEstadoAsync(id, request.Activo))
            {
                return NotFound(new { mensaje = "Bote no encontrado." });
            }

            return NoContent();
        }

        private ActionResult? Validar(GuardarEquipoRequest request)
        {
            var cupo = (int)request.Tipo;
            if (request.Tipo is not (TipoBote.K2 or TipoBote.K4))
            {
                return BadRequest(new { mensaje = "El bote tiene que ser K2 o K4." });
            }

            if (request.AtletaIds.Count != cupo)
            {
                return BadRequest(new { mensaje = $"Un {request.Tipo} lleva {cupo} atletas." });
            }

            if (request.AtletaIds.Distinct().Count() != request.AtletaIds.Count)
            {
                return BadRequest(new { mensaje = "No se puede repetir un atleta en el mismo bote." });
            }

            return null;
        }

        private async Task<bool> AtletasValidos(IReadOnlyList<int> ids)
        {
            var validos = await _atletas.ObtenerTodosAsync(busqueda: null, incluirInactivos: false);
            var activos = validos.Select(a => a.Id).ToHashSet();
            return ids.All(activos.Contains);
        }
    }
}
