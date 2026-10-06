using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Controladores.Contratos;
using Controladores.Integraciones;
using Entidades.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controladores
{
    /// <summary>
    /// Vincula la cuenta de Garmin del entrenador y trae las actividades
    /// del dia de sus amigos para la sesion.
    /// </summary>
    [ApiController]
    [Route("api/garmin")]
    [Authorize]
    public class GarminController : ControllerBase, IGarminController
    {
        private readonly IGarminServicio _garmin;

        public GarminController(IGarminServicio garmin)
        {
            _garmin = garmin;
        }

        /// <summary>Dice si el entrenador ya vinculo Garmin.</summary>
        [HttpGet("estado")]
        public async Task<ActionResult<EstadoGarminDto>> Estado()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId is null)
            {
                return Unauthorized();
            }

            return Ok(await _garmin.EstadoAsync(usuarioId.Value));
        }

        /// <summary>Abre sesion en Garmin y guarda los tokens, no la contrasena.</summary>
        [HttpPost("vincular")]
        public async Task<ActionResult<EstadoGarminDto>> Vincular([FromBody] VincularGarminRequest request)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId is null)
            {
                return Unauthorized();
            }

            try
            {
                var estado = await _garmin.VincularAsync(
                    usuarioId.Value,
                    request.Usuario,
                    request.Password,
                    HttpContext.RequestAborted);

                return Ok(estado);
            }
            catch (GarminExcepcion ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>Olvida la sesion de Garmin de este entrenador.</summary>
        [HttpDelete]
        public async Task<IActionResult> Desvincular()
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId is null)
            {
                return Unauthorized();
            }

            await _garmin.DesvincularAsync(usuarioId.Value);
            return NoContent();
        }

        /// <summary>
        /// Lee las actividades de los amigos en la fecha de la sesion
        /// y las asocia a los atletas que se llaman igual.
        /// </summary>
        [HttpPost("sesiones/{entrenamientoId:int}")]
        public async Task<ActionResult<SincronizacionGarminDto>> Sincronizar(int entrenamientoId)
        {
            var usuarioId = UsuarioActualId();
            if (usuarioId is null)
            {
                return Unauthorized();
            }

            try
            {
                var resultado = await _garmin.SincronizarAsync(
                    usuarioId.Value,
                    entrenamientoId,
                    HttpContext.RequestAborted);

                if (resultado is null)
                {
                    return NotFound(new { mensaje = "Entrenamiento no encontrado." });
                }

                return Ok(resultado);
            }
            catch (GarminExcepcion ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private int? UsuarioActualId()
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("nameid");

            return int.TryParse(valor, out var id) ? id : null;
        }
    }
}
