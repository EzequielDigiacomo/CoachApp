using Entidades;
using Entidades.Enums;

namespace AccesoDatos.Repositorios
{
    public interface IEntrenamientoRepositorio
    {
        /// <summary>Lista sesiones filtradas y paginadas. atletaId limita en SQL.</summary>
        Task<ResultadoPaginado<Entrenamiento>> ObtenerAsync(
            DateOnly? desde,
            DateOnly? hasta,
            Turno? turno,
            int? sesion,
            int? atletaId,
            int? pagina,
            int? tamano);

        /// <summary>Trae la sesion con atletas, equipos y trabajos (detalle completo).</summary>
        Task<Entrenamiento?> ObtenerPorIdAsync(int id);

        /// <summary>Detalle sin grafos de trabajos (asistencia / cabecera).</summary>
        Task<Entrenamiento?> ObtenerResumenPorIdAsync(int id);

        Task<bool> ExisteSlotAsync(DateOnly fecha, Turno turno, int sesion, int? excluirId = null);

        Task<Entrenamiento> CrearAsync(Entrenamiento entrenamiento);

        Task ActualizarAsync(Entrenamiento entrenamiento);

        Task EliminarAsync(Entrenamiento entrenamiento);

        Task<List<int>> FiltrarAtletasValidosAsync(IEnumerable<int> atletaIds);

        Task AgregarAtletasAsync(Entrenamiento entrenamiento, IEnumerable<int> atletaIds);

        Task<bool> QuitarAtletaAsync(int entrenamientoId, int atletaId);

        Task<bool> MarcarAsistenciaAsync(int entrenamientoId, int atletaId, bool? asistio);

        Task<List<EntrenamientoAtleta>> ObtenerHistorialAsistenciaAsync(int atletaId);
    }
}
