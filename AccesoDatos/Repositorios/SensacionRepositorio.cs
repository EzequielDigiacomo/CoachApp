using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class SensacionRepositorio : ISensacionRepositorio
    {
        private readonly CoachDbContext _context;

        public SensacionRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<List<Sensacion>> ListarAsync(int? atletaId)
        {
            var consulta = _context.Sensaciones
                .Include(s => s.Atleta)
                .AsQueryable();

            if (atletaId is int id)
            {
                consulta = consulta.Where(s => s.AtletaId == id);
            }

            return consulta
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();
        }

        public Task<Sensacion?> ObtenerAsync(int id) =>
            _context.Sensaciones.Include(s => s.Atleta).FirstOrDefaultAsync(s => s.Id == id);

        public async Task<Sensacion> CrearAsync(Sensacion sensacion)
        {
            _context.Sensaciones.Add(sensacion);
            await _context.SaveChangesAsync();
            return sensacion;
        }

        public async Task ActualizarAsync(Sensacion sensacion)
        {
            _context.Sensaciones.Update(sensacion);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Sensacion sensacion)
        {
            _context.Sensaciones.Remove(sensacion);
            await _context.SaveChangesAsync();
        }
    }
}
