using Entidades;
using Entidades.Enums;

namespace AccesoDatos.Repositorios
{
    public interface IControlRepositorio
    {
        Task<List<Control>> ObtenerAsync(int? atletaId, TipoTrabajo? tipo);

        Task<Control?> ObtenerPorIdAsync(int id);

        Task<bool> ExisteAtletaAsync(int atletaId);

        /// <summary>Ids de trabajos que ya tienen al menos un control vinculado.</summary>
        Task<HashSet<int>> IdsTrabajosMarcadosAsync(IEnumerable<int> trabajoIds);

        Task<List<Control>> ObtenerPorTrabajoIdAsync(int trabajoId);

        Task<Control> CrearAsync(Control control, IEnumerable<ControlEjercicio> ejercicios);

        Task ActualizarAsync(Control control, IEnumerable<ControlEjercicio> ejercicios);

        Task EliminarAsync(Control control);

        Task EliminarRangoAsync(IEnumerable<Control> controles);
    }
}
