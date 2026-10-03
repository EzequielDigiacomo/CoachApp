using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class AtletaRepositorio : IAtletaRepositorio
    {
        private readonly CoachDbContext _context;

        public AtletaRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public async Task<List<Atleta>> ObtenerTodosAsync(string? busqueda, bool incluirInactivos)
        {
            var query = _context.Atletas.AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(a => a.Activo);
            }

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                query = query.Where(a =>
                    EF.Functions.ILike(a.Nombre, $"%{texto}%") ||
                    EF.Functions.ILike(a.Apellido, $"%{texto}%") ||
                    a.Dni.Contains(texto));
            }

            return await query
                .OrderBy(a => a.Apellido)
                .ThenBy(a => a.Nombre)
                .ToListAsync();
        }

        public Task<Atleta?> ObtenerPorIdAsync(int id) =>
            _context.Atletas.FirstOrDefaultAsync(a => a.Id == id);

        public Task<Atleta?> ObtenerPorDniAsync(string dni) =>
            _context.Atletas.FirstOrDefaultAsync(a => a.Dni == dni);

        public Task<bool> ExisteDniAsync(string dni, int? excluirId = null) =>
            _context.Atletas.AnyAsync(a => a.Dni == dni && (excluirId == null || a.Id != excluirId));

        public async Task<Atleta> CrearAsync(Atleta atleta)
        {
            _context.Atletas.Add(atleta);
            await _context.SaveChangesAsync();
            return atleta;
        }

        public async Task ActualizarAsync(Atleta atleta)
        {
            _context.Atletas.Update(atleta);
            await _context.SaveChangesAsync();
        }
    }
}
