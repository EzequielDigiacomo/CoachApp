using AccesoDatos.Repositorios;
using Controladores.Contratos;
using Entidades;
using Entidades.DTOs;
using Entidades.Enums;
using Controladores.Seguridad;
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
        private readonly IEntrenamientoRepositorio _entrenamientoRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public TrabajosController(
            ITrabajoRepositorio trabajoRepositorio,
            IEntrenamientoRepositorio entrenamientoRepositorio,
            IUsuarioRepositorio usuarioRepositorio)
        {
            _trabajoRepositorio = trabajoRepositorio;
            _entrenamientoRepositorio = entrenamientoRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        /// <summary>Lista los trabajos del atleta en esa sesion, del mas temprano al mas tarde.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrabajoDto>>> Listar(int entrenamientoId, int atletaId)
        {
            if (await Ajeno(atletaId) is { } rechazo)
            {
                return rechazo;
            }

            if (!await _trabajoRepositorio.ExisteVinculoAsync(entrenamientoId, atletaId))
            {
                return NotFound(new { mensaje = "El atleta no está asignado a esa sesión." });
            }

            var propios = await _trabajoRepositorio.ObtenerPorAtletaAsync(entrenamientoId, atletaId);
            var deBote = await _trabajoRepositorio.ObtenerDeEquiposPorAtletaAsync(entrenamientoId, atletaId);
            var trabajos = propios
                .Concat(deBote)
                .OrderBy(t => t.HoraInicio)
                .ThenBy(t => t.Id)
                .Select(t => t.ToDto());

            return Ok(trabajos);
        }

        /// <summary>Agrega un trabajo al atleta, con los parciales que vengan.</summary>
        [HttpPost]
        public async Task<ActionResult<TrabajoDto>> Crear(int entrenamientoId, int atletaId, [FromBody] GuardarTrabajoRequest request)
        {
            if (await Ajeno(atletaId) is { } rechazo)
            {
                return rechazo;
            }

            if (!await _trabajoRepositorio.ExisteVinculoAsync(entrenamientoId, atletaId))
            {
                return NotFound(new { mensaje = "El atleta no está asignado a esa sesión." });
            }

            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(entrenamientoId);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            if (AtletaSinPermisoDeCarga(entrenamiento) is { } sinPermiso)
            {
                return sinPermiso;
            }

            // El tipo lo fija la sesion; el cliente no puede mandar otro.
            request.Tipo = entrenamiento.Tipo;

            var trabajo = new Trabajo
            {
                EntrenamientoId = entrenamientoId,
                AtletaId = atletaId,
                Tipo = entrenamiento.Tipo,
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
            if (await Ajeno(atletaId) is { } rechazo)
            {
                return rechazo;
            }

            var trabajo = await _trabajoRepositorio.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null || trabajo.EntrenamientoId != entrenamientoId || trabajo.AtletaId != atletaId)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(entrenamientoId);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            if (AtletaSinPermisoDeCarga(entrenamiento) is { } sinPermiso)
            {
                return sinPermiso;
            }

            request.Tipo = entrenamiento.Tipo;
            trabajo.Tipo = entrenamiento.Tipo;
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
            if (await Ajeno(atletaId) is { } rechazo)
            {
                return rechazo;
            }

            var trabajo = await _trabajoRepositorio.ObtenerPorIdAsync(trabajoId);
            if (trabajo is null || trabajo.EntrenamientoId != entrenamientoId || trabajo.AtletaId != atletaId)
            {
                return NotFound(new { mensaje = "Trabajo no encontrado." });
            }

            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(entrenamientoId);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            if (AtletaSinPermisoDeCarga(entrenamiento) is { } sinPermiso)
            {
                return sinPermiso;
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
            if (await Ajeno(atletaId) is { } rechazo)
            {
                return rechazo;
            }

            var trabajos = await _trabajoRepositorio.ObtenerHistorialAsync(atletaId);

            return Ok(trabajos.Select(t => t.ToDto()));
        }

        /// <summary>
        /// El atleta solo puede cargar/editar trabajos de gimnasio.
        /// Tierra y agua los carga el staff o llegan de Garmin.
        /// </summary>
        private ActionResult? AtletaSinPermisoDeCarga(Entrenamiento entrenamiento)
        {
            if (!Identidad.EsAtleta(User))
            {
                return null;
            }

            if (entrenamiento.Tipo == TipoTrabajo.Gimnasio)
            {
                return null;
            }

            return StatusCode(403, new
            {
                mensaje = "En tierra y agua solo el entrenador puede cargar trabajos. Vos podés ver lo cargado y lo que viene de Garmin."
            });
        }

        /// <summary>
        /// Un atleta solo puede tocar sus propios trabajos. Si el token no
        /// trae el claim, se confirma contra el usuario en la base.
        /// </summary>
        private async Task<ActionResult?> Ajeno(int atletaId)
        {
            if (!Identidad.EsAtleta(User))
            {
                return null;
            }

            var propio = Identidad.AtletaId(User);
            if (propio is null)
            {
                var usuarioId = Identidad.UsuarioId(User);
                if (usuarioId is int id)
                {
                    var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(id);
                    propio = usuario?.AtletaId;
                }
            }

            return propio == atletaId
                ? null
                : StatusCode(403, new { mensaje = "No podés ver los trabajos de otro atleta." });
        }

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
