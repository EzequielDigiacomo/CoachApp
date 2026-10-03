using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface IAnotacionRepositorio
    {
        /// <summary>
        /// Lista anotaciones, de la mas nueva a la mas vieja, con sus vinculos
        /// cargados. Los filtros son opcionales y se combinan entre si.
        /// </summary>
        Task<List<Anotacion>> ObtenerAsync(
            int? atletaId,
            int? entrenamientoId,
            int? trabajoId,
            string? busqueda);

        Task<Anotacion?> ObtenerPorIdAsync(int id);

        Task<Anotacion> CrearAsync(Anotacion anotacion);

        Task ActualizarAsync(Anotacion anotacion);

        Task EliminarAsync(Anotacion anotacion);
    }
}
