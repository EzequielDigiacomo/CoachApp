using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Entidades.Mapeos;
using Entidades.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>Trabajos de agua de un bote de equipo en la sesion.</summary>
    [ApiController]
    [Route("api/entrenamientos/{entrenamientoId:int}/equipos/{entrenamientoEquipoId:int}/trabajos")]
    [Authorize]
    public class TrabajosEquipoController : ControllerBase
    {
        private readonly ITrabajoRepositorio _trabajos;
        private readonly IEntrenamientoEquipoRepositorio _equipos;
        private readonly IUsuarioRepositorio _usuarios;

        public TrabajosEquipoController(
            ITrabajoRepositorio trabajos,
            IEntrenamientoEquipoRepositorio equipos,
            IUsuarioRepositorio usuarios)
        {
            _trabajos = trabajos;
            _equipos = equipos;
            _usuarios = usuarios;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrabajoDto>>> Listar(int entrenamientoId, int entrenamientoEquipoId)
        {
            if (!await _equipos.ExisteEnSesionAsync(entrenamientoId, entrenamientoEquipoId))
            {
                return NotFound(new { mensaje = "Bote no encontrado en la sesión." });
            }

            if (await SoloLecturaPermitida(entrenamientoEquipoId) is { } rechazo)
            {
                return rechazo;
            }

            var trabajos = await _trabajos.ObtenerPorEquipoAsync(entrenamientoEquipoId);
            return Ok(trabajos.Select(t => t.ToDto()));
        }

        [HttpPost]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<TrabajoDto>> Crear(
            int entrenamientoId,
            int entrenamientoEquipoId,
            [FromBody] GuardarTrabajoRequest request)
        {
            if (!await _equipos.ExisteEnSesionAsync(entrenamientoId, entrenamientoEquipoId))
            {
                return NotFound(new { mensaje = "Bote no encontrado en la sesión." });
            }

            if (request.Tipo != TipoTrabajo.Agua)
            {
                return BadRequest(new { mensaje = "Los trabajos de equipo son de agua." });
            }

            var trabajo = new Trabajo
            {
                EntrenamientoId = entrenamientoId,
                EntrenamientoEquipoId = entrenamientoEquipoId,
                Tipo = TipoTrabajo.Agua,
                HoraInicio = TiempoTexto.IntentaParseHora(request.HoraInicio, out var hora) ? hora : null,
                Observaciones = Limpiar(request.Observaciones),
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _trabajos.CrearAsync(
                trabajo,
                ArmarParciales(request),
                [],
                ArmarPaladas(request));

            return Ok(creado.ToDto());
        }

        [HttpPut("{trabajoId:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<TrabajoDto>> Actualizar(
            int entrenamientoId,
            int entrenamientoEquipoId,
            int trabajoId,
            [FromBody] GuardarTrabajoRequest request)
        {
            var trabajo = await _trabajos.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null
                || trabajo.EntrenamientoId != entrenamientoId
                || trabajo.EntrenamientoEquipoId != entrenamientoEquipoId)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            if (request.Tipo != TipoTrabajo.Agua)
            {
                return BadRequest(new { mensaje = "Los trabajos de equipo son de agua." });
            }

            trabajo.Tipo = TipoTrabajo.Agua;
            trabajo.HoraInicio = TiempoTexto.IntentaParseHora(request.HoraInicio, out var hora) ? hora : null;
            trabajo.Observaciones = Limpiar(request.Observaciones);

            await _trabajos.ActualizarAsync(
                trabajo,
                ArmarParciales(request),
                [],
                ArmarPaladas(request));

            var actualizado = await _trabajos.ObtenerPorIdAsync(trabajoId);
            return Ok(actualizado!.ToDto());
        }

        [HttpDelete("{trabajoId:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<IActionResult> Eliminar(int entrenamientoId, int entrenamientoEquipoId, int trabajoId)
        {
            var trabajo = await _trabajos.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null
                || trabajo.EntrenamientoId != entrenamientoId
                || trabajo.EntrenamientoEquipoId != entrenamientoEquipoId)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            await _trabajos.EliminarAsync(trabajo);
            return NoContent();
        }

        private static IEnumerable<TrabajoParcial> ArmarParciales(GuardarTrabajoRequest request) =>
            request.Parciales
                .Where(p => TiempoTexto.IntentaParse(p.Tiempo, out _))
                .Select(p =>
                {
                    TiempoTexto.IntentaParse(p.Tiempo, out var tiempo);
                    return new TrabajoParcial
                    {
                        DistanciaMetros = p.DistanciaMetros,
                        Tiempo = tiempo
                    };
                });

        private static IEnumerable<TrabajoPalada> ArmarPaladas(GuardarTrabajoRequest request) =>
            request.Paladas
                .Where(p => TiempoTexto.IntentaParse(p.Tiempo, out _))
                .Select(p =>
                {
                    TiempoTexto.IntentaParse(p.Tiempo, out var tiempo);
                    return new TrabajoPalada
                    {
                        Tiempo = tiempo,
                        Ppm = p.Ppm
                    };
                });

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        /// <summary>Staff o remero del bote pueden leer los trabajos.</summary>
        private async Task<ActionResult?> SoloLecturaPermitida(int entrenamientoEquipoId)
        {
            if (Identidad.EsStaff(User))
            {
                return null;
            }

            var atletaId = Identidad.AtletaId(User);
            if (atletaId is null && Identidad.EsAtleta(User))
            {
                // Token viejo sin claim: se lee del usuario en la base.
                var usuarioId = Identidad.UsuarioId(User);
                if (usuarioId is int id)
                {
                    var usuario = await _usuarios.ObtenerPorIdAsync(id);
                    atletaId = usuario?.AtletaId;
                }
            }

            if (atletaId is null || !await _equipos.EsMiembroAsync(entrenamientoEquipoId, atletaId.Value))
            {
                return StatusCode(403, new
                {
                    mensaje = "No podés ver los trabajos de este bote."
                });
            }

            return null;
        }
    }
}
