using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class AnotacionRepositorio : IAnotacionRepositorio
    {
        private readonly CoachDbContext _context;

        public AnotacionRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public async Task<List<Anotacion>> ObtenerAsync(
            int? atletaId,
            int? entrenamientoId,
            int? trabajoId,
            string? busqueda)
        {
            var query = _context.Anotaciones.AsQueryable();

            if (atletaId is not null)
            {
                query = query.Where(a => a.AtletaId == atletaId);
            }

            if (entrenamientoId is not null)
            {
                query = query.Where(a => a.EntrenamientoId == entrenamientoId);
            }

            if (trabajoId is not null)
            {
                query = query.Where(a => a.TrabajoId == trabajoId);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(a =>
                    EF.Functions.ILike(a.Texto, $"%{texto}%") ||
                    (a.Titulo != null && EF.Functions.ILike(a.Titulo, $"%{texto}%")));
            }

            return await ConVinculos(query)
                .OrderByDescending(a => a.FechaCreacion)
                .ThenByDescending(a => a.Id)
                .ToListAsync();
        }

        public Task<Anotacion?> ObtenerPorIdAsync(int id) =>
            ConVinculos(_context.Anotaciones.Where(a => a.Id == id))
                .FirstOrDefaultAsync();

        public async Task<Anotacion> CrearAsync(Anotacion anotacion)
        {
            _context.Anotaciones.Add(anotacion);
            await _context.SaveChangesAsync();
            return anotacion;
        }

        public async Task ActualizarAsync(Anotacion anotacion)
        {
            _context.Anotaciones.Update(anotacion);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Anotacion anotacion)
        {
            _context.Anotaciones.Remove(anotacion);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Trae los vinculos que el DTO necesita para mostrar de que atleta,
        /// sesion y trabajo se trata cada anotacion.
        /// </summary>
        private static IQueryable<Anotacion> ConVinculos(IQueryable<Anotacion> query) =>
            query
                .Include(a => a.Atleta)
                .Include(a => a.Entrenamiento)
                .Include(a => a.Trabajo)
                .Include(a => a.Usuario);
    }
}
