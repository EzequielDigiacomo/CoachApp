using AccesoDatos;
using Entidades;
using Entidades.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Controladores.Seguridad
{
    /// <summary>
    /// Crea la cuenta de superadmin inicial (una sola vez) para poder entrar al sistema.
    /// Los datos se toman de la seccion "SuperAdmin" de appsettings.
    /// </summary>
    public class SuperAdminSeeder
    {
        private readonly CoachDbContext _context;
        private readonly PasswordServicio _passwordServicio;
        private readonly IConfiguration _configuracion;
        private readonly ILogger<SuperAdminSeeder> _logger;

        public SuperAdminSeeder(
            CoachDbContext context,
            PasswordServicio passwordServicio,
            IConfiguration configuracion,
            ILogger<SuperAdminSeeder> logger)
        {
            _context = context;
            _passwordServicio = passwordServicio;
            _configuracion = configuracion;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _context.Usuarios.AnyAsync(u => u.Rol == RolUsuario.SuperAdmin))
            {
                return;
            }

            var nombreUsuario = _configuracion["SuperAdmin:NombreUsuario"] ?? "superadmin";
            var password = _configuracion["SuperAdmin:Password"] ?? "Cambiar123!";

            var admin = new Usuario
            {
                NombreUsuario = nombreUsuario,
                Email = _configuracion["SuperAdmin:Email"] ?? "superadmin@coachapp.local",
                Nombre = "Super",
                Apellido = "Admin",
                Rol = RolUsuario.SuperAdmin,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            admin.PasswordHash = _passwordServicio.Hashear(admin, password);

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Superadmin inicial creado con usuario '{Usuario}'.", nombreUsuario);
        }
    }
}
