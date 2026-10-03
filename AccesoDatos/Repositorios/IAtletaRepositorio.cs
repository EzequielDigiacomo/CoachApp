using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface IAtletaRepositorio
    {
        /// <summary>
        /// Lista atletas, opcionalmente filtrando por nombre, apellido o DNI.
        /// Por defecto excluye los dados de baja.
        /// </summary>
        Task<List<Atleta>> ObtenerTodosAsync(string? busqueda, bool incluirInactivos);

        Task<Atleta?> ObtenerPorIdAsync(int id);

        Task<Atleta?> ObtenerPorDniAsync(string dni);

        Task<bool> ExisteDniAsync(string dni, int? excluirId = null);

        Task<Atleta> CrearAsync(Atleta atleta);

        Task ActualizarAsync(Atleta atleta);
    }
}
