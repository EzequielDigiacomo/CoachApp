using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface IAnotacionAtletaRepositorio
    {
        Task<List<AnotacionAtleta>> ListarAsync(int? atletaId);

        Task<AnotacionAtleta?> ObtenerAsync(int id);

        Task<AnotacionAtleta> CrearAsync(AnotacionAtleta nota);

        Task ActualizarAsync(AnotacionAtleta nota);

        Task EliminarAsync(AnotacionAtleta nota);
    }
}
