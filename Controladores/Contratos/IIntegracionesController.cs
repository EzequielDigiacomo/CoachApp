using Entidades.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Controladores.Contratos
{
    /// <summary>Contrato del controlador de integraciones externas.</summary>
    public interface IIntegracionesController
    {
        Task<ActionResult<IEnumerable<string>>> ListarPestanas(LeerHojaRequest request);

        Task<ActionResult<HojaLeidaDto>> LeerHoja(LeerHojaRequest request);
    }
}
