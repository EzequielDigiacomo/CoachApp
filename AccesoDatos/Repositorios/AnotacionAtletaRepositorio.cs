using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class AnotacionAtletaRepositorio : IAnotacionAtletaRepositorio
    {
        private readonly CoachDbContext _context;

        public AnotacionAtletaRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<List<AnotacionAtleta>> ListarAsync(int? atletaId)
        {
            var consulta = _context.AnotacionesAtleta
                .Include(a => a.Atleta)
                .AsQueryable();

            if (atletaId is int id)
            {
                consulta = consulta.Where(a => a.AtletaId == id);
            }

            return consulta
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
        }

        public Task<AnotacionAtleta?> ObtenerAsync(int id) =>
            _context.AnotacionesAtleta.Include(a => a.Atleta).FirstOrDefaultAsync(a => a.Id == id);

        public async Task<AnotacionAtleta> CrearAsync(AnotacionAtleta nota)
        {
            _context.AnotacionesAtleta.Add(nota);
            await _context.SaveChangesAsync();
            return nota;
        }

        public async Task ActualizarAsync(AnotacionAtleta nota)
        {
            _context.AnotacionesAtleta.Update(nota);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(AnotacionAtleta nota)
        {
            _context.AnotacionesAtleta.Remove(nota);
            await _context.SaveChangesAsync();
        }
    }
}
