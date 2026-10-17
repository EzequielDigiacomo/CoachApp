using Entidades;
using Entidades.Enums;

namespace AccesoDatos.Repositorios
{
    public interface IEquipoRepositorio
    {
        Task<List<Equipo>> ListarAsync(bool incluirInactivos = false);

        Task<Equipo?> ObtenerAsync(int id);

        Task<Equipo> CrearAsync(TipoBote tipo, IReadOnlyList<int> atletaIds);

        Task<Equipo?> ActualizarAsync(int id, TipoBote tipo, IReadOnlyList<int> atletaIds);

        Task<bool> CambiarEstadoAsync(int id, bool activo);
    }
}
