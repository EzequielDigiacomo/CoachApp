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

        public async Task<ResultadoPaginado<Entrenamiento>> ObtenerAsync(
            DateOnly? desde,
            DateOnly? hasta,
            Turno? turno,
            int? sesion,
            int? atletaId,
            int? pagina,
            int? tamano)
        {
            // Listado: solo atletas para contar/filtrar; sin trabajos ni series.
            var query = _context.Entrenamientos
                .AsNoTracking()
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

            if (atletaId is not null)
            {
                query = query.Where(e => e.Atletas.Any(a => a.AtletaId == atletaId));
            }

            query = query
                .OrderByDescending(e => e.Fecha)
                .ThenBy(e => e.Turno)
                .ThenBy(e => e.Sesion);

            return await ConsultaPaginada.EjecutarAsync(query, pagina, tamano);
        }

        public Task<Entrenamiento?> ObtenerPorIdAsync(int id) =>
            _context.Entrenamientos
                .AsSplitQuery()
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Atleta)
                // Resumen en la fila del atleta: parciales y ejercicios con series.
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Trabajos)
                        .ThenInclude(t => t.Parciales)
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Trabajos)
                        .ThenInclude(t => t.Ejercicios)
                            .ThenInclude(e => e.Series)
                .Include(e => e.Equipos)
                    .ThenInclude(eq => eq.Miembros)
                        .ThenInclude(m => m.Atleta)
                .Include(e => e.Equipos)
                    .ThenInclude(eq => eq.Trabajos)
                        .ThenInclude(t => t.Parciales)
                .Include(e => e.Equipos)
                    .ThenInclude(eq => eq.Trabajos)
                        .ThenInclude(t => t.Ejercicios)
                            .ThenInclude(ej => ej.Series)
                .FirstOrDefaultAsync(e => e.Id == id);

        /// <summary>Detalle liviano: atletas y equipos sin grafos de trabajos.</summary>
        public Task<Entrenamiento?> ObtenerResumenPorIdAsync(int id) =>
            _context.Entrenamientos
                .AsNoTracking()
                .AsSplitQuery()
                .Include(e => e.Atletas)
                    .ThenInclude(ea => ea.Atleta)
                .Include(e => e.Equipos)
                    .ThenInclude(eq => eq.Miembros)
                        .ThenInclude(m => m.Atleta)
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

        public Task ActualizarAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Update(entrenamiento);
            return _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Remove(entrenamiento);
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> FiltrarAtletasValidosAsync(IEnumerable<int> atletaIds)
        {
            var ids = atletaIds.Distinct().ToList();
            return await _context.Atletas
                .AsNoTracking()
                .Where(a => ids.Contains(a.Id) && a.Activo)
                .Select(a => a.Id)
                .ToListAsync();
        }

        public async Task AgregarAtletasAsync(Entrenamiento entrenamiento, IEnumerable<int> atletaIds)
        {
            var existentes = entrenamiento.Atletas.Select(a => a.AtletaId).ToHashSet();
            foreach (var atletaId in atletaIds.Distinct())
            {
                if (existentes.Contains(atletaId))
                {
                    continue;
                }

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

        public Task<List<EntrenamientoAtleta>> ObtenerHistorialAsistenciaAsync(int atletaId) =>
            _context.EntrenamientoAtletas
                .AsNoTracking()
                .Include(ea => ea.Entrenamiento)
                .Where(ea => ea.AtletaId == atletaId && ea.Asistio != null)
                .OrderByDescending(ea => ea.Entrenamiento.Fecha)
                .ThenBy(ea => ea.Entrenamiento.Turno)
                .ThenBy(ea => ea.Entrenamiento.Sesion)
                .ToListAsync();
    }
}
