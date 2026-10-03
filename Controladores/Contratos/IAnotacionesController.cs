using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>Contrato del controlador de anotaciones.</summary>
    public interface IAnotacionesController
    {
        Task<ActionResult<IEnumerable<AnotacionDto>>> Listar(
            int? atletaId,
            int? entrenamientoId,
            int? trabajoId,
            string? busqueda);

        Task<ActionResult<AnotacionDto>> ObtenerPorId(int id);

        Task<ActionResult<AnotacionDto>> Crear(GuardarAnotacionRequest request);

        Task<ActionResult<AnotacionDto>> Actualizar(int id, GuardarAnotacionRequest request);

        Task<IActionResult> Eliminar(int id);
    }
}
