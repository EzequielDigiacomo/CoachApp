using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>
    /// Contrato del controlador de autenticacion.
    /// </summary>
    public interface IAuthController
    {
        Task<ActionResult<LoginResponse>> Login(LoginRequest request);

        Task<ActionResult<UsuarioDto>> Me();
    }
}
