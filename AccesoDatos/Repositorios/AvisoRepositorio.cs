using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class AvisoRepositorio : IAvisoRepositorio
    {
        private readonly CoachDbContext _context;

        public AvisoRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<List<Aviso>> ListarAsync(bool soloActivos)
        {
            var consulta = _context.Avisos.AsQueryable();

            if (soloActivos)
            {
                consulta = consulta.Where(a => a.Activo);
            }

            return consulta
                .OrderByDescending(a => a.FechaCreacion)
                .ToListAsync();
        }

        public Task<Aviso?> ObtenerAsync(int id) =>
            _context.Avisos.FirstOrDefaultAsync(a => a.Id == id);

        public async Task<Aviso> CrearAsync(Aviso aviso)
        {
            _context.Avisos.Add(aviso);
            await _context.SaveChangesAsync();
            return aviso;
        }

        public async Task ActualizarAsync(Aviso aviso)
        {
            _context.Avisos.Update(aviso);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Aviso aviso)
        {
            _context.Avisos.Remove(aviso);
            await _context.SaveChangesAsync();
        }
    }
}
