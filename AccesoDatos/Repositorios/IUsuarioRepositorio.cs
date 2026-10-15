using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorIdAsync(int id);

        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);

        Task<List<Usuario>> ObtenerTodosAsync();

        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);

        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int exceptoId);

        Task<bool> ExisteEmailAsync(string email);

        Task<bool> ExisteEmailAsync(string email, int exceptoId);

        Task<Usuario?> ObtenerPorAtletaAsync(int atletaId);

        Task<bool> ExisteAlgunSuperAdminAsync();

        Task<Usuario> CrearAsync(Usuario usuario);

        Task ActualizarAsync(Usuario usuario);
    }
}
