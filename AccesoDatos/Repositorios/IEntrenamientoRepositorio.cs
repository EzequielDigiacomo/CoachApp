using Entidades;
using Entidades.Enums;

namespace AccesoDatos.Repositorios
{
    public interface IEntrenamientoRepositorio
    {
        /// <summary>Lista sesiones por rango de fechas, turno y numero de sesion.</summary>
        Task<List<Entrenamiento>> ObtenerAsync(DateOnly? desde, DateOnly? hasta, Turno? turno, int? sesion);

        /// <summary>Trae la sesion con los atletas asignados.</summary>
        Task<Entrenamiento?> ObtenerPorIdAsync(int id);

        /// <summary>
        /// Busca otra sesion que ocupe el mismo dia, turno y numero de sesion.
        /// Se usa para impedir duplicados.
        /// </summary>
        Task<bool> ExisteSlotAsync(DateOnly fecha, Turno turno, int sesion, int? excluirId = null);

        Task<Entrenamiento> CrearAsync(Entrenamiento entrenamiento);

        Task ActualizarAsync(Entrenamiento entrenamiento);

        Task EliminarAsync(Entrenamiento entrenamiento);

        /// <summary>Devuelve los ids de atletas que existen y estan activos.</summary>
        Task<List<int>> FiltrarAtletasValidosAsync(IEnumerable<int> atletaIds);

        /// <summary>Suma atletas a la sesion, salteando los que ya estaban.</summary>
        Task AgregarAtletasAsync(Entrenamiento entrenamiento, IEnumerable<int> atletaIds);

        /// <summary>Quita un atleta de la sesion. Devuelve false si no estaba.</summary>
        Task<bool> QuitarAtletaAsync(int entrenamientoId, int atletaId);

        /// <summary>Marca la asistencia de un atleta. Devuelve false si no pertenece a la sesion.</summary>
        Task<bool> MarcarAsistenciaAsync(int entrenamientoId, int atletaId, bool? asistio);
    }
}
