using Entidades;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class EntrenamientoEquipoRepositorio : IEntrenamientoEquipoRepositorio
    {
        private readonly CoachDbContext _context;

        public EntrenamientoEquipoRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<EntrenamientoEquipo?> ObtenerAsync(int id) =>
            ConDetalle(_context.EntrenamientoEquipos)
                .FirstOrDefaultAsync(e => e.Id == id);

        public Task<bool> ExisteEnSesionAsync(int entrenamientoId, int entrenamientoEquipoId) =>
            _context.EntrenamientoEquipos.AnyAsync(e =>
                e.Id == entrenamientoEquipoId && e.EntrenamientoId == entrenamientoId);

        public Task<bool> EsMiembroAsync(int entrenamientoEquipoId, int atletaId) =>
            _context.EntrenamientoEquipoMiembros.AnyAsync(m =>
                m.EntrenamientoEquipoId == entrenamientoEquipoId && m.AtletaId == atletaId);

        public async Task<EntrenamientoEquipo> CargarAsync(
            int entrenamientoId,
            TipoBote tipo,
            IReadOnlyList<int> atletaIds,
            int? equipoCatalogoId)
        {
            var equipo = new EntrenamientoEquipo
            {
                EntrenamientoId = entrenamientoId,
                Tipo = tipo,
                EquipoId = equipoCatalogoId,
                FechaCreacion = DateTime.UtcNow
            };

            for (var i = 0; i < atletaIds.Count; i++)
            {
                equipo.Miembros.Add(new EntrenamientoEquipoMiembro
                {
                    AtletaId = atletaIds[i],
                    Orden = i + 1
                });
            }

            _context.EntrenamientoEquipos.Add(equipo);
            await _context.SaveChangesAsync();
            return (await ObtenerAsync(equipo.Id))!;
        }

        public async Task EliminarAsync(EntrenamientoEquipo equipo)
        {
            _context.EntrenamientoEquipos.Remove(equipo);
            await _context.SaveChangesAsync();
        }

        private static IQueryable<EntrenamientoEquipo> ConDetalle(IQueryable<EntrenamientoEquipo> consulta) =>
            consulta
                .Include(e => e.Miembros)
                    .ThenInclude(m => m.Atleta)
                .Include(e => e.Trabajos)
                    .ThenInclude(t => t.Parciales)
                .Include(e => e.Trabajos)
                    .ThenInclude(t => t.Ejercicios)
                        .ThenInclude(ej => ej.Series);
    }
}
