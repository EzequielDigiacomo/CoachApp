using Entidades;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class EntrenamientoRepositorio : IEntrenamientoRepositorio
    {
        private readonly CoachDbContext _context;

        public EntrenamientoRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public async Task<List<Entrenamiento>> ObtenerAsync(
            DateOnly? desde,
            DateOnly? hasta,
            Turno? turno,
            int? sesion)
        {
            // El listado necesita los atletas para contar, pero no sus trabajos:
            // esos solo hacen falta en el detalle.
            var query = _context.Entrenamientos
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Atleta)
                .AsQueryable();

            if (desde is not null)
            {
                query = query.Where(e => e.Fecha >= desde);
            }

            if (hasta is not null)
            {
                query = query.Where(e => e.Fecha <= hasta);
            }

            if (turno is not null)
            {
                query = query.Where(e => e.Turno == turno);
            }

            if (sesion is not null)
            {
                query = query.Where(e => e.Sesion == sesion);
            }

            // Orden cronologico: lo que viene primero es lo que el coach tiene mas cerca.
            return await query
                .OrderBy(e => e.Fecha)
                .ThenBy(e => e.Turno)
                .ThenBy(e => e.Sesion)
                .ToListAsync();
        }

        public Task<Entrenamiento?> ObtenerPorIdAsync(int id) =>
            _context.Entrenamientos
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Atleta)
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Trabajos)
                .FirstOrDefaultAsync(e => e.Id == id);

        public Task<bool> ExisteSlotAsync(DateOnly fecha, Turno turno, int sesion, int? excluirId = null) =>
            _context.Entrenamientos.AnyAsync(e =>
                e.Fecha == fecha &&
                e.Turno == turno &&
                e.Sesion == sesion &&
                (excluirId == null || e.Id != excluirId));

        public async Task<Entrenamiento> CrearAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Add(entrenamiento);
            await _context.SaveChangesAsync();
            return entrenamiento;
        }

        public async Task ActualizarAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Update(entrenamiento);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Remove(entrenamiento);
            await _context.SaveChangesAsync();
        }

        public Task<List<int>> FiltrarAtletasValidosAsync(IEnumerable<int> atletaIds)
        {
            var ids = atletaIds.Distinct().ToList();

            return _context.Atletas
                .Where(a => a.Activo && ids.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();
        }

        public async Task AgregarAtletasAsync(Entrenamiento entrenamiento, IEnumerable<int> atletaIds)
        {
            var existentes = entrenamiento.Atletas
                .Select(a => a.AtletaId)
                .ToHashSet();

            var nuevos = atletaIds
                .Distinct()
                .Where(id => !existentes.Contains(id));

            foreach (var atletaId in nuevos)
            {
                entrenamiento.Atletas.Add(new EntrenamientoAtleta
                {
                    EntrenamientoId = entrenamiento.Id,
                    AtletaId = atletaId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> QuitarAtletaAsync(int entrenamientoId, int atletaId)
        {
            var vinculo = await _context.EntrenamientoAtletas
                .FirstOrDefaultAsync(ea => ea.EntrenamientoId == entrenamientoId && ea.AtletaId == atletaId);

            if (vinculo is null)
            {
                return false;
            }

            _context.EntrenamientoAtletas.Remove(vinculo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarcarAsistenciaAsync(int entrenamientoId, int atletaId, bool? asistio)
        {
            var vinculo = await _context.EntrenamientoAtletas
                .FirstOrDefaultAsync(ea => ea.EntrenamientoId == entrenamientoId && ea.AtletaId == atletaId);

            if (vinculo is null)
            {
                return false;
            }

            vinculo.Asistio = asistio;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
