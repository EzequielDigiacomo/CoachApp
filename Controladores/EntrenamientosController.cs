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
    /// Carga y administracion de sesiones de entrenamiento, y la asistencia
    /// de los atletas asignados. Requiere estar autenticado.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EntrenamientosController : ControllerBase, IEntrenamientosController
    {
        private readonly IEntrenamientoRepositorio _entrenamientoRepositorio;

        public EntrenamientosController(IEntrenamientoRepositorio entrenamientoRepositorio)
        {
            _entrenamientoRepositorio = entrenamientoRepositorio;
        }

        /// <summary>
        /// Lista sesiones de entrenamiento. Sin filtros devuelve todas,
        /// ordenadas de la mas proxima a la mas lejana.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntrenamientoDto>>> Listar(
            [FromQuery] DateOnly? desde,
            [FromQuery] DateOnly? hasta,
            [FromQuery] Turno? turno,
            [FromQuery] int? sesion)
        {
            var entrenamientos = await _entrenamientoRepositorio.ObtenerAsync(desde, hasta, turno, sesion);

            // En el listado no hacen falta los atletas de cada sesion.
            var dtos = entrenamientos
                .Select(e => e.ToDto())
                .Select(dto =>
                {
                    dto.Atletas = [];
                    return dto;
                });

            return Ok(dtos);
        }

        /// <summary>Devuelve una sesion con los atletas asignados y su asistencia.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EntrenamientoDto>> ObtenerPorId(int id)
        {
            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            return Ok(entrenamiento.ToDto());
        }

        /// <summary>Crea una sesion de entrenamiento, con atletas opcionales.</summary>
        [HttpPost]
        public async Task<ActionResult<EntrenamientoDto>> Crear([FromBody] CrearEntrenamientoRequest request)
        {
            if (await _entrenamientoRepositorio.ExisteSlotAsync(request.Fecha, request.Turno, request.Sesion))
            {
                return Conflict(new { mensaje = MensajeSlotOcupado });
            }

            var entrenamiento = new Entrenamiento
            {
                Fecha = request.Fecha,
                Turno = request.Turno,
                Sesion = request.Sesion,
                Descripcion = Limpiar(request.Descripcion),
                Club = Limpiar(request.Club),
                FechaCreacion = DateTime.UtcNow
            };

            await _entrenamientoRepositorio.CrearAsync(entrenamiento);

            if (request.AtletaIds.Count > 0)
            {
                var validos = await _entrenamientoRepositorio.FiltrarAtletasValidosAsync(request.AtletaIds);

                if (validos.Count > 0)
                {
                    await _entrenamientoRepositorio.AgregarAtletasAsync(entrenamiento, validos);
                }
            }

            var creado = await _entrenamientoRepositorio.ObtenerPorIdAsync(entrenamiento.Id);

            return CreatedAtAction(nameof(ObtenerPorId), new { id = entrenamiento.Id }, creado!.ToDto());
        }

        /// <summary>Modifica el dia, turno, sesion o descripcion de una sesion.</summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EntrenamientoDto>> Actualizar(int id, [FromBody] ActualizarEntrenamientoRequest request)
        {
            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            if (await _entrenamientoRepositorio.ExisteSlotAsync(request.Fecha, request.Turno, request.Sesion, excluirId: id))
            {
                return Conflict(new { mensaje = MensajeSlotOcupado });
            }

            entrenamiento.Fecha = request.Fecha;
            entrenamiento.Turno = request.Turno;
            entrenamiento.Sesion = request.Sesion;
            entrenamiento.Descripcion = Limpiar(request.Descripcion);
            entrenamiento.Club = Limpiar(request.Club);

            await _entrenamientoRepositorio.ActualizarAsync(entrenamiento);

            return Ok(entrenamiento.ToDto());
        }

        /// <summary>
        /// Elimina la sesion. Es una baja fisica: se borran tambien los vinculos
        /// con los atletas, pero no los atletas.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            await _entrenamientoRepositorio.EliminarAsync(entrenamiento);

            return NoContent();
        }

        /// <summary>Suma atletas a la sesion. Los ya asignados se saltean.</summary>
        [HttpPost("{id:int}/atletas")]
        public async Task<ActionResult<EntrenamientoDto>> AgregarAtletas(int id, [FromBody] AsignarAtletasRequest request)
        {
            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            if (request.AtletaIds.Count == 0)
            {
                return BadRequest(new { mensaje = "Hay que indicar al menos un atleta." });
            }

            var validos = await _entrenamientoRepositorio.FiltrarAtletasValidosAsync(request.AtletaIds);

            if (validos.Count == 0)
            {
                return BadRequest(new { mensaje = "Ninguno de los atletas indicados existe o esta activo." });
            }

            await _entrenamientoRepositorio.AgregarAtletasAsync(entrenamiento, validos);

            var actualizado = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);

            return Ok(actualizado!.ToDto());
        }

        /// <summary>Quita un atleta de la sesion.</summary>
        [HttpDelete("{id:int}/atletas/{atletaId:int}")]
        public async Task<ActionResult<EntrenamientoDto>> QuitarAtleta(int id, int atletaId)
        {
            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            var quitado = await _entrenamientoRepositorio.QuitarAtletaAsync(id, atletaId);
            if (!quitado)
            {
                return NotFound(new { mensaje = "El atleta no esta asignado a esta sesion." });
            }

            var actualizado = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);

            return Ok(actualizado!.ToDto());
        }

        /// <summary>
        /// Marca la asistencia de un atleta. Asistio en null deja la marca vacia.
        /// </summary>
        [HttpPatch("{id:int}/asistencia")]
        public async Task<ActionResult<EntrenamientoDto>> MarcarAsistencia(int id, [FromBody] MarcarAsistenciaRequest request)
        {
            var entrenamiento = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);
            if (entrenamiento is null)
            {
                return NotFound(new { mensaje = "Entrenamiento no encontrado." });
            }

            var marcado = await _entrenamientoRepositorio.MarcarAsistenciaAsync(id, request.AtletaId, request.Asistio);
            if (!marcado)
            {
                return NotFound(new { mensaje = "El atleta no esta asignado a esta sesion." });
            }

            var actualizado = await _entrenamientoRepositorio.ObtenerPorIdAsync(id);

            return Ok(actualizado!.ToDto());
        }

        /// <summary>
        /// Sesiones del atleta con asistencia marcada, de la mas nueva a la
        /// mas vieja. Presente y ausente entran aunque no haya trabajos.
        /// </summary>
        [HttpGet("/api/atletas/{atletaId:int}/sesiones")]
        public async Task<ActionResult<IEnumerable<SesionHistorialDto>>> HistorialAsistencia(int atletaId)
        {
            var vinculos = await _entrenamientoRepositorio.ObtenerHistorialAsistenciaAsync(atletaId);

            return Ok(vinculos.Select(vinculo => new SesionHistorialDto
            {
                EntrenamientoId = vinculo.EntrenamientoId,
                Fecha = vinculo.Entrenamiento.Fecha,
                Turno = vinculo.Entrenamiento.Turno,
                Sesion = vinculo.Entrenamiento.Sesion,
                Asistio = vinculo.Asistio!.Value
            }));
        }

        private const string MensajeSlotOcupado =
            "Ya existe una sesion cargada para esa fecha, ese turno y ese numero de sesion.";

        private static string? Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
