using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>Contrato del controlador de Garmin Connect.</summary>
    public interface IGarminController
    {
        Task<ActionResult<EstadoGarminDto>> Estado();

        Task<ActionResult<EstadoGarminDto>> Vincular(VincularGarminRequest request);

        Task<IActionResult> Desvincular();

        Task<ActionResult<SincronizacionGarminDto>> Sincronizar(int entrenamientoId);
    }
}
