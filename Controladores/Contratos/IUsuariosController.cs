using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>
    /// Contrato del controlador de administracion de cuentas.
    /// </summary>
    public interface IUsuariosController
    {
        Task<ActionResult<IEnumerable<UsuarioDto>>> Listar();

        Task<ActionResult<UsuarioDto>> ObtenerPorId(int id);

        Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioRequest request);

        Task<ActionResult<UsuarioDto>> CambiarEstado(int id, ActualizarEstadoUsuarioRequest request);
    }
}
