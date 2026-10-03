using Controladores.Contratos;
using Controladores.Integraciones;
using Entidades.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Lee datos de fuentes externas. Hoy: planillas de Google Sheets, para
    /// traer la descripcion de una sesion tal como esta escrita. Requiere
    /// estar autenticado.
    /// </summary>
    [ApiController]
    [Route("api/integraciones")]
    [Authorize]
    public class IntegracionesController : ControllerBase, IIntegracionesController
    {
        private readonly ILectorHojasGoogle _lector;

        public IntegracionesController(ILectorHojasGoogle lector)
        {
            _lector = lector;
        }

        /// <summary>
        /// Lista los nombres de las pestañas del libro. Es lo que le permite
        /// elegir al entrenador cuando el libro tiene muchas pestañas.
        /// </summary>
        [HttpPost("hojas/pestanas")]
        public async Task<ActionResult<IEnumerable<string>>> ListarPestanas([FromBody] LeerHojaRequest request)
        {
            try
            {
                return Ok(await _lector.ListarPestanasAsync(request.Url));
            }
            catch (HojaException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Devuelve el contenido de una pestaña, sin interpretar nada. Los
        /// problemas con el link se devuelven como 400 con el mensaje ya
        /// escrito para mostrarle al entrenador.
        /// </summary>
        [HttpPost("hojas/leer")]
        public async Task<ActionResult<HojaLeidaDto>> LeerHoja([FromBody] LeerHojaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Pestana))
            {
                return BadRequest(new { mensaje = "Hay que indicar qué pestaña leer." });
            }

            try
            {
                return Ok(await _lector.LeerAsync(request.Url, request.Pestana.Trim(), request.Rango));
            }
            catch (HojaException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
