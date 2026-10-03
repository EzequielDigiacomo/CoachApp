using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorIdAsync(int id);

        Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario);

        Task<List<Usuario>> ObtenerTodosAsync();

        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario);

        Task<bool> ExisteEmailAsync(string email);

        Task<bool> ExisteAlgunSuperAdminAsync();

        Task<Usuario> CrearAsync(Usuario usuario);

        Task ActualizarAsync(Usuario usuario);
    }
}
