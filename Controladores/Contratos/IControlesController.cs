using Entidades.DTOs;
using Entidades.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    public interface IControlesController
    {
        Task<ActionResult<IEnumerable<ControlDto>>> Listar(
            [FromQuery] int? atletaId,
            [FromQuery] TipoTrabajo? tipo);

        Task<ActionResult<IEnumerable<ControlDto>>> Mios([FromQuery] TipoTrabajo? tipo);

        Task<ActionResult<ControlDto>> ObtenerPorId(int id);

        Task<ActionResult<ControlDto>> Crear([FromBody] GuardarControlRequest request);

        Task<ActionResult<ControlDto>> Actualizar(int id, [FromBody] GuardarControlRequest request);

        Task<IActionResult> Eliminar(int id);
    }
}
