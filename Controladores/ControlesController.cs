using AccesoDatos.Repositorios;
using Controladores.Contratos;
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
    /// <summary>
    /// Controles independientes de sesion (gimnasio, tierra, agua).
    /// El staff carga y edita; el atleta solo lee los propios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ControlesController : ControllerBase, IControlesController
    {
        private readonly IControlRepositorio _controlRepositorio;

        public ControlesController(IControlRepositorio controlRepositorio)
        {
            _controlRepositorio = controlRepositorio;
        }

        /// <summary>Lista controles. Staff: todos o filtrados. Atleta: solo los suyos.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ControlDto>>> Listar(
            [FromQuery] int? atletaId,
            [FromQuery] TipoTrabajo? tipo)
        {
            if (Identidad.EsAtleta(User))
            {
                var propio = Identidad.AtletaId(User);
                if (propio is null)
                {
                    return Forbid();
                }

                if (atletaId is not null && atletaId != propio)
                {
                    return Forbid();
                }

                atletaId = propio;
            }
            else if (!Identidad.EsStaff(User))
            {
                return Forbid();
            }

            var controles = await _controlRepositorio.ObtenerAsync(atletaId, tipo);
            return Ok(controles.Select(c => c.ToDto()));
        }

        /// <summary>Controles del atleta logueado.</summary>
        [HttpGet("mios")]
        public async Task<ActionResult<IEnumerable<ControlDto>>> Mios([FromQuery] TipoTrabajo? tipo)
        {
            var atletaId = Identidad.AtletaId(User);
            if (atletaId is null)
            {
                return Forbid();
            }

            var controles = await _controlRepositorio.ObtenerAsync(atletaId, tipo);
            return Ok(controles.Select(c => c.ToDto()));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ControlDto>> ObtenerPorId(int id)
        {
            var control = await _controlRepositorio.ObtenerPorIdAsync(id);
            if (control is null)
            {
                return NotFound(new { mensaje = "Control no encontrado." });
            }

            if (await Ajeno(control.AtletaId) is { } rechazo)
            {
                return rechazo;
            }

            return Ok(control.ToDto());
        }

        [HttpPost]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<ControlDto>> Crear([FromBody] GuardarControlRequest request)
        {
            if (!await _controlRepositorio.ExisteAtletaAsync(request.AtletaId!.Value))
            {
                return BadRequest(new { mensaje = "El atleta indicado no existe." });
            }

            var control = new Control
            {
                AtletaId = request.AtletaId.Value,
                Tipo = request.Tipo!.Value,
                Fecha = request.Fecha!.Value,
                Observaciones = Limpiar(request.Observaciones),
                FechaCreacion = DateTime.UtcNow
            };
            AplicarDetalle(control, request);

            var creado = await _controlRepositorio.CrearAsync(control, ArmarEjercicios(request));
            return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado.ToDto());
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<ControlDto>> Actualizar(int id, [FromBody] GuardarControlRequest request)
        {
            var control = await _controlRepositorio.ObtenerPorIdAsync(id);
            if (control is null)
            {
                return NotFound(new { mensaje = "Control no encontrado." });
            }

            if (!await _controlRepositorio.ExisteAtletaAsync(request.AtletaId!.Value))
            {
                return BadRequest(new { mensaje = "El atleta indicado no existe." });
            }

            control.AtletaId = request.AtletaId.Value;
            control.Tipo = request.Tipo!.Value;
            control.Fecha = request.Fecha!.Value;
            control.Observaciones = Limpiar(request.Observaciones);
            AplicarDetalle(control, request);

            await _controlRepositorio.ActualizarAsync(control, ArmarEjercicios(request));
            var actualizado = await _controlRepositorio.ObtenerPorIdAsync(id);
            return Ok(actualizado!.ToDto());
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<IActionResult> Eliminar(int id)
        {
            var control = await _controlRepositorio.ObtenerPorIdAsync(id);
            if (control is null)
            {
                return NotFound(new { mensaje = "Control no encontrado." });
            }

            await _controlRepositorio.EliminarAsync(control);
            return NoContent();
        }

        private async Task<ActionResult?> Ajeno(int atletaId)
        {
            if (Identidad.EsStaff(User))
            {
                return null;
            }

            if (!Identidad.EsAtleta(User) || Identidad.AtletaId(User) != atletaId)
            {
                return Forbid();
            }

            await Task.CompletedTask;
            return null;
        }

        private static void AplicarDetalle(Control control, GuardarControlRequest request)
        {
            if (request.Tipo == TipoTrabajo.Gimnasio)
            {
                control.EsAutoCarga = request.EsAutoCarga;
                control.DistanciaMetros = null;
                control.Tiempo = null;
                control.TiempoTestigo = null;
                control.AnioTestigo = null;
                return;
            }

            control.EsAutoCarga = false;
            control.DistanciaMetros = request.DistanciaMetros;
            control.Tiempo = TiempoTexto.IntentaParse(request.Tiempo, out var tiempo) ? tiempo : null;
            control.TiempoTestigo = TiempoTexto.IntentaParse(request.TiempoTestigo, out var testigo)
                ? testigo
                : null;
            control.AnioTestigo = string.IsNullOrWhiteSpace(request.TiempoTestigo)
                ? null
                : request.AnioTestigo;
        }

        private static List<ControlEjercicio> ArmarEjercicios(GuardarControlRequest request)
        {
            if (request.Tipo != TipoTrabajo.Gimnasio)
            {
                return [];
            }

            var auto = request.EsAutoCarga;

            return request.Ejercicios
                .Select(e => new ControlEjercicio
                {
                    Nombre = e.Nombre,
                    Series = e.Series
                        .Select(s => new ControlSerie
                        {
                            Repeticiones = s.Repeticiones,
                            PesoKg = auto ? null : s.PesoKg,
                            EsUnoRm = auto ? false : s.EsUnoRm
                        })
                        .ToList()
                })
                .ToList();
        }

        private static string? Limpiar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return null;
            }

            return texto.Trim();
        }
    }
}
