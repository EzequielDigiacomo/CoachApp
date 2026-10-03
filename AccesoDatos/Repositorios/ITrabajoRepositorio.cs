using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface ITrabajoRepositorio
    {
        /// <summary>Trabajos del atleta en esa sesion, con sus parciales, ejercicios y paladas.</summary>
        Task<List<Trabajo>> ObtenerPorAtletaAsync(int entrenamientoId, int atletaId);

        /// <summary>
        /// Todos los trabajos del atleta a lo largo de sus sesiones, del mas
        /// nuevo al mas viejo. Es la base del historial por atleta.
        /// </summary>
        Task<List<Trabajo>> ObtenerHistorialAsync(int atletaId);

        /// <summary>Trae un trabajo con sus parciales, ejercicios y paladas.</summary>
        Task<Trabajo?> ObtenerPorIdAsync(int trabajoId);

        /// <summary>Indica si el atleta esta asignado a esa sesion.</summary>
        Task<bool> ExisteVinculoAsync(int entrenamientoId, int atletaId);

        Task<Trabajo> CrearAsync(
            Trabajo trabajo,
            IEnumerable<TrabajoParcial> parciales,
            IEnumerable<TrabajoEjercicio> ejercicios,
            IEnumerable<TrabajoPalada> paladas);

        /// <summary>Guarda los datos del trabajo y reemplaza sus parciales, ejercicios y paladas.</summary>
        Task ActualizarAsync(
            Trabajo trabajo,
            IEnumerable<TrabajoParcial> parciales,
            IEnumerable<TrabajoEjercicio> ejercicios,
            IEnumerable<TrabajoPalada> paladas);

        Task EliminarAsync(Trabajo trabajo);
    }
}
