using Entidades;
using Entidades.Enums;

namespace AccesoDatos.Repositorios
{
    public interface IEntrenamientoEquipoRepositorio
    {
        Task<EntrenamientoEquipo?> ObtenerAsync(int id);

        Task<bool> ExisteEnSesionAsync(int entrenamientoId, int entrenamientoEquipoId);

        Task<EntrenamientoEquipo> CargarAsync(
            int entrenamientoId,
            TipoBote tipo,
            IReadOnlyList<int> atletaIds,
            int? equipoCatalogoId);

        Task EliminarAsync(EntrenamientoEquipo equipo);
    }
}
