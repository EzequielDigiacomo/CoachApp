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
        private readonly ITrabajoRepositorio _trabajoRepositorio;
        private readonly IEntrenamientoRepositorio _entrenamientoRepositorio;

        public ControlesController(
            IControlRepositorio controlRepositorio,
            ITrabajoRepositorio trabajoRepositorio,
            IEntrenamientoRepositorio entrenamientoRepositorio)
        {
            _controlRepositorio = controlRepositorio;
            _trabajoRepositorio = trabajoRepositorio;
            _entrenamientoRepositorio = entrenamientoRepositorio;
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

        /// <summary>
        /// Promueve un trabajo de sesion a control(es) del historial del atleta.
        /// Agua/tierra: un control por parcial. Gimnasio: un control con los ejercicios.
        /// </summary>
        [HttpPost("desde-trabajo/{trabajoId:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<IEnumerable<ControlDto>>> MarcarDesdeTrabajo(int trabajoId)
        {
            var trabajo = await _trabajoRepositorio.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            if (trabajo.AtletaId is null || trabajo.EntrenamientoEquipoId is not null)
            {
                return BadRequest(new { mensaje = "Solo se pueden marcar trabajos individuales del atleta." });
            }

            var existentes = await _controlRepositorio.ObtenerPorTrabajoIdAsync(trabajoId);
            if (existentes.Count > 0)
            {
                return Ok(existentes.Select(c => c.ToDto()));
            }

            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(trabajo.EntrenamientoId);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            if (trabajo.Tipo is TipoTrabajo.Agua or TipoTrabajo.Tierra)
            {
                if (trabajo.Parciales.Count == 0)
                {
                    return BadRequest(new { mensaje = "El trabajo no tiene parciales para marcar como control." });
                }

                var creados = new List<Control>();
                foreach (var parcial in trabajo.Parciales.OrderBy(p => p.Orden))
                {
                    var control = new Control
                    {
                        AtletaId = trabajo.AtletaId.Value,
                        Tipo = trabajo.Tipo,
                        Fecha = entrenamiento.Fecha,
                        DistanciaMetros = parcial.DistanciaMetros,
                        Tiempo = parcial.Tiempo,
                        Observaciones = Limpiar(trabajo.Observaciones),
                        FechaCreacion = DateTime.UtcNow,
                        TrabajoId = trabajo.Id,
                        TrabajoParcialId = parcial.Id
                    };
                    creados.Add(await _controlRepositorio.CrearAsync(control, []));
                }

                return Ok(creados.Select(c => c.ToDto()));
            }

            if (trabajo.Tipo == TipoTrabajo.Gimnasio)
            {
                if (trabajo.Ejercicios.Count == 0)
                {
                    return BadRequest(new { mensaje = "El trabajo no tiene ejercicios para marcar como control." });
                }

                var control = new Control
                {
                    AtletaId = trabajo.AtletaId.Value,
                    Tipo = TipoTrabajo.Gimnasio,
                    Fecha = entrenamiento.Fecha,
                    EsAutoCarga = trabajo.Ejercicios
                        .SelectMany(e => e.Series)
                        .All(s => s.PesoKg is null),
                    Observaciones = Limpiar(trabajo.Observaciones),
                    FechaCreacion = DateTime.UtcNow,
                    TrabajoId = trabajo.Id
                };

                var ejercicios = trabajo.Ejercicios
                    .OrderBy(e => e.Orden)
                    .Select(e => new ControlEjercicio
                    {
                        Nombre = e.Nombre,
                        Series = e.Series
                            .OrderBy(s => s.Orden)
                            .Select(s => new ControlSerie
                            {
                                Repeticiones = s.Repeticiones,
                                PesoKg = s.PesoKg,
                                EsUnoRm = false
                            })
                            .ToList()
                    })
                    .ToList();

                var creado = await _controlRepositorio.CrearAsync(control, ejercicios);
                return Ok(new[] { creado.ToDto() });
            }

            return BadRequest(new { mensaje = "Tipo de trabajo no soportado." });
        }

        /// <summary>Quita del historial los controles creados desde ese trabajo.</summary>
        [HttpDelete("desde-trabajo/{trabajoId:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<IActionResult> DesmarcarDesdeTrabajo(int trabajoId)
        {
            var controles = await _controlRepositorio.ObtenerPorTrabajoIdAsync(trabajoId);
            if (controles.Count == 0)
            {
                return NoContent();
            }

            await _controlRepositorio.EliminarRangoAsync(controles);
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

            if (request.Tipo == TipoTrabajo.Agua)
            {
                control.TiempoTestigo = TiempoTexto.IntentaParse(request.TiempoTestigo, out var testigo)
                    ? testigo
                    : null;
                control.AnioTestigo = string.IsNullOrWhiteSpace(request.TiempoTestigo)
                    ? null
                    : request.AnioTestigo;
            }
            else
            {
                control.TiempoTestigo = null;
                control.AnioTestigo = null;
            }
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
