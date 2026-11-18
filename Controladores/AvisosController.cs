using AccesoDatos.Repositorios;
using Controladores.Seguridad;
using Entidades;
using Entidades.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Avisos del staff hacia los atletas. Cualquier autenticado lee los activos;
    /// solo el cuerpo técnico crea, edita o borra.
    /// </summary>
    [ApiController]
    [Route("api/avisos")]
    [Authorize]
    public class AvisosController : ControllerBase
    {
        private readonly IAvisoRepositorio _avisos;

        public AvisosController(IAvisoRepositorio avisos)
        {
            _avisos = avisos;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AvisoDto>>> Listar()
        {
            var soloActivos = !Identidad.EsStaff(User);
            var avisos = await _avisos.ListarAsync(soloActivos);
            return Ok(avisos.Select(ADto));
        }

        [HttpPost]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<AvisoDto>> Crear([FromBody] GuardarAvisoRequest request)
        {
            var aviso = new Aviso
            {
                Titulo = request.Titulo.Trim(),
                Texto = request.Texto.Trim(),
                Link = NormalizarLink(request.Link),
                Activo = request.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            await _avisos.CrearAsync(aviso);
            return Ok(ADto(aviso));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<ActionResult<AvisoDto>> Actualizar(int id, [FromBody] GuardarAvisoRequest request)
        {
            var aviso = await _avisos.ObtenerAsync(id);
            if (aviso is null)
            {
                return NotFound(new { mensaje = "Aviso no encontrado." });
            }

            aviso.Titulo = request.Titulo.Trim();
            aviso.Texto = request.Texto.Trim();
            aviso.Link = NormalizarLink(request.Link);
            aviso.Activo = request.Activo;

            await _avisos.ActualizarAsync(aviso);
            return Ok(ADto(aviso));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Identidad.RolesStaff)]
        public async Task<IActionResult> Eliminar(int id)
        {
            var aviso = await _avisos.ObtenerAsync(id);
            if (aviso is null)
            {
                return NotFound(new { mensaje = "Aviso no encontrado." });
            }

            await _avisos.EliminarAsync(aviso);
            return NoContent();
        }

        private static string? NormalizarLink(string? link)
        {
            var limpio = link?.Trim();
            return string.IsNullOrEmpty(limpio) ? null : limpio;
        }

        private static AvisoDto ADto(Aviso aviso) => new()
        {
            Id = aviso.Id,
            Titulo = aviso.Titulo,
            Texto = aviso.Texto,
            Link = aviso.Link,
            Activo = aviso.Activo,
            FechaCreacion = aviso.FechaCreacion
        };
    }
}
