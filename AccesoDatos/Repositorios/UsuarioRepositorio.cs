using Entidades;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly CoachDbContext _context;

        public UsuarioRepositorio(CoachDbContext context)
        {
            _context = context;
        }

        public Task<Usuario?> ObtenerPorIdAsync(int id) =>
            _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario) =>
            _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario);

        public Task<List<Usuario>> ObtenerTodosAsync() =>
            _context.Usuarios.OrderBy(u => u.NombreUsuario).ToListAsync();

        public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario) =>
            _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario);

        public Task<bool> ExisteEmailAsync(string email) =>
            _context.Usuarios.AnyAsync(u => u.Email == email);

        public Task<bool> ExisteAlgunSuperAdminAsync() =>
            _context.Usuarios.AnyAsync(u => u.Rol == RolUsuario.SuperAdmin);

        public async Task<Usuario> CrearAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}
