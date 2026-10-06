using Entidades.DTOs;
using Entidades.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>
    /// Contrato del controlador de entrenamientos.
    /// </summary>
    public interface IEntrenamientosController
    {
        Task<ActionResult<IEnumerable<EntrenamientoDto>>> Listar(
            DateOnly? desde,
            DateOnly? hasta,
            Turno? turno,
            int? sesion);

        Task<ActionResult<EntrenamientoDto>> ObtenerPorId(int id);

        Task<ActionResult<EntrenamientoDto>> Crear(CrearEntrenamientoRequest request);

        Task<ActionResult<EntrenamientoDto>> Actualizar(int id, ActualizarEntrenamientoRequest request);

        Task<IActionResult> Eliminar(int id);

        Task<ActionResult<EntrenamientoDto>> AgregarAtletas(int id, AsignarAtletasRequest request);

        Task<ActionResult<EntrenamientoDto>> QuitarAtleta(int id, int atletaId);

        Task<ActionResult<EntrenamientoDto>> MarcarAsistencia(int id, MarcarAsistenciaRequest request);

        Task<ActionResult<IEnumerable<SesionHistorialDto>>> HistorialAsistencia(int atletaId);
    }
}
