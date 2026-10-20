using Entidades;
using Entidades.Enums;

namespace AccesoDatos.Repositorios
{
    public interface IControlRepositorio
    {
        Task<List<Control>> ObtenerAsync(int? atletaId, TipoTrabajo? tipo);

        Task<Control?> ObtenerPorIdAsync(int id);

        Task<bool> ExisteAtletaAsync(int atletaId);

        Task<Control> CrearAsync(Control control, IEnumerable<ControlEjercicio> ejercicios);

        Task ActualizarAsync(Control control, IEnumerable<ControlEjercicio> ejercicios);

        Task EliminarAsync(Control control);
    }
}
