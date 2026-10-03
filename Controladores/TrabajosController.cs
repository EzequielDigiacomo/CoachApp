using AccesoDatos.Repositorios;
using Controladores.Contratos;
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
    /// Trabajos o controles que hizo cada atleta dentro de una sesion
    /// (distancias, tiempos totales y parciales). Requiere estar autenticado.
    /// </summary>
    [ApiController]
    [Route("api/entrenamientos/{entrenamientoId:int}/atletas/{atletaId:int}/trabajos")]
    [Authorize]
    public class TrabajosController : ControllerBase, ITrabajosController
    {
        private readonly ITrabajoRepositorio _trabajoRepositorio;

        public TrabajosController(ITrabajoRepositorio trabajoRepositorio)
        {
            _trabajoRepositorio = trabajoRepositorio;
        }

        /// <summary>Lista los trabajos del atleta en esa sesion, del mas temprano al mas tarde.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrabajoDto>>> Listar(int entrenamientoId, int atletaId)
        {
            if (!await _trabajoRepositorio.ExisteVinculoAsync(entrenamientoId, atletaId))
            {
                return NotFound(new { mensaje = "El atleta no está asignado a esa sesión." });
            }

            var trabajos = await _trabajoRepositorio.ObtenerPorAtletaAsync(entrenamientoId, atletaId);

            return Ok(trabajos.Select(t => t.ToDto()));
        }

        /// <summary>Agrega un trabajo al atleta, con los parciales que vengan.</summary>
        [HttpPost]
        public async Task<ActionResult<TrabajoDto>> Crear(int entrenamientoId, int atletaId, [FromBody] GuardarTrabajoRequest request)
        {
            if (!await _trabajoRepositorio.ExisteVinculoAsync(entrenamientoId, atletaId))
            {
                return NotFound(new { mensaje = "El atleta no está asignado a esa sesión." });
            }

            var trabajo = new Trabajo
            {
                EntrenamientoId = entrenamientoId,
                AtletaId = atletaId,
                Tipo = request.Tipo!.Value,
                HoraInicio = TiempoTexto.IntentaParseHora(request.HoraInicio, out var hora) ? hora : null,
                Observaciones = Limpiar(request.Observaciones),
                FechaCreacion = DateTime.UtcNow
            };

            var creado = await _trabajoRepositorio.CrearAsync(
                trabajo,
                ArmarParciales(request),
                ArmarEjercicios(request),
                ArmarPaladas(request));

            return CreatedAtAction(
                nameof(Listar),
                new { entrenamientoId, atletaId },
                creado.ToDto());
        }

        /// <summary>Modifica un trabajo y reemplaza sus parciales por los que vengan.</summary>
        [HttpPut("{trabajoId:int}")]
        public async Task<ActionResult<TrabajoDto>> Actualizar(
            int entrenamientoId,
            int atletaId,
            int trabajoId,
            [FromBody] GuardarTrabajoRequest request)
        {
            var trabajo = await _trabajoRepositorio.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null || trabajo.EntrenamientoId != entrenamientoId || trabajo.AtletaId != atletaId)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            trabajo.Tipo = request.Tipo!.Value;
            trabajo.HoraInicio = TiempoTexto.IntentaParseHora(request.HoraInicio, out var hora) ? hora : null;
            trabajo.Observaciones = Limpiar(request.Observaciones);

            await _trabajoRepositorio.ActualizarAsync(
                trabajo,
                ArmarParciales(request),
                ArmarEjercicios(request),
                ArmarPaladas(request));

            var actualizado = await _trabajoRepositorio.ObtenerPorIdAsync(trabajoId);

            return Ok(actualizado!.ToDto());
        }

        /// <summary>Elimina el trabajo junto con sus parciales.</summary>
        [HttpDelete("{trabajoId:int}")]
        public async Task<IActionResult> Eliminar(int entrenamientoId, int atletaId, int trabajoId)
        {
            var trabajo = await _trabajoRepositorio.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null || trabajo.EntrenamientoId != entrenamientoId || trabajo.AtletaId != atletaId)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            await _trabajoRepositorio.EliminarAsync(trabajo);

            return NoContent();
        }

        /// <summary>
        /// Historial completo del atleta: sus trabajos en todas las sesiones,
        /// del mas nuevo al mas viejo, con la fecha y el turno de cada sesion.
        /// </summary>
        [HttpGet("/api/atletas/{atletaId:int}/trabajos")]
        public async Task<ActionResult<IEnumerable<TrabajoDto>>> HistorialPorAtleta(int atletaId)
        {
            var trabajos = await _trabajoRepositorio.ObtenerHistorialAsync(atletaId);

            return Ok(trabajos.Select(t => t.ToDto()));
        }

        /// <summary>
        /// Los parciales solo aplican a tierra y agua. Si el trabajo es de
        /// gimnasio se ignoran, para que no queden datos mezclados.
        /// </summary>
        private static IEnumerable<TrabajoParcial> ArmarParciales(GuardarTrabajoRequest request)
        {
            if (request.Tipo == TipoTrabajo.Gimnasio)
            {
                return [];
            }

            return request.Parciales
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
        }

        /// <summary>Los ejercicios solo aplican a los trabajos de gimnasio.</summary>
        private static IEnumerable<TrabajoEjercicio> ArmarEjercicios(GuardarTrabajoRequest request)
        {
            if (request.Tipo != TipoTrabajo.Gimnasio)
            {
                return [];
            }

            return request.Ejercicios
                .Where(e => !string.IsNullOrWhiteSpace(e.Nombre))
                .Select(e =>
                {
                    var ejercicio = new TrabajoEjercicio
                    {
                        Nombre = e.Nombre.Trim()
                    };

                    foreach (var serie in e.Series)
                    {
                        ejercicio.Series.Add(new TrabajoSerie
                        {
                            Repeticiones = serie.Repeticiones,
                            Porcentaje = serie.Porcentaje,
                            PesoKg = serie.PesoKg
                        });
                    }

                    return ejercicio;
                });
        }

        /// <summary>
        /// Las muestras de paladas solo aplican a los trabajos de agua. En el
        /// resto se ignoran, para que no queden datos mezclados.
        /// </summary>
        private static IEnumerable<TrabajoPalada> ArmarPaladas(GuardarTrabajoRequest request)
        {
            if (request.Tipo != TipoTrabajo.Agua)
            {
                return [];
            }

            return request.Paladas
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
        }

        /// <summary>Un texto en blanco se guarda como null, no como cadena vacia.</summary>
        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
