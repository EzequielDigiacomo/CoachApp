using Entidades;

namespace AccesoDatos.Repositorios
{
    public interface ICuentaGarminRepositorio
    {
        Task<CuentaGarmin?> ObtenerPorUsuarioAsync(int usuarioId);

        Task GuardarAsync(CuentaGarmin cuenta);

        Task EliminarAsync(CuentaGarmin cuenta);
    }
}
