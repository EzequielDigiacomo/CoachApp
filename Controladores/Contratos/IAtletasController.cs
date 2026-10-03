using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>
    /// Contrato del controlador de atletas.
    /// </summary>
    public interface IAtletasController
    {
        Task<ActionResult<IEnumerable<AtletaDto>>> Listar(string? busqueda, bool incluirInactivos);

        Task<ActionResult<AtletaDto>> ObtenerPorId(int id);

        Task<ActionResult<AtletaDto>> ObtenerPorDni(string dni);

        Task<ActionResult<AtletaDto>> Crear(CrearAtletaRequest request);

        Task<ActionResult<AtletaDto>> Actualizar(int id, ActualizarAtletaRequest request);

        Task<IActionResult> DarDeBaja(int id);

        Task<ActionResult<AtletaDto>> CambiarEstado(int id, ActualizarEstadoAtletaRequest request);
    }
}
