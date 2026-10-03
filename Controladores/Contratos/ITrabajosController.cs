using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>
    /// Contrato del controlador de trabajos. Las rutas cuelgan de la sesion y
    /// del atleta, porque un trabajo siempre pertenece a los dos.
    /// </summary>
    public interface ITrabajosController
    {
        Task<ActionResult<IEnumerable<TrabajoDto>>> Listar(int entrenamientoId, int atletaId);

        Task<ActionResult<TrabajoDto>> Crear(int entrenamientoId, int atletaId, GuardarTrabajoRequest request);

        Task<ActionResult<TrabajoDto>> Actualizar(int entrenamientoId, int atletaId, int trabajoId, GuardarTrabajoRequest request);

        Task<IActionResult> Eliminar(int entrenamientoId, int atletaId, int trabajoId);

        /// <summary>
        /// Historial completo del atleta: sus trabajos en todas las sesiones,
        /// del mas nuevo al mas viejo, con los datos de la sesion de cada uno.
        /// </summary>
        Task<ActionResult<IEnumerable<TrabajoDto>>> HistorialPorAtleta(int atletaId);
    }
}
