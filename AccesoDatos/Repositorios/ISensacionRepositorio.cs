using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface ISensacionRepositorio
    {
        Task<List<Sensacion>> ListarAsync(int? atletaId);

        Task<Sensacion?> ObtenerAsync(int id);

        Task<Sensacion> CrearAsync(Sensacion sensacion);

        Task ActualizarAsync(Sensacion sensacion);

        Task EliminarAsync(Sensacion sensacion);
    }
}
