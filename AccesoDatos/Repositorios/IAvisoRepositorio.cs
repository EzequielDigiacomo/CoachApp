using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface IAvisoRepositorio
    {
        Task<List<Aviso>> ListarAsync(bool soloActivos);

        Task<Aviso?> ObtenerAsync(int id);

        Task<Aviso> CrearAsync(Aviso aviso);

        Task ActualizarAsync(Aviso aviso);

        Task EliminarAsync(Aviso aviso);
    }
}
