using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Mapeos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>Botes de equipo dentro de una sesion.</summary>
    [ApiController]
    [Route("api/entrenamientos/{entrenamientoId:int}/equipos")]
    [Authorize(Roles = Identidad.RolesStaff)]
    public class EntrenamientoEquiposController : ControllerBase
    {
        private readonly IEntrenamientoRepositorio _entrenamientos;
        private readonly IEntrenamientoEquipoRepositorio _equiposSesion;
        private readonly IEquipoRepositorio _catalogo;

        public EntrenamientoEquiposController(
            IEntrenamientoRepositorio entrenamientos,
            IEntrenamientoEquipoRepositorio equiposSesion,
            IEquipoRepositorio catalogo)
        {
            _entrenamientos = entrenamientos;
            _equiposSesion = equiposSesion;
            _catalogo = catalogo;
        }

        [HttpPost]
        public async Task<ActionResult<EntrenamientoDto>> Cargar(int entrenamientoId, [FromBody] CargarEquipoSesionRequest request)
        {
            var entrenamiento = await _entrenamientos.ObtenerPorIdAsync(entrenamientoId);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            TipoBote tipo;
            List<int> atletaIds;
            int? catalogoId = null;

            if (request.EquipoId is int equipoId)
            {
                var delCatalogo = await _catalogo.ObtenerAsync(equipoId);
                if (delCatalogo is null || !delCatalogo.Activo)
                {
                    return NotFound(new { mensaje = "Bote del catálogo no encontrado." });
                }

                tipo = delCatalogo.Tipo;
                atletaIds = delCatalogo.Miembros.OrderBy(m => m.Orden).Select(m => m.AtletaId).ToList();
                catalogoId = delCatalogo.Id;
            }
            else
            {
                if (request.Tipo is not TipoBote tipoPedido)
                {
                    return BadRequest(new { mensaje = "Indicá K2 o K4, o elegí un bote del catálogo." });
                }

                tipo = tipoPedido;
                atletaIds = request.AtletaIds.Distinct().ToList();
                if (atletaIds.Count != (int)tipo)
                {
                    return BadRequest(new { mensaje = $"Un {tipo} lleva {(int)tipo} atletas." });
                }
            }

            var enSesion = entrenamiento.Atletas.Select(a => a.AtletaId).ToHashSet();
            if (atletaIds.Any(id => !enSesion.Contains(id)))
            {
                return BadRequest(new { mensaje = "Todos los remeros del bote tienen que estar en la sesión." });
            }

            await _equiposSesion.CargarAsync(entrenamientoId, tipo, atletaIds, catalogoId);
            var actualizado = await _entrenamientos.ObtenerPorIdAsync(entrenamientoId);
            return Ok(actualizado!.ToDto());
        }

        [HttpDelete("{entrenamientoEquipoId:int}")]
        public async Task<ActionResult<EntrenamientoDto>> Quitar(int entrenamientoId, int entrenamientoEquipoId)
        {
            var equipo = await _equiposSesion.ObtenerAsync(entrenamientoEquipoId);
            if (equipo is null || equipo.EntrenamientoId != entrenamientoId)
            {
                return NotFound(new { mensaje = "Bote no encontrado en la sesión." });
            }

            await _equiposSesion.EliminarAsync(equipo);
            var actualizado = await _entrenamientos.ObtenerPorIdAsync(entrenamientoId);
            return Ok(actualizado!.ToDto());
        }
    }
}
