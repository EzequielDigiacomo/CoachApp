using Entidades;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class CuentaGarminRepositorio : ICuentaGarminRepositorio
    {
        private readonly CoachDbContext _context;

        public CuentaGarminRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<CuentaGarmin?> ObtenerPorUsuarioAsync(int usuarioId) =>
            _context.CuentasGarmin.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        public async Task GuardarAsync(CuentaGarmin cuenta)
        {
            if (_context.Entry(cuenta).State == EntityState.Detached)
            {
                _context.CuentasGarmin.Add(cuenta);
            }

            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(CuentaGarmin cuenta)
        {
            _context.CuentasGarmin.Remove(cuenta);
            await _context.SaveChangesAsync();
        }
    }
}
